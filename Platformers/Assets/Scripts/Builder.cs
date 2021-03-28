using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Builder : Worker
{
    #region Fields

    bool isBuilding;

    BuildingArea buildArea;

    #endregion

    #region Initializing

    protected override void Start()
    {
        base.Start();
    }

    #endregion


    public override void HandleMouseMessage(MouseMessage msg)
    {
        if (msg.Platform is BuildingAreaPlatform baP && !interacting)
        {
            UnSubscribe();
            MoveToBuildArea(baP);
        }
        else
        {
            base.HandleMouseMessage(msg);
        }
    }


    #region Building

    public void MoveToBuildArea(BuildingAreaPlatform platform)
    {
        StartCoroutine(MoveToBuildingArea(platform));
    }

    IEnumerator MoveToBuildingArea(BuildingAreaPlatform platform)
    {
        if (buildArea != platform.BuildingArea)
        {
            List<Platform> path;
            MoveOn(platform, MoveMode.Interacting, out path);
            if (path == null) yield break;
            DisconnectFromBuildingArea();
            yield return new WaitWhile(() => state == States.Move);

            var bArea = platform.BuildingArea;
            if (bArea.IsComponentsFullFiled) // there is no check if the building is already have built.
                ConnectArea(bArea);
            else
                AddprogressToBuildArea(bArea);
        }
        else
            print("You are already working on this BuildingArea.");
    }

    void AddprogressToBuildArea(BuildingArea bArea)
    {
        BuildingComponent[] remainingProgress = bArea.GetRemainingProgress();
        for (int i = 0; i < remainingProgress.Length; i++)
        {
            Item c = Export(remainingProgress[i].Item, remainingProgress[i].Item.Quantity);
            if (c != null)
                bArea.AddProgress((BuildingComponent)c);
        }
        if (bArea.IsComponentsFullFiled)
            ConnectArea(bArea);
    }

    public void BuildingFinished()
    {
        isBuilding = false;
        buildArea = null;
    }

    void ConnectArea(BuildingArea to)
    {
        buildArea = to;
        isBuilding = true;
        buildArea = to;
        to.ConnectBuilder(this);
    }

    protected void DisconnectFromBuildingArea()
    {
        if (isBuilding)
        {
            isBuilding = false;
            buildArea.DisConnectBuilder(this);
            buildArea = null;
        }
    }

    #endregion

    protected override IEnumerator Move(Platform[] path)
    {
        DisconnectFromBuildingArea();
        return base.Move(path);
    }
}