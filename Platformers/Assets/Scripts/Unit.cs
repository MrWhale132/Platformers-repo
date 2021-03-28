using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : LivingEntity, IAtack, IMove
{
    public int dmgAmount;

    [SerializeField]
    protected float moveSpeed;
    [SerializeField]
    protected bool diagonalMove;

    public Vector2Int[] moveField;
    public Color moveFieldColor;

    public Vector2Int[] atackField;
    public Color atackFieldColor;

    public string[] moveFails;
    public string[] atackFails;

    PathFinding pathfinder;

    Platform platform;


    protected override void Start()
    {
        base.Start();
        pathfinder = FindObjectOfType<PathFinding>();
    }


    #region Atack

    public virtual void Atack(Platform platform)
    {
        TryAtack(platform, out int resultCode);
        AtackHandler(platform, resultCode);
        Atacking(platform, dmgAmount, resultCode);
    }

    public virtual void AtackHandler(Platform platform, int resultCode)
    {
        if (resultCode < 0)
        {
            DeShowUnitAtackField();
            print(GetType() + " atacked.");
        }
        else
        {
            if (resultCode == 2) DeShowUnitAtackField();
            print(atackFails[resultCode]);
        }
    }

    public virtual void TryAtack(Platform platform, out int resultCode)
    {
        object atackedObject = platform.objAtPlatform;
        if (atackedObject != null)
        {
            if (atackField.Contains(platform.Coord - platform.Coord))
            {
                if (atackedObject is IDamagable)
                {
                    resultCode = -1;
                }
                else
                {
                    resultCode = 1;
                }
            }
            else
            {
                if (atackedObject as Unit == this)
                {
                    resultCode = 2;
                }
                else
                {
                    resultCode = 0;
                }
            }
        }
        else
        {
            resultCode = 4;
        }
    }

    public virtual void Atacking(Platform platform, int _dmgAmount, int resultCode)
    {
        if (resultCode < 0)
        {
            var damagable = platform.objAtPlatform as IDamagable;
            damagable.TakeDamage(_dmgAmount);
        }
    }

    #endregion


    #region Move

    public virtual void Move(Platform _platform)
    {
        TryMove(_platform, out int resultCode);
        MoveHandler(resultCode);
        MoveOn(_platform, resultCode);
    }

    public virtual void MoveHandler(int resultCode)
    {
        if (resultCode < 0)
        {
            DeShowUnitMoveField();
            print(GetType() + " moved.");
        }
        else
        {
            if (resultCode == 2) DeShowUnitMoveField();
            print(moveFails[resultCode]);
        }
    }

    public virtual void TryMove(Platform platform, out int resultCode)
    {
        if (moveField.Contains(platform.Coord - platform.Coord))
        {
            if (platform.objAtPlatform == null)
            {
                resultCode = -1;
            }
            else
            {
                resultCode = 1;
            }
        }
        else
        {
            if (platform.objAtPlatform as Unit == this)
            {
                resultCode = 2;
            }
            else
            {
                resultCode = 0;
            }
        }
    }

    public virtual void MoveOn(Platform targetPlatform, int resultCode)
    {
        if (resultCode < 0)
        {
            List<Platform> path = pathfinder.Findpath(transform.position, targetPlatform.Vector_3, diagonalMove);

            StartCoroutine(Move(path.ToArray()));
        }
    }

    IEnumerator Move(Platform[] path)
    {
        for (int i = 0; i < path.Length; i++)
        {
            Adminastritation(path[i]);

            float percent = 0;
            Vector3 startPos = transform.position;
            Vector3 endPos = MapGenerator.GetPositionFromCoord(path[i].Coord);

            while (percent < 1f)
            {
                percent += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, percent);
                yield return null;
            }
        }
    }

    #endregion


    void Adminastritation(Platform targetPlatform)
    {
        platform.objAtPlatform = null;
        platform.walkable = true;
        platform = targetPlatform;
        platform.objAtPlatform = this;
        platform.walkable = false;
    }


    protected void ShowField(bool display, Vector2Int[] field, Color color)
    {
        Vector2Int[] fieldInGlobal = FieldSetUp(field);

        if (display)
            AddCoordsToList(fieldInGlobal);
        else
            RemoveCoordsFromList(fieldInGlobal);

        Utility.HighLightArea(fieldInGlobal, color);
    }


    public void ShowUnitAtackField()
    {
        Vector2Int[] fieldInGlobal = FieldSetUp(atackField);
        AddCoordsToList(fieldInGlobal);
        Utility.HighLightArea(fieldInGlobal, atackFieldColor);
    }

    public void DeShowUnitAtackField()
    {
        Vector2Int[] fieldInGlobal = FieldSetUp(atackField);
        RemoveCoordsFromList(fieldInGlobal);
        Utility.HighLightArea(fieldInGlobal, platform.StartColor);
    }

    public void ShowUnitMoveField()
    {
        Vector2Int[] fieldInGlobal = FieldSetUp(moveField);
        AddCoordsToList(fieldInGlobal);
        Utility.HighLightArea(fieldInGlobal, moveFieldColor);
    }

    public void DeShowUnitMoveField()
    {
        Vector2Int[] fieldInGlobal = FieldSetUp(moveField);
        RemoveCoordsFromList(fieldInGlobal);
        Utility.HighLightArea(FieldSetUp(moveField), platform.StartColor);
    }

    public void AddCoordsToList(Vector2Int[] field)
    {
        foreach (Vector2Int coord in field)
            if (!Platform.hoverExceptions.Contains(coord))
                Platform.hoverExceptions.Add(coord);
    }

    public void RemoveCoordsFromList(Vector2Int[] field)
    {
        foreach (Vector2Int coord in field)
            if (Platform.hoverExceptions.Contains(coord))
                Platform.hoverExceptions.Remove(coord);
    }

    Vector2Int[] FieldSetUp(Vector2Int[] fieldToShow)
    {
        Vector2Int unitCoord = MapGenerator.GetCoordFromPosition(transform.position);
        return Utility.LocalToGlobal(fieldToShow, unitCoord);
    }
}