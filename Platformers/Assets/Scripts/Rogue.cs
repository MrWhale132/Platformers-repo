
public class Rogue : Hero
{
    protected override void Start()
    {
        base.Start();
        atackField = moveField;
    }
}
