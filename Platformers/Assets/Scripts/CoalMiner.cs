using UnityEngine;


[RequireComponent(typeof(OptionPaneController))]
[RequireComponent(typeof(PanelController))]
public class CoalMiner : Gatherer<Coal>
{
    protected override void Start()
    {
        base.Start();
        inventory.AddItem(new Coal(20));
    }
}