[System.Serializable]
public class Edge
{
    public EdgeType Type;

    public int Region;

    public override string ToString()
    {
        return $"{Type},{Region}";
    }
}

public enum EdgeType
{
    None = 0,
    Street = 1,
    Castle = 2,
    Forest = 3,
    River = 4
}
