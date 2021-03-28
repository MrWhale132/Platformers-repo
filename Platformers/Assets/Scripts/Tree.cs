using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tree : Plants, IGatherable<Wood>, ITrade
{
    protected int treeLevel = 1;

    protected Vector2Int gRange;
    protected int startingAmount;

    Inventory items;

    public Inventory Inventory { get => items; set { } }


    protected override void Start()
    {
        base.Start();
        gRange.x = treeLevel * 5;
        gRange.y = gRange.x + 5 + treeLevel - 1;
        startingAmount = UnityEngine.Random.Range(gRange.x, gRange.y);

        items = new Inventory(1);
        items.AddItem(new Wood(40));
    }

    private void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, platform));
    }


    public Wood GetGathered(int amount)
    {
        if (amount == 0)
            return new Wood(0);

        Item wood = Export(new Wood(), amount);
        if (wood == null)
            DialogMessages.CreateFloatMessage(transform.position + Vector3.up * transform.localScale.y, "This Tree is exhausted.");
        return wood as Wood;
    }


    public Item Export(Item item, int amount)
    {
        int avaibleAmount = PrepareExport(item, amount);

        if (avaibleAmount != 0)

            return
            items.GetItem(item, avaibleAmount);

        return null;
    }

    public void Import(Item item)
    {
        throw new NotImplementedException();
    }

    public int PrepareExport(Item item, int amount)
    {
        int actualQuantity = items.CheckQuantity(item, amount);
        return actualQuantity;
    }

    public int PrepareImport(Item item, int amount)
    {
        throw new NotImplementedException();
    }

    public void CallOnMouseUpAsButton()
    {
        OnMouseUpAsButton();
    }
}
