public class vehicle
{
    public bool hasFinished;
    public string name;
    public int node;

    public vehicle(int node, string name, bool HasFinished)
    {
        this.node = node;
        this.name = name;
        hasFinished = HasFinished;
    }
}