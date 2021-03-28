using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.UI;

public class BakeryPanel : Panel
{
    public enum Inventorys { Client = 0, Bakeable = 1, Burnable = 2, Product = 3 }

    Bakery owner;

    [SerializeField]
    Transform clientHolder;
    [SerializeField]
    Transform furnacesHolder;

    ItemSlot[] clientSlots;

    Inventory clientInv;

    BakeryFurnaceDesigner selectedDesigner;

    BakeryFurnaceGUI firstGUI;

    List<BakeryTradeInfo> infos;

    [SerializeField]
    Image[] buttonsImages;

    int clickMultiplier = 1;

    bool max;


    public bool IsActive => gameObject.activeSelf;
    public int ClickMultiplier => clickMultiplier;
    public bool Max => max;


    void Awake()
    {
        firstGUI = furnacesHolder.GetComponentInChildren<BakeryFurnaceGUI>();

        infos = new List<BakeryTradeInfo>();

        clientSlots = clientHolder.GetComponentsInChildren<ItemSlot>();
        clientInv = new Inventory(6);
        clientInv.SetGUI(clientHolder, clientSlots);
        clientInv.AddClickListener(OnClientSlotClick);

        HighLight(0);
    }

    public void SetClientStatistics(Item[] statistics)
    {
        clientInv.SetInventoryStatistics(statistics);
        for (int i = 0; i < clientSlots.Length; i++)
        {
            clientSlots[i].gameObject.SetActive(i < statistics.Length);
        }
    }

    void OnClientSlotClick(int at)
    {
        if (selectedDesigner.Editor.BakeStatus.Bakeing)
        {
            selectedDesigner.Editor.GUI.StartFloatMessage("You can not edit an on going process.");
            return;
        }

        Item clicked = clientInv[at];
        if (clicked != null)
        {
            Inventorys target;
            Inventory targetInv;
            if (clicked is IBakeable)
            {
                targetInv = selectedDesigner.GetBakeablesInventory();
                target = Inventorys.Bakeable;
            }
            else if (clicked is IBurnable)
            {
                if (selectedDesigner.Editor.BakeStatus.Burning)
                {
                    selectedDesigner.Editor.GUI.StartFloatMessage("You can not add burnables to the furnace while it is burning");
                    return;
                }

                targetInv = selectedDesigner.GetBurnablesInventory();
                target = Inventorys.Burnable;
            }
            else return;

            int amount = max ? clicked.Quantity : clickMultiplier;
            int capacity = targetInv.CheckCapacity(clicked, amount);
            if (capacity > 0)
            {
                Item item = clientInv.GetItemAt(at, capacity);
                int id = selectedDesigner.Editor.ID;
                var info = new BakeryTradeInfo(id, Inventorys.Client, target, item.Copy());
                AddTradeInfo(info);
                targetInv.AddItem(item);
            }
            else selectedDesigner.Editor.GUI.StartFloatMessage("The " + target.ToString() + " inventory is full");
        }
    }


    public void AddTradeInfo(BakeryTradeInfo curr)
    {
        BakeryTradeInfo last = null;
        if (infos.Count > 0)
            last = infos.Last();
        if (curr.Equals(last))
        {
            last += curr;
        }
        else infos.Add(curr);
        //print(infos.Count);
        //infos.ForEach(info => info.GetItems().ToList().ForEach(item => print(item)));
    }

    public void Display(bool value)
    {
        gameObject.SetActive(value);
    }

    public void DesignerSelected(BakeryFurnaceDesigner designer)
    {
        if (selectedDesigner != null)
            selectedDesigner.Deselect();
        selectedDesigner = designer;
        selectedDesigner.Select();
    }

    public Inventory GetClientInventory()
    {
        return clientInv;
    }

    public void SetOwner(Bakery owner)
    {
        if (this.owner != null) throw new System.Exception("Invalid owner setup.");

        this.owner = owner;
    }

    public void Command()
    {
        Display(false);
        var lease = new BakeryLease(owner, infos.ToArray());
        SendMessage(this, new BakeryPanelMessage(BakeryPanelMessage.Buttons.Command, lease));
        infos.Clear();
    }

    public void Exit()
    {
        Display(false);
        SendMessage(this, new BakeryPanelMessage(BakeryPanelMessage.Buttons.Exit));
        infos.Clear();
    }

    public void SetClickMultiplier(int times)
    {
        clickMultiplier = times;
        max = false;
    }

    public void SetMax()
    {
        max = true;
    }

    public void HighLight(int index)
    {
        for (int i = 0; i < buttonsImages.Length; i++)
            buttonsImages[i].color = Color.white;
        buttonsImages[index].color = Color.yellow;
    }

    void OnDisable()
    {
        foreach (var slot in clientSlots) slot.gameObject.SetActive(true);
        clientInv.FlushInv();
    }

    public BakeryFurnaceGUI GetFirstGUI() => firstGUI;
}


public class BakeryPanelMessage : System.EventArgs
{
    public enum Buttons { Exit = 0, Command = 1 }
    Buttons pressed;
    BakeryLease lease;

    public Buttons Pressed => pressed;
    public BakeryLease BakeryLease => lease;

    public BakeryPanelMessage(Buttons pressed)
    {
        this.pressed = pressed;
    }

    public BakeryPanelMessage(Buttons pressed, BakeryLease lease)
    {
        this.pressed = pressed;
        this.lease = lease;
    }
}


public class BakeryLease : IEnumerable
{
    Bakery bakery;
    BakeryTradeInfo[] infos;

    public BakeryLease(Bakery owner, BakeryTradeInfo[] infos)
    {
        bakery = owner;
        this.infos = infos;
    }

    public Bakery GetBakery() => bakery;
    public BakeryTradeInfo[] GetTradeInfos() => infos;

    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < infos.Length; i++)
        {
            yield return infos[i];
        }
    }
}

public class BakeryTradeInfo
{
    int id;
    BakeryPanel.Inventorys source;
    BakeryPanel.Inventorys target;
    List<Item> items;

    public int ID => id;
    public BakeryPanel.Inventorys Source => source;
    public BakeryPanel.Inventorys Target => target;

    public BakeryTradeInfo(int id, BakeryPanel.Inventorys source, BakeryPanel.Inventorys target, Item item)
    {
        this.id = id;
        this.source = source;
        this.target = target;
        items = new List<Item>() { item };
    }

    public static BakeryTradeInfo operator +(BakeryTradeInfo a, BakeryTradeInfo b)
    {
        if (a.Equals(b))
        {
            foreach (var curr in b.items)
            {
                if (a.items.Contains(curr))
                {
                    Item match = a.items.Find(item => item.Equals(curr));
                    match.Quantity += curr.Quantity;
                    curr.Quantity = 0;
                }
                else a.items.Add(curr);
            }
            return a;
        }
        throw new System.InvalidOperationException();
    }

    public Item[] GetItems()
    {
        return items.Select(item => item.Copy()).ToArray();
    }

    public override bool Equals(object obj)
    {
        if (obj is BakeryTradeInfo info)
        {
            if (info.id == id &&
                info.source == source &&
                info.target == target)
            {
                return true;
            }
        }
        return false;
    }

    public override int GetHashCode()
    {
        int hashCode = 2087185999;
        hashCode = hashCode * -1521134295 + id.GetHashCode();
        hashCode = hashCode * -1521134295 + source.GetHashCode();
        hashCode = hashCode * -1521134295 + target.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<List<Item>>.Default.GetHashCode(items);
        return hashCode;
    }
}