using System;


public class WareHouseOptionPane : OptionPane
{
    public void LoadIn()
    {
        InvokeMessenger(this, new WareHouseOptionPaneMessage(TradeFlow.Export));
    }

    public void LoadOut()
    {
        InvokeMessenger(this, new WareHouseOptionPaneMessage(TradeFlow.Import));
    }
}

public class WareHouseOptionPaneMessage : EventArgs
{
    TradeFlow tradeFlow;

    public TradeFlow TradeFlow => tradeFlow;

    public WareHouseOptionPaneMessage(TradeFlow tradeFlow)
    {
        this.tradeFlow = tradeFlow;
    }
}
