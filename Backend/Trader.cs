using System.Text;

namespace Backend
{
    public abstract class Trader : Entity
    {
        protected static Random rnd = new(DateTime.Now.Millisecond);
        public double Account { get; set; }
        public TraderState CurrentState { get; set; }
        public List<Trade> Trades { get; set; }

        public double TotalGain { get; set; }

        public Trader()
        {
            Trades = new List<Trade>();
        }

        public enum TraderState
        {
            InStation = 0,
            Flying = 1
        }
    }

    public class FixedTrader : Trader
    {
        public Station WorkingStation { get; set; }

        public async Task StorageCleanup()
        {
            // boucle principale du jeu 
            while (true)
            {
                Thread.Sleep(1000);

                // scan des biens pour réaligner les stocks en cas de dépassement temporaire
                foreach (TradingLine tradeline in WorkingStation.Board.TradingLines)
                {
                    if (tradeline.QuantityToBuy < 0)
                        tradeline.Item.Quantity = (int)Math.Floor(WorkingStation.StorageCapacity / (decimal)tradeline.Item.UniversalMerchendise.Weight);

                    if (tradeline.Item.Quantity < 0)
                        tradeline.Item.Quantity = 0;

                    WorkingStation.UpdateLine(tradeline, tradeline.Item);
                }
            }
        }
    }

    public class FlyingTrader : Trader
    {
        public Ship OwnedShip { get; set; }
        public Entity StartingPoint { get; set; }
        public Entity DestinationPoint { get; set; }
        public double TotalDistance { get; set; }

        public async Task MainLoop()
        {
            // boucle principale du marchand
            while (true)
            {
                // attente systématique aléatoire
                Thread.Sleep(rnd.Next(1000, 5000));

                switch (CurrentState)
                {
                    case TraderState.InStation:

                        // 1. vente des marchandises en soute (si possible)
                        // si le point de départ est au autre station, commerce
                        if (OwnedShip.Merchendises.Any() && StartingPoint.ID != OwnedShip.Localisation.ID)
                        {
                            // paiement

                            // uniquement une seule marchandise pour le moment !  
                            // on vérifie pas s'il reste de la place, on s'en bat les couilles on balance tout !
                            TradingLine line = (OwnedShip.Localisation as Station).Board.TradingLines.First(x =>
                                x.Item.UniversalMerchendise.MerchendiseType ==
                                OwnedShip.Merchendises[0].UniversalMerchendise.MerchendiseType);

                            // mise à jour du trade pour historique
                            Trade trade = Trades.Last(x => !x.Completed);
                            trade.SoldPrice = OwnedShip.Merchendises[0].Quantity * line.UnitBuyingPrice;
                            trade.SoldQuantity = OwnedShip.Merchendises[0].Quantity;
                            trade.Completed = true;

                            // ajout au stockage de la station
                            Merchendise merchendise = (OwnedShip.Localisation as Station).Merchendises.First(x =>
                                x.UniversalMerchendise.MerchendiseType ==
                                line.Item.UniversalMerchendise.MerchendiseType);
                            merchendise.Quantity += trade.SoldQuantity;

                            // mise à jour du solde
                            trade.FinalGain = trade.SoldPrice - trade.BoughtPrice - trade.TransportationCost;
                            TotalGain += trade.FinalGain;
                            Account += trade.SoldPrice - trade.TransportationCost;
                            TotalDistance += trade.TransportationDistance;

                            // suppression de la marchandise de la soute
                            OwnedShip.Merchendises.RemoveAt(0);

                            // mise à jour de la TradingLine associée dans la station
                            (OwnedShip.Localisation as Station).UpdateLine(line, merchendise);
                        }

                        // 2. attente aléatoire
                        Thread.Sleep(rnd.Next(1000, 5000));

                        StartingPoint = OwnedShip.Localisation;
                        DestinationPoint = null;
                        int buyQty = 0;
                        Station selectedStation = null;
                        TradingLine selectedLine = null;
                        double
                            estimatedgain =
                                -1000; // limite de perte. si aucun trade > à cette valeur, le trader attend le prochain tick
                        double transportationCost = 0;

                        // 3. calcul du meilleur trade
                        // itération sur la liste des marchandises présentes dans la station
                        foreach (TradingLine line in (OwnedShip.Localisation as Station).Board.TradingLines.Where(x =>
                                     x.QuantityToSell > 0))
                        {
                            // on vérifie la quantité max à acheter
                            var maxtoBuy = Convert.ToInt32(Math.Min(
                                OwnedShip.GetRemainingStorageSpace() / line.Item.UniversalMerchendise.Weight,
                                Math.Min(line.QuantityToSell, Math.Floor(Account / line.UnitSellingPrice))));

                            // on cherche le prix d'achat le plus élevé dans les autres stations
                            if (maxtoBuy > 0)
                            {
                                IOrderedEnumerable<TradingLine> sortedLines = World.Instance.SimulationParameters.Stations
                                    .Where(x => x.ID != StartingPoint.ID) // on exclu la station actuelle
                                    .SelectMany(x =>
                                        x.Board.TradingLines) // on liste toutes les lignes de toutes les stations
                                    .Where(x => x.Item.UniversalMerchendise.MerchendiseType ==
                                                line.Item.UniversalMerchendise.MerchendiseType &&
                                                x.QuantityToBuy >= maxtoBuy /*&&
                                                x.UnitBuyingPrice >= line.UnitSellingPrice*/) // critères de choix : même marchandise, peut acheter, trade brut positif ou neutre
                                    .OrderByDescending(x =>
                                        x.UnitBuyingPrice); // tri descendant sur le prix d'achat cible

                                // on parcours les trades candidats pour sélectionner le meilleur
                                foreach (TradingLine sortedLine in sortedLines)
                                {
                                    // calcul de l'estimation du gain avec le prix le plus élevé
                                    double buyprice = line.UnitSellingPrice * maxtoBuy;
                                    double sellingprice = sortedLine.UnitBuyingPrice * maxtoBuy;

                                    // déduction du prix du trajet
                                    transportationCost = Math.Round(
                                        line.Owner.DistanceWithOtherStorage(sortedLine.Owner) *
                                        (OwnedShip.StorageCapacity * 0.1),
                                        2);

                                    double localgain = sellingprice - buyprice - transportationCost;

                                    if (localgain > estimatedgain)
                                    {
                                        estimatedgain = localgain;
                                        selectedLine = line;
                                        selectedStation = sortedLine.Owner;
                                        buyQty = maxtoBuy;
                                    }
                                }
                            }
                        }

                        // 4. si un trade a été trouvé, on procède à l'achat
                        if (buyQty > 0)
                        {
                            // retrait du stockage de la station
                            Merchendise item = (OwnedShip.Localisation as Station).Merchendises.First(x =>
                                x.UniversalMerchendise.MerchendiseType ==
                                selectedLine.Item.UniversalMerchendise.MerchendiseType);
                            item.Quantity -= buyQty;

                            Trade trade = new Trade
                            {
                                Item = item,
                                FromStation = (OwnedShip.Localisation as Station),
                                ToStation = selectedStation,
                                BoughtPrice = buyQty * selectedLine.UnitSellingPrice,
                                BoughtQuantity = buyQty,
                                TransportationCost = transportationCost,
                                TransportationDistance =
                                    Convert.ToInt32(
                                        (OwnedShip.Localisation as Station).DistanceWithOtherStorage(selectedStation)),
                                EstimatedGain = estimatedgain
                            };

                            // paiement
                            Account -= trade.BoughtPrice;

                            // ajout dans la soute
                            OwnedShip.Merchendises.Add(new Merchendise
                            {
                                UniversalMerchendise = item.UniversalMerchendise,
                                Name = trade.Item.Name,
                                Quantity = trade.BoughtQuantity,

                            });

                            // mise à jour de la TradingLine associée dans la station
                            (OwnedShip.Localisation as Station).UpdateLine(selectedLine, item);

                            // définition de la destination
                            DestinationPoint = selectedStation;

                            // placement en zone de vol
                            CurrentState = TraderState.Flying;

                            Trades.Add(trade);
                        }

                        break;
                    case TraderState.Flying:
                        // simulation du voyage
                        OwnedShip.Localisation = null;
                        Thread.Sleep((int) Trades.Last(x => !x.Completed).TransportationDistance * 2000);

                        // arrivée
                        OwnedShip.Localisation = DestinationPoint;
                        CurrentState = TraderState.InStation;
                        break;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{Name,-20} | {MoneyText(TotalGain),-10} | {MoneyText(Account),-10} | {TradesCountText,-8} | {TotalDistance,-10} | {LastTradeGainText(),-15} | {CargoText,-15} | {CurrentActionText(),-80} | {StorageText(),-20}");
            return result.ToString();
        }

        private string TradesCountText => (Trades.Any(x=>x.Completed)) ? Trades.Count(x => x.Completed).ToString() : "0";        

        private string LastTradeGainText()
        {
            var lastTrade = Trades.LastOrDefault(x => x.Completed);
            if (lastTrade != null)
            {
                double gain = lastTrade.FinalGain;
                if (gain == 0)
                    return "0.00";
                else
                    return gain.ToString("#.#");
            }
            return "--";

        }

        private string MoneyText(double money)
        {
            if (money == 0)
                return "0.00";
            else
                return money.ToString("#.#");
        }

        private string CargoText => $"{OwnedShip.TotalCargoHold()} / {OwnedShip.StorageCapacity}";

        private string CurrentActionText()
        {
            switch (CurrentState)
            {
                case TraderState.InStation:
                    return $"Fait du commerce dans {((Station)OwnedShip.Localisation)?.Name}";
                case TraderState.Flying:
                    return
                        $"{Trades.Last(x => !x.Completed).FromStation.Name} =>>> {Trades.Last(x => !x.Completed).ToStation.Name}" +
                        $" [Distance : {Trades.Last(x => !x.Completed).TransportationDistance}]" +
                        $" [Résultat estimé : {Trades.Last(x => !x.Completed).EstimatedGain:#.#}]";
                default:
                    return string.Empty;
            }
        }

        private string StorageText()
        {
            if (OwnedShip.Merchendises.Any())
                return $"{OwnedShip.Merchendises?[0].Name} x {OwnedShip.Merchendises?[0].Quantity}";

            return "Vide";
        }
    }
}