using System.Text;

namespace Backend
{
    public class Station : Storage
    {
        public TradingBoard Board { get; set; }

        public MerchendiseType Production { get; set; }
        public MerchendiseType Consumption { get; set; }
        public int MainTick { get; set; }

        public async Task UpdateProdAndConso()
        {
            while (true)
            {
                // Production et consommation toutes les MainTick secondes
                Thread.Sleep(MainTick);

                // Production des biens
                TradingLine line =
                    Board.TradingLines.FirstOrDefault(x =>
                        x.Item.UniversalMerchendise.MerchendiseType.Equals(Production));
                if (line != null)
                {
                    int remainingQuantity =
                        (int) Math.Floor(
                            (StorageCapacity - (decimal) (line.Item.Quantity * line.Item.UniversalMerchendise.Weight)) /
                            line.Item.UniversalMerchendise.Weight);
                    if (remainingQuantity > line.Item.UniversalMerchendise.BaseProductionRate)
                        line.Item.Quantity += line.Item.UniversalMerchendise.BaseProductionRate;
                    UpdateLine(line, line.Item);
                }

                // Consommation des biens
                line = Board.TradingLines.FirstOrDefault(x =>
                    x.Item.UniversalMerchendise.MerchendiseType.Equals(Consumption));
                if (line != null && line.Item.Quantity > line.Item.UniversalMerchendise.BaseConsumptionRate)
                {
                    line.Item.Quantity -= line.Item.UniversalMerchendise.BaseConsumptionRate;
                    UpdateLine(line, line.Item);
                }
            }
        }

        /// <summary>
        /// Initialisation du TradingBoard
        /// </summary>
        public void InitializeTradingBoard()
        {
            Board = new TradingBoard();

            // pour chaque bien présent dans la station
            foreach (Merchendise merchendise in Merchendises)
            {
                // création d'une nouvelle ligne
                TradingLine line = new TradingLine(this, merchendise);

                // mise à jour selon les donnée de la marchandise (espace restant etc)
                UpdateLine(line, merchendise);

                // ajout au board
                Board.TradingLines.Add(line);
            }
        }

        private object objectToLock = new();

        public void UpdateLine(TradingLine line, Merchendise merchendise)
        {
            lock (objectToLock)
            {
                // 1. calcul de la capacité restante
                int remainingSpace = StorageCapacity - (merchendise.Quantity * merchendise.UniversalMerchendise.Weight);

                // 2. calcul de la quantité du produit qu'il est possible d'acheter

                // la station n'achète pas ce qu'elle produit
                if (Production == merchendise.UniversalMerchendise.MerchendiseType)
                    line.QuantityToBuy = 0;
                else
                {
                    var buyingQuantity =
                        (int) Math.Floor(remainingSpace / (decimal) merchendise.UniversalMerchendise.Weight);
                    line.QuantityToBuy = buyingQuantity;
                }

                // 3. calcul des prix

                // règles spécifiques si la station produit le bien
                if (Production == merchendise.UniversalMerchendise.MerchendiseType)
                {
                    //line.UnitBuyingPrice = -1;
                    // le prix de vente initial est fixé à -20% par rapport à sa valeur universelle
                    line.UnitSellingPrice = 0.8 * merchendise.UniversalMerchendise.Value;
                }
                // règles spécifiques si la station consomme le bien
                else if (Consumption == merchendise.UniversalMerchendise.MerchendiseType)
                {
                    // line.UnitSellingPrice = -1;
                    // le prix d'achat initial est fixé à +20% par rapport à sa valeur universelle
                    line.UnitBuyingPrice = 1.2 * merchendise.UniversalMerchendise.Value;
                }
                else
                {
                    // si quantité restante < 25% du total achetable = shortage => hausse du prix d'achat de 10% et hausse du prix de vente de 10%
                    if (merchendise.Quantity < 0.25 * (merchendise.Quantity + line.QuantityToBuy))
                    {
                        line.UnitBuyingPrice = merchendise.UniversalMerchendise.Value +
                                               0.1 * merchendise.UniversalMerchendise.Value;
                        line.UnitSellingPrice = merchendise.UniversalMerchendise.Value +
                                                0.1 * merchendise.UniversalMerchendise.Value;
                    }
                    // si quantité restante > 75% du total achetable = excès => baisse du prix d'achat de 10% et baisse du prix de vente de 10%
                    else if (merchendise.Quantity > 0.75 * (merchendise.Quantity + line.QuantityToBuy))
                    {
                        line.UnitBuyingPrice = merchendise.UniversalMerchendise.Value -
                                               0.1 * merchendise.UniversalMerchendise.Value;
                        line.UnitSellingPrice = merchendise.UniversalMerchendise.Value -
                                                0.1 * merchendise.UniversalMerchendise.Value;
                    }
                    // si pas de shortage ni d'excès, retour au prix universel avec une marge <> 1% entre achat et vente
                    else
                    {
                        line.UnitBuyingPrice = merchendise.UniversalMerchendise.Value;
                        line.UnitSellingPrice = merchendise.UniversalMerchendise.Value;
                    }

                    // dans tous les cas, marge entre achat et vente (A -1% / V + 1%)
                    line.UnitBuyingPrice -= 0.01 * merchendise.UniversalMerchendise.Value;
                    line.UnitSellingPrice += 0.01 * merchendise.UniversalMerchendise.Value;
                }

                // 4. quantité de produit à la vente => correspond au stock
                if (Consumption == merchendise.UniversalMerchendise.MerchendiseType)
                    line.QuantityToSell = 0;
                else
                    line.QuantityToSell = merchendise.Quantity;
            }
        }

        public IEnumerable<FlyingTrader> NegociatingTraders => World.Instance.FlyingTraders.Where(x =>
            x.OwnedShip.Localisation != null && x.OwnedShip.Localisation.Equals(this));

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine($"Biens proposés au commerce - Station {Name}");
            result.AppendLine("*".PadRight(50, '*'));
            result.AppendLine(Board.BoardText());
            return result.ToString();
        }
    }
}