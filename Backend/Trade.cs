using System.Text;

namespace Backend
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
        public double EstimatedGain { get; set; }
        public double FinalGain { get; set; }
        public Trade()
        {
            Completed = false;
        }
    }

    public class TradingLine
    {
        public Station Owner { get; set; }
        public Merchendise Item { get; set; }
        public int QuantityToBuy { get; set; }
        public int QuantityToSell { get; set; }
        public double UnitSellingPrice { get; set; }
        public double UnitBuyingPrice { get; set; }

        public TradingLine(Station owner, Merchendise item)
        {
            Owner = owner;
            Item = item;
        }

        public override string ToString()
        {
            string itemName = Item.Name;

            if (Item.UniversalMerchendise.MerchendiseType == Owner.Production)
                itemName = Item.Name + "(P)";

            else if (Item.UniversalMerchendise.MerchendiseType == Owner.Consumption)
                itemName = Item.Name + "(C)";

            return $"{itemName,-10} | {QuantityText,-20} | {PricesText,-12}";
        }
        private string QuantityText => $"{Item.Quantity} ({QuantityToBuy}/{QuantityToSell})";

        private string PricesText => $"{PrinceToString(QuantityToBuy, UnitBuyingPrice)}/{PrinceToString(QuantityToSell, UnitSellingPrice)}";

        private string PrinceToString(int quantity, double price)
        {
            if (quantity == 0)
                return "--";
            else
                return $"{price:#.#}";
        }
    }

    public class TradingBoard
    {
        public List<TradingLine> TradingLines { get; set; }

        public TradingBoard()
        {
            TradingLines = new List<TradingLine>();
        }

        public string BoardText()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine($"{"Nom",-10} | {"Quantités dispo",-20} | {"Tarifs",-10}");
            foreach (TradingLine line in TradingLines)
            {
                result.AppendLine(line.ToString());
            }
            return result.ToString();
        }
    }
}
