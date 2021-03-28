using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Marble : MonoBehaviour, IGatherable<Stone>, ITrade
{
    Platform platform;

    protected int marbleLevel = 1;

    protected Vector2Int gRange;
    protected int startingAmount;

    Inventory items;

    public Platform Platform { get { return platform; } }

    public Inventory Inventory { get => items; set { } }


    protected virtual void Start()
    {
        platform = MapGenerator.GetPlatformFromPosition(transform.position);
        platform.walkable = false;
        platform.objAtPlatform = this;

        gRange.x = marbleLevel * 5;
        gRange.y = gRange.x + 5 + marbleLevel - 1;
        startingAmount = UnityEngine.Random.Range(gRange.x, gRange.y);

        items = new Inventory(1);
        items.AddItem(new Stone(10));
    }

    void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, platform));
    }


    public Stone GetGathered(int amount)
    {
        if (amount == 0)
            return new Stone(0);

        Item stone = Export(new Stone(), amount);
        if (stone == null)
            DialogMessages.CreateFloatMessage(transform.position + Vector3.up * transform.localScale.y, "This Marble is exhausted.");
        return stone as Stone;
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