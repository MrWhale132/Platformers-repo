using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Gatherer<T> : Worker, IGather<T> where T : Item
{
    #region Fields

    [SerializeField]
    protected int gTakeAmount;

    #endregion

    #region Gather

    void GatherAt(Platform platform)
    {
        StartCoroutine(MoveThenGatherAt(platform));
    }

    IEnumerator MoveThenGatherAt(Platform platform)
    {
        List<Platform> path;
        MoveOn(platform, MoveMode.Interacting, out path);
        if (path == null) yield break;

        yield return new WaitWhile(() => state == States.Move);
        GatheringEventArgs<T> geA = new GatheringEventArgs<T>(platform, gTakeAmount);
        Gathering(geA);
    }

    public void Gathering(GatheringEventArgs<T> args)
    {
        GatherFrom(args.Gatherable, args.Amount);
    }

    public void GatherFrom(IGatherable<T> gatherable, int amount)
    {
        int avaibleAmount = PrepareImport(gatherable.GetGathered(0), amount);

        if (avaibleAmount > 0)
        {
            T gathered = gatherable.GetGathered(avaibleAmount);

            if (gathered != null)
            {
                inventory.AddItem(gathered);
                messages.Create(GetMessagePosition(), "Successful gathering.");
                return;
            }
        }
        messages.Create(GetMessagePosition(), "Gathering was unsuccesful.");
    }

    #endregion

    public override void HandleMouseMessage(MouseMessage msg)
    {
        if (msg.Sender is IGatherable<T> && !interacting)
        {
            UnSubscribe();
            GatherAt(msg.Platform);
        }
        else
        {
            base.HandleMouseMessage(msg);
        }
    }
}    