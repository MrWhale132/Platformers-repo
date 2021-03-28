
public interface IGather<in T> where T : Item
{
    void GatherFrom(IGatherable<T> gatherable, int amount);
}