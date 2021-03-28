using UnityEngine;
using UnityEngine.UI;


public class CropStateInfoBox : MonoBehaviour
{
    [SerializeField]
    Text cropName;
    [SerializeField]
    Image cropImage;
    [SerializeField]
    Text percent;
    [SerializeField]
    Transform bar;


    public void SetCropName(string name)
    {
        cropName.text = name;
    }

    public void SetSprite(Sprite sprite)
    {
        cropImage.sprite = sprite;
    }

    public void SetPercent(int percent)
    {
        this.percent.text = percent + "%";
        Vector3 scale = bar.localScale;
        bar.localScale = new Vector3(percent / 100f, scale.y, scale.z);
    }
}