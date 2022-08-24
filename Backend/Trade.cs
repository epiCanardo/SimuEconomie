using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend;

namespace Economy
{
    public class Trade
    {
        public bool Completed { get; set; }
        public Merchendise Item { get; set; }
        public Station FromStation { get; set; }
        public Station ToStation { get; set; }
        public int BoughtQuantity { get; set; }
        public double BoughtPrice { get; set; }
        public int SoldQuantity { get; set; }
        public double SoldPrice { get; set; }
        public double TransportationCost { get; set; }
        public double TransportationDistance { get; set; }
        public Trade()
        {
            Completed = false;
        }
    }

    public class TradingLine
    {
        public Storage Owner { get; set; }
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
}
