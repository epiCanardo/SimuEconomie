using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Economy
{
    public sealed class World
    {
        private static readonly Lazy<World> lazy = new Lazy<World>(() => new World(), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        public static World Instance => lazy.Value;
        private static Random rnd = new Random(DateTime.Now.Millisecond);

        private static bool closureRequested = false;
        private static Stopwatch _timer;

        public List<Station> Stations { get; set; }
        public List<FixedTrader> FixedTraders { get; set; }
        public List<FlyingTrader> FlyingTraders { get; set; }
        public List<Merchendise> Merchendises { get; set; }

        private World()
        {
            // démarrage du temps
            _timer = new Stopwatch();
            _timer.Start();

            // les stations
            Stations = new List<Station>
            {
                { new Station{ ID = 0, Coordinates=new double[3]{0, 0, 0}, Name = "Le marché", StorageCapacity = 1000 } },
                { new Station{ ID = 1, Coordinates=new double[3]{0, 0, 1}, Name = "Le trou perdu", StorageCapacity = 2500 } },
                { new Station{ ID = 2, Coordinates=new double[3]{0, 0, 2}, Name = "Le mauvais endroit", StorageCapacity = 5000 } },
                { new Station{ ID = 3, Coordinates=new double[3]{0, 0, 3}, Name = "La fortune", StorageCapacity = 10000 } }
            };

            // les traders fixes
            FixedTraders = new List<FixedTrader>
            {
                { new FixedTrader { ID = 100, Account = 10000, Name = "", WorkingStation = Stations.First(x=>x.ID==0), Production = MerchendiseType.Gold, CurrentState = Trader.TraderState.InStation} },
                { new FixedTrader { ID = 101, Account = 10000, Name = "", WorkingStation = Stations.First(x=>x.ID==1), Production = MerchendiseType.Iron, CurrentState = Trader.TraderState.InStation} },
                { new FixedTrader { ID = 102, Account = 10000, Name = "", WorkingStation = Stations.First(x=>x.ID==2), Production = MerchendiseType.Orionum, CurrentState = Trader.TraderState.InStation} },
                { new FixedTrader { ID = 103, Account = 10000, Name = "", WorkingStation = Stations.First(x=>x.ID==3), Production = MerchendiseType.Water, CurrentState = Trader.TraderState.InStation} }
            };

            // les traders itinérants
            FlyingTraders = new List<FlyingTrader>
            {
                { new FlyingTrader {
                    ID = 1000, Account = 10000,
                    Name = "Golan Trevize",
                    OwnedShip = new Ship {ID = 10000, StorageCapacity = 100, Localisation = Stations[0] },
                    CurrentState = Trader.TraderState.InStation } },
                { new FlyingTrader {
                    ID = 1001,
                    Account = 10000,
                    Name = "Salvor Hardin",
                    OwnedShip = new Ship {ID = 10001, StorageCapacity = 100, Localisation = Stations[1] },
                    CurrentState = Trader.TraderState.InStation } },
                { new FlyingTrader {
                    ID = 1002, Account = 10000,
                    Name = "Donald J. Trump",
                    OwnedShip = new Ship {ID = 10002, StorageCapacity = 100, Localisation = Stations[2] },
                    CurrentState = Trader.TraderState.InStation } },
                { new FlyingTrader {
                    ID = 1003,
                    Account = 10000,
                    Name = "Elon Musk",
                    OwnedShip = new Ship {ID = 10003, StorageCapacity = 100, Localisation = Stations[3] },
                    CurrentState = Trader.TraderState.InStation } },
                { new FlyingTrader {
                    ID = 1004, Account = 10000,
                    Name = "Ultra Doux",
                    OwnedShip = new Ship {ID = 10004, StorageCapacity = 100, Localisation = Stations[0] },
                    CurrentState = Trader.TraderState.InStation } },
                { new FlyingTrader {
                    ID = 1005,
                    Account = 10000,
                    Name = "Médiocre UI",
                    OwnedShip = new Ship {ID = 10005, StorageCapacity = 100, Localisation = Stations[1] },
                    CurrentState = Trader.TraderState.InStation } },
                { new FlyingTrader {
                    ID = 1006, Account = 10000,
                    Name = "Marie Curie",
                    OwnedShip = new Ship {ID = 10006, StorageCapacity = 100, Localisation = Stations[2] },
                    CurrentState = Trader.TraderState.InStation } },
                { new FlyingTrader {
                    ID = 1007,
                    Account = 10000,
                    Name = "Hiroko",
                    OwnedShip = new Ship {ID = 10007, StorageCapacity = 100, Localisation = Stations[3] },
                    CurrentState = Trader.TraderState.InStation } },
                 { new FlyingTrader {
                    ID = 1008, Account = 10000,
                    Name = "Jeff Bezos",
                    OwnedShip = new Ship {ID = 10004, StorageCapacity = 100, Localisation = Stations[0] },
                    CurrentState = Trader.TraderState.InStation } },
                { new FlyingTrader {
                    ID = 1009,
                    Account = 10000,
                    Name = "X71-F",
                    OwnedShip = new Ship {ID = 10005, StorageCapacity = 100, Localisation = Stations[1] },
                    CurrentState = Trader.TraderState.InStation } },
                { new FlyingTrader {
                    ID = 1010, Account = 10000,
                    Name = "Cléon XXII",
                    OwnedShip = new Ship {ID = 10006, StorageCapacity = 100, Localisation = Stations[2] },
                    CurrentState = Trader.TraderState.InStation } },
                { new FlyingTrader {
                    ID = 1011,
                    Account = 10000,
                    Name = "Caliméro",
                    OwnedShip = new Ship {ID = 10007, StorageCapacity = 100, Localisation = Stations[3] },
                    CurrentState = Trader.TraderState.InStation } }
            };

            // détermination des poids et valeurs

            // répartition des marchandises dans les stations
            for (int i = 0; i < 4; i++)
            {
                Stations[i].Merchendises = new List<Merchendise>
                {
                    { new Merchendise{MerchendiseType = MerchendiseType.Gold, Name="Or", Weight = 20, UniversalValue = 100, 
                        Quantity = (rnd.Next(0, 2) == 1) ? rnd.Next(0, GetShortageMaxBound(20, Stations[i].StorageCapacity)) : rnd.Next(GetExcessMinBound(20, Stations[i].StorageCapacity), GetMaxBound(20, Stations[i].StorageCapacity)) } },
                    { new Merchendise{MerchendiseType = MerchendiseType.Orionum, Name="Orionum", Weight = 50, UniversalValue = 500,
                        Quantity = (rnd.Next(0, 2) == 1) ? rnd.Next(0, GetShortageMaxBound(50, Stations[i].StorageCapacity)) : rnd.Next(GetExcessMinBound(50, Stations[i].StorageCapacity), GetMaxBound(50, Stations[i].StorageCapacity)) } },
                    { new Merchendise{MerchendiseType = MerchendiseType.Deuterium, Name="H2", Weight = 1, UniversalValue = 300,
                        Quantity = (rnd.Next(0, 2) == 1) ? rnd.Next(0, GetShortageMaxBound(1, Stations[i].StorageCapacity)) : rnd.Next(GetExcessMinBound(1, Stations[i].StorageCapacity), GetMaxBound(1, Stations[i].StorageCapacity)) } },
                    { new Merchendise{MerchendiseType = MerchendiseType.Iron, Name="Fer", Weight = 10, UniversalValue = 50,
                        Quantity = (rnd.Next(0, 2) == 1) ? rnd.Next(0, GetShortageMaxBound(10, Stations[i].StorageCapacity)) : rnd.Next(GetExcessMinBound(10, Stations[i].StorageCapacity), GetMaxBound(10, Stations[i].StorageCapacity)) } },
                    { new Merchendise{MerchendiseType = MerchendiseType.Silicon, Name="Silicone", Weight = 3, UniversalValue = 80,
                        Quantity = (rnd.Next(0, 2) == 1) ? rnd.Next(0, GetShortageMaxBound(3, Stations[i].StorageCapacity)) : rnd.Next(GetExcessMinBound(3, Stations[i].StorageCapacity), GetMaxBound(3, Stations[i].StorageCapacity)) } },
                    { new Merchendise{MerchendiseType = MerchendiseType.Water, Name="Eau pure",Weight = 1, UniversalValue = 10,
                        Quantity = (rnd.Next(0, 2) == 1) ? rnd.Next(0, GetShortageMaxBound(1, Stations[i].StorageCapacity)) : rnd.Next(GetExcessMinBound(1, Stations[i].StorageCapacity), GetMaxBound(1, Stations[i].StorageCapacity)) } },
                    { new Merchendise{MerchendiseType = MerchendiseType.Wood, Name="Bois", Weight = 2, UniversalValue = 1,
                        Quantity = (rnd.Next(0, 2) == 1) ? rnd.Next(0, GetShortageMaxBound(2, Stations[i].StorageCapacity)) : rnd.Next(GetExcessMinBound(2, Stations[i].StorageCapacity), GetMaxBound(2, Stations[i].StorageCapacity)) } },
                };
                Stations[i].InitializeTadringBoard();
            }
        }

        private int GetShortageMaxBound(int weight, int capacity)
        {
            return (int) Math.Floor((double)(capacity / weight) * 0.25);
        }

        private int GetExcessMinBound(int weight, int capacity)
        {
            return (int)Math.Floor((double)(capacity / weight) * 0.75);
        }

        private int GetMaxBound(int weight, int capacity)
        {
            return (int)Math.Floor((double)(capacity / weight));
        }

        public void StartSimulation()
        {
            List<Task> tasks = new List<Task>();

            // instanciation des task par marchand fixe
            foreach (var fixedTrader in FixedTraders)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {                    
                    fixedTrader.Loop().Wait();                    
                }));
            }

            // instanciation des task par marchand volant
            foreach (var flyingTrader in FlyingTraders)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(rnd.Next(1000, 5000));
                    flyingTrader.Loop().Wait();
                }));
            }

            Task.WaitAll(tasks.ToArray());
        }

        public long GetElapsedTime()
        {
            return _timer.ElapsedMilliseconds;
        }

        public long GetElapsedTicks()
        {
            return _timer.ElapsedTicks;
        }

        public void Close()
        {
            closureRequested = true;
        }
    }
}