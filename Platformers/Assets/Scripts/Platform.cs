using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;


public class Platform : MonoBehaviour
{
    static Platform instance;

    protected Color startColor;
    [SerializeField]
    protected Color defaultHoverColor;

    [HideInInspector]
    public int gCost;
    [HideInInspector]
    public int hCost;

    [HideInInspector]
    public Platform gridParent;

    public int fCost { get { return gCost + hCost; } }

    [HideInInspector]
    public bool walkable;

    [HideInInspector]
    public int moveCost;

    public static List<Vector2Int> hoverExceptions = new List<Vector2Int>();

    public Vector2Int Coord { get { return MapGenerator.GetCoordFromPosition(Vector_3); } private set { } }
    public Vector3 Vector_3 { get { return transform.position; } private set { } }

    public object objAtPlatform;

    [SerializeField]
    Renderer[] renderers;


    public static Platform Instance => instance;

    public Color StartColor => startColor;

    public Color DefaultHoverColor => defaultHoverColor;

    public Color CurrentColor => renderers[0].material.color;


    protected virtual void Awake()
    {
        startColor = renderers[0].material.color;
    }

    protected virtual void Start()
    {
        if (objAtPlatform != null) walkable = false;
        else walkable = true;
    }

    protected virtual void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (objAtPlatform != null)
        {
            objAtPlatform.GetType().GetMethod("CallOnMouseUpAsButton").Invoke(objAtPlatform, new object[] { });
        }
        else
            MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, null, this));
    }


    public virtual void CallOnMouseUpAsButton()
    {
        OnMouseUpAsButton();
    }

    public void SetColorBy(object sender)
    {
        if (sender is MouseManager)
        {
            if (BuildManager.IsConstructing) ChangeColor(BuildManager.Instance.PlatformColorWhenConstructing);
            else ChangeColor(defaultHoverColor);
        }
    }

    public void ChangeColor(Color to)
    {
        foreach (var item in renderers)
            item.material.color = to;
    }
}
