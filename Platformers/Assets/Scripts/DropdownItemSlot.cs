using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DropdownItemSlot : ItemSlot
{
    [SerializeField]
    Dropdown dropdown;

    public Dropdown Dropdown => dropdown;
}
