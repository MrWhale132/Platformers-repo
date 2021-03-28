using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Platform platformPrefab;

    static MapGenerator instance;

    public Vector2Int currentMapSize;
    public float platformScale;
    public float gapBetweenPlatforms;
    public float gridCellSize;

    Platform[,] platformMap;

    public static MapGenerator Instance { get { return instance; } }

    public float PlatformScale => platformScale;


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There is more than one MapGeneraot instance!");
        }

        instance = this;

        gridCellSize = platformScale + gapBetweenPlatforms;
        GenerateMap();
    }

    public void GenerateMap()
    {
        string holderName = "Generated Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        platformMap = new Platform[currentMapSize.y, currentMapSize.x];

        for (int y = 0; y < currentMapSize.y; y++)
        {
            for (int x = 0; x < currentMapSize.x; x++)
            {
                Vector3 platfromPosition = GetPositionFromCoord(x, y);
                Platform newPlatform = Instantiate(platformPrefab, platfromPosition, Quaternion.identity);
                newPlatform.transform.parent = mapHolder;
                platformMap[y, x] = newPlatform;
                newPlatform.name = y.ToString() + " , " + x.ToString();
            }
        }
    }


    public void ReplacePlatformAt(Vector2Int coord, Platform platform)
    {
        platformMap[coord.y, coord.x] = platform;
    }

    public static Platform GetPlatformFromCoord(Vector2Int coord)
    {
        return instance.platformMap[coord.y, coord.x];
    }

    public static Platform GetPlatformFromCoord(int x, int y)
    {
        return instance.platformMap[y, x];
    }

    public static Platform GetPlatformFromPosition(Vector3 position)
    {
        return instance.platformMap[Mathf.RoundToInt(position.z / instance.gridCellSize), Mathf.RoundToInt(position.x / instance.gridCellSize)];
    }

    public static Vector3 GetPositionFromCoord(Vector2Int coord)
    {
        return new Vector3(instance.gridCellSize * coord.x, 0, instance.gridCellSize * coord.y);
    }

    public static Vector3 GetPositionFromCoord(int x, int y)
    {
        return new Vector3(instance.gridCellSize * x, 0, instance.gridCellSize * y);
    }

    public static Vector2Int GetCoordFromPosition(Vector3 position)
    {
        return new Vector2Int(Mathf.RoundToInt(position.x / instance.gridCellSize), Mathf.RoundToInt(position.z / instance.gridCellSize));
    }
}
