using System.Linq;
using UnityEngine;
using System;


public class BakeryReceipts : MonoBehaviour
{
    static BakeryReceipts instance;

    BakeReceipt[] receipts;

    void Awake()
    {
        if (instance != null)
        {
            throw new Exception();
        }

        instance = this;

        receipts = new BakeReceipt[]
        {
            new BakeReceipt(10, 5f, new Item[] {new WheatFlour(2) }, new Item[] { new Loaf(1) })
        };
    }

    public static BakeReceipt[] FindReceipts(int craftSum)
    {
        return Array.FindAll(instance.receipts, receipt => receipt.CraftSum == craftSum);
    }
}

public class BakeReceipt
{
    readonly int craftSum;
    readonly float bakeTime;
    Item[] components;
    Item[] products;

    public int CraftSum => craftSum;
    public float BakeTime => bakeTime;
    public Item[] Components => components;
    public Item[] Products => products;

    public BakeReceipt(int craftSum, float bakeTime, Item[] components, Item[] products)
    {
        this.craftSum = craftSum;
        this.bakeTime = bakeTime;
        this.components = components;
        this.products = products;
    }

    public bool CheckComponents(Inventory inventory)
    {
        return components.All(item => inventory.CheckQuantity(item) >= item.Quantity);
    }

    public int ManyTimes(Inventory inventory)
    {
        return components.Select(item => inventory.QuantityOf(item) / item.Quantity).Min();
    }
}