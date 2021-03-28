
public class FarmerSelectionOptionpane : OptionPane
{
    public void OptionSelected(int enumCode)
    {
        InvokeMessenger(this, new FarmerSelectionMessage(enumCode));
    }
}