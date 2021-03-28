using System;
using System.Collections;
using UnityEngine;
using System.Linq;

public enum TradeFlow { Import, Export }

public class Inventory : IEnumerable
{
    event Action InventoryChanged;
    event Action<int> ItemSlotClicked;

    public int Size { get { return inventory.Length; } private set { } }
    public bool IsFull
    {
        get => inventory.All(item => item != null && item.Quantity == item.StackLimit);
    }

    public bool IsActive => inventoryHolder != null && inventoryHolder.gameObject.activeSelf;
    public bool IsEmpty
    {
        get
        {
            for (int i = 0; i < inventory.Length; i++)
                if (inventory[i] != null)
                    return false;
            return true;
        }
    }

    Item[] inventory;

    ItemSlot[] itemSlots;

    Transform inventoryHolder;

    public ItemSlot[] ItemSlots
    {
        get => itemSlots;
        set
        {
            if (itemSlots != null)
            {
                foreach (var item in itemSlots)
                {
                    item.RemoveListener(ItemSlotOnClickHandler);
                }
            }
            itemSlots = value;
            foreach (var item in itemSlots)
            {
                item.AddListener(ItemSlotOnClickHandler);
            }
            Refresh();
        }
    }
    public Transform InventoryHolder
    {
        get => inventoryHolder;
        set
        {
            inventoryHolder = value;
            InventoryChanged += Refresh;  //Same method added multipletimes !!!
        }
    }

    public void SetGUI(Transform holder, ItemSlot[] slots)
    {
        inventoryHolder = holder;

        if (itemSlots != null)
        {
            foreach (var item in itemSlots)
            {
                item.RemoveListener(ItemSlotOnClickHandler);
            }
        }
        itemSlots = slots;
        foreach (var item in itemSlots)
        {
            item.AddListener(ItemSlotOnClickHandler);
        }
        Refresh();
    }


    public Inventory(int size)
    {
        Size = size;
        inventory = new Item[size];
        InventoryChanged += Refresh;
    }


    public Inventory Clone(bool onlyStats = true)
    {
        Inventory clone = new Inventory(inventory.Length);
        clone.inventory = inventory.Select(c => Item.Clone(c)).ToArray();
        if (!onlyStats)
        {
            // too lazy....
        }
        return clone;
    }


    void ItemSlotOnClickHandler(ItemSlot slot)
    {
        int index = Array.IndexOf(itemSlots, slot);
        if (index == -1)
            throw new Exception("An itemslot belong to two different inventory.");
        ItemSlotClicked?.Invoke(index);
    }

    public void ReSize(int newSize)
    {
        Array.Resize(ref inventory, newSize);
    }


    public void AddClickListener(Action<int> call)
    {
        ItemSlotClicked += call;
    }

    public void AddChangeListener(Action call)
    {
        InventoryChanged += call;
    }

    public Item[] GetInventoryStatistics()
    {
        Item[] clone = inventory.Select(c => Item.Clone(c)).ToArray();
        return clone;
    }

    public void SetInventoryStatistics(Item[] statistics)
    {
        inventory = statistics;
        InventoryChanged();
    }


    public void Display(bool setActive)
    {
        if (inventoryHolder != null)
        {
            inventoryHolder.gameObject.SetActive(setActive);
            if (setActive) Refresh();
        }
        else
            InitializeInventory();
    }

    void InitializeInventory()
    {
        if (inventoryHolder == null)
        {
            itemSlots = new ItemSlot[Size];
            inventoryHolder = new GameObject().transform;
            inventoryHolder.gameObject.name = "Inventory Holder";
            inventoryHolder.parent = GameObject.FindGameObjectWithTag("Canvas").transform;

            for (int i = 0; i < Size; i++)
            {
                ItemSlot curr = UnityEngine.Object.Instantiate(Utility.Instance.itemSlotPrefab, new Vector3(Screen.width - 50 - i * 80, 0 + 50, 0), Quaternion.identity);
                curr.transform.SetParent(inventoryHolder);
                curr.AddListener(ItemSlotOnClickHandler);
                itemSlots[Size - i - 1] = curr;
            }
            InventoryChanged += Refresh;
            Refresh();
        }
        else
            Debug.LogError("You are trying to initialize an already existing inventory!");
    }

    void Refresh()
    {
        if (inventoryHolder != null && inventoryHolder.gameObject.activeSelf)
        {
            (Sprite sprite, string quantity)[] datas = GetVisualInformations();
            for (int i = 0; i < inventory.Length; i++)
            {
                itemSlots[i].ItemImage.sprite = datas[i].sprite;
                itemSlots[i].AmountText.text = datas[i].quantity;
            }
        }
    }


    public void AddItem(Item itemToAdd)
    {
        int i = 0;
        while (itemToAdd.Quantity > 0 && i < inventory.Length)
        {
            Item currItem = inventory[i];

            if (currItem != null && currItem.Equals(itemToAdd))
            {
                if (currItem.Quantity < currItem.StackLimit)
                {
                    int freeSpace = currItem.StackLimit - currItem.Quantity;

                    if (freeSpace > itemToAdd.Quantity)
                    {
                        inventory[i] = currItem + (itemToAdd - itemToAdd.Quantity);
                    }
                    else
                    {
                        inventory[i] = currItem + (itemToAdd - freeSpace);
                    }
                }
            }
            i++;
        }

        i = 0;
        while (itemToAdd.Quantity > 0 && i < inventory.Length)
        {
            Item currItem = inventory[i];
            if (currItem == null)
            {
                if (itemToAdd.Quantity < itemToAdd.StackLimit)
                {
                    inventory[i] = itemToAdd - itemToAdd.Quantity;
                }
                else
                {
                    inventory[i] = itemToAdd - itemToAdd.StackLimit;
                }
            }
            i++;
        }

        InventoryChanged?.Invoke();
    }

    public void AddItems(params Item[] itemsToAdd)
    {
        foreach (var itemToAdd in itemsToAdd)
        {
            int i = 0;
            while (itemToAdd.Quantity > 0 && i < inventory.Length)
            {
                Item currItem = inventory[i];
                if (currItem != null && currItem.Equals(itemToAdd))
                {
                    if (currItem.Quantity < currItem.StackLimit)
                    {
                        int freeSpace = currItem.StackLimit - currItem.Quantity;

                        if (freeSpace > itemToAdd.Quantity)
                        {
                            inventory[i] = currItem + (itemToAdd - itemToAdd.Quantity);
                        }
                        else
                        {
                            inventory[i] = currItem + (itemToAdd - freeSpace);
                        }
                    }
                }
                i++;
            }

            i = 0;
            while (itemToAdd.Quantity > 0 && i < inventory.Length)
            {
                Item currItem = inventory[i];
                if (currItem == null)
                {
                    if (itemToAdd.Quantity < itemToAdd.StackLimit)
                    {
                        inventory[i] = itemToAdd - itemToAdd.Quantity;
                    }
                    else
                    {
                        inventory[i] = itemToAdd - itemToAdd.StackLimit;
                    }
                }
                i++;
            }
        }

        InventoryChanged?.Invoke();
    }

    public Item GetItem(Item item)
    {
        int targetAmount = item.Quantity;
        item.Quantity = 0;

        int i = 0;
        while (item.Quantity < targetAmount && i < inventory.Length)
        {
            Item currItem = inventory[i];
            if (currItem != null && currItem.Equals(item))
            {
                int remainder = targetAmount - item.Quantity;

                if (currItem.Quantity > remainder)
                {
                    item = item + (currItem - remainder);
                }
                else
                {
                    item = item + (currItem - currItem.Quantity);
                    inventory[i] = null;
                }
            }
            i++;
        }

        InventoryChanged?.Invoke();

        return item;
    }

    public Item[] GetItems(Item[] items)
    {
        for (int j = 0; j < items.Length; j++)
        {
            Item item = items[j];
            int targetAmount = item.Quantity;
            item.Quantity = 0;

            int i = 0;
            while (item.Quantity < targetAmount && i < inventory.Length)
            {
                Item currItem = inventory[i];
                if (currItem != null && currItem.Equals(item))
                {
                    int remainder = targetAmount - item.Quantity;

                    if (currItem.Quantity > remainder)
                    {
                        item = item + (currItem - remainder);
                    }
                    else
                    {
                        item = item + (currItem - currItem.Quantity);
                        inventory[i] = null;
                    }
                }
                i++;
            }
            items[j] = item;
        }
        InventoryChanged?.Invoke();

        return items;
    }

    public Item GetItem(Item item, int amount)    // if item is point to an object what is in the inventory then this method wont work (use item.copy)
    {
        item.Quantity = 0;

        int i = 0;
        while (item.Quantity < amount && i < inventory.Length)
        {                                                                  //    ^
            Item currItem = inventory[i];                                  //    |
                                                                           //    |
            if (currItem == item) throw new ArgumentException();           // I Told you 

            if (currItem != null && currItem.Equals(item))
            {
                int remainder = amount - item.Quantity;

                if (currItem.Quantity > remainder)
                {
                    item = item + (currItem - remainder);
                }
                else
                {
                    item = item + (currItem - currItem.Quantity);
                    inventory[i] = null;
                }
            }
            i++;
        }
        InventoryChanged?.Invoke();

        return item;
    }

    public Item GetItem(Predicate<Item> Criteria, int amount)
    {
        Item returnItem = Item.Clone(inventory.
                               First(item => item != null && Criteria(item)));
        returnItem.Quantity = 0;

        int i = 0;
        while (returnItem.Quantity < amount && i < inventory.Length)
        {
            Item currItem = inventory[i];
            if (currItem != null && Criteria(currItem))
            {
                int remainder = amount - returnItem.Quantity;

                if (currItem.Quantity > remainder)
                {
                    returnItem = returnItem + (currItem - remainder);
                }
                else
                {
                    returnItem = returnItem + (currItem - currItem.Quantity);
                    inventory[i] = null;
                }
            }
            i++;
        }

        InventoryChanged?.Invoke();

        return returnItem;
    }

    public Item GetItemAt(int slotIndex, int amount)
    {
        if (slotIndex < 0 || slotIndex > inventory.Length || amount < 1)
            throw new ArgumentException("Mi csinálsz Ember?.");

        Item curr = inventory[slotIndex];
        amount = curr.Quantity - amount > -1 ? amount : curr.Quantity;
        Item amountC = curr - amount;
        if (curr.Quantity == 0) inventory[slotIndex] = null;
        InventoryChanged?.Invoke();
        return amountC;
    }


    public int CheckCapacity(Item item)
    {
        int capacity = 0;

        foreach (Item currItem in inventory)
        {
            if (currItem == null)
            {
                capacity += item.StackLimit;
            }
            else if (currItem.Equals(item))
            {
                capacity += currItem.StackLimit - currItem.Quantity;
            }

            if (capacity >= item.Quantity)
            {
                return item.Quantity;
            }
        }

        return capacity;
    }

    public int CheckCapacity(Item item, int amount)
    {
        int capacity = 0;

        foreach (Item currItem in inventory)
        {
            if (currItem == null)
            {
                capacity += item.StackLimit;
            }
            else if (currItem.Equals(item))
            {
                if (currItem.Quantity < currItem.StackLimit)
                    capacity += currItem.StackLimit - currItem.Quantity;
            }

            if (capacity >= amount)
            {
                return amount;
            }
        }

        return capacity;
    }

    public int CheckQuantity(Item item)
    {
        int quantity = 0;

        foreach (Item currItem in inventory)
        {
            if (currItem != null && currItem.Equals(item))
            {
                quantity += currItem.Quantity;
            }

            if (quantity >= item.Quantity)
            {
                return item.Quantity;
            }
        }

        return quantity;
    }

    public int CheckQuantity(Item item, int amount)
    {
        int quantity = 0;

        foreach (Item currItem in inventory)
        {
            if (currItem != null && currItem.Equals(item))
            {
                quantity += currItem.Quantity;
            }

            if (quantity >= amount)
            {
                return amount;
            }
        }

        return quantity;
    }

    public int CheckQuantity(Predicate<Item> criteria, int amount)
    {
        int quantity = 0;

        foreach (Item item in inventory)
        {
            if (item != null && criteria(item))
            {
                quantity += item.Quantity;
            }
            if (quantity >= amount)
            {
                return amount;
            }
        }
        return quantity;
    }

    public int CapacityFor(Item itemToCheck)
    {
        int capacity = 0;

        foreach (Item item in inventory)
        {
            if (item == null)
            {
                capacity += itemToCheck.StackLimit;
            }
            else if (item.Equals(itemToCheck))
            {
                if (item.Quantity < item.StackLimit)
                    capacity += item.StackLimit - item.Quantity;
            }
        }

        return capacity;
    }

    public int CapacityFor(Predicate<Item> Criteria)
    {
        int capacity = 0;

        foreach (Item item in inventory)
        {
            if (item != null && Criteria(item))
            {
                capacity += item.StackLimit - item.Quantity;
            }
        }
        return capacity;
    }

    public int QuantityOf(Item item)
    {
        return inventory.Where(curr => curr != null && curr.Equals(item)).Sum(curr => curr.Quantity);
    }

    public int QuantityOf(Predicate<Item> Criteria)
    {
        int quantity = 0;

        foreach (Item item in inventory)
        {
            if (item != null && Criteria(item))
            {
                quantity += item.Quantity;
            }
        }
        return quantity;
    }


    public (Sprite, string)[] GetVisualInformations()
    {
        (Sprite sprite, string quantity)[] info = new (Sprite sprite, string quantity)[inventory.Length];
        for (int i = 0; i < inventory.Length; i++)
        {
            info[i].sprite = GetSpriteAt(i);
            if (inventory[i] != null)
            {
                info[i].quantity = inventory[i].Quantity.ToString();
            }
            else
            {
                info[i].quantity = String.Empty;
            }
        }
        return info;
    }

    public Sprite GetSpriteAt(int index)
    {
        if (inventory[index] != null)
        {
            return inventory[index].GetSprite();
        }
        else
        {
            return Utility.GetSprite(typeof(ItemSlot));
        }
    }


    public void FlushInv()
    {
        for (int i = 0; i < inventory.Length; i++)
            inventory[i] = null;
        Refresh();
    }

    public static void PrintInv(Inventory inv)
    {
        foreach (Item item in inv)
            Debug.Log(item == null ? null : item.ToString());
    }

    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            yield return inventory[i];
        }
    }

    public Item this[int index]
    {
        get { return inventory[index]; }
        private set { inventory[index] = value; }
    }
}