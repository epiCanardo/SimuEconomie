using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend;

namespace Economy
{
    public abstract class Trader : Entity
    {
        protected static Random rnd = new(DateTime.Now.Millisecond);

        public double Account { get; set; }
        public TraderState CurrentState { get; set; }
        public List<Trade> Trades { get; set; }
        //public double TotalGain()
        //{
        //    double result = 0;
        //    foreach (var trade in Trades.Where(x => x.Completed))
        //    {
        //        result += trade.SoldPrice - trade.BoughtPrice;
        //    }
        //    return result;
        //}

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
        public MerchendiseType Production { get; set; }

        private bool closureRequested = false;

        public async Task Loop()
        {
            // démarrage du temps

            // boucle principale du jeu 
            while (!closureRequested)
            {
                Thread.Sleep(50);
                // scan des biens pour réaligner les stocks en cas de dépassement temporaire
                foreach (TradingLine tradeline in WorkingStation.Board.TradingLines)
                {
                    if (tradeline.QuantityToBuy < 0)
                        tradeline.Item.Quantity = (int)Math.Floor((decimal)WorkingStation.StorageCapacity / (decimal)tradeline.Item.Weight);

                    if (tradeline.Item.Quantity < 0)
                        tradeline.Item.Quantity = 0;

                    WorkingStation.UpdateLine(tradeline, tradeline.Item);
                }

                Thread.Sleep(1000);

                // modification des stocks
                TradingLine line = WorkingStation.Board.TradingLines.First(x => x.Item.MerchendiseType.Equals(Production));
                int remainingQuantity = (int)Math.Floor(((decimal)WorkingStation.StorageCapacity - (decimal)(line.Item.Quantity * line.Item.Weight)) / (decimal)line.Item.Weight);
                line.Item.Quantity += Math.Min(remainingQuantity, 10);
                WorkingStation.UpdateLine(line, line.Item);

            }
        }

        public void Close()
        {
            closureRequested = true;
        }
    }

    public class FlyingTrader : Trader
    {
        public Ship OwnedShip { get; set; }
        public Entity StartingPoint { get; set; }
        public Entity DestinationPoint { get; set; }

        private bool closureRequested = false;

        public async Task Loop()
        {
            // boucle principale du marchand
            while (!closureRequested)
            {
                // attente systématique
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
                            TradingLine line = (OwnedShip.Localisation as Station).Board.TradingLines.First(x => x.Item.MerchendiseType == OwnedShip.Merchendises[0].MerchendiseType);
                            Account += OwnedShip.Merchendises[0].Quantity * line.UnitBuyingPrice;                  

                            // mise à jour du trade pour historique
                            Trade trade = Trades.Last(x => !x.Completed);
                            trade.SoldPrice = OwnedShip.Merchendises[0].Quantity * line.UnitBuyingPrice;
                            trade.SoldQuantity = OwnedShip.Merchendises[0].Quantity;
                            trade.Completed = true;
                            TotalGain += (trade.SoldPrice - trade.BoughtPrice);

                            // ajout au stockage de la station
                            Merchendise merchendise = (OwnedShip.Localisation as Station).Merchendises.First(x => x.MerchendiseType == line.Item.MerchendiseType);
                            merchendise.Quantity += trade.SoldQuantity;

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
                        double estimatedgain = -1000; // limite de perte
                        double transportationCost = 0;

                        // 3. calcul du meilleur trade
                        // itération sur la liste des marchandises présentes dans la station
                        foreach (TradingLine line in (OwnedShip.Localisation as Station).Board.TradingLines.Where(x => x.QuantityToSell > 0))
                        {
                            // on vérifie la quantité max à acheter
                            var maxtoBuy = Convert.ToInt32(Math.Min(OwnedShip.GetRemainingStorageSpace() / line.Item.Weight, Math.Min(line.QuantityToSell, Math.Floor(Account / line.UnitSellingPrice))));

                            // on cherche le prix d'achat le plus élevé dans les autres stations
                            if (maxtoBuy > 0)
                            {
                                var sortedLines = World.Instance.Stations
                                    .Where(x => x.ID != StartingPoint.ID) // on exclu la station actuelle
                                    .SelectMany(x => x.Board.TradingLines) // on liste toutes les lignes de toutes les stations
                                    .Where(x => x.Item.MerchendiseType == line.Item.MerchendiseType &&
                                              x.QuantityToBuy >= maxtoBuy &&
                                              x.UnitBuyingPrice >= line.UnitSellingPrice) // critères de choix : même marchandise, peut acheter, trade brut positif ou neutre
                                    .OrderByDescending(x => x.UnitBuyingPrice); // tri descendant sur le prix d'achat cible

                                if (sortedLines.Any())
                                {
                                    // calcul de l'estimation du gain avec le prix le plus élevé
                                    double buyprice = line.UnitSellingPrice * maxtoBuy;
                                    double sellingprice = sortedLines.First().UnitBuyingPrice * maxtoBuy;
                                    
                                    // déduction du prix du trajet
                                    transportationCost = Math.Round(line.Owner.DistanceWithOtherStorage(sortedLines.First().Owner) * (OwnedShip.StorageCapacity / 2),
                                        2);

                                    double localgain = sellingprice - buyprice - transportationCost;

                                    if (localgain > estimatedgain)
                                    {
                                        estimatedgain = localgain;
                                        selectedLine = line;
                                        selectedStation = (Station)sortedLines.First().Owner;
                                        buyQty = maxtoBuy;
                                    }
                                }
                            }
                        }

                        // 4. si un trade a été trouvé, on procède à l'achat
                        if (buyQty > 0)
                        {
                            // retrait du stockage de la station
                            Merchendise item = (OwnedShip.Localisation as Station).Merchendises.First(x => x.MerchendiseType == selectedLine.Item.MerchendiseType);
                            item.Quantity -= buyQty;

                            // construction du trade pour historique
                            Trades.Add(new Trade
                            {
                                Item = item,
                                FromStation = (OwnedShip.Localisation as Station),
                                ToStation = selectedStation,
                                BoughtPrice = buyQty * selectedLine.UnitSellingPrice,
                                BoughtQuantity = buyQty,
                                TransportationCost = transportationCost,
                                TransportationDistance = Convert.ToInt32((OwnedShip.Localisation as Station).DistanceWithOtherStorage(selectedStation))
                            });

                            // paiement
                            Account -= (buyQty * selectedLine.UnitSellingPrice);

                            // ajout dans la soute
                            OwnedShip.Merchendises.Add(new Merchendise
                            {
                                MerchendiseType = selectedLine.Item.MerchendiseType,
                                Name = selectedLine.Item.Name,
                                Quantity = buyQty,
                                UniversalValue = selectedLine.Item.UniversalValue,
                                Weight = selectedLine.Item.Weight
                            });

                            // mise à jour de la TradingLine associée dans la station
                            (OwnedShip.Localisation as Station).UpdateLine(selectedLine, item);

                            // définition de la destination
                            DestinationPoint = selectedStation;

                            // placement en zone de vol
                            CurrentState = TraderState.Flying;
                        }

                        // 4. attente
                        //Thread.Sleep(1000);

                        break;
                    case TraderState.Flying:
                        // simulation du voyage
                        Thread.Sleep((int)Trades.Last(x => !x.Completed).TransportationDistance * 2000);

                        // arrivée
                        OwnedShip.Localisation = DestinationPoint;
                        CurrentState = TraderState.InStation;
                        break;
                    default:
                        break;
                }
            }
        }

        public void Close()
        {
            closureRequested = true;
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append($"{Name,-20} | {TotalGain.ToString("#.##"),-20} | {Account.ToString("#.##"),-20} | {OwnedShip.StorageCapacity,-20} | {GetCurrentAction(),-100} | {GetStorage(),-20}");
            return result.ToString();
        }

        private string GetCurrentAction()
        {
            switch (CurrentState)
            {
                case TraderState.InStation:
                    return $"Fait du commerce dans {((Station)OwnedShip.Localisation)?.Name}";
                case TraderState.Flying:
                    return
                        $"{Trades.Last(x => !x.Completed).FromStation.Name} =>>> {Trades.Last(x => !x.Completed).ToStation.Name}" +
                        $" [Distance : {Trades.Last(x => !x.Completed).TransportationDistance}]" +
                        $" [Coût de transport : {Trades.Last(x => !x.Completed).TransportationCost}]";
           // return $"{((Station)OwnedShip.Localisation)?.Name} =>>> {DestinationPoint?.Name} [Distance : {Trades.Last(x => !x.Completed).TransportationDistance}]";
            default:
                    return string.Empty;
            }
        }

        private string GetStorage()
        {
            if (OwnedShip.Merchendises.Any())
                return $"{OwnedShip.Merchendises?[0].Name} x {OwnedShip.Merchendises?[0].Quantity}";
            else
                return "Vide";
        }
    }
}
