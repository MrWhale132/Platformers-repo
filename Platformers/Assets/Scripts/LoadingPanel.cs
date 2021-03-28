using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class LoadingPanel : Panel
{
    [SerializeField]
    Transform inv1Holder;
    [SerializeField]
    Transform inv2Holder;
    [SerializeField]
    Transform diffHolder;
    [SerializeField]
    Text messagePrefab;

    ItemSlot[] inv1Slots;
    ItemSlot[] inv2Slots;
    ItemSlot[] diffSlots;

    [SerializeField]
    Image[] buttonsColors;

    Inventory inv1;
    Inventory inv2;
    Inventory diff;

    Queue<Text> messageQueue;

    int currDisplayedSlots = 1;

    int clickMultiplier = 1;

    bool max;

    TradeFlow tradeFlow;


    void Awake()
    {
        messageQueue = new Queue<Text>();

        ItemSlot[] itemSlots = GetComponentsInChildren<ItemSlot>(true);
        inv1Slots = new ItemSlot[5];
        inv2Slots = new ItemSlot[5];
        diffSlots = new ItemSlot[5];
        for (int i = 0; i < 5; i++)
        {
            inv1Slots[i] = itemSlots[i];
            diffSlots[i] = itemSlots[i + 5];
            inv2Slots[i] = itemSlots[i + 10];
        }

        diff = new Inventory(5);
        diff.InventoryHolder = diffHolder;
        diff.ItemSlots = diffSlots;
        for (int i = 1; i < 5; i++) diff.ItemSlots[i].gameObject.SetActive(false);
    }

    public override void Enable(params object[] values)
    {
        gameObject.SetActive(true);
        Activate((Inventory)values[0], (Inventory)values[1], (TradeFlow)values[2]);
    }
    public void Activate(Inventory inventory1, Inventory inventory2, TradeFlow tradeFlow)
    {
        inv1 = inventory1;
        inv2 = inventory2;
        this.tradeFlow = tradeFlow;
        inv1.InventoryHolder = inv1Holder;
        inv2.InventoryHolder = inv2Holder;
        inv1.ItemSlots = inv1Slots;
        inv2.ItemSlots = inv2Slots;

        inv1.Display(true);
        inv2.Display(true);
        diff.Display(true);
        for (int i = 0; i < 5; i++) inv1Slots[i].gameObject.SetActive(i < inv1.Size);
        for (int i = 0; i < 5; i++) inv2Slots[i].gameObject.SetActive(i < inv2.Size);
        diff.ItemSlots[0].gameObject.SetActive(true);
        HighLight(0);
    }

    void MoveItmesAt(int i, int amount)
    {
        int inv2Space = inv2.CheckCapacity(inv1[i], amount);

        if (inv2Space > 0)
        {
            Item c = inv1.GetItemAt(i, amount);
            diff.AddItem(Item.Clone(c));
            inv2.AddItem(c);
            int length = diff.Size;
            for (int j = 0; j < diff.Size; j++) if (diff[j] == null) length--;
            if (length > currDisplayedSlots)
            {
                for (int j = currDisplayedSlots + 1; j < length + 1; j++)
                {
                    diff.ItemSlots[j - 1].gameObject.SetActive(true);
                }
                currDisplayedSlots = length;
            }
        }
        else
        {
            string message;
            if (tradeFlow == TradeFlow.Export)
                message = "The Warehouse is full.";
            else
                message = "Your inventory is full.";
            StartFloatMessage(message);
        }
    }

    public void Inv1Buttons(int i)
    {
        if (inv1[i] != null)
        {
            int amount = max ? inv1[i].Quantity : clickMultiplier;
            MoveItmesAt(i, amount);
        }
    }

    public void All()
    {
        int i = 0;
        while (!inv2.IsFull && i < inv1.Size)
        {
            if (inv1[i] != null)
                MoveItmesAt(i, inv1[i].Quantity);
            i++;
        }
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

    public void TradeFinished()
    {
        SendMessage(this, new TradeMessage(diff.Clone(), tradeFlow));
        Disable();
    }

    public void TradeCanceld()
    {
        SendMessage(this, new TradeMessage(null, tradeFlow));
        Disable();
    }

    public override void Disable()
    {
        gameObject.SetActive(false);
        diff.FlushInv();
        for (int i = 1; i < 5; i++) diff.ItemSlots[i].gameObject.SetActive(false);
        currDisplayedSlots = 1;
        clickMultiplier = 1;
        max = false;
    }

    public void HighLight(int index)
    {
        for (int i = 0; i < buttonsColors.Length; i++)
            buttonsColors[i].color = Color.white;
        buttonsColors[index].color = Color.yellow;
    }

    void StartFloatMessage(string text)
    {
        StartCoroutine(FloatMessage(text));
    }

    System.Collections.IEnumerator FloatMessage(string text)
    {
        Transform parent = GameObject.FindGameObjectWithTag("Canvas").transform;
        Text message = Instantiate(messagePrefab, parent);
        messageQueue.Enqueue(message);
        message.transform.position = messagePrefab.transform.position;
        message.text = text;
        Color originalColor = message.color;
        float time = 0;
        while (time < 2)
        {
            time += Time.deltaTime;
            message.transform.position += Vector3.up * Time.deltaTime;
            message.color = Color.Lerp(originalColor, Color.clear, time / 6);
            yield return null;
        }
        Destroy(message.gameObject);
        messageQueue.Dequeue();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        foreach (var message in messageQueue)
        {
            Destroy(message.gameObject);
        }
        messageQueue.Clear();
    }
}