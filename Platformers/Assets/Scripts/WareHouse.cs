using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class WareHouse : Building, ITrade, IInteractable
{
    Inventory storage;
    public Inventory Inventory { get { return storage; } set { } }

    [SerializeField]
    int inventorySize;


    protected override void Start()
    {
        base.Start();
        storage = new Inventory(inventorySize);
    }

    protected override void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (!GameManager.ValidSelection()) goto Skip;

        if (storage.IsActive)
        {
            storage.Display(false);
            MouseManager.MouseClicked -= HandleMouseMsg;
        }
        else
        {
            storage.Display(true);
            MouseManager.MouseClicked += HandleMouseMsg;
        }

    Skip:

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, Platform));
    }

    void HandleMouseMsg(MouseMessage msg)
    {
        if (msg.Sender as WareHouse == this) return;

        storage.Display(false);
        MouseManager.MouseClicked -= HandleMouseMsg;
    }


    public void InteractedBy(object interacter)
    {
        if (interacter is Worker ||
            interacter is Hero)
        {
            storage.Display(true);
        }
    }

    public void CancelInteractionWith(object interacter)
    {
        if (interacter is Worker ||
            interacter is Builder ||
            interacter is Hero)
        {
            storage.Display(false);
        }
    }


    public Item Export(Item item, int amount)
    {
        int acutalAmount = storage.CheckQuantity(item, amount);

        if (acutalAmount != 0)

            return
            storage.GetItem(item, acutalAmount);

        return null;
    }

    public void Import(Item itemToAdd)
    {
        if (itemToAdd == null ||
            storage.IsFull ||
            storage.CheckCapacity(itemToAdd, itemToAdd.Quantity) < itemToAdd.Quantity)

            throw new Exception("Invalid Import.");

        storage.AddItem(itemToAdd);
    }

    public int PrepareImport(Item item, int amount)
    {
        int avaibleSpace = storage.CheckCapacity(item, amount);
        return avaibleSpace;

    }

    void ReceiveMessage(object from)
    {
        if (from as Platform == platform)
        {
            OnMouseUpAsButton();
        }
    }
}