using UnityEngine;
using System;


public class OptionPane : MonoBehaviour
{
    protected event EventHandler Messenger;

    [SerializeField]
    string menuName;

    public string Name { get { return menuName; } }


    public virtual void Enable(params object[] values)
    {
        gameObject.SetActive(true);
    }

    public virtual void Disable()
    {
        gameObject.SetActive(false);
    }

    protected void InvokeMessenger(object sender, EventArgs message)
    {
        Messenger.Invoke(sender, message);
    }

    public void AddListener(EventHandler call)
    {
        Messenger += call;
    }
}