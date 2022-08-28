﻿using Backend;

namespace Economy
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
        public double UniversalValue { get; set; }
        public int Weight { get; set; }
        public int Quantity { get; set; }
        public MerchendiseType MerchendiseType { get; set; }
        public int BaseProductionRate => 1;
        public int BaseConsumptionRate => 1;
        public int StackWeight => Weight * Quantity;

    }
}