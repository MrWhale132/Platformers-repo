using UnityEngine;
using UnityEngine.UI;

public class BuildingProgressRecord : MonoBehaviour
{
    [SerializeField]
    Image image;
    [SerializeField]
    Text ratio;
    [SerializeField]
    Text missing;

    RectTransform own;

    public Image Image => image;
    public Text Ratio => ratio;
    public Text Missing => missing;


    private void Awake()
    {
        own = GetComponent<RectTransform>();
    }

    public void SetUp(Transform parent)
    {
        transform.SetParent(parent);
        own.localScale = Vector3.one;
        transform.rotation = parent.rotation;
        own.localPosition = new Vector3(transform.localRotation.x, transform.localScale.x, 0);
    }
}
