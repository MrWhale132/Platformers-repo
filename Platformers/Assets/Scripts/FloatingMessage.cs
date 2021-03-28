using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FloatingMessage : MonoBehaviour
{
    [SerializeField]
    Text text;
    [SerializeField]
    Vector3 offset;

    public Vector3 Offset => offset;

    public void SetText(string text) => this.text.text = text;

    void Start()
    {
        StartCoroutine(FloatUp());
    }

    IEnumerator FloatUp()
    {
        //Material mat = text.material;    wow, bizare bullshit idk if its a bug, anyway interesting
        Color originalColor = text.color;
        float time = 0;
        float speed = 1.4f;
        while (time < 2)
        {
            time += Time.deltaTime * speed;
            transform.position += Vector3.up * Time.deltaTime;
            yield return null;
        }
        time = 0;   // if more than 1 than you need a specific percent variable
        while (time < 1)
        {
            time += Time.deltaTime * speed;
            transform.position += Vector3.up * Time.deltaTime;
            text.color = Color.Lerp(originalColor, Color.clear, time);
            yield return null;
        }
        Destroy(gameObject);
    }
}