using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CoalMine : MonoBehaviour, IGatherable<Coal>
{
    Platform platform;

    protected int marbleLevel = 1;

    protected Vector2Int gRange;
    protected int startingAmount;

    Inventory items;

    public Platform Platform => platform;


    void Awake()
    {
        platform = MapGenerator.GetPlatformFromPosition(transform.position);
        platform.walkable = false;
        platform.objAtPlatform = this;

        items = new Inventory(1);
        items.AddItem(new Coal(8));
    }

    void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, platform));
    }


    public Coal GetGathered(int amount)
    {
        if (amount == 0)
            return new Coal(0);

        Item coal = Export(new Coal(amount));
        if (coal == null)
            DialogMessages.CreateFloatMessage(transform.position + Vector3.up * transform.localScale.y, "This CoalMine is exhausted.");
        return coal as Coal;
    }

    public Item Export(Item item)
    {
        int avaibleAmount = PrepareExport(item);

        if (avaibleAmount != 0)

            return
            items.GetItem(item, avaibleAmount);

        return null;
    }

    public int PrepareExport(Item item)
    {
        int actualQuantity = items.CheckQuantity(item);
        return actualQuantity;
    }

    public void CallOnMouseUpAsButton()
    {
        OnMouseUpAsButton();
    }
}