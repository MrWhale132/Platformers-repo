using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class MillProcessLine
{
    event Action<MillTradeInfo> TradeInfoFlow;

    Inventory grindInv;
    Inventory productInv;

    int id;
    bool grinding;

    MillPanel.Permissions.ProcessLine panelPermission;

    List<MillTradeInfo> itemTrades;

    MillPanelProcessLine gui;

    CoroutineManager coManager;

    GrindReceipt inUseReceipt;


    public Inventory GrindInventory => grindInv;
    public Inventory ProductInventory => productInv;
    public int Id => id;
    public bool IsGrinding => grinding;
    public MillPanelProcessLine GUI => gui;


    public MillProcessLine()
    {
        itemTrades = new List<MillTradeInfo>();
        grindInv = new Inventory(1);
        productInv = new Inventory(1);
        coManager = CoroutineManager.Instance;
    }

    public void SetUpGUIForPanel(MillPanelProcessLine gui)
    {
        this.gui = gui;
        grindInv.InventoryHolder = gui.GetGrindHolder();
        productInv.InventoryHolder = gui.GetProductHolder();

        gui.AddStartGrindingListener(PanelStartGrinding);
        gui.AddStopGrindingListener(PanelStopGrinding);

        gui.AddOnSelectionListener(OnSelectionHandler);
        grindInv.AddClickListener(GrindItemSlotClicked);
        productInv.AddClickListener(ProductItemSlotClicked);

        grindInv.AddChangeListener(PanelGrindInventoryChanged);

        AddGrindSlot();
        AddProductSlot();
    }

    public void SetUpGUIForMill(MillPanelProcessLine gui)
    {
        this.gui = gui;
        gui.AddStartGrindingListener(StartGrinding);
        gui.AddStopGrindingListener(StopGrinding);
        grindInv.InventoryHolder = gui.GetGrindHolder();
        productInv.InventoryHolder = gui.GetProductHolder();
        grindInv.ItemSlots = gui.GetGrindItemSltos();
        productInv.ItemSlots = gui.GetProductSlots();

        grindInv.AddChangeListener(MillGrindInventoryChanged);
    }

    public void AddGrindSlot()
    {
        gui.AddGrindSlot();
        grindInv.ReSize(gui.GrindSlotsCount);
        grindInv.ItemSlots = gui.GetGrindItemSltos();
    }

    public void AddProductSlot()
    {
        gui.AddProductSlot();
        productInv.ReSize(gui.ProductSlotsCount);
        productInv.ItemSlots = gui.GetProductSlots();
    }


    void GrindItemSlotClicked(int at)
    {
        if (grinding)
        {
            panelPermission.StartFloatMessage("You can't design a process line while it is grinding.");
            return;
        }

        Item itemAt = grindInv[at];
        if (itemAt != null)
        {
            Inventory clientInv = panelPermission.GetClientInventory();
            int capacity = clientInv.CheckCapacity(itemAt, 1);
            if (capacity > 0)
            {
                Item item = grindInv.GetItemAt(at, 1);
                TradeInfoFlow(new MillTradeInfo(id, Item.Clone(item), MillPanel.Inventorys.Grind, MillPanel.Inventorys.Client));
                clientInv.AddItem(item);
            }
        }
    }

    void ProductItemSlotClicked(int at)
    {
        if (grinding)
        {
            panelPermission.StartFloatMessage("You can't design a process line while it is grinding.");
            return;
        }

        Item itemAt = productInv[at];
        if (itemAt != null)
        {
            Inventory clientInv = panelPermission.GetClientInventory();
            int freeSpace = clientInv.CheckCapacity(itemAt, 1);
            if (freeSpace > 0)
            {
                Item item = productInv.GetItemAt(at, 1);
                TradeInfoFlow(new MillTradeInfo(id, Item.Clone(item), MillPanel.Inventorys.Product, MillPanel.Inventorys.Client));
                clientInv.AddItem(item);
            }
        }
    }

    void PanelGrindInventoryChanged()
    {
        if (inUseReceipt == null)
        {
            GrindReceipt[] recepits = GetReceiptsByGrouping();
            if (recepits.Length > 0)
            {
                inUseReceipt = recepits[0];
            }
            else
            {
                gui.SetCurrentTime(0);
                gui.SetTotalTime(0);
                return;
            }
        }
        if (productInv.IsFull && grinding)
        {
            StopGrinding();
            gui.SetCurrentTime(0);
            gui.SetTotalTime(0);
            gui.SetTotalTime(0);
            gui.SetTotalProgress(0);
            return;
        }
        if (inUseReceipt.CheckComponents(grindInv) == false)
        {
            gui.SetCurrentTime(inUseReceipt.GrindTime);
            if (grinding)
            {
                coManager.Add(nameof(FinalCountDown), FinalCountDown(inUseReceipt.GrindTime));
                IEnumerator FinalCountDown(float time)
                {
                    while (time > 0)
                    {
                        time -= Time.deltaTime;
                        yield return null;
                    }

                    grinding = false;
                    coManager.Stop(nameof(FinalCountDown));
                }
            }
            return;
        }
        times = inUseReceipt.ManyTimes(grindInv);
        totalTime = times * inUseReceipt.GrindTime;
        gui.SetCurrentTime(inUseReceipt.GrindTime);
        gui.SetTotalTime(totalTime);
    }

    void MillGrindInventoryChanged()
    {
        if (grindInv.IsEmpty)
        {
            gui.SetCurrentTime(0);
            gui.SetTotalTime(0);
            return;
        }
        if (inUseReceipt == null)
        {
            inUseReceipt = GetReceiptsByGrouping()[0];
        }
        int oldTimes = times;
        times = inUseReceipt.ManyTimes(grindInv);
        if (grinding) times++;
        int diff = times - oldTimes;
        if (diff == 0) return;

        startingTimes += diff;
        totalTime += diff * inUseReceipt.GrindTime;
        gui.SetTotalTime(totalTime);

        if (startingTimes == 0) return;

        float percent = 1 - (times / (float)startingTimes);
        gui.SetTotalProgress(percent);
    }

    int times;
    float totalTime;
    int startingTimes;
    float remainingTime;

    IEnumerator Grinding(GrindReceipt receipt)
    {
        grinding = true;
        times = receipt.ManyTimes(grindInv);
        totalTime = times * receipt.GrindTime;
        startingTimes = times;
        float percent = 0;
        while (times > 0)
        {
            remainingTime = receipt.GrindTime;
            Item[] products = receipt.CreateProduct(grindInv);
            foreach (var item in receipt.Components)
            {
                TradeInfoFlow(new MillTradeInfo(id, Item.Clone(item), MillPanel.Inventorys.Grind, MillPanel.Inventorys.Grind));
            }
            while (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                totalTime -= Time.deltaTime;
                if (gui.IsActive)
                {
                    gui.SetCurrentTime(remainingTime);
                    gui.SetTotalTime(totalTime);
                    percent = 1 - remainingTime / receipt.GrindTime;
                    gui.SetCurrentProgress(percent);
                }
                yield return null;
            }
            foreach (Item item in products)
            {
                TradeInfoFlow(new MillTradeInfo(id, Item.Clone(item), MillPanel.Inventorys.Grind, MillPanel.Inventorys.Product));
                productInv.AddItem(item);
            }
            times--;
            percent = 1 - (times / (float)startingTimes);
            gui.SetTotalProgress(percent);
        }
        gui.SetCurrentTime(0);
        gui.SetCurrentProgress(0);
        gui.SetTotalTime(0);
        gui.SetTotalProgress(0);
        grinding = false;
        inUseReceipt = null;
        coManager.Stop(nameof(Grinding));
    }

    public void StartGrinding()
    {
        if (grinding) return;

        if (productInv.IsFull)
        {
            panelPermission.StartFloatMessage("The product inventory is full.");
            return;
        }

        GrindReceipt[] grindReceipts = GetReceiptsByGrouping();
        if (grindReceipts.Length > 0)
        {
            if (grindReceipts[0].CheckComponents(grindInv))
            {
                inUseReceipt = grindReceipts[0];
                coManager.Add(nameof(Grinding), Grinding(inUseReceipt));
            }
            else panelPermission.StartFloatMessage("There is not enough component to start grinding.");
        }
        else panelPermission.StartFloatMessage("There is no receipt");
    }

    public void StopGrinding()
    {
        if (!grinding) return;

        grinding = false;
        coManager.Stop(nameof(Grinding));
        float grindTime = inUseReceipt.GrindTime;
        int times = inUseReceipt.ManyTimes(grindInv);
        gui.SetCurrentTime(grindTime);
        gui.SetTotalTime(grindTime * times);
        gui.SetCurrentProgress(0);
        gui.SetTotalProgress(0);
    }

    GrindReceipt[] GetReceiptsByGrouping()
    {
        IGrindable[] grindables = Array.ConvertAll(grindInv.GetInventoryStatistics(), item => (IGrindable)item);
        int grindSum = grindables.GroupBy(grindable => grindable?.GetType()).
                                  Where(group => group.Key != null).
                                  Sum(typeGroup => typeGroup.First().GrindValue);
        return GrindReceipts.FindReceipts(grindSum);
    }

    void PanelStartGrinding()
    {
        if (grinding) return;

        if (productInv.IsFull)
        {
            return;
        }

        GrindReceipt[] grindReceipts = GetReceiptsByGrouping();
        if (grindReceipts.Length > 0 &&
            grindReceipts[0].CheckComponents(grindInv))
        {
            inUseReceipt = grindReceipts[0];
            grinding = true;
        }
    }

    void PanelStopGrinding()
    {
        if (!grinding) return;

        if (coManager.Running("FinalCountDown"))
            coManager.Stop("FinalCountDown");
        grinding = false;
    }


    public void SetId(int id)
    {
        this.id = id;
    }

    void OnSelectionHandler()
    {
        panelPermission.Selected(this);
    }

    public void SelectGUI()
    {
        gui.Select();
    }

    public void DeselectGUI()
    {
        gui.Deselect();
    }

    public void DestroyGUI()
    {
        GameObject.Destroy(gui.gameObject);
    }

    public void GivePermission(MillPanel.Permissions.ProcessLine permission)
    {
        panelPermission = permission;
    }

    public void AddTradeFlowListener(Action<MillTradeInfo> listener)
    {
        TradeInfoFlow += listener;
    }
}