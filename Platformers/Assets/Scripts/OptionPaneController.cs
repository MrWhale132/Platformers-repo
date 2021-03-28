using System.Collections.Generic;
using UnityEngine;
using System;


public class OptionPaneController : MonoBehaviour
{
    [SerializeField]
    OptionPane[] prefabs;

    OptionPane currMenu;

    IHaveOptionPane owner;

    Dictionary<Type, OptionPane> optionpanes;

    public Vector3 CurrentMenuPosition { get { return currMenu.transform.position; } }


    private void Start()
    {
        owner = GetComponent<IHaveOptionPane>();
        optionpanes = new Dictionary<Type, OptionPane>();
    }

    public void CreateMenu(string name, Vector3 position)
    {
        if (currMenu == null || currMenu.Name != name)
        {
            currMenu = Instantiate(GetMenu(name), position, Quaternion.identity);
        }
        else currMenu.transform.position = position;
    }

    public void CreateMenu(Type name, Vector3 position, params object[] values)
    {
        if (!optionpanes.ContainsKey(name))
        {
            OptionPane optionPane = Instantiate(GetMenu(name));
            optionPane.AddListener(OnOptionpaneMessage);
            optionpanes.Add(name, optionPane);
        }

        currMenu = optionpanes[name];
        currMenu.transform.position = position;
        currMenu.Enable(values);
    }

    public void ShowMenu(Type name)
    {
        currMenu = optionpanes[name];
        currMenu.gameObject.SetActive(true);
    }

    public void ShowCurrentMenu()
    {
        currMenu.gameObject.SetActive(true);
    }

    public void SetParentCurrentMenu(Transform parent)
    {
        currMenu.transform.SetParent(parent);
    }

    public void HideMenu(Type name)
    {
        optionpanes[name].gameObject.SetActive(false);
    }

    public void HideCurrentMenu()
    {
        currMenu.gameObject.SetActive(false);
    }

    public void DestroyCurrentMenu()
    {
        if (currMenu != null)
        {
            optionpanes.Remove(currMenu.GetType());
            Destroy(currMenu.gameObject);
            currMenu = null;
        }
        else throw new Exception();
    }

    void OnOptionpaneMessage(object sender, EventArgs message)
    {
        owner.OptionpaneMessageHandler(sender, message);
    }

    OptionPane GetMenu(Type name)
    {
        for (int i = 0; i < prefabs.Length; i++)
            if (prefabs[i].GetType() == name) return prefabs[i];
        throw new Exception();
    }

    OptionPane GetMenu(string name)
    {
        for (int i = 0; i < prefabs.Length; i++)
            if (prefabs[i].Name == name) return prefabs[i];
        throw new Exception();
    }
}