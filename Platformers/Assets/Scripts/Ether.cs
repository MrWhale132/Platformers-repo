using UnityEngine;
using UnityEngine.EventSystems;


public class Ether : MonoBehaviour
{
    void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, null));
    }
}