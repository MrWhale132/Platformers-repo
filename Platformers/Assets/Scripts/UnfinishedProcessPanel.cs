using UnityEngine;
using UnityEngine.UI;

public class UnfinishedProcessPanel : Panel
{
    [SerializeField]
    Text text;

    public override void Enable(params object[] values)
    {
        gameObject.SetActive(true);
        Activate((string)values[0]);
    }

    public void Activate(string text)
    {
        this.text.text = text;
    }
    public void Buttons(bool ok)
    {
        SendMessage(this, new SimpleChooseMessage(ok));
    }
}
