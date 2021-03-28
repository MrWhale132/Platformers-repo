using System;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    event System.Action BuildingSelected;

    enum BuildMode { BuildArea = 0, Building = 1 }
    BuildMode buildMode;

    static BuildManager instance;

    [SerializeField]
    Building[] prefabs;

    Building buildingToBuild;

    [SerializeField]
    Color platformColorWhenConstructing;

    bool constructing;

    [SerializeField]
    Platform standardP;

    [SerializeField]
    BuildingAreaPlatform bAPPrefab;

    [SerializeField]
    Transform map;

    public static BuildManager Instance => instance;

    public static bool IsConstructing => instance.constructing;

    public Color PlatformColorWhenConstructing => platformColorWhenConstructing;


    private void Start()
    {
        if (instance != null)
        {
            Debug.LogError("More than one BuildManager instance was created!");
            return;
        }
        instance = this;
    }

    public void SetBuildingToBuild(Building building)
    {
        buildingToBuild = building;
        buildMode = BuildMode.Building;
        ConstructionModeOn();
    }

    public void SetBuildingToBuild(Type buildingType)
    {
        buildingToBuild = GetBuilding(buildingType);
        buildMode = BuildMode.Building;
        ConstructionModeOn();
    }

    public void PlaceBuildArea(Type buildingType)
    {
        buildingToBuild = GetBuilding(buildingType);
        buildMode = BuildMode.BuildArea;
        ConstructionModeOn();
    }

    public void BuildAreaFinished(Vector3 at, Building with)
    {
        Platform platform = Instantiate(standardP, at, Quaternion.identity);
        platform.transform.parent = map.Find("Generated Map");
        MapGenerator.Instance.ReplacePlatformAt(platform.Coord, platform);
        Instantiate(with, at + with.OffsetFromGround, with.transform.rotation);
    }

    void HandleMouseMessage(MouseMessage msg)
    {
        Platform platform = msg.Platform;
        if (platform != null && platform.objAtPlatform == null)
        {
            if (buildMode == BuildMode.BuildArea)
            {
                BuildingAreaPlatform bAP = Instantiate(bAPPrefab, platform.Vector_3, Quaternion.identity);
                bAP.transform.parent = map.Find("Generated Map");
                bAP.BuildingArea.Building = buildingToBuild;
                MapGenerator.Instance.ReplacePlatformAt(platform.Coord, bAP);
                Destroy(platform.gameObject);
            }
            else
            {
                Building building = Instantiate(buildingToBuild, platform.Vector_3 + buildingToBuild.OffsetFromGround, Quaternion.identity);
                platform.objAtPlatform = building;
                platform.walkable = false;
                print("You have built a " + building.GetType());
            }
        }
        else
        {
            print("The buildingprocess was unsuccessful.");
        }
        MouseManager.MouseClicked -= HandleMouseMessage;
        constructing = false;
        buildingToBuild = null;
    }

    Building GetBuilding(Type buildingType)
    {
        foreach (Building building in prefabs)
        {
            if (building.GetType() == buildingType) return building;
        }
        throw new Exception();
    }

    void ConstructionModeOn()
    {
        if (!constructing)
            MouseManager.MouseClicked += HandleMouseMessage;
        constructing = true;
        BuildingSelected?.Invoke();
    }
}
