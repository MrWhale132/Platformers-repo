
public class BakeryFurnaceDesigner
{
    Inventory bakeablesInv;
    Inventory burnablesInv;
    Inventory productsInv;

    BakeryFurnace editor;

    BakeryFurnaceGUI gui;

    BakeryPanel panel;


    public BakeryFurnace Editor => editor;


    public BakeryFurnaceDesigner(BakeryFurnace owner, BakeryFurnaceGUI gui, BakeryPanel container)
    {
        editor = owner;
        this.gui = gui;
        panel = container;

        SetUpInventories();
        gui.AddController(this);
    }


    void BakeInventoryChanged()
    {
        if (editor.BakeStatus.Bakeing)
        {
            editor.BakeStatus.RecalculateProgress(bakeablesInv);
            return;
        }

        var receipts = editor.GetReceiptsByGrouping(bakeablesInv);
        if (receipts.Length > 0)
        {
            editor.BakeStatus.SetReceipt(receipts[0]);
            if (editor.BakeStatus.FromPause)
                editor.BakeStatus.RecalculateProgress(bakeablesInv);
            else editor.BakeStatus.SetUpStart(bakeablesInv);
        }
    }

    void OnBakeablesClicked(int at)
    {
        if (editor.BakeStatus.Bakeing)
        {
            gui.StartFloatMessage("You can not edit an on going process.");
            return;
        }

        Item clicked = bakeablesInv[at];
        if (clicked != null)
        {
            Inventory clientInv = panel.GetClientInventory();
            int amount = panel.Max ? clicked.Quantity : panel.ClickMultiplier;
            int capacity = clientInv.CheckCapacity(clicked, amount);
            if (capacity > 0)
            {
                Item item = bakeablesInv.GetItemAt(at, capacity);
                var info = new BakeryTradeInfo(editor.ID, BakeryPanel.Inventorys.Bakeable, BakeryPanel.Inventorys.Client, item.Copy());
                panel.AddTradeInfo(info);
                clientInv.AddItem(item);
            }
            else gui.StartFloatMessage("Your inventory is full");
        }
    }

    void OnBurnablesClicked(int at)
    {
        if (editor.BakeStatus.Bakeing)
        {
            gui.StartFloatMessage("You can not edit an on going process.");
            return;
        }
        else if (editor.BakeStatus.Burning)
        {
            gui.StartFloatMessage("You can not add burnables to the furnace while it is burning");
            return;
        }

        Item clicked = burnablesInv[at];
        if (clicked != null)
        {
            Inventory clientInv = panel.GetClientInventory();
            int amount = panel.Max ? clicked.Quantity : panel.ClickMultiplier;
            int capacity = clientInv.CheckCapacity(clicked, amount);
            if (capacity > 0)
            {
                Item item = burnablesInv.GetItemAt(at, capacity);
                var info = new BakeryTradeInfo(editor.ID, BakeryPanel.Inventorys.Burnable, BakeryPanel.Inventorys.Client, item.Copy());
                panel.AddTradeInfo(info);
                clientInv.AddItem(item);
            }
            else gui.StartFloatMessage("Your inventory is full");
        }
    }

    void OnProductsClicked(int at)
    {
        if (editor.BakeStatus.Bakeing)
        {
            gui.StartFloatMessage("You can not edit an on going process.");
            return;
        }

        Item clicked = productsInv[at];
        if (clicked != null)
        {
            Inventory clientInv = panel.GetClientInventory();
            int amount = panel.Max ? clicked.Quantity : panel.ClickMultiplier;
            int capacity = clientInv.CheckCapacity(clicked, amount);
            if (capacity > 0)
            {
                Item item = productsInv.GetItemAt(at, capacity);
                var info = new BakeryTradeInfo(editor.ID, BakeryPanel.Inventorys.Product, BakeryPanel.Inventorys.Client, item.Copy());
                panel.AddTradeInfo(info);
                clientInv.AddItem(item);
            }
            else gui.StartFloatMessage("Your inventory is full");
        }
    }


    public void Select()
    {

    }

    public void Deselect()
    {

    }

    public void OnSelection()
    {
        panel.DesignerSelected(this);
    }


    void SetUpInventories()
    {
        bakeablesInv = new Inventory(1);
        burnablesInv = new Inventory(1);
        productsInv = new Inventory(1);

        bakeablesInv.AddClickListener(OnBakeablesClicked);
        burnablesInv.AddClickListener(OnBurnablesClicked);
        productsInv.AddClickListener(OnProductsClicked);

        bakeablesInv.AddChangeListener(BakeInventoryChanged);
    }

    public Inventory GetBakeablesInventory() => bakeablesInv;
    public Inventory GetBurnablesInventory() => burnablesInv;
    public Inventory GetProductsInventory() => productsInv;
}