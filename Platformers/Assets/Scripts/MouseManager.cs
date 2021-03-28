using System;
using UnityEngine;
using UnityEngine.EventSystems;


public class MouseManager : MonoBehaviour
{
    Ray ray;
    public LayerMask clickable;
    Platform prevPlatform;

    public static event EventHandler GUIElementClicked;

    public static event Action<MouseMessage> MouseClicked;

    static MouseManager instance;

    bool suspendHover;


    private void Start()
    {
        if (instance != null)
        {
            Debug.LogError("More than one MouseManager instance was created!");
            return;
        }
        instance = this;
    }


    private void Update()
    {
        if (suspendHover) return;

        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (prevPlatform != null)
            {
                if (!Platform.hoverExceptions.Contains(prevPlatform.Coord))
                    prevPlatform.ChangeColor(prevPlatform.StartColor);
                prevPlatform = null;
            }
            return;
        }


        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, clickable))
        {
            Platform platform = MapGenerator.GetPlatformFromPosition(hit.collider.gameObject.transform.position);

            if (prevPlatform != null && !Platform.hoverExceptions.Contains(prevPlatform.Coord))
            {
                prevPlatform.ChangeColor(prevPlatform.StartColor);
            }

            if (!Platform.hoverExceptions.Contains(platform.Coord))
            {
                platform.SetColorBy(this);
            }
            prevPlatform = platform;
        }
        else if (prevPlatform != null)
        {
            if (!Platform.hoverExceptions.Contains(prevPlatform.Coord))
                prevPlatform.ChangeColor(prevPlatform.StartColor);

            prevPlatform = null;
        }
    }


    public static Platform RayCastPlatform()
    {
        Platform platform = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit info, 5000, instance.clickable))
        {
            platform = MapGenerator.GetPlatformFromPosition(info.collider.gameObject.transform.position);
        }
        return platform;
    }

    public static void InvokeGUIElemntClicked(object sender, EventArgs e)
    {
        GUIElementClicked?.Invoke(sender, e);
    }


    public static void BroadCastClick(MouseMessage msg)
    {
        MouseClicked?.Invoke(msg);
    }

    public static void SuspendHoverColor(bool activate)
    {
        instance.suspendHover = activate;
    }
}


public enum ClickType { Down = 0, Up = 1, AsButton = 2, Drag = 3 }

public class MouseMessage : EventArgs
{
    ClickType mouseClick;
    object sender;
    Platform platform;

    public MouseMessage(ClickType clickTpye, object sender, Platform platform)
    {
        mouseClick = clickTpye;
        this.sender = sender;
        this.platform = platform;
    }

    public ClickType MouseClick => mouseClick;
    public object Sender => sender;
    public Platform Platform => platform;
}