using System;
using UnityEngine;


public class TradeMessage : EventArgs
{
    Inventory result;
    TradeFlow tradeFlow;
    ITrade with;

    public Inventory Result { get => result; }
    public TradeFlow TradeFlow { get => tradeFlow; }
    public ITrade Trader { get => with; set => with = value; }

    public TradeMessage(Inventory inventory, TradeFlow tradeFlow)
    {
        result = inventory;
        this.tradeFlow = tradeFlow;
    }
}


public enum MoveMode { StartToEnd = 0, Interacting = 1 }

public class MoveEventArgs : EventArgs
{
    Platform currPlatform;
    Platform platformToMove;
    float moveSpeed;
    bool diagonalMove;
    MoveMode moveMode;

    public Platform CurrentPlatform { get => currPlatform; }
    public Platform TargetPlatform { get => platformToMove; }
    public float MoveSpeed { get => moveSpeed; }
    public bool DiagonalMove { get => diagonalMove; }
    public MoveMode MoveMode { get => moveMode; set => moveMode = value; }

    public MoveEventArgs(Platform start, Platform destination, float speed, bool diagonal, MoveMode moveMode)
    {
        currPlatform = start;
        platformToMove = destination;
        moveSpeed = speed;
        diagonalMove = diagonal;
        this.moveMode = moveMode;
    }

    public MoveEventArgs(Vector3 startPos, Platform destination, float speed, bool diagonal, MoveMode moveMode)
    {
        currPlatform = MapGenerator.GetPlatformFromPosition(startPos);
        platformToMove = destination;
        moveSpeed = speed;
        diagonalMove = diagonal;
        this.moveMode = moveMode;
    }
}


public class GatheringEventArgs<T> : EventArgs where T : Item
{
    Platform at;
    IGatherable<T> gatherable;
    int amount;

    public Platform PlatformAt { get => at; }
    public IGatherable<T> Gatherable { get => gatherable; }
    public int Amount { get => amount; }

    public GatheringEventArgs(Platform platformAt, int Amount)
    {
        at = platformAt;
        gatherable = at.objAtPlatform as IGatherable<T>;
        amount = Amount;
    }

    public GatheringEventArgs(IGatherable<T> gatherable, int Amount)
    {
        at = null;
        this.gatherable = gatherable;
        amount = Amount;
    }
}


public class SimpleChooseMessage : EventArgs
{
    bool choose;

    public SimpleChooseMessage(bool choose)
    {
        this.choose = choose;
    }

    public bool OK => choose;
}

public class FarmerSelectionMessage : EventArgs
{
    int enumCode;

    public int EnumCode => enumCode;


    public FarmerSelectionMessage(int enumCode)
    {
        this.enumCode = enumCode;
    }
}