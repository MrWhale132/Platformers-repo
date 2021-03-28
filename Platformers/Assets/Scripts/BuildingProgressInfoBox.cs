using UnityEngine;
using UnityEngine.UI;

public class BuildingProgressInfoBox : MonoBehaviour
{
    [SerializeField]
    BuildingProgressRecord record;

    [SerializeField]
    RectTransform[] rects;
    [SerializeField]
    Transform list;
    [SerializeField]
    RectTransform offSet;
    [SerializeField]
    Transform bar;

    BuildingProgressRecord timer;

    public Transform Bar => bar;


    public void AddRecord(Sprite sprite, int curr, int goal)
    {
        BuildingProgressRecord record = Instantiate(this.record);
        record.SetUp(list);
        Strech();

        record.Image.sprite = sprite;
        record.Ratio.text = curr + "/" + goal;
        record.Missing.text = (goal - curr).ToString();
    }

    public void SetUpTimer(string time)
    {
        timer = Instantiate(record);
        timer.SetUp(list);
        Strech();
        timer.Image.sprite = Utility.GetSprite(typeof(ItemSlot));
        timer.Missing.text = string.Empty;
        timer.Ratio.text = time;
    }

    public void SetTimer(string time)
    {
        timer.Ratio.text = time;
    }

    public void Refresh(int i, int current, int goal)
    {
        var record = list.GetChild(i).gameObject.GetComponent<BuildingProgressRecord>();
        if (!record.isActiveAndEnabled) return;
        record.Ratio.text = current + "/" + goal;
        record.Missing.text = (goal - current).ToString();
    }

    public void RemoveAt(int i)
    {
        if (i >= list.childCount) return;
        for (int j = 0; j < rects.Length; j++)
        {
            rects[j].sizeDelta = new Vector2(150, rects[j].rect.height - 35);
        }
        offSet.sizeDelta = new Vector2(offSet.sizeDelta.x, offSet.sizeDelta.y - 5);
        list.GetChild(i).gameObject.SetActive(false);
    }

    void Strech()
    {
        for (int i = 0; i < rects.Length; i++)
            rects[i].sizeDelta = new Vector2(150, rects[i].rect.height + 35);
        offSet.sizeDelta = new Vector2(offSet.sizeDelta.x, offSet.sizeDelta.y + 5);
    }
}
