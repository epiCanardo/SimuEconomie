using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Economy
{
    public abstract class Entity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double[] Coordinates { get; set; }
    }

    public enum MerchendiseType
    {
        Gold = 0,
        Orionum = 1,
        Wood = 2,
        Iron = 3,
        Water = 4,
        Silicon = 5,
        Deuterium = 6
    }

    public class Merchendise : Entity
    {
        public double UniversalValue { get; set; }
        public int Weight { get; set; }
        public int Quantity { get; set; }
        public MerchendiseType MerchendiseType { get; set; }
    }
    
    public abstract class Storage : Entity
    {
        public int StorageCapacity { get; set; }
        public List<Merchendise> Merchendises { get; set; }

        public int GetRemainingStorageSpace()
        {
            int space = 0;
            foreach (var item in Merchendises)
            {
                space += item.Quantity * item.Weight;
            }
            return StorageCapacity - space;
        }

        public Storage()
        {
            Merchendises = new List<Merchendise>();
        }
    }

    public class Trade
    {
        public bool Completed { get; set; }
        public Merchendise Item { get; set; }
        public int BoughtQuantity { get; set; }
        public double BoughtPrice { get; set; }
        public int SoldQuantity { get; set; }
        public double SoldPrice { get; set; }
        public Trade()
        {
            Completed = false;
        }
    }

    public class TradingLine
    {
        public Entity Owner { get; set; }
        public Merchendise Item { get; set; }
        public int QuantityToBuy { get; set; }
        public int QuantityToSell { get; set; }
        public double UnitSellingPrice { get; set; }
        public double UnitBuyingPrice { get; set; }

        public TradingLine()
        {

        }


        public override string ToString()
        {
            // cas de stock plein : pas de prix d'achat
            //string buyingPrice = (UnitBuyingPrice > 0) ? UnitBuyingPrice.ToString("#.##") : "--";
            // cas de stock vide : pas de prix de vente
            //string sellingPrice = (UnitSellingPrice < 100000) ? UnitSellingPrice.ToString("#.##") : "--";

            string buyingPrice = UnitBuyingPrice.ToString("#.##");
            string sellingPrice = UnitSellingPrice.ToString("#.##");

            return $"{Item.Name,-20} | {Item.Quantity,-20} | {QuantityToBuy,-20} | {buyingPrice,-20} | {QuantityToSell,-20} | {sellingPrice,-20}";
        }
    }

    public class TradingBoard
    {
        public List<TradingLine> TradingLines { get; set; }

        public TradingBoard()
        {
            TradingLines = new List<TradingLine>();
        }

        public override string ToString()
        {            
            StringBuilder result = new StringBuilder();
            result.AppendLine($"{"Nom",-20} | {"Quantité dispo",-20} | {"Achète max",-20} | {"Achète aux prix",-20} | {"Vends max",-20} | {"Vend aux prix",-20}");
            foreach (TradingLine line in TradingLines)
            {
                result.AppendLine(line.ToString());
            }
            return result.ToString();
        }
    }

    public class Station : Storage
    {
        public TradingBoard Board { get; set; }

        /// <summary>
        /// Initialisation du TradingBoard
        /// </summary>
        public void InitializeTadringBoard()
        {
            Board = new TradingBoard();

            // pour chaque bien présent dans la station
            foreach (Merchendise merchendise in Merchendises)
            {
                // création d'une nouvelle ligne
                TradingLine line = new TradingLine();
                line.Owner = this;
                line.Item = merchendise;

                // mise à jour selon les donnée de la marchandise (espace restant etc)
                UpdateLine(line, merchendise);

                // ajout au board
                Board.TradingLines.Add(line);
            }
        }

        private object objectToLock = new object();

        public void UpdateLine(TradingLine line, Merchendise merchendise)
        {
            lock (objectToLock)
            {

                // calcul du prix d'achat initial et de la quantité
                // 1. calcul de la capacité restante
                //int remainingSpace = GetRemainingStorageSpace();
                int remainingSpace = StorageCapacity - (merchendise.Quantity * merchendise.Weight);

                // 2. calcul de la quantité du produit qu'il est possible d'acheter
                var buyingQuantity = (int)Math.Floor((decimal)remainingSpace / (decimal)merchendise.Weight);
                line.QuantityToBuy = buyingQuantity;

                // 3. calcul des prix
                // si quantité restante < 25% du total achetable = shortage => hausse du prix d'achat de 10% et hausse du prix de vente de 10%
                if ((double)merchendise.Quantity < 0.25 * (double)(merchendise.Quantity + line.QuantityToBuy))
                {
                    line.UnitBuyingPrice = merchendise.UniversalValue + 0.1 * merchendise.UniversalValue;
                    line.UnitSellingPrice = merchendise.UniversalValue + 0.1 * merchendise.UniversalValue;
                }
                // si quantité restante > 75% du total achetable = excés => baisse du prix d'achat de 10% et baisse du prix de vente de 10%
                else if ((double)merchendise.Quantity > 0.75 * (double)(merchendise.Quantity + line.QuantityToBuy))
                {
                    line.UnitBuyingPrice = merchendise.UniversalValue - 0.1 * merchendise.UniversalValue;
                    line.UnitSellingPrice = merchendise.UniversalValue - 0.1 * merchendise.UniversalValue;
                }
                // si pas de shortage ni d'excès, retour au prix universel
                else
                {
                    line.UnitBuyingPrice = merchendise.UniversalValue;
                    line.UnitSellingPrice = merchendise.UniversalValue;
                }
                
                line.QuantityToSell = merchendise.Quantity;
                
                //line.UnitSellingPrice = (line.QuantityToSell > 0) ? merchendise.UniversalValue - malus : 0;

                //var malus = ((double)remainingSpace / (double)StorageCapacity) / 10 * merchendise.UniversalValue;
                //var malus = (1 - ((double)buyingQuantity / (double)(merchendise.Quantity + buyingQuantity))) / 10 * merchendise.UniversalValue;                
                //line.UnitBuyingPrice = (buyingQuantity > 0) ? merchendise.UniversalValue + malus : 0;

                // le prix de vente est fixé à prix d'achat + 1%
                //line.QuantityToSell = merchendise.Quantity;
                //line.UnitSellingPrice = (merchendise.Quantity > 0) ? line.UnitBuyingPrice + line.UnitBuyingPrice * 0.01 : 100000;
                //line.UnitSellingPrice = line.UnitBuyingPrice + line.UnitBuyingPrice * 0.01;
            }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine($"Biens proposés au commerce - Station {Name}");
            result.AppendLine("*".PadRight(50, '*'));
            result.AppendLine(Board.ToString());
            return result.ToString();
        }

        public string ToStringNew()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine($"Biens proposés au commerce - Station {Name}");
            result.AppendLine("*".PadRight(50, '*'));
            result.AppendLine(Board.ToString());
            return result.ToString();
        }
    }

    public class Ship : Storage
    {

        public Entity Localisation { get; set; }
    }    

    public abstract class Trader : Entity    
    {
        protected static Random rnd = new Random(DateTime.Now.Millisecond);

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
                            // on vérifie pas s'il reste de la place, on s'en bat les couilles on balance tout !
                            var line = (OwnedShip.Localisation as Station).Board.TradingLines.First(x => x.Item.MerchendiseType == OwnedShip.Merchendises[0].MerchendiseType);
                            Account += OwnedShip.Merchendises[0].Quantity * line.UnitBuyingPrice; // uniquement une seule marchandise pour le moment !                   

                            // mise à jour du trade pour historique
                            Trade trade = Trades.Last(x=>!x.Completed);
                            trade.SoldPrice = OwnedShip.Merchendises[0].Quantity * line.UnitBuyingPrice;
                            trade.SoldQuantity = OwnedShip.Merchendises[0].Quantity;
                            trade.Completed = true;
                            TotalGain += (trade.SoldPrice - trade.BoughtPrice);

                            // ajout au stockage de la station
                            var item = (OwnedShip.Localisation as Station).Merchendises.First(x => x.MerchendiseType == line.Item.MerchendiseType);
                            item.Quantity += trade.SoldQuantity;

                            // suppression de la marchandise de la soute
                            OwnedShip.Merchendises.RemoveAt(0);

                            // mise à jour de la TradingLine associée dans la station
                            (OwnedShip.Localisation as Station).UpdateLine(line, item);
                        }

                        // 2. attente
                        Thread.Sleep(rnd.Next(1000, 5000));

                        StartingPoint = OwnedShip.Localisation;
                        DestinationPoint = null;
                        int buyQty = 0;
                        Station selectedStation = null;
                        TradingLine selectedLine = null;
                        double estimatedgain = -1;

                        // 3. calcul du meilleur trade
                        // itération sur la liste des marchandises présentes dans la station
                        foreach (TradingLine line in (OwnedShip.Localisation as Station).Board.TradingLines.Where(x=>x.QuantityToSell > 0))
                        {
                            // on vérifie la quantité max à acheter
                            var maxtoBuy = Convert.ToInt32(Math.Min(OwnedShip.GetRemainingStorageSpace() / line.Item.Weight, Math.Min(line.QuantityToSell, Math.Floor(Account / line.UnitSellingPrice))));

                            // on cherche le prix d'achat le plus élevé dans les autres stations
                            if (maxtoBuy > 0)
                            {
                                var sortedLines = World.Instance.Stations
                                    .Where(x => x.ID != StartingPoint.ID) // on exclu la station actuelle
                                    .SelectMany(x => x.Board.TradingLines) // on liste toutes les lignes de toutes les stations
                                    .Where(x=>x.Item.MerchendiseType == line.Item.MerchendiseType && 
                                              x.QuantityToBuy >= maxtoBuy && 
                                              x.UnitBuyingPrice >= line.UnitSellingPrice) // critères de choix : même marchandise, peut acheter, trade brut positif ou neutre
                                    .OrderByDescending(x => x.UnitBuyingPrice); // tri descendant sur le prix d'achat cible

                                if (sortedLines.Any())
                                {
                                    // calcul de l'estimation du gain avec le prix le plus élevé
                                    double buyprice = line.UnitSellingPrice * maxtoBuy;
                                    double sellingprice = sortedLines.First().UnitBuyingPrice * maxtoBuy;
                                    var localgain = sellingprice - buyprice;

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
                                BoughtPrice = buyQty * selectedLine.UnitSellingPrice,
                                BoughtQuantity = buyQty
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
                        Thread.Sleep(rnd.Next(5000, 10000));

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
            result.Append($"{Name,-20} | {TotalGain.ToString("#.##"),-20} | {Account.ToString("#.##"),-20}| {GetCurrentLocalisation(),-50}| {GetCurrentAction(),-50}| {GetStorage(),-20}");
            return result.ToString();
        }

        private string GetCurrentLocalisation()
        {
            switch (CurrentState)
            {
                case TraderState.InStation:
                    return ((Station)OwnedShip.Localisation)?.Name;                    
                case TraderState.Flying:
                    return $"En transit vers {DestinationPoint?.Name}";                    
                default:
                    return string.Empty;
            }
        }

        private string GetCurrentAction()
        {
            switch (CurrentState)
            {
                case TraderState.InStation:
                    return $"Fait du commerce";
                case TraderState.Flying:
                    return $"Glande dans son vaisseau";
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
