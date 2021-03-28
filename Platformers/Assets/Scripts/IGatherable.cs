
public interface IGatherable<out T>  where T : Item
{
    T GetGathered(int amount);
}