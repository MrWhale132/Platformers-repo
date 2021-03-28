using System;
using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField]
    BuildingComponent[] componentsToBuild;

    [SerializeField]
    protected Vector3 offsetFromGround;

    protected Platform platform;

    [SerializeField]
    string buildingTime;

    [SerializeField]
    int level = 1;

    public Vector3 OffsetFromGround => offsetFromGround;

    public BuildingComponent[] ComponentsToBuild => componentsToBuild;

    public Platform Platform { get { return (Platform)platform; } }


    protected virtual void Start()
    {
        platform = MapGenerator.GetPlatformFromPosition(transform.position);
        platform.objAtPlatform = this;
        platform.walkable = false;
    }

    protected virtual void OnMouseUpAsButton()
    {
        
    }


    public int GetBuildingTimeInSeconds()
    {
        string[] temp = buildingTime.Split(':');
        return 3600 * int.Parse(temp[0]) + 60 * int.Parse(temp[1]) + int.Parse(temp[2]);
    }

    public void CallOnMouseUpAsButton()
    {
        OnMouseUpAsButton();
    }
}


[Serializable]
public class BuildingComponent
{
    [SerializeField]
    Materials material;
    [SerializeField]
    int quantity;
    Sprite sprite;
    bool fulfiled;

    Item item;

    public Item Item
    {
        get => item;
    }

    public Sprite Sprite { get => sprite; set => sprite = value; }
    public bool Fulfilled { get => fulfiled; set => fulfiled = value; }


    public BuildingComponent(Materials materialType, int quantity)
    {
        material = materialType;
        this.quantity = quantity;
        if (material == Materials.Wood)
        {
            sprite = Utility.GetSprite(typeof(Wood));
            item = new Wood(quantity);
        }
        else if (material == Materials.Stone)
        {
            sprite = Utility.GetSprite(typeof(Stone));
            item = new Stone(quantity);
        }
    }

    public BuildingComponent(BuildingComponent copy)
    {
        material = copy.material;
        quantity = copy.quantity;
        sprite = copy.sprite;
        if (copy.item == null)
        {
            copy.item = new BuildingComponent(material, quantity).item;
        }
        item = Item.Clone(copy.item);
    }

    public static BuildingComponent[] Copy(BuildingComponent[] from)
    {
        BuildingComponent[] copyto = new BuildingComponent[from.Length];
        for (int i = 0; i < from.Length; i++)
            copyto[i] = new BuildingComponent(from[i]);
        return copyto;
    }

    public static explicit operator BuildingComponent(Item c)
    {
        if (c is Wood)
            return new BuildingComponent(Materials.Wood, c.Quantity);
        if (c is Stone)
            return new BuildingComponent(Materials.Stone, c.Quantity);
        throw new Exception();
    }

    public static BuildingComponent operator -(BuildingComponent a, BuildingComponent b)
    {
        if (a.material != b.material ||
            a.item.Quantity - b.item.Quantity < 0)

            throw new Exception();

        return new BuildingComponent(a.material, a.item.Quantity - b.item.Quantity);
    }

    public override string ToString()
    {
        return item.ToString();
    }
}