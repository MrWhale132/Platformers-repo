using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MillPanelProcessLine : MonoBehaviour
{
    event Action OnSelection;
    event Action StopGrindingClicked;
    event Action GrindingClicked;

    [SerializeField]
    GameObject backGround;
    [SerializeField]
    Transform grindablesHolder;
    [SerializeField]
    Transform productsHolder;
    [SerializeField]
    ItemSlot itemSlotPrefab;
    [SerializeField]
    Transform totalBar;
    [SerializeField]
    Transform currentBar;
    [SerializeField]
    Text totalTime;
    [SerializeField]
    Text currentText;
    [SerializeField]
    GameObject appointButton;

    List<ItemSlot> grindableSlots = new List<ItemSlot>();
    List<ItemSlot> productSlots = new List<ItemSlot>();

    bool active;

    public int GrindSlotsCount => grindableSlots.Count;
    public int ProductSlotsCount => productSlots.Count;
    public bool IsActive => active;


    public void AddGrindSlot(int times = 1)
    {
        for (int i = 0; i < times; i++)
        {
            var slot = Instantiate(itemSlotPrefab, grindablesHolder);
            grindableSlots.Add(slot);
        }
    }

    public void AddProductSlot(int times = 1)
    {
        for (int i = 0; i < times; i++)
        {
            var slot = Instantiate(itemSlotPrefab, productsHolder);
            productSlots.Add(slot);
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


    public Transform GetGrindHolder()
    {
        return grindablesHolder;
    }

    public Transform GetProductHolder()
    {
        return productsHolder;
    }

    public ItemSlot[] GetGrindItemSltos()
    {
        return grindableSlots.ToArray();
    }

    public ItemSlot[] GetProductSlots()
    {
        return productSlots.ToArray();
    }


    public void Deselect()
    {
        appointButton.SetActive(true);
        backGround.SetActive(false);
    }

    public void SelectButton()   // for the button
    {
        OnSelection.Invoke();
    }

    public void Select()   // for out side control
    {
        appointButton.SetActive(false);
        backGround.SetActive(true);
    }

    public void StopGrinding()
    {
        StopGrindingClicked();
    }

    public void StartGrinding()
    {
        GrindingClicked();
    }

    public void AddStopGrindingListener(Action call)
    {
        StopGrindingClicked += call;
    }

    public void AddStartGrindingListener(Action call)
    {
        GrindingClicked += call;
    }

    public void AddOnSelectionListener(Action listener)
    {
        OnSelection += listener;
    }

    void OnEnable()
    {
        active = true;    
    }

    void OnDisable()
    {
        active = false;     
    }
}