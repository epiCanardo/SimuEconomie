namespace Backend
{
    public abstract class Entity
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public double[] Coordinates { get; set; }
    }
}