using UnityEngine;


[RequireComponent(typeof(OptionPaneController))]
[RequireComponent(typeof(PanelController))]
public class WoodWorker : Gatherer<Wood>
{
    protected override void Start()
    {
        base.Start();
        inventory.AddItem(new Wood(6));
    }
}