using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum Materials { Wood = 0, Stone = 1 }  //dead

public class Items : MonoBehaviour
{
    public static Item[] Clone(Item[] items) => items.Select(item => item.Copy()).ToArray();
}


public class Wood : Item, ICollectable
{
    Sprite sprite;

    int stackLimit = 10;
    public override int StackLimit => stackLimit;


    public Wood() : this(1) { }

    public Wood(int _quantity, bool intended = false) : base(_quantity, intended)
    {

    }

    public Wood(Wood copyFrom) : base(copyFrom)
    {

    }


    public override Sprite GetSprite()
    {
        return Utility.GetSprite(typeof(Wood));
    }

    public override Item Copy()
    {
        return new Wood(this);
    }


    public override bool Equals(object obj)   // Quantity is not part of the identicalness!
    {
        if (obj != null && obj is Wood s)
        {
            if (base.Equals(s))
            {
                return true;
            }
        }

        return false;
    }

    public override int GetHashCode()   // Auto generated, implement your own!
    {
        return 433308056 + quantity.GetHashCode();
    }



    public ICollectable Copy(ICollectable copyFrom)
    {
        return default;
    }
}

public class Stone : Item, ICollectable
{
    Sprite sprite;

    int stackLimit = 10;
    public override int StackLimit => stackLimit;

    public Stone() : this(1) { }

    public Stone(int _quantity, bool intended = false) : base(_quantity, intended)
    {

    }

    public Stone(Stone copyFrom) : base(copyFrom)
    {

    }


    public override Item Copy()
    {
        return new Stone(this);
    }


    public static Stone operator +(Stone a, Stone b)
    {
        if (a.Equals(b))
        {
            return new Stone(a) { Quantity = a.quantity + b.quantity };
        }

        throw new InvalidOperationException("The two Stone class are not identical!");
    }

    public static Stone operator -(Stone a, Stone b)
    {
        if (a.Equals(b))
        {
            return new Stone(a) { Quantity = a.quantity - b.quantity };
        }

        throw new InvalidOperationException("The two Stone class are not identical!");
    }

    public static Stone operator -(Stone a, int b)
    {
        a.Quantity -= b;
        return new Stone(a) { Quantity = b };
    }

    public override Sprite GetSprite()
    {
        return Utility.GetSprite(typeof(Stone));
    }

    public override bool Equals(object obj)   // Quantity is not part of the identicalness!
    {
        if (obj != null && obj is Stone s)
        {
            if (base.Equals(s))
            {
                return true;
            }
        }

        return false;
    }

    public override int GetHashCode()   // Auto generated, implement your own!
    {
        return 433308056 + quantity.GetHashCode();
    }

    public ICollectable Copy(ICollectable copyFrom)
    {
        return new Stone((Stone)copyFrom);
    }

}

public class WheatFlour : Item, IBakeable
{
    Sprite sprite;

    int stackLimit = 10;

    int bakeReceiptValue = 10;

    public override int StackLimit => stackLimit;
    public int BakeReceiptValue => bakeReceiptValue;


    public WheatFlour() : this(1) { }

    public WheatFlour(int _quantity, bool intended = false) : base(_quantity, intended)
    {

    }

    public WheatFlour(WheatFlour copyFrom) : base(copyFrom)
    {

    }


    public override Item Copy()
    {
        return new WheatFlour(this);
    }


    public override Sprite GetSprite()
    {
        return Utility.GetSprite(typeof(WheatFlour));
    }

    public override bool Equals(object obj)   // Quantity is not part of the identicalness!
    {
        if (obj != null && obj is WheatFlour s)
        {
            if (base.Equals(s))
            {
                return true;
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        int hashCode = 619031444;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + quantity.GetHashCode();
        hashCode = hashCode * -1521134295 + Quantity.GetHashCode();
        hashCode = hashCode * -1521134295 + intended.GetHashCode();
        hashCode = hashCode * -1521134295 + Intended.GetHashCode();
        hashCode = hashCode * -1521134295 + StackLimit.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Sprite>.Default.GetHashCode(sprite);
        hashCode = hashCode * -1521134295 + stackLimit.GetHashCode();
        hashCode = hashCode * -1521134295 + StackLimit.GetHashCode();
        return hashCode;
    }
}

public class Coal : Item, IBurnable
{
    static Coal instance;

    static Sprite sSprite;

    Sprite sprite;

    int stackLimit = 20;

    int burnValue = 20;


    public override int StackLimit => stackLimit;
    public int BurnValue => burnValue;


    static Coal()
    {
        sSprite = Utility.GetSprite(typeof(Coal));
    }

    public Coal() : this(1) { }

    public Coal(int _quantity, bool intended = false) : base(_quantity, intended) { }

    public Coal(Coal copyFrom) : base(copyFrom) { }


    public override Sprite GetSprite()
    {
        return sSprite;
    }


    public override Item Copy()
    {
        return new Coal(this);
    }


    public override bool Equals(object obj)   // Quantity is not part of the identicalness!
    {
        if (obj is Coal s)
        {
            if (base.Equals(s))
            {
                return true;
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        int hashCode = 619031444;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + quantity.GetHashCode();
        hashCode = hashCode * -1521134295 + Quantity.GetHashCode();
        hashCode = hashCode * -1521134295 + intended.GetHashCode();
        hashCode = hashCode * -1521134295 + Intended.GetHashCode();
        hashCode = hashCode * -1521134295 + StackLimit.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Sprite>.Default.GetHashCode(sprite);
        hashCode = hashCode * -1521134295 + stackLimit.GetHashCode();
        hashCode = hashCode * -1521134295 + StackLimit.GetHashCode();
        return hashCode;
    }
}

public class Loaf : Item
{
    static Coal instance;

    static Sprite sSprite;

    Sprite sprite;

    int stackLimit = 8;
    public override int StackLimit => stackLimit;


    static Loaf()
    {
        sSprite = Utility.GetSprite(typeof(Loaf));
    }

    public Loaf() : this(1) { }

    public Loaf(int _quantity, bool intended = false) : base(_quantity, intended) { }

    public Loaf(Loaf copyFrom) : base(copyFrom) { }


    public override Sprite GetSprite()
    {
        return sSprite;
    }


    public override Item Copy()
    {
        return new Loaf(this);
    }


    public override bool Equals(object obj)   // Quantity is not part of the identicalness!
    {
        if (obj is Loaf s)
        {
            if (base.Equals(s))
            {
                return true;
            }
        }

        return false;
    }

    public override int GetHashCode()
    {
        int hashCode = 619031444;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + quantity.GetHashCode();
        hashCode = hashCode * -1521134295 + Quantity.GetHashCode();
        hashCode = hashCode * -1521134295 + intended.GetHashCode();
        hashCode = hashCode * -1521134295 + Intended.GetHashCode();
        hashCode = hashCode * -1521134295 + StackLimit.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Sprite>.Default.GetHashCode(sprite);
        hashCode = hashCode * -1521134295 + stackLimit.GetHashCode();
        hashCode = hashCode * -1521134295 + StackLimit.GetHashCode();
        return hashCode;
    }
}