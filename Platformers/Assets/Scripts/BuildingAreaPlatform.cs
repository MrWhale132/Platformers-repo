using UnityEngine;


[RequireComponent(typeof(BuildingArea))]
public class BuildingAreaPlatform : Platform
{
    BuildingArea buildingArea;

    public BuildingArea BuildingArea => buildingArea;


    protected override void Awake()
    {
        base.Awake();
        buildingArea = GetComponent<BuildingArea>();
        objAtPlatform = buildingArea;
        walkable = false;
    }
}
