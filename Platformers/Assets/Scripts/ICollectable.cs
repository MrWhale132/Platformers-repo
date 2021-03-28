
public interface ICollectable
{
    int Quantity { get; set; }

    ICollectable Copy(ICollectable copyFrom);
}