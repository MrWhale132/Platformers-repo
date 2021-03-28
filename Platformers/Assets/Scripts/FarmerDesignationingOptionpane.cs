
public class FarmerDesignationingOptionpane : OptionPane
{
    public void AbortClicked()
    {
        InvokeMessenger(this, new FarmerDesignationingMessage());
    }
}

public class FarmerDesignationingMessage : System.EventArgs
{

}