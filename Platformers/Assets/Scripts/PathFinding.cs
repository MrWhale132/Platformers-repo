using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public Transform seeker, target;

    List<Platform> path;
    public bool diagonalMoveAlloved;

    //private void Update()
    //{
    //    if (path != null)
    //    {
    //        foreach (Platform platform in path)
    //        {
    //            platform.rend.material.color = platform.startColor;
    //        }
    //    }
    //    path = Findpath(seeker.position, target.position, diagnoalMoveAlloved);
    //    foreach (Platform platform in path)
    //    {
    //        platform.rend.material.color = Color.green;
    //    }
    //}

    public List<Platform> Findpath(Vector3 startPos, Vector3 targetPos, bool diagnoalMoveAlloved)
    {
        Platform startPlatform = MapGenerator.GetPlatformFromPosition(startPos);
        Platform targetPlatform = MapGenerator.GetPlatformFromPosition(targetPos);

        List<Platform> openSet = new List<Platform>();
        HashSet<Platform> closedSet = new HashSet<Platform>();
        openSet.Add(startPlatform);
        
        while (openSet.Count > 0)
        {
            Platform currPlatform = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currPlatform.fCost || openSet[i].fCost == currPlatform.fCost && openSet[i].hCost < currPlatform.hCost)
                {
                    currPlatform = openSet[i];
                }
            }

            openSet.Remove(currPlatform);
            closedSet.Add(currPlatform);

            if (currPlatform == targetPlatform)
            {
                return ReTracePath(startPlatform, targetPlatform);
            }

            foreach (Platform neighbour in GetNeighbours(currPlatform, diagnoalMoveAlloved))
            {
                if ((!neighbour.walkable || closedSet.Contains(neighbour)) && neighbour != targetPlatform)
                {
                    continue;
                }

                int newMoveCostToNeighbour = currPlatform.gCost + GetDistance(currPlatform, neighbour);
                if (newMoveCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMoveCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetPlatform);
                    neighbour.gridParent = currPlatform;
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
            }
        }
        return null;
    }

    List<Platform> ReTracePath(Platform startPlatform, Platform endPlatform)
    {
        List<Platform> path = new List<Platform>();
        Platform currPlatform = endPlatform;

        while (currPlatform != startPlatform)
        {
            path.Add(currPlatform);
            currPlatform = currPlatform.gridParent;
        }
        path.Reverse();
        return path;
    }

    List<Platform> GetNeighbours(Platform relativeTo, bool diagnoalMoveAllved)
    {
        List<Platform> neighbours = new List<Platform>();
        for (int x = -1; x <= 1; x++)
            for (int y = -1; y <= 1; y++)
                if (diagnoalMoveAllved || x == 0 || y == 0)
                    if (x != 0 || y != 0)
                        if (Utility.CoordInMapRange(new Vector2Int(relativeTo.Coord.x - x, relativeTo.Coord.y - y)))
                            neighbours.Add(MapGenerator.GetPlatformFromCoord(relativeTo.Coord.x - x, relativeTo.Coord.y - y));
        return neighbours;
    }

    int GetDistance(Platform platformA, Platform platformB)
    {
        int distX = Mathf.Abs(platformA.Coord.x - platformB.Coord.x);
        int distY = Mathf.Abs(platformA.Coord.y - platformB.Coord.y);

        return Mathf.Min(distY, distX) * 14 + 10 * Mathf.Abs(distY - distX);
    }
}
