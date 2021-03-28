using System;
using UnityEngine;

public class Item
{
    protected int quantity;
    public virtual int Quantity
    {
        get { return quantity; }
        set
        {
            quantity = value;
            if (quantity < 0 && !intended)
                throw new NegativeItemQuantityException();
        }
    }

    protected bool intended;
    public bool Intended { get { return intended; } set { intended = value; } }

    int stackLimit = 10;
    public virtual int StackLimit => stackLimit;


    public Item() : this(1) { }

    public Item(int _quantity, bool intended = false)
    {
        if (_quantity < 0 && !intended) throw new NegativeItemQuantityException();

        quantity = _quantity;
        this.intended = intended;
    }

    public Item(Item copyFrom)
    {
        quantity = copyFrom.Quantity;
        intended = copyFrom.Intended;
    }

    public virtual Sprite GetSprite()  // an item it self can not exist
    {
        throw new Exception();
    }

    public virtual Item Copy()
    {
        throw new NotImplementedException();
    }


    public static Item Clone(Item copyForm)
    {
        if (copyForm != null)
            return (Item)copyForm.GetType().GetConstructor(new[] { copyForm.GetType() }).Invoke(new[] { copyForm });
        return null;
    }

    public static Item operator +(Item a, Item b)
    {
        if (a.Equals(b))
        {
            Item instance = (Item)a.GetType().GetConstructor(new Type[1] { a.GetType() }).Invoke(new object[1] { a });
            instance.Quantity = a.quantity + b.quantity;
            return instance;
        }

        throw new InvalidOperationException("The two Collectable class are not identical!");
    }

    public static Item operator -(Item a, Item b)
    {
        if (a.Equals(b))
        {
            Item instance = (Item)a.GetType().GetConstructor(new Type[1] { a.GetType() }).Invoke(new object[1] { a });
            instance.Quantity = a.quantity - b.quantity;
            return instance;
        }

        throw new InvalidOperationException("The - operand can be only applied between operands if they have the same type.");
    }

    public static Item operator -(Item a, int b)
    {
        a.Quantity -= b;
        Item instance = (Item)a.GetType().GetConstructor(new Type[1] { a.GetType() }).Invoke(new object[1] { a });
        instance.Quantity = b;
        return instance;
    }


    public override bool Equals(object obj)   // Quantity is not part of the identicalness!
    {
        if (obj != null && obj is Item c)
        {
            if (intended == c.intended)
            {
                return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        return quantity + " " + GetType();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}