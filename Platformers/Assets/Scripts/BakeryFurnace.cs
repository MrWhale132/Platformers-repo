using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class BakeryFurnace
{
    int id;

    Inventory bakeablesInv;
    Inventory burnablesInv;
    Inventory productsInv;

    BakeryFurnaceDesigner designer;

    BakeryFurnaceGUI gui;

    BakeryPanel panel;

    BakeState bakeState;

    CoroutineManager coManager;

    int ignitionValue = 4 * 20;   // 4 coal


    public int ID => id;
    public BakeryFurnaceDesigner Designer => designer;
    public BakeryFurnaceGUI GUI => gui;
    public BakeState BakeStatus => bakeState;


    public BakeryFurnace(BakeryPanel owner, BakeryFurnaceGUI gui, int id)
    {
        this.gui = gui;
        panel = owner;
        this.id = id;

        designer = new BakeryFurnaceDesigner(this, gui, owner);
        bakeState = new BakeState(gui);
        coManager = CoroutineManager.Instance;

        SetUpInventories();
        gui.AddController(this);
    }

   
    IEnumerator Bake()
    {
        bakeState.Bakeing = true;
        while (bakeState.Times > 0)
        {
            if (!bakeState.FromPause)
            {
                bakeablesInv.GetItems(Items.Clone(bakeState.Receipt.Components));
                if (panel.IsActive) designer.GetBakeablesInventory().GetItems(Items.Clone(bakeState.Receipt.Components));
            }
            else bakeState.FromPause = false;

            while (bakeState.RemainingTime > 0)
            {
                bakeState.ProceedProgress();
                yield return null;
            }
            CreateProducts();

            if (!CheckProductCapacity(bakeState.Receipt)) break;
            gui.SetCurrentProgress(0);
        }
        Stop();
    }

    IEnumerator Burn()
    {
        bakeState.Burning = true;
        while (bakeState.Bakeing && CheckIgnition())
        {
            int burnTime = Ignite();
            float remainingTime = burnTime;
            while (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                gui.SetBurnProgress(remainingTime / burnTime);
                yield return null;
            }
            gui.SetBurnProgress(0);
        }
        bakeState.Burning = false;
        if (bakeState.Bakeing)
        {
            Stop();
        }
        else if (bakeState.FromPause)
            bakeState.SetDefaultState();
        coManager.Stop(nameof(Burn));
    }

    public int Ignite()
    {
        int sum = 0;
        int i = 0;
        while (sum < ignitionValue && i < burnablesInv.Size)
        {
            if (burnablesInv[i] == null) continue;
            int amount = Mathf.CeilToInt((float)ignitionValue / (burnablesInv[i] as IBurnable).BurnValue);
            Item item = burnablesInv.GetItemAt(i, amount);
            sum += (item as IBurnable).BurnValue * item.Quantity;
            i++;
        }
        if (sum < ignitionValue) throw new Exception();
        return sum;
    }

    public bool CheckIgnition()
    {
        int sum = 0;
        foreach (Item item in burnablesInv)
        {
            if (item == null) continue;
            sum += (item as IBurnable).BurnValue * item.Quantity;
            if (sum >= ignitionValue) return true;
        }
        return false;
    }


    public void Start()
    {
        if (bakeState.Bakeing) return;

        if (bakeState.FromPause)
        {
            if (bakeState.Burning || CheckIgnition())
            {
                gui.SetActivePause(true);
                gui.SetActiveStop(false);
                coManager.Add(nameof(Bake), Bake());
                if (bakeState.Burning == false)
                    coManager.Add(nameof(Burn), Burn());
            }
            else gui.StartFloatMessage("There is not enough burnables in the furnace to ignite");
            return;
        }

        BakeReceipt[] bakeReceipts = GetReceiptsByGrouping(bakeablesInv);
        if (bakeReceipts.Length > 0)
        {
            if (bakeReceipts[0].CheckComponents(bakeablesInv))
            {
                if (CheckProductCapacity(bakeReceipts[0]))
                {
                    if (bakeState.Burning || CheckIgnition())
                    {
                        gui.SetActivePause(true);
                        coManager.Add(nameof(Bake), Bake());
                        if (bakeState.Burning == false)
                            coManager.Add(nameof(Burn), Burn());
                    }
                    else gui.StartFloatMessage("There is not enough burnables in the furnace to ignite");
                }
                else gui.StartFloatMessage("The product inventory is full.");
            }
            else gui.StartFloatMessage("There is not enough component to start bakeing.");
        }
        else gui.StartFloatMessage("There is no receipt");
    }

    public void Pause()
    {
        if (!bakeState.Bakeing) return;
        else bakeState.Bakeing = false;

        gui.SetActivePause(false);
        gui.SetActiveStop(true);

        bakeState.FromPause = true;
        coManager.Stop(nameof(Bake));
    }

    public void Stop()
    {
        if (!bakeState.FromPause)
        {
            if (!bakeState.Bakeing) return;
            else bakeState.Bakeing = false;
        }
        else bakeState.FromPause = false;

        gui.SetActiveStop(false);

        coManager.Stop(nameof(Bake));
        var receipts = GetReceiptsByGrouping(bakeablesInv);
        if (receipts.Length > 0)
        {
            bakeState.SetReceipt(receipts[0]);
            bakeState.SetUpStart(bakeablesInv);
        }
        else
            bakeState.SetDefaultState();
    }

    void CreateProducts()
    {
        Item[] products = Items.Clone(bakeState.Receipt.Products);

        if (panel.IsActive) designer.GetProductsInventory().AddItems(Items.Clone(products));

        productsInv.AddItems(products);
        bakeState.RemainingTime = bakeState.Receipt.BakeTime;
        bakeState.Times--;
        gui.SetTotalProgress(1 - bakeState.Times / (float)bakeState.StartTimes);

    }

    public BakeReceipt[] GetReceiptsByGrouping(Inventory from)
    {
        IBakeable[] bakeables = Array.ConvertAll(from.GetInventoryStatistics(), item => (IBakeable)item);
        int bakeSum = bakeables.GroupBy(bakeable => bakeable?.GetType()).
                                Where(group => group.Key != null).
                                Sum(typeGroup => typeGroup.First().BakeReceiptValue);
        return BakeryReceipts.FindReceipts(bakeSum);
    }

    bool CheckProductCapacity(BakeReceipt receipt)
    {
        return receipt.Products.All(item => productsInv.CheckCapacity(item) >= item.Quantity);
    }


    void SetUpInventories()
    {
        bakeablesInv = new Inventory(1);
        burnablesInv = new Inventory(1);
        productsInv = new Inventory(1);
    }

    public Inventory GetBakeablesInventory() => bakeablesInv;
    public Inventory GetBurnablesInventory() => burnablesInv;
    public Inventory GetProductsInventory() => productsInv;


    public class BakeState
    {
        BakeryFurnaceGUI gui;
        BakeReceipt receipt;

        bool bakeing;
        int times;
        int startTimes;
        float totalTime;
        float remainingTime;
        bool burning;

        bool fromPause;

        public bool Bakeing { get => bakeing; set => bakeing = value; }
        public int Times { get => times; set => times = value; }
        public int StartTimes => startTimes;
        public float TotalTime => totalTime;
        public float RemainingTime { get => remainingTime; set => remainingTime = value; }
        public bool FromPause { get => fromPause; set => fromPause = value; }
        public BakeReceipt Receipt => receipt;
        public bool Burning { get => burning; set => burning = value; }

        public BakeState(BakeryFurnaceGUI gui)
        {
            this.gui = gui;
        }

        public void ProceedProgress()
        {
            float ellapsedTime = Time.deltaTime;

            totalTime -= ellapsedTime;
            remainingTime -= ellapsedTime;
            gui.SetTotalTime(totalTime);
            gui.SetCurrentTime(remainingTime);
            float percent = 1 - remainingTime / receipt.BakeTime;
            gui.SetCurrentProgress(percent);
        }

        public void RecalculateProgress(Inventory from)
        {
            int prevTimes = times;
            times = receipt.ManyTimes(from) + 1;
            if (times == prevTimes) return;
            int proceeded = startTimes - prevTimes;
            startTimes = times + proceeded;
            float percent = 1 - times / (float)startTimes;
            gui.SetTotalProgress(percent);
            totalTime += (times - prevTimes) * receipt.BakeTime;
            gui.SetTotalTime(totalTime);
        }

        public void SetUpStart(Inventory from)
        {
            times = receipt.ManyTimes(from);
            startTimes = times;
            totalTime = times * receipt.BakeTime;
            remainingTime = receipt.BakeTime;

            gui.SetCurrentProgress(0);
            gui.SetTotalProgress(0);
            gui.SetCurrentTime(receipt.BakeTime);
            gui.SetTotalTime(totalTime);
        }

        public void SetDefaultState()
        {
            times = 0;
            startTimes = 0;
            totalTime = 0;
            remainingTime = 0;
            bakeing = false;
            fromPause = false;
            gui.SetCurrentProgress(0);
            gui.SetTotalProgress(0);
            gui.SetCurrentTime(0);
            gui.SetTotalTime(0);
            gui.SetActiveStop(false);
            receipt = null;
        }

        public void SetReceipt(BakeReceipt receipt)
        {
            this.receipt = receipt;
        }
    }
}