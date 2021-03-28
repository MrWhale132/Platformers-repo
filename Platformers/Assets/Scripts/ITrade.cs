using System;

public interface ITrade
{
    Inventory Inventory { get; set; }

    void Import(Item item);

    Item Export(Item item, int amount);

    int PrepareImport(Item item, int amount);
}