using System;
using UnityEngine;

public class BuildMenu : MonoBehaviour
{
    [SerializeField]
    GameObject menuHolder;

    BuildManager bM;


    private void Start()
    {
        bM = BuildManager.Instance;
    }

    public void OnMenuClick()
    {
        if (menuHolder.activeSelf) menuHolder.SetActive(false);
        else menuHolder.SetActive(true);
        MouseManager.InvokeGUIElemntClicked(this, new BuildMenuEventArgs(typeof(BuildMenu)));
    }

    public void SelectWareHouse()
    {
        CommonThings(typeof(WareHouse));
    }

    public void SelectMill()
    {
        CommonThings(typeof(Mill));
    }

    public void SelectBakery()
    {
        CommonThings(typeof(Bakery));
    }

    public void SelectBarack()
    {
        menuHolder.SetActive(false);
    }

    void CommonThings(Type type)
    {
        menuHolder.SetActive(false);
        bM.PlaceBuildArea(type);
        MouseManager.InvokeGUIElemntClicked(this, new BuildMenuEventArgs(type));
    }
}

public class BuildMenuEventArgs : EventArgs
{
    Type buildingType;
    public Type BuildingType { get { return buildingType; } }
    public BuildMenuEventArgs(Type buildingType)
    {
        this.buildingType = buildingType;
    }
}
