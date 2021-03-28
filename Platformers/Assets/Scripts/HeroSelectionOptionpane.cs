using System;


public class HeroSelectionOptionpane : OptionPane
{
    public void OnSelection(string selection)
    {
        InvokeMessenger(this, new HeroSelectionMessage(selection));
    }
}


public class HeroSelectionMessage : EventArgs
{
    string selection;


    public string Selection => selection;


    public HeroSelectionMessage(string selection)
    {
        this.selection = selection;
    }
}