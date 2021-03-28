using UnityEngine;

[RequireComponent(typeof(OptionPaneController))]
public class StoneWorker : Gatherer<Stone>
{
    protected override void Start()
    {
        base.Start();
        inventory.AddItem(new Stone(11));
    }
}