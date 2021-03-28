using System.Collections;
using UnityEngine;


public class Wheat : Crop
{
    protected void OnEnable()
    {
        stateInfoBox = Instantiate(stateInfoBoxprefab);
        stateInfoBox.transform.position = transform.position + Vector3.up * 3;
        stateInfoBox.gameObject.SetActive(false);
        stateInfoBox.SetCropName(nameof(Wheat));
        stateInfoBox.SetSprite(Utility.GetSprite(typeof(Wheat)));
        stateInfoBox.SetPercent(0);
    }

    private void OnMouseEnter()
    {
        if (!growingFinished)
        {
            stateInfoBox.gameObject.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (!growingFinished)
        {
            stateInfoBox.gameObject.SetActive(false);
        }
    }


    public override Item GetHarvested()
    {
        Destroy(gameObject, 0.1f);
        var sp = platform as SoilPlatform;
        sp.CropHarvested();
        return new WheatSeed(harvestQuantity);
    }

    public override Seed GetItemSample()
    {
        return new WheatSeed(0);
    }

    public override void GrowSeeds()
    {
        coManager.Add(nameof(GrowSeeds), GrowingSeeds());
    }


    protected virtual IEnumerator GrowingSeeds()
    {
        float currTime = 0;
        float ratio;
        Vector3 currScale = seedPrefab.transform.localScale;

        while (currTime < growTime)
        {
            currTime += Time.deltaTime * growSpeed;
            ratio = currTime / growTime;
            if (!Utility.Approximately(currScale.y, ratio))
            {
                stateInfoBox.SetPercent((int)(ratio * 100));
                for (int i = 0; i < seeds.Length; i++)
                {
                    Vector3 seedScale = seeds[i].transform.localScale;
                    Vector3 position = seeds[i].transform.position;
                    float pY = platform.transform.position.y;
                    float lsY = platform.transform.localScale.y;
                    seeds[i].transform.localScale = new Vector3(seedScale.x, ratio, seedScale.z);
                    seeds[i].transform.position = new Vector3(position.x, pY + (lsY + ratio) / 2, position.z);
                }
                currScale.y = ratio;
            }
            yield return null;
        }
        growingFinished = true;
        Destroy(stateInfoBox.gameObject);
    }
}