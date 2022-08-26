using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Economy
{
    public class Station : Storage
    {
        public TradingBoard Board { get; set; }

        public MerchendiseType Production { get; set; }

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
            result.AppendLine(Board.BoardText());
            return result.ToString();
        }
    }
}
