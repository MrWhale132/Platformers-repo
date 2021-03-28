using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingArea : MonoBehaviour
{
    [SerializeField]
    BuildingProgressInfoBox infoBoxPrefab;

    BuildingProgressInfoBox infoBox;

    Building buildingToBuild;
    public Building Building
    {
        get { return buildingToBuild; }
        set
        {
            buildingToBuild = value;
            currentProgress = BuildingComponent.Copy(value.ComponentsToBuild);
            SetUpProgress();
        }
    }

    BuildingComponent[] currentProgress;

    public BuildingComponent[] CurrentProgress => currentProgress;

    public bool IsComponentsFullFiled => componentsFullFiled;

    public Platform Platform => platform;

    Platform platform;

    BuildManager bM;

    bool mouseOver;

    int buildTime;

    float remainingTime;

    bool componentsFullFiled;

    List<Builder> builders;


    private void Awake()
    {
        bM = BuildManager.Instance;
        CreateInfoBox();
        builders = new List<Builder>();
        platform = GetComponent<Platform>();
    }

    IEnumerator CountDown()
    {
        Transform t = infoBox.Bar;
        while (remainingTime > 0)
        {
            if (builders.Count > 0)
            {
                remainingTime -= Time.deltaTime * builders.Count;
                if (mouseOver)
                {
                    if (buildTime - remainingTime >= 1)
                        infoBox.SetTimer(Utility.SecondsToDate(Mathf.CeilToInt(remainingTime)));

                    float percent = 1 - (remainingTime / buildTime);
                    if (!Utility.Approximately(t.localScale.x, percent))
                        t.localScale = new Vector3(percent, t.localScale.y);
                }
            }
            yield return null;
        }
        BuildingFinished();
    }

    void BuildingFinished()
    {
        bM.BuildAreaFinished(transform.position, buildingToBuild);
        foreach (var builder in builders)
            builder.BuildingFinished();
        Destroy(gameObject);
    }

    public bool ConnectBuilder(Builder builder)
    {
        if (builders.Contains(builder)) return false;
        builders.Add(builder);
        if (builders.Count == 1) StartCoroutine(CountDown());
        return true;
    }

    public bool DisConnectBuilder(Builder builder)
    {
        if (!builders.Contains(builder)) throw new Exception();
        builders.Remove(builder);
        if (builders.Count == 0) StopAllCoroutines();
        return true;
    }

    void OnMouseEnter()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        mouseOver = true;
        if (infoBox != null && !infoBox.Equals(null))
        {
            if (!componentsFullFiled) RefreshInfoBox();
            infoBox.gameObject.SetActive(true);
        }
    }

    void OnMouseExit()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        mouseOver = false;
        if (infoBox != null && !infoBox.Equals(null))
            infoBox.gameObject.SetActive(false);
    }

    void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, platform));
    }

    public void CallOnMouseUpAsButton()
    {
        OnMouseUpAsButton();
    }

    void ComponentsFullFiled()
    {
        componentsFullFiled = true;
        StopAllCoroutines();
        Destroy(infoBox.gameObject);
        CreateInfoBox();
        if (mouseOver) infoBox.gameObject.SetActive(true);
        infoBox.SetUpTimer(Utility.SecondsToDate(buildTime));
    }

    void CheckProgress()
    {
        if (mouseOver) RefreshInfoBox();
        for (int i = 0; i < currentProgress.Length; i++)
            if (currentProgress[i].Item.Quantity < buildingToBuild.ComponentsToBuild[i].Item.Quantity)
                return;
        ComponentsFullFiled();
    }

    public void AddProgress(BuildingComponent component)
    {
        for (int i = 0; i < currentProgress.Length; i++)
            if (currentProgress[i].Item.Equals(component.Item))
            {
                currentProgress[i].Item.Quantity += component.Item.Quantity;  // No actual quantity loss from the other side (component.Item)
                break;
            }

        CheckProgress();
    }

    public BuildingComponent[] GetRemainingProgress()
    {
        int length = buildingToBuild.ComponentsToBuild.Length;
        BuildingComponent[] goal = buildingToBuild.ComponentsToBuild;
        BuildingComponent[] current = currentProgress;
        BuildingComponent[] difference = new BuildingComponent[length];
        for (int i = 0; i < length; i++)
            difference[i] = goal[i] - current[i];
        return difference;
    }

    void RefreshInfoBox()
    {
        int total = 0;
        int achived = 0;
        for (int i = 0; i < currentProgress.Length; i++)
        {
            int curr = currentProgress[i].Item.Quantity;
            int goal = buildingToBuild.ComponentsToBuild[i].Item.Quantity;
            total += goal;
            achived += curr;

            if (currentProgress[i].Fulfilled) continue;

            if (curr < goal)
                infoBox.Refresh(i, curr, goal);
            else
            {
                infoBox.RemoveAt(i);
                currentProgress[i].Fulfilled = true;
            }
        }
        StopAllCoroutines();
        StartCoroutine(AnimateProgressBar((float)achived / total));
    }

    IEnumerator AnimateProgressBar(float to)
    {
        Transform t = infoBox.Bar;
        while (t.localScale.x < to)
        {
            t.localScale += new Vector3(Time.deltaTime, 0, 0);
            yield return null;
        }
    }

    void SetUpProgress()
    {
        for (int i = 0; i < currentProgress.Length; i++)
        {
            var curr = currentProgress[i];
            var goal = buildingToBuild.ComponentsToBuild[i];
            curr.Item.Quantity = 0;
            if (goal.Item.GetType() == typeof(Wood)) curr.Sprite = Utility.GetSprite(typeof(Wood));
            else if (goal.Item.GetType() == typeof(Stone)) curr.Sprite = Utility.GetSprite(typeof(Stone));
            infoBox.AddRecord(curr.Sprite, 0, goal.Item.Quantity);
        }
        buildTime = buildingToBuild.GetBuildingTimeInSeconds();
        remainingTime = buildTime;
    }



    void CreateInfoBox()
    {
        infoBox = Instantiate(infoBoxPrefab);
        infoBox.transform.position = transform.position + new Vector3(0, 1, 0) * 5;
        infoBox.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (infoBox != null && !infoBox.Equals(null))
            Destroy(infoBox.gameObject);
    }

    void ReceiveMessage(object from)
    {
        if (from as Platform == platform)
        {
            OnMouseUpAsButton();
        }
    }
}