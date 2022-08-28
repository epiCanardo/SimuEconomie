using System.Text;

namespace Economy
{
    public class Station : Storage
    {
        public TradingBoard Board { get; set; }

        public MerchendiseType Production { get; set; }
        public MerchendiseType Consumption { get; set; }

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
                TradingLine line = new TradingLine();
                line.Owner = this;
                line.Item = merchendise;

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
                int remainingSpace = StorageCapacity - (merchendise.Quantity * merchendise.Weight);

                // 2. calcul de la quantité du produit qu'il est possible d'acheter

                // la station n'achète pas ce qu'elle produit
                if (Production == merchendise.MerchendiseType)
                    line.QuantityToBuy = 0;
                else
                {
                    var buyingQuantity = (int)Math.Floor(remainingSpace / (decimal)merchendise.Weight);
                    line.QuantityToBuy = buyingQuantity;
                }

                // 3. calcul des prix

                // règles spécifiques si la station produit le bien
                if (Production == merchendise.MerchendiseType)
                {                    
                    line.UnitBuyingPrice = -1;
                    // le prix de vente initial est fixé à -20% par rapport à sa valeur universelle
                    line.UnitSellingPrice = 0.8 * merchendise.UniversalValue;
                }
                // règles spécifiques si la station consomme le bien
                else if (Consumption == merchendise.MerchendiseType)
                {
                    line.UnitSellingPrice = -1;
                    // le prix d'achat initial est fixé à +20% par rapport à sa valeur universelle
                    line.UnitBuyingPrice = 1.2 * merchendise.UniversalValue;
                }
                else
                {                    
                    // si quantité restante < 25% du total achetable = shortage => hausse du prix d'achat de 10% et hausse du prix de vente de 10%
                    if (merchendise.Quantity < 0.25 * (merchendise.Quantity + line.QuantityToBuy))
                    {
                        line.UnitBuyingPrice = merchendise.UniversalValue + 0.1 * merchendise.UniversalValue;
                        line.UnitSellingPrice = merchendise.UniversalValue + 0.1 * merchendise.UniversalValue;
                    }
                    // si quantité restante > 75% du total achetable = excès => baisse du prix d'achat de 10% et baisse du prix de vente de 10%
                    else if (merchendise.Quantity > 0.75 * (merchendise.Quantity + line.QuantityToBuy))
                    {
                        line.UnitBuyingPrice = merchendise.UniversalValue - 0.1 * merchendise.UniversalValue;
                        line.UnitSellingPrice = merchendise.UniversalValue - 0.1 * merchendise.UniversalValue;
                    }
                    // si pas de shortage ni d'excès, retour au prix universel avec une marge <> 1% entre achat et vente
                    else
                    {
                        line.UnitBuyingPrice = merchendise.UniversalValue;
                        line.UnitSellingPrice = merchendise.UniversalValue;
                    }

                    // dans tous les cas, marge entre achat et vente (A -1% / V + 1%)
                    line.UnitBuyingPrice -= 0.01 * merchendise.UniversalValue;
                    line.UnitSellingPrice += 0.01 * merchendise.UniversalValue;
                }

                // 4. quantité de produit à la vente => correspond au stock
                if (Consumption == merchendise.MerchendiseType)
                    line.QuantityToSell = 0;
                else
                    line.QuantityToSell = merchendise.Quantity;

                // 5. Pour affichage lorsque le bien n'est pas à acheter ou à vendre
                if (line.QuantityToBuy == 0)
                    line.UnitBuyingPrice = -1;

                if (line.QuantityToSell == 0)
                    line.UnitSellingPrice = -1;
            }
        }

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
