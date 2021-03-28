using System;
using System.Collections.Generic;
using UnityEngine;


public class Seeds : MonoBehaviour
{
    Dictionary<Type, Crop> crops;

    [SerializeField]
    Crop[] prefabs;

    static Seeds instance;


    private void Start()
    {
        if (instance != null)
        {
            Debug.LogError("instance problem");
            return;
        }

        instance = this;

        crops = new Dictionary<Type, Crop>();
        foreach (Crop crop in prefabs)
        {
            crops.Add(crop.GetType(), crop);
        }
    }

    public static Crop GetCrop(Type type)
    {
        return instance.crops[type];
    }
}

public class WheatSeed : Seed, IGrindable
{
    Sprite sprite;

    int stackLimit = 15;
    readonly int grindValue = 10;

    public override int StackLimit => stackLimit;
    public int GrindValue => grindValue;


    public WheatSeed() : this(1) { }

    public WheatSeed(int _quantity, bool intended = false) : base(_quantity, intended)
    {

    }

    public WheatSeed(WheatSeed copy) : base(copy)
    {
        sprite = copy.sprite;
        stackLimit = copy.stackLimit;
        grindValue = copy.grindValue;
    }


    public override Item Copy()
    {
        return new WheatSeed(this);
    }

    public override Crop GetCrop()
    {
        return Seeds.GetCrop(typeof(Wheat));
    }

    public override Sprite GetSprite()
    {
        return Utility.GetSprite(typeof(WheatSeed));
    }


    public override bool Equals(object obj)   // Quantity is not part of the identicalness!
    {
        if (obj != null && obj is WheatSeed s)
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
        int hashCode = 488866374;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + quantity.GetHashCode();
        hashCode = hashCode * -1521134295 + Quantity.GetHashCode();
        hashCode = hashCode * -1521134295 + intended.GetHashCode();
        hashCode = hashCode * -1521134295 + Intended.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Sprite>.Default.GetHashCode(sprite);
        return hashCode;
    }
}