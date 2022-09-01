namespace Backend;

public abstract class Storage : Entity
{
    public int StorageCapacity { get; set; }
    public List<Merchendise> Merchendises { get; set; }

    public int GetRemainingStorageSpace()
    {
        int space = 0;
        foreach (var item in Merchendises)
        {
            space += item.Quantity * item.UniversalMerchendise.Weight;
        }

        return StorageCapacity - space;
    }

    public Storage()
    {
        Merchendises = new List<Merchendise>();
    }

    public double DistanceWithOtherStorage(Storage otherStorage)
    {
        double deltaX = otherStorage.Coordinates[0] - Coordinates[0];
        double deltaY = otherStorage.Coordinates[1] - Coordinates[1];
        double deltaZ = otherStorage.Coordinates[2] - Coordinates[2];

        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
    }
}