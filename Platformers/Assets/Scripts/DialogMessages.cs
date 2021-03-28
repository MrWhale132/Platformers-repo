using System;
using UnityEngine;

[CreateAssetMenu(menuName ="Global stuffs")]
public class DialogMessages : ScriptableObject
{
    static DialogMessages instance;

    [SerializeField]
    FloatingMessage prefab;

    [SerializeField]
    string[] moveFails;
    [SerializeField]
    string[] atackFails;
    [SerializeField]
    string[] workerFails;
    [SerializeField]
    string[] builderFails;

    [SerializeField]
    MoveFails moveFail;
    [SerializeField]
    AtackFails atackFail;
    
    public MoveFails MoveFails => moveFail;
    public AtackFails AtackFails => atackFail;

    private void OnEnable()
    {
        if (instance == null)
            instance = this;
    }

    public void Create(Vector3 at, string text)
    {
        FloatingMessage message = Instantiate(prefab, at, Quaternion.identity);
        message.SetText(text);
    }

    public static void CreateFloatMessage(Vector3 at, string text)
    {
        FloatingMessage message = Instantiate(instance.prefab, at, Quaternion.identity);
        message.SetText(text);
    }

    public string[] GetMoveFails() => moveFails;
    public string[] GetAtackFails() => atackFails;
    public string[] GetWorkerFails() => workerFails;
    public string[] GetBuilderFails() => builderFails;
}

[Serializable]
public class MoveFails
{
    [SerializeField]
    string outOfField;
    [SerializeField]
    string notEmpty;

    public string OutOfField => outOfField;
    public string NotEmpty => notEmpty;
}

[Serializable]
public class AtackFails
{
    [SerializeField]
    string outOfField;
    [SerializeField]
    string notDamagable;
    [SerializeField]
    string noObject;
    [SerializeField]
    string cantAllys;

    public string OutOfField => outOfField;
    public string NotDamagable => notDamagable;
    public string NoObject => noObject;
    public string CantAllys => cantAllys;
}