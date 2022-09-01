namespace Backend
{
    public enum MerchendiseType
    {
        Gold = 0,
        Orionum = 1,
        Wood = 2,
        Iron = 3,
        Water = 4,
        Silicon = 5,
        Deuterium = 6,
        Nothing = 100
    }

    public class Merchendise : Entity
    {
        public UniversalMerchendise UniversalMerchendise { get; set; }
        public int Quantity { get; set; }
        public int StackWeight => UniversalMerchendise.Weight * Quantity;

        public Merchendise()
        {
            UniversalMerchendise = new UniversalMerchendise();
        }
    }

    public class UniversalMerchendise
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public int Weight { get; set; }
        public MerchendiseType MerchendiseType { get; set; }
        public int BaseProductionRate { get; set; }
        public int BaseConsumptionRate { get; set; }
        public string News { get; set; }
        public int Demand { get; set; }

        public UniversalMerchendise()
        {
            Demand = 1;
            ApplyNewDemand();
        }

        public void SetNewDemand(int rndResult)
        {
            if (rndResult == 0)
            {
                Demand = Math.Max(0, Demand - 1);
                Value -= Value * 0.1;
            }
            else
            {
                Demand = Math.Min(2, Demand + 1);
                Value += Value * 0.1;
            }

            ApplyNewDemand();
        }

        public void ApplyNewDemand()
        {
            if (Demand == 0)
            {
                BaseConsumptionRate = 1;
                BaseProductionRate = 1;
                News = $"Consommation basse ({BaseConsumptionRate} pour 5 secondes)";
            }
            else if (Demand == 1)
            {
                BaseConsumptionRate = 2;
                BaseProductionRate = 2;
                News = $"Consommation normale ({BaseConsumptionRate} pour 5 secondes)";
            }
            else
            {
                BaseConsumptionRate = 4;
                BaseProductionRate = 4;
                News = $"Consommation élevée ({BaseConsumptionRate} pour 5 secondes)";
            }
        }
    }
}