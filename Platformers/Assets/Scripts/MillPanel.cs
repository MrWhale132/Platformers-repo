using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class MillPanel : Panel
{
    public enum Inventorys { Client = 0, Grind = 1, Product = 2 }

    [SerializeField]
    Transform clientInvHolder;
    ItemSlot[] clientItemSlots;
    Inventory clientInv;

    [SerializeField]
    ItemSlot itemSlotPrefab;
    [SerializeField]
    Text messagePrefab;

    [SerializeField]
    Transform processParent;

    [SerializeField]
    MillPanelProcessLine millProcessLinePrefab;

    List<MillProcessLine> processes;

    List<int> inUseIds;

    MillProcessLine selectedProcess;

    List<MillTradeInfo> tradeInfos;

    Queue<Text> messageQueue;


    void Awake()
    {
        inUseIds = new List<int>();
        messageQueue = new Queue<Text>();

        CreateClientItemSlots();
        clientInv = new Inventory(6);
        clientInv.InventoryHolder = clientInvHolder;
        clientInv.ItemSlots = clientItemSlots;
        clientInv.AddClickListener(ClientItemSlotClicked);

        processes = new List<MillProcessLine>();
        tradeInfos = new List<MillTradeInfo>();
    }

    void CreateClientItemSlots()
    {
        clientItemSlots = new ItemSlot[6];
        for (int i = 0; i < 6; i++)
        {
            var slot = Instantiate(itemSlotPrefab, clientInvHolder);
            clientItemSlots[i] = slot;
        }
    }

    void SetClientInv(Item[] statistics)
    {
        clientInv.SetInventoryStatistics(statistics);
    }


    public void RemoveProcessLine(MillProcessLine processLine)
    {
        processLine.DestroyGUI();
        processes.Remove(processLine);
        if (processLine.Id == selectedProcess.Id) selectedProcess = null;
    }


    public void Command()
    {
        SendMessage(this, new MillPanelMessage(MillPanelMessage.Buttons.Command, tradeInfos.ToArray()));
        gameObject.SetActive(false);
        tradeInfos.Clear();
    }

    public void Exit()
    {
        gameObject.SetActive(false);
        SendMessage(this, new MillPanelMessage(MillPanelMessage.Buttons.Exit));
        tradeInfos.Clear();
    }

    public void ProcessLineSelected(int id)
    {
        if (selectedProcess != null)
            selectedProcess.DeselectGUI();
        selectedProcess = processes.Single(process => process.Id == id);
        selectedProcess.SelectGUI();
    }

    public Permissions.Client GetClientPermission()
    {
        return new Permissions.Client(SetClientInv, AddListener, RemoveListener);
    }

    void OnSelectionHandler(MillProcessLine processLine)
    {
        SendMessage(this, new MillPanelMessage(processLine));
    }


    public void SetUpProcessLine(MillProcessLine processLine)
    {
        var newProcess = new MillProcessLine();   // to display the changes made on tha panel but not to modify a mill's actual process

        var newProcessGUI = Instantiate(millProcessLinePrefab, processParent);
        newProcess.SetUpGUIForPanel(newProcessGUI);   // must be fist
        processLine.SetUpGUIForMill(newProcessGUI);
        int id = GetProcessId();
        inUseIds.Add(id);
        processLine.SetId(id);
        newProcess.SetId(id);
        processLine.GivePermission(new Permissions.ProcessLine(OnSelectionHandler, GetClientInventory, StartFloatMessage));
        newProcess.GivePermission(new Permissions.ProcessLine(OnSelectionHandler, GetClientInventory, StartFloatMessage));

        newProcess.AddTradeFlowListener(AddItemTradeInfo);
        processLine.AddTradeFlowListener(AddItemTradeInfo);
        processes.Add(newProcess);
    }

    int GetProcessId()
    {
        int id;
        do
            id = UnityEngine.Random.Range(0, int.MaxValue);
        while (inUseIds.Contains(id));
        return id;
    }

    public void ExtendGrindables(int processIndex)
    {
        var process = processes[processIndex];
        process.AddGrindSlot();
    }

    public void ExtendProducts(int processIndex)
    {
        var process = processes[processIndex];
        process.AddProductSlot();
    }

    void ClientItemSlotClicked(int at)
    {
        if (selectedProcess.IsGrinding)  // no null-check --> always have one selected process
        {
            print("You can not edit an on-going process!");
            return;
        }

        if (clientInv[at] is IGrindable)
        {
            int capacity = selectedProcess.GrindInventory.CheckCapacity(clientInv[at], 1);
            if (capacity > 0)
            {
                Item item = clientInv.GetItemAt(at, 1);
                AddItemTradeInfo(new MillTradeInfo(selectedProcess.Id, Item.Clone(item), Inventorys.Client, Inventorys.Grind));
                Inventory grindInv = selectedProcess.GrindInventory;
                grindInv.AddItem(item);
            }
        }
    }

    public void AddItemTradeInfo(MillTradeInfo tradeInfo)
    {
        if (tradeInfo.Source == Inventorys.Grind && tradeInfo.Target == Inventorys.Product)
        {
            var process = processes.Single(curr => curr.Id == tradeInfo.ID);
            process.ProductInventory.AddItem(tradeInfo.Item);
        }
        else if (tradeInfo.Source == Inventorys.Grind && tradeInfo.Target == Inventorys.Grind)
        {
            var process = processes.Single(curr => curr.Id == tradeInfo.ID);
            process.GrindInventory.GetItem(tradeInfo.Item);
        }
        else tradeInfos.Add(tradeInfo);
    }

    Inventory GetClientInventory()
    {
        return clientInv;
    }

    public MillProcessLine GetProcessByID(int id)
    {
        return processes.Single(item => item.Id == id);
    }

    public void Display(bool value)
    {
        gameObject.SetActive(value);
        clientInv.FlushInv();
    }

    public void InjectItemDatas(List<MillProcessLine> processLines)
    {
        for (int i = 0; i < processLines.Count; i++)
        {
            processes[i].GrindInventory.SetInventoryStatistics(processLines[i].GrindInventory.GetInventoryStatistics());
            processes[i].ProductInventory.SetInventoryStatistics(processLines[i].ProductInventory.GetInventoryStatistics());
        }
    }

    public void StartFloatMessage(string message)
    {
        StartCoroutine(FloatMessage(message));
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
            message.color = Color.Lerp(originalColor, Color.clear, time / 3);
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


    public class Permissions
    {
        public class Client
        {
            Action<Item[]> setClientInv;
            Action<EventHandler> addListener;
            Action<EventHandler> removeListener;

            public Client(Action<Item[]> setter, Action<EventHandler> listenerAdder, Action<EventHandler> listenerRemover)
            {
                setClientInv = setter;
                addListener = listenerAdder;
                removeListener = listenerRemover;
            }

            public void SetInventory(Item[] statistics)
            {
                setClientInv(statistics);
            }

            public void AddListener(EventHandler call)
            {
                addListener(call);
            }

            public void RemoveListener(EventHandler call)
            {
                removeListener(call);
            }
        }

        public class ProcessLine
        {
            Action<MillProcessLine> findBy;
            Func<Inventory> getClientInv;
            Action<string> startFloatMessage;

            public ProcessLine(Action<MillProcessLine> selector, Func<Inventory> retreviewer, Action<string> starter)
            {
                findBy = selector;
                getClientInv = retreviewer;
                startFloatMessage = starter;
            }

            public void Selected(MillProcessLine processLine)
            {
                findBy(processLine);
            }

            public Inventory GetClientInventory()
            {
                return getClientInv();
            }

            public void StartFloatMessage(string message)
            {
                startFloatMessage(message);
            }
        }
    }
}

public class MillPanelMessage : EventArgs
{
    public enum Buttons { Exit = 0, Selection = 1, Command = 2 }
    Buttons pressed;

    MillProcessLine selectedProcess;
    MillTradeInfo[] tradeInfos;

    public MillPanelMessage(Buttons pressed)
    {
        this.pressed = pressed;
    }

    public MillPanelMessage(MillProcessLine processLine)
    {
        selectedProcess = processLine;
        pressed = Buttons.Selection;
    }

    public MillPanelMessage(Buttons pressed, MillTradeInfo[] tradeInfos)
    {
        this.pressed = pressed;
        this.tradeInfos = tradeInfos;
    }

    public Buttons Pressed => pressed;
    public MillProcessLine SelectedProcess => selectedProcess;
    public MillTradeInfo[] TradeInfos => tradeInfos;
}

public class MillTradeInfo
{
    int id;
    Item item;
    MillPanel.Inventorys source;
    MillPanel.Inventorys target;


    public MillTradeInfo(Item item, MillPanel.Inventorys source, MillPanel.Inventorys target)
    {
        this.item = item;
        this.source = source;
        this.target = target;
    }

    public MillTradeInfo(int id, Item item, MillPanel.Inventorys source, MillPanel.Inventorys target)
    {
        this.id = id;
        this.item = item;
        this.source = source;
        this.target = target;
    }

    public int ID => id;
    public Item Item => item;
    public MillPanel.Inventorys Source => source;
    public MillPanel.Inventorys Target => target;
}