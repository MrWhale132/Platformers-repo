using System;


public class FarmersFarmingOptionpane : OptionPane
{
    public void OnButton()
    {
        InvokeMessenger(this, new FarmersInFarmingMessage());
    }
}


public class FarmersInFarmingMessage : EventArgs
{

}
