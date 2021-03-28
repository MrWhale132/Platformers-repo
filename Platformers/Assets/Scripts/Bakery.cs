using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Bakery : Building, IInteractable
{
    [SerializeField]
    BakeryFurnaceGUI guiPrefab;

    [SerializeField]
    BakeryPanel panel;

    List<BakeryFurnace> furnaces;


    void Awake()
    {
        furnaces = new List<BakeryFurnace>();

        Transform parent = GameObject.FindGameObjectWithTag("Canvas").transform;
        panel = Instantiate(panel, parent);
        panel.SetOwner(this);

        var first = new BakeryFurnace(panel, panel.GetFirstGUI(), 1);
        first.GUI.InvokeOnSelection();
        furnaces.Add(first);

        panel.AddListener(PanelMessageHandler);
        panel.Display(false);
    }

    protected override void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (!GameManager.ValidSelection()) goto Skip;

        FetchDataToDesigners();
        panel.Display(true);

    Skip:

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, platform));
    }

    void FetchDataToDesigners()
    {
        foreach (var furnace in furnaces)
        {
            furnace.Designer.GetBakeablesInventory().SetInventoryStatistics(furnace.GetBakeablesInventory().GetInventoryStatistics());
            furnace.Designer.GetBurnablesInventory().SetInventoryStatistics(furnace.GetBurnablesInventory().GetInventoryStatistics());
            furnace.Designer.GetProductsInventory().SetInventoryStatistics(furnace.GetProductsInventory().GetInventoryStatistics());
        }
    }


    public void ValidateLease(Inventory clientInv, BakeryLease lease)
    {
        Item[] items = null;
        Item[] CloneItems() => items.Select(item => item.Copy()).ToArray();

        foreach (BakeryTradeInfo info in lease)
        {
            switch (info.Source)
            {
                case BakeryPanel.Inventorys.Client:
                    items = clientInv.GetItems(info.GetItems()); break;
                case BakeryPanel.Inventorys.Bakeable:
                    var curr = furnaces.Find(furnace => furnace.ID == info.ID);
                    items = curr.GetBakeablesInventory().GetItems(info.GetItems());
                    if (panel.IsActive)
                        curr.Designer.GetBakeablesInventory().GetItems(CloneItems()); break;
                case BakeryPanel.Inventorys.Burnable:
                    curr = furnaces.Find(furnace => furnace.ID == info.ID);
                    items = curr.GetBurnablesInventory().GetItems(info.GetItems());
                    if (panel.IsActive)
                        curr.Designer.GetBurnablesInventory().GetItems(CloneItems()); break;
                case BakeryPanel.Inventorys.Product:
                    curr = furnaces.Find(furnace => furnace.ID == info.ID);
                    items = curr.GetProductsInventory().GetItems(info.GetItems());
                    if (panel.IsActive)
                        curr.Designer.GetProductsInventory().GetItems(CloneItems()); break;
                default: break;
            }
            switch (info.Target)
            {
                case BakeryPanel.Inventorys.Client:
                    clientInv.AddItems(items); break;
                case BakeryPanel.Inventorys.Bakeable:
                    var curr = furnaces.Find(furnace => furnace.ID == info.ID);
                    curr.GetBakeablesInventory().AddItems(CloneItems());
                    if (panel.IsActive)
                        curr.Designer.GetBakeablesInventory().AddItems(items); break;
                case BakeryPanel.Inventorys.Burnable:
                    curr = furnaces.Find(furnace => furnace.ID == info.ID);
                    curr.GetBurnablesInventory().AddItems(CloneItems());
                    if (panel.IsActive)
                        curr.Designer.GetBurnablesInventory().AddItems(items); break;
                case BakeryPanel.Inventorys.Product:
                    curr = furnaces.Find(furnace => furnace.ID == info.ID);
                    curr.GetProductsInventory().AddItems(CloneItems());
                    if (panel.IsActive)
                        curr.Designer.GetProductsInventory().AddItems(items); break;
                default: break;
            }
        }
    }

    public void FetchClientStatistics(Item[] statistics)
    {
        panel.SetClientStatistics(statistics);
    }

    public void CancelInteractionWith(object with)
    {
        if (with is Farmer farmer)
        {
            panel.RemoveListener(farmer.PanelMessageHandler);
        }
    }

    public void InteractedBy(object interacter)
    {
        if (interacter is Farmer farmer)
        {
            panel.AddListener(farmer.PanelMessageHandler);
        }
    }

    void PanelMessageHandler(object sender, System.EventArgs msg)
    {

    }
}