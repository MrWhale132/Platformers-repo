using System;
using UnityEngine;

public class Panel : MonoBehaviour
{
    protected event EventHandler Messenger;

    public virtual void Enable(params object[] values)
    {
        gameObject.SetActive(true);
    }

    protected void SendMessage(object sender, EventArgs message)
    {
        Messenger(sender, message);
    }

    public void AddListener(EventHandler call)
    {
        Messenger += call;
    }

    public void RemoveListener(EventHandler call)
    {
        Messenger -= call;
    }

    public virtual void Disable()
    {
        gameObject.SetActive(false);
    }
}
