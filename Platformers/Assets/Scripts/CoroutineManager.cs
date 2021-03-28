using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    static CoroutineManager instance;

    Dictionary<string, Queue<IEnumerator>> coroutines;

    public static CoroutineManager Instance => instance;

    void Awake()
    {
        if (GetComponent<GameManager>())
        {
            instance = this;
        }
        coroutines = new Dictionary<string, Queue<IEnumerator>>();
    }


    public void Add(string key, IEnumerator value)
    {
        if (!coroutines.ContainsKey(key))
        {
            coroutines.Add(key, new Queue<IEnumerator>());
        }
        coroutines[key].Enqueue(value);
        StartCoroutine(value);
    }

    public void Stop(string key)
    {
        StopCoroutine(coroutines[key].Dequeue());
        if (coroutines[key].Count == 0)
            coroutines.Remove(key);
    }

    public void StopAll(string key)
    {
        for (int i = 0; i < coroutines[key].Count; i++)
        {
            StopCoroutine(coroutines[key].Dequeue());
        }
        coroutines.Remove(key);
    }

    public bool Running(string key)
    {
        return coroutines.ContainsKey(key);
    }
}