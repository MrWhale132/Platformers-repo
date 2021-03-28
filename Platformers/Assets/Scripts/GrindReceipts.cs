using UnityEngine;
using System;
using System.Linq;


public class GrindReceipts : MonoBehaviour
{
    static GrindReceipts instance;

    GrindReceipt[] receipts;

    void Awake()
    {
        if (instance != null)
        {
            throw new Exception();
        }

        instance = this;

        receipts = new GrindReceipt[]
        {
            new GrindReceipt(10, 5f, new Item[] {new WheatSeed(3) }, new Item[] { new WheatFlour(1) })  //WheatFlour
        };
    }

    public static GrindReceipt[] FindReceipts(int craftSum)
    {
        return Array.FindAll(instance.receipts, receipt => receipt.CraftSum == craftSum);
    }
}

public class GrindReceipt
{
    readonly int craftSum;
    readonly float grindTime;
    Item[] components;
    Item[] products;

    public int CraftSum => craftSum;
    public float GrindTime => grindTime;
    public Item[] Components => components;

    public GrindReceipt(int craftSum, float grindTime, Item[] components, Item[] products)
    {
        this.craftSum = craftSum;
        this.grindTime = grindTime;
        this.components = components;
        this.products = products;
    }

    public Item[] CreateProduct(Inventory fromInv)
    {
        foreach (var item in components)
        {
            fromInv.GetItem(Item.Clone(item));
        }
        return products.Select(item => Item.Clone(item)).ToArray();
    }

    public bool CheckComponents(Inventory inventory)
    {
        return components.All(item => inventory.CheckQuantity(item) >= item.Quantity);

        //for (int i = 0; i < components.Length; i++)
        //{
        //    Item curr = components[i];
        //    int quantity = inventory.CheckQuantity(curr);
        //    if (quantity < curr.Quantity)
        //    {
        //        return false;
        //    }
        //}
        //return true;
    }

    public int ManyTimes(Inventory inventory)
    {
        return components.Select(item => inventory.QuantityOf(item) / item.Quantity).Min();
    }
}