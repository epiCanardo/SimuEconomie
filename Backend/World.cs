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
        public static World Instance => _lazy.Value;
        
        private static readonly Lazy<World> _lazy = new(() => new World(), LazyThreadSafetyMode.ExecutionAndPublication);
        private static Random _rnd = new(DateTime.Now.Millisecond);
        private static bool closureRequested = false;
        private static Stopwatch _timer;

        public List<Station> Stations { get; set; }
        public List<FixedTrader> FixedTraders { get; set; }
        public List<FlyingTrader> FlyingTraders { get; set; }

        private World()
        {
            // démarrage du temps
            _timer = new Stopwatch();
            _timer.Start();

            // les stations
            Stations = new List<Station>
            {
                { new() { ID = 0, Coordinates=new double[3]{0, 0, 0}, Name = "Le marché", StorageCapacity = 1000 } },
                { new() { ID = 1, Coordinates=new double[3]{0, 0, 1}, Name = "Le trou perdu", StorageCapacity = 2500 } },
                { new() { ID = 2, Coordinates=new double[3]{0, 0, 2}, Name = "Le mauvais endroit", StorageCapacity = 5000 } },
                { new() { ID = 3, Coordinates=new double[3]{0, 0, 3}, Name = "La fortune", StorageCapacity = 10000 } }
            };

            // les traders fixes
            FixedTraders = new List<FixedTrader>
            {
                { new() { ID = 100, Account = 10000, Name = "", WorkingStation = Stations.First(x=>x.ID==0), Production = MerchendiseType.Gold, CurrentState = Trader.TraderState.InStation} },
                { new() { ID = 101, Account = 10000, Name = "", WorkingStation = Stations.First(x=>x.ID==1), Production = MerchendiseType.Iron, CurrentState = Trader.TraderState.InStation} },
                { new() { ID = 102, Account = 10000, Name = "", WorkingStation = Stations.First(x=>x.ID==2), Production = MerchendiseType.Orionum, CurrentState = Trader.TraderState.InStation} },
                { new() { ID = 103, Account = 10000, Name = "", WorkingStation = Stations.First(x=>x.ID==3), Production = MerchendiseType.Water, CurrentState = Trader.TraderState.InStation} }
            };

            // les traders itinérants
            FlyingTraders = new List<FlyingTrader>
            {
                { new()
                {
                    ID = 1000, Account = 10000,
                    Name = "Golan Trevize",
                    OwnedShip = new Ship {ID = 10000, StorageCapacity = 100, Localisation = Stations[0] },
                    CurrentState = Trader.TraderState.InStation } },
                { new()
                {
                    ID = 1001,
                    Account = 10000,
                    Name = "Salvor Hardin",
                    OwnedShip = new Ship {ID = 10001, StorageCapacity = 100, Localisation = Stations[1] },
                    CurrentState = Trader.TraderState.InStation } },
                { new()
                {
                    ID = 1002, Account = 10000,
                    Name = "Donald J. Trump",
                    OwnedShip = new Ship {ID = 10002, StorageCapacity = 100, Localisation = Stations[2] },
                    CurrentState = Trader.TraderState.InStation } },
                { new()
                {
                    ID = 1003,
                    Account = 10000,
                    Name = "Elon Musk",
                    OwnedShip = new Ship {ID = 10003, StorageCapacity = 100, Localisation = Stations[3] },
                    CurrentState = Trader.TraderState.InStation } },
                { new()
                {
                    ID = 1004, Account = 10000,
                    Name = "Ultra Doux",
                    OwnedShip = new Ship {ID = 10004, StorageCapacity = 200, Localisation = Stations[0] },
                    CurrentState = Trader.TraderState.InStation } },
                { new()
                {
                    ID = 1005,
                    Account = 10000,
                    Name = "Médiocre UI",
                    OwnedShip = new Ship {ID = 10005, StorageCapacity = 200, Localisation = Stations[1] },
                    CurrentState = Trader.TraderState.InStation } },
                { new()
                {
                    ID = 1006, Account = 10000,
                    Name = "Marie Curie",
                    OwnedShip = new Ship {ID = 10006, StorageCapacity = 200, Localisation = Stations[2] },
                    CurrentState = Trader.TraderState.InStation } },
                { new()
                {
                    ID = 1007,
                    Account = 10000,
                    Name = "Hiroko",
                    OwnedShip = new Ship {ID = 10007, StorageCapacity = 200, Localisation = Stations[3] },
                    CurrentState = Trader.TraderState.InStation } },
                 { new()
                 {
                    ID = 1008, Account = 10000,
                    Name = "Jeff Bezos",
                    OwnedShip = new Ship {ID = 10004, StorageCapacity = 500, Localisation = Stations[0] },
                    CurrentState = Trader.TraderState.InStation } },
                { new()
                {
                    ID = 1009,
                    Account = 10000,
                    Name = "X71-F",
                    OwnedShip = new Ship {ID = 10005, StorageCapacity = 500, Localisation = Stations[1] },
                    CurrentState = Trader.TraderState.InStation } },
                { new()
                {
                    ID = 1010, Account = 10000,
                    Name = "Cléon XXII",
                    OwnedShip = new Ship {ID = 10006, StorageCapacity = 500, Localisation = Stations[2] },
                    CurrentState = Trader.TraderState.InStation } },
                { new()
                {
                    ID = 1011,
                    Account = 10000,
                    Name = "Caliméro",
                    OwnedShip = new Ship {ID = 10007, StorageCapacity = 500, Localisation = Stations[3] },
                    CurrentState = Trader.TraderState.InStation } }
            };

            // détermination des poids et valeurs

            // répartition des marchandises dans les stations
            for (int i = 0; i < Stations.Count; i++)
            {
                Stations[i].Merchendises = new List<Merchendise>
                {
                    { new()
                    {MerchendiseType = MerchendiseType.Gold, Name="Or", Weight = 20, UniversalValue = 100, 
                        Quantity = (_rnd.Next(0, 2) == 1) ? _rnd.Next(0, GetShortageMaxBound(20, Stations[i].StorageCapacity)) : _rnd.Next(GetExcessMinBound(20, Stations[i].StorageCapacity), GetMaxBound(20, Stations[i].StorageCapacity)) } },
                    { new()
                    {MerchendiseType = MerchendiseType.Orionum, Name="Orionum", Weight = 50, UniversalValue = 500,
                        Quantity = (_rnd.Next(0, 2) == 1) ? _rnd.Next(0, GetShortageMaxBound(50, Stations[i].StorageCapacity)) : _rnd.Next(GetExcessMinBound(50, Stations[i].StorageCapacity), GetMaxBound(50, Stations[i].StorageCapacity)) } },
                    { new()
                    {MerchendiseType = MerchendiseType.Deuterium, Name="H2", Weight = 1, UniversalValue = 300,
                        Quantity = (_rnd.Next(0, 2) == 1) ? _rnd.Next(0, GetShortageMaxBound(1, Stations[i].StorageCapacity)) : _rnd.Next(GetExcessMinBound(1, Stations[i].StorageCapacity), GetMaxBound(1, Stations[i].StorageCapacity)) } },
                    { new()
                    {MerchendiseType = MerchendiseType.Iron, Name="Fer", Weight = 10, UniversalValue = 50,
                        Quantity = (_rnd.Next(0, 2) == 1) ? _rnd.Next(0, GetShortageMaxBound(10, Stations[i].StorageCapacity)) : _rnd.Next(GetExcessMinBound(10, Stations[i].StorageCapacity), GetMaxBound(10, Stations[i].StorageCapacity)) } },
                    { new()
                    {MerchendiseType = MerchendiseType.Silicon, Name="Silicone", Weight = 3, UniversalValue = 80,
                        Quantity = (_rnd.Next(0, 2) == 1) ? _rnd.Next(0, GetShortageMaxBound(3, Stations[i].StorageCapacity)) : _rnd.Next(GetExcessMinBound(3, Stations[i].StorageCapacity), GetMaxBound(3, Stations[i].StorageCapacity)) } },
                    { new()
                    {MerchendiseType = MerchendiseType.Water, Name="Eau pure",Weight = 1, UniversalValue = 10,
                        Quantity = (_rnd.Next(0, 2) == 1) ? _rnd.Next(0, GetShortageMaxBound(1, Stations[i].StorageCapacity)) : _rnd.Next(GetExcessMinBound(1, Stations[i].StorageCapacity), GetMaxBound(1, Stations[i].StorageCapacity)) } },
                    { new()
                    {MerchendiseType = MerchendiseType.Wood, Name="Bois", Weight = 2, UniversalValue = 1,
                        Quantity = (_rnd.Next(0, 2) == 1) ? _rnd.Next(0, GetShortageMaxBound(2, Stations[i].StorageCapacity)) : _rnd.Next(GetExcessMinBound(2, Stations[i].StorageCapacity), GetMaxBound(2, Stations[i].StorageCapacity)) } },
                };
                Stations[i].InitializeTradingBoard();
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
                    Thread.Sleep(_rnd.Next(1000, 5000));
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