using UnityEngine.UI;
using UnityEngine;
using System;

public class ItemSlot : MonoBehaviour
{
    [SerializeField]
    Image itemFrame;
    [SerializeField]
    Image itemImage;
    [SerializeField]
    Text amountText;

    event Action<ItemSlot> OnClick;

    public Image ItemFrame { get { return itemFrame; } }
    public Image ItemImage { get { return itemImage; } }
    public Text AmountText { get { return amountText; } }


    public void SetScale(float newScale)
    {
        transform.localScale = new Vector3(newScale, newScale);
    }

    public void InvokeOnClick()
    {
        OnClick?.Invoke(this);
    }

    public void AddListener(Action<ItemSlot> call)
    {
        OnClick += call;
    }

    public void RemoveListener(Action<ItemSlot> call)
    {
        OnClick -= call;
    }
}
