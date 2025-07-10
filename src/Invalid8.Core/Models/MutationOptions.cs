namespace Invalid8.Core;

public class MutationOptions
{
    public bool InvalidateQueries { get; set; } = true;
    public List<string[]> InvalidationKeys { get; set; } = new List<string[]>();
}

