using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Utility : MonoBehaviour
{
    static Utility instance;

    public static Utility Instance { get { return instance; } }

    static MapGenerator mapGen;

    public float moveSpeed;

    public Sprite[] icons;

    Dictionary<Type, Sprite> sprites;

    public ItemSlot itemSlotPrefab;
    public Transform parent;

    const int ApproximationAccuracy = 3;


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("There is more than one Utility instance.");
            return;
        }
        mapGen = FindObjectOfType<MapGenerator>();

        sprites = new Dictionary<Type, Sprite>
        {
            { typeof(ItemSlot), icons[0] },
            { typeof(Wood), icons[1] },
            { typeof(Stone), icons[2] },
            { typeof(WheatSeed), icons[3] },
            { typeof(WheatFlour), icons[4] },
            { typeof(Wheat), icons[5] },
            { typeof(Coal), icons[6] },
            { typeof(Loaf), icons[7] }
        };

        instance = this;
    }


    public static void PrintArray(IEnumerable array)
    {
        foreach (var item in array)
        {
            print(item);
        }
    }

    public static string SecondsToDate(int _seconds)
    {
        int hours = _seconds / 3600;
        int minutes = _seconds / 60 - hours * 60;
        int seconds = _seconds % 60;
        if (hours > 0) return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        if (minutes > 0) return string.Format("{0:00}:{1:00}", minutes, seconds);
        if (seconds > 9) return string.Format("{0:00}", seconds);
        return string.Format("{0:0}", seconds);
    }

    public static int Circulate(int position, int diversion, int a, int b)
    {
        if (position < a || position > b)
            throw new ArgumentOutOfRangeException("Position must between a and b.");

        if (a == b)
            return a;
        if (a > b)
        {
            int temp = a;
            a = b;
            b = temp;
        }
        int shift = position + diversion;
        if (shift >= a && shift <= b)
            return shift;
        int range = b - a + 1;
        if (shift < a)
        {
            int diff = a - shift;
            int r = diff % range - 1;
            return b - r;
        }
        else
        {
            int diff = shift - b;
            int r = diff % range - 1;
            return a + r;
        }
    }

    public static bool Approximately(float a, float b, int accuracy = ApproximationAccuracy)
    {
        char[] aNums = a.ToString().ToCharArray();
        char[] bNums = b.ToString().ToCharArray();
        int aCommaIndex = Array.IndexOf(aNums, ',');
        int bCommaIndex = Array.IndexOf(bNums, ',');
        if (aCommaIndex == -1 && bCommaIndex == -1 && a == b) return true;
        if (aCommaIndex == -1 || bCommaIndex == -1) return false;
        if (accuracy > aNums.Length - aCommaIndex - 1 ||
            accuracy > bNums.Length - aCommaIndex - 1)
            return false;
        for (int i = 0; i < accuracy; i++)
            if (aNums[aCommaIndex + i + 1] != bNums[bCommaIndex + i + 1])
                return false;
        return true;
    }

    public static Sprite GetSprite(Type type)
    {
        if (type == null)
        {
            return instance.sprites[typeof(ItemSlot)];
        }
        return instance.sprites[type];
    }


    public static void HighLightArea(Vector2Int[] area, Color color)
    {
        foreach (Vector2Int coord in area)
        {
            if (CoordInMapRange(coord))
            {
                Platform platform = MapGenerator.GetPlatformFromCoord(coord);
                platform.gameObject.GetComponent<Renderer>().material.color = color;
            }
        }
    }

    public static Vector2Int[] LocalToGlobal(Vector2Int[] local, Vector2Int relativeTo)
    {
        Vector2Int[] global = new Vector2Int[local.Length];
        for (int i = 0; i < local.Length; i++)
        {
            global[i] = relativeTo + local[i];
        }
        return global;
    }

    public static Vector2Int[] LocalToGlobal(Vector2Int[] local, Vector2Int relativeTo, Predicate<Platform> Criteria)
    {
        List<Vector2Int> filteredGlobal = new List<Vector2Int>();
        for (int i = 0; i < local.Length; i++)
        {
            Vector2Int coord = relativeTo + local[i];
            if (!CoordInMapRange(coord)) continue;

            Platform curr = MapGenerator.GetPlatformFromCoord(coord);
            if (Criteria(curr))
                filteredGlobal.Add(curr.Coord);
        }
        return filteredGlobal.ToArray();
    }

    public static GameObject FindChildOf(GameObject parent, string childName)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject child = parent.transform.GetChild(i).gameObject;
            if (child.name == childName)
                return child;
            child = FindChildOf(child, childName);
            if (child != null)
                return child;
        }
        return null;
    }

    public static bool CoordInMapRange(Vector2Int coord)
    {
        return coord.x >= 0 && coord.y >= 0 && coord.x < mapGen.currentMapSize.x && coord.y < mapGen.currentMapSize.y;
    }

    public static string GetRandomProcessID()
    {
        string randomID = "";
        for (int i = 0; i < 50; i++)
            randomID += (char)UnityEngine.Random.Range(33, 127);
        return randomID;
    }

    public static int SumCoord(Vector2Int vector)
    {
        return Mathf.Abs(vector.x) + Mathf.Abs(vector.y);
    }
}
