using System;
using System.Collections.Generic;
using UnityEngine;

public class SoilPlatform : Platform
{
    bool[,] freeSquares;

    Crop cropToGrow;

    int cropLandWilde;
    int cropLandHeight;


    public bool FreeToGrow => cropToGrow == null;

    public Crop GrowingCrop => cropToGrow;

    protected override void Awake()
    {
        base.Awake();

        freeSquares = new bool[100, 100];
        cropLandWilde = freeSquares.GetLength(0);
        cropLandHeight = freeSquares.GetLength(1);
        for (int i = 0; i < cropLandHeight; i++)
            for (int j = 0; j < cropLandWilde; j++)
                freeSquares[i, j] = true;
    }

    public override void CallOnMouseUpAsButton()
    {
        base.CallOnMouseUpAsButton();
    }


    public void SowSeeds(Crop crop)
    {
        if (cropToGrow != null || crop == null)
            throw new Exception("There is a \"Crop problem\"");

        float yOffSet = transform.localScale.y / 2;
        cropToGrow = Instantiate(crop, transform.position + Vector3.up * yOffSet, Quaternion.identity);
        GameObject seedPrefab = cropToGrow.GetSeed();

        System.Random r = new System.Random();

        int sideLength = cropToGrow.SeedSquareSideLength;
        int radius = cropToGrow.SeedRadius;

        int maxSeeds = freeSquares.GetLength(0) * freeSquares.GetLength(1) / radius * radius * 3;   //Pi
        int numOfSeeds = cropToGrow.NumberOfSeed;

        List<GameObject> seeds = new List<GameObject>();

        for (int i = 0; i < maxSeeds; i++)
        {
            int xCoord = r.Next(5, cropLandWilde - 5 - sideLength);
            int yCoord = r.Next(5, cropLandHeight - 5 - sideLength);

            int middleX = Mathf.RoundToInt(xCoord + sideLength / 2f);
            int middleY = Mathf.RoundToInt(yCoord + sideLength / 2f);

            List<Vector2Int> coords = new List<Vector2Int>();

            bool cell = freeSquares[middleX, middleY];
            if (cell == true)
            {
                for (int x = middleX - radius; x <= middleX + radius; x++)
                {
                    for (int y = middleY - radius; y <= middleY + radius; y++)
                    {
                        if (x > -1 &&
                            y > -1 &&
                            x < cropLandWilde &&
                            y < cropLandHeight &&
                            GetDistance(middleX, middleY, x, y) <= radius * 10)
                        {
                            if (freeSquares[x, y] != true)
                            {
                                goto Invalid;
                            }
                            else coords.Add(new Vector2Int(x, y));
                        }
                    }
                }
                for (int j = 0; j < coords.Count; j++)
                {
                    freeSquares[coords[j].x, coords[j].y] = false;
                }
                Vector3 position = transform.position;
                Vector3 scale = transform.localScale;
                Vector2 bottomLeft = new Vector2(position.x - scale.x / 2, position.z - scale.z / 2);
                Vector3 spawnPos = new Vector3(bottomLeft.x + (scale.x / cropLandWilde * middleX),
                                               position.y + scale.y / 2 + seedPrefab.transform.localScale.y / 2,
                                               bottomLeft.y + scale.z / cropLandHeight * middleY);

                GameObject seed = Instantiate(seedPrefab, spawnPos, seedPrefab.transform.rotation);
                seed.transform.SetParent(cropToGrow.transform);
                seeds.Add(seed);
                if (seeds.Count >= numOfSeeds) break;
            }
        Invalid:;
        }
        cropToGrow.SetSeeds(seeds.ToArray());
    }


    public void CropHarvested()
    {
        cropLandWilde = freeSquares.GetLength(0);
        cropLandHeight = freeSquares.GetLength(1);
        for (int i = 0; i < cropLandHeight; i++)
            for (int j = 0; j < cropLandWilde; j++)
                freeSquares[i, j] = true;
    }

    int GetDistance(int startX, int startY, int endX, int endY)
    {
        int distX = Mathf.Abs(startX - endX);
        int distY = Mathf.Abs(startY - endY);

        return Mathf.Min(distY, distX) * 14 + 10 * Mathf.Abs(distY - distX);
    }
}