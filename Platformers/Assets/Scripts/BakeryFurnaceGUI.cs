using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BakeryFurnaceGUI : MonoBehaviour
{
    event Action OnSelection;
    event Action OnBake;
    event Action OnPause;
    event Action OnStop;


    [SerializeField]
    Transform bakeablesHolder;
    [SerializeField]
    Transform burnablesHolder;
    [SerializeField]
    Transform productsHolder;
    [SerializeField]
    Transform totalBar;
    [SerializeField]
    Transform currentBar;
    [SerializeField]
    Transform burnTimeBar;
    [SerializeField]
    Text totalTime;
    [SerializeField]
    Text currentText;
    [SerializeField]
    GameObject pauseBtn;
    [SerializeField]
    GameObject stopBtn;
    [SerializeField]
    Text messagePrefab;


    ItemSlot[] bakeablesSlots;
    ItemSlot[] burnablesSlots;
    ItemSlot[] productsSlots;


    [SerializeField]
    ItemSlot itemSlotPrefab;

    Queue<Text> messageQueue;


    void Awake()
    {
        messageQueue = new Queue<Text>();

        bakeablesSlots = bakeablesHolder.GetComponentsInChildren<ItemSlot>();
        burnablesSlots = burnablesHolder.GetComponentsInChildren<ItemSlot>();
        productsSlots = productsHolder.GetComponentsInChildren<ItemSlot>();
    }


    public void AddController(BakeryFurnace furnace)
    {
        Inventory bakeables = furnace.GetBakeablesInventory();
        Inventory burnables = furnace.GetBurnablesInventory();
        Inventory products = furnace.GetProductsInventory();

        bakeables.SetGUI(bakeablesHolder, bakeablesSlots);
        burnables.SetGUI(burnablesHolder, burnablesSlots);
        products.SetGUI(productsHolder, productsSlots);

        OnBake += furnace.Start;
        OnPause += furnace.Pause;
        OnStop += furnace.Stop;
    }

    public void AddController(BakeryFurnaceDesigner designer)
    {
        Inventory bakeables = designer.GetBakeablesInventory();
        Inventory burnables = designer.GetBurnablesInventory();
        Inventory products = designer.GetProductsInventory();

        bakeables.SetGUI(bakeablesHolder, bakeablesSlots);
        burnables.SetGUI(burnablesHolder, burnablesSlots);
        products.SetGUI(productsHolder, productsSlots);

        OnSelection += designer.OnSelection;
    }

    public void AddBakeableSlot(int times = 1)
    {
        int originalLength = bakeablesSlots.Length;
        Array.Resize(ref bakeablesSlots, originalLength + times);
        for (int i = 0; i < times; i++)
        {
            var slot = Instantiate(itemSlotPrefab, bakeablesHolder);
            bakeablesSlots[originalLength + i] = slot;
        }
    }


    public void SetCurrentProgress(float percent)
    {
        Vector3 scale = currentBar.localScale;
        currentBar.localScale = new Vector3(percent, scale.y, scale.z);
    }

    public void SetCurrentTime(float time)
    {
        string timeFormat = Utility.SecondsToDate(Mathf.CeilToInt(time));
        currentText.text = timeFormat;
    }

    public void SetTotalTime(float time)
    {
        string timeFormat = Utility.SecondsToDate(Mathf.CeilToInt(time));
        totalTime.text = timeFormat;
    }

    public void SetTotalProgress(float percent)
    {
        Vector3 scale = totalBar.localScale;
        totalBar.localScale = new Vector3(percent, scale.y, scale.z);
    }

    public void SetBurnProgress(float percent)
    {
        Vector3 scale = burnTimeBar.localScale;
        burnTimeBar.localScale = new Vector3(percent, scale.y, scale.z);
    }

    public void SetActivePause(bool value)
    {
        pauseBtn.SetActive(value);  
    }

    public void SetActiveStop(bool value)
    {
        stopBtn.SetActive(value);
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
            message.color = Color.Lerp(originalColor, Color.clear, time / 4);
            yield return null;
        }
        Destroy(message.gameObject);
        messageQueue.Dequeue();
    }

    public void Select()
    {

    }

    public void Deselect()
    {

    }

    public void Bake()
    {
        OnBake();
    }

    public void Pause()
    {
        OnPause();
    }

    public void Stop()
    {
        OnStop();
    }

    public void InvokeOnSelection()
    {
        OnSelection();
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