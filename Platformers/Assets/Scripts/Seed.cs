using System;


public class Seed : Item
{
    public Seed() : this(1) { }

    public Seed(int quanity, bool intended = false) : base(quanity, intended)
    {

    }

    public Seed(Seed copy): base(copy)
    {

    }


    public virtual Crop GetCrop()
    {
        throw new NotImplementedException();
    }
}