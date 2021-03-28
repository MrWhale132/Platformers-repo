using System;
using UnityEngine;


[RequireComponent(typeof(CoroutineManager))]
public class Crop : MonoBehaviour
{
    [SerializeField]
    protected int harvestQuantity;
    [SerializeField]
    protected float growTime;
    [SerializeField]
    protected int growSpeed = 1;
    [SerializeField]
    protected int numberOfSeeds;
    [SerializeField]
    protected int seedRadius;
    [SerializeField]
    protected int seedSquareSideLength;

    protected Platform platform;

    protected GameObject[] seeds;

    [SerializeField]
    protected GameObject seedPrefab;

    protected CoroutineManager coManager;

    protected bool growingFinished;

    [SerializeField]
    protected CropStateInfoBox stateInfoBoxprefab;

    protected CropStateInfoBox stateInfoBox;


    public bool IsFinished => growingFinished;

    public int SeedRadius => seedRadius;

    public int SeedSquareSideLength => seedSquareSideLength;

    public int NumberOfSeed => numberOfSeeds;

    public int HarvestQuantity => harvestQuantity;


    protected virtual void Awake()
    {
        platform = MapGenerator.GetPlatformFromPosition(transform.position);

        coManager = GetComponent<CoroutineManager>();
    }

    protected void OnMouseUpAsButton()
    {
        platform.CallOnMouseUpAsButton();
    }


    public virtual Item GetHarvested()
    {
        throw new NotImplementedException();
    }

    public virtual Seed GetItemSample()
    {
        throw new NotImplementedException();
    }

    public virtual void GrowSeeds()
    {
        throw new NotImplementedException();    
    }
    
    public void SetSeeds(GameObject[] seeds)
    {
        this.seeds = seeds;
    }

    public GameObject GetSeed()
    {
        return seedPrefab;
    }
}
