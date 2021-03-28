using System;
using System.Collections.Generic;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    [SerializeField]
    Panel[] prefabs;

    Dictionary<Type, Panel> panels;

    Panel currPanel;

    IHavePanel owner;

    public bool IsActive { get => currPanel != null; }


    private void Start()
    {
        panels = new Dictionary<Type, Panel>();
        owner = GetComponent<IHavePanel>();
    }


    public void SetActive(Type name, params object[] values)
    {
        if (!panels.ContainsKey(name))
        {
            Panel panel = Instantiate(GetPanel(name));
            panel.transform.SetParent(GameObject.FindGameObjectWithTag("Canvas").transform);
            panel.transform.localPosition = Vector3.zero;
            panel.transform.localRotation = Quaternion.identity;
            panel.transform.localScale = Vector3.one;
            panel.AddListener(OnPanelMessage);
            panels.Add(name, panel);
        }

        currPanel = panels[name];
        currPanel.Enable(values);
    }

    public void Disable()
    {
        currPanel.Disable();
        currPanel = null;
    }

    void OnPanelMessage(object sender, EventArgs message)
    {
        owner.PanelMessageHandler(sender, message);
    }

    Panel GetPanel(Type name)
    {
        for (int i = 0; i < prefabs.Length; i++)
            if (prefabs[i].GetType() == name) return prefabs[i];
        throw new Exception();
    }
}