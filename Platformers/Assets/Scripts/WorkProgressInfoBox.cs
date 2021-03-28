using UnityEngine;
using UnityEngine.UI;

public class WorkProgressInfoBox : MonoBehaviour
{
    [SerializeField]
    Text currentProgress;
    [SerializeField]
    Text totalProgress;
    [SerializeField]
    Text timer;
    [SerializeField]
    Transform currentBar;
    [SerializeField]
    Transform totalBar;

    public void SetCurrentPercent(int percent)
    {
        currentProgress.text = percent + "%";
    }

    public void SetTotalPercent(int percent)
    {
        totalProgress.text = percent + "%";
        Vector3 size = totalBar.localScale;
        totalBar.localScale = new Vector3(percent / 100f, size.y, size.z);
    }

    public void SetTimer(string time)
    {
        timer.text = time;
    }

    public void SetCurrentBarX(float x)
    {
        Vector3 size = currentBar.localScale;
        currentBar.localScale = new Vector3(x, size.y, size.z);
    }

    public float GetTotalBarX()
    {
        return totalBar.localScale.x;
    }
}
