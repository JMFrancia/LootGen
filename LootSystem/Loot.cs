using System;

public struct Loot
{
    public string Name;
    public int Count;

    private const string ERR_CANNOT_MERGE = "Cannot merge unlike Loot types {0} and {1}";

    public Loot(string name, int count)
    {
        Name = name;
        Count = count;
    }

    public bool TryMerge(Loot other)
    {
        if (other.Name.Equals(Name))
        {
            Count += other.Count;
            return true;
        }

        //TODO: Move this outside the struct
        Console.WriteLine(ERR_CANNOT_MERGE, Name, other.Name);
        return false;
    }

    public override string ToString()
    {
        return $"{Count} {Name}";
    }
}