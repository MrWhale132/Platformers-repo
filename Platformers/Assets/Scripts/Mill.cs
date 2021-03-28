using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using System.Linq;

public class Mill : Building, IInteractable, ITrade
{
    [SerializeField]
    MillPanel millPanelPrefab;

    MillPanel millPanel;

    List<MillProcessLine> processes;

    MillProcessLine selectedProcess;


    public Inventory Inventory { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


    void Awake()
    {
        processes = new List<MillProcessLine>();
    }

    protected override void Start()
    {
        base.Start();
        Transform parent = GameObject.FindGameObjectWithTag("Canvas").transform;
        millPanel = Instantiate(millPanelPrefab, parent);
        millPanel.gameObject.SetActive(false);
        AddProcessLine();
        SelectProcessLine(processes[0]);
        millPanel.AddListener(MillPanelMessageHandler);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AddProcessLine();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            RemoveProcessLine(processes.Last());
        }
    }

    protected override void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (!GameManager.ValidSelection()) goto Skip;

        if (!millPanel.gameObject.activeSelf)
        {
            millPanel.InjectItemDatas(processes);
            millPanel.Display(true);
        }

    Skip:

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, platform));
    }


    public void CheckOutTradeInfos(Inventory clientInv, MillTradeInfo[] tradeInfos)
    {
        Inventory source = null;
        Inventory target = null;
        foreach (var info in tradeInfos)
        {
            switch (info.Source)
            {
                case MillPanel.Inventorys.Client:
                    source = clientInv; break;
                case MillPanel.Inventorys.Grind:
                    var process = processes.Single(item => item.Id == info.ID);
                    source = process.GrindInventory;
                    if (millPanel.gameObject.activeSelf)
                        millPanel.GetProcessByID(info.ID).GrindInventory.GetItem(Item.Clone(info.Item));
                    break;
                case MillPanel.Inventorys.Product:
                    process = processes.Single(item => item.Id == info.ID);
                    source = process.ProductInventory;
                    if (millPanel.gameObject.activeSelf)
                        millPanel.GetProcessByID(info.ID).ProductInventory.GetItem(Item.Clone(info.Item));
                    break;
                default: break;
            }
            switch (info.Target)
            {
                case MillPanel.Inventorys.Client:
                    target = clientInv; break;
                case MillPanel.Inventorys.Grind:
                    var process = processes.Single(item => item.Id == info.ID);
                    target = process.GrindInventory;
                    if (millPanel.gameObject.activeSelf)
                        millPanel.GetProcessByID(info.ID).GrindInventory.AddItem(Item.Clone(info.Item));
                    break;
                case MillPanel.Inventorys.Product:
                    process = processes.Single(item => item.Id == info.ID);
                    target = process.ProductInventory;
                    break;
                default: break;
            }
            int capacity = target.CheckCapacity(info.Item);
            if (capacity > 0)
            {
                int quantity = source.CheckQuantity(info.Item);
                if (quantity > 0)
                {
                    Item item = source.GetItem(info.Item);
                    target.AddItem(item);
                }
            }
        }
    }

    void SelectProcessLine(MillProcessLine processLine)
    {
        selectedProcess = processLine;
        millPanel.ProcessLineSelected(processLine.Id);
    }

    void AddProcessLine()
    {
        var newProcess = new MillProcessLine();
        millPanel.SetUpProcessLine(newProcess);
        processes.Add(newProcess);
    }

    void RemoveProcessLine(MillProcessLine processLine)
    {
        processes.Remove(processLine);
        millPanel.RemoveProcessLine(processLine);
        if (processLine.Id == selectedProcess.Id) selectedProcess = null;
    }


    public void InteractedBy(object with)
    {
        millPanel.InjectItemDatas(processes);
        millPanel.Display(true);
    }

    public MillPanel.Permissions.Client RequestClientPermission()
    {
        return millPanel.GetClientPermission();
    }


    public void OnValueChanged(Dropdown dropdown)
    {
        print(dropdown.value);
    }


    public void CancelInteractionWith(object with)
    {
        millPanel.gameObject.SetActive(false);
    }


    public void MillPanelMessageHandler(object sender, EventArgs message)
    {
        var msg = message as MillPanelMessage;
        if (msg.Pressed == MillPanelMessage.Buttons.Exit)
        {
            MillPanelExit();
        }
        else if (msg.Pressed == MillPanelMessage.Buttons.Selection)
        {
            SelectProcessLine(msg.SelectedProcess);
        }
        else if (msg.Pressed == MillPanelMessage.Buttons.Command)
        {

        }
    }


    void MillPanelExit()
    {

    }

    public void Import(Item item)
    {
        throw new System.NotImplementedException();
    }

    public Item Export(Item item, int amount)
    {
        throw new System.NotImplementedException();
    }

    public int PrepareImport(Item item, int amount)
    {
        throw new System.NotImplementedException();
    }
}
