using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(OptionPaneController))]
[RequireComponent(typeof(PanelController))]
[RequireComponent(typeof(CoroutineManager))]
public class Worker : MonoBehaviour, IHit, IDamagable, ISelectable, ITrade, IHaveOptionPane, IHavePanel
{
    #region Fields

    protected enum States { Idle = 0, Move = 1 }
    protected States state = States.Idle;

    protected bool selected;
    protected bool interacting;

    [SerializeField]
    int startingHealth;

    [SerializeField]
    protected int health;
    protected bool dead;
    [HideInInspector]
    protected Platform platform;

    protected Platform selectedPlatform;

    [SerializeField]
    protected float moveSpeed;
    [SerializeField]
    protected bool diagonalMove;

    public int workerLevel;

    protected int inventorySize;
    protected int inventoryCellSize;

    protected PathFinding pathfinder;

    protected string[] workerFails;

    protected Inventory inventory;

    protected OptionPaneController opController;

    protected IInteractable interacted;

    protected bool switchOccured;

    [SerializeField]
    protected GameObject pointerArrowPrefab;
    protected GameObject pointerArrow;

    [SerializeField]
    protected Vector3 offsetFromGround;

    [SerializeField]
    protected DialogMessages messages;

    protected PanelController panelController;

    protected bool unreachablePlatformEncountered;

    protected CoroutineManager coManager;

    public int StartingHelath => startingHealth;
    public Inventory Inventory { get { return inventory; } set { } }
    public Vector3 OffsetFromGround => offsetFromGround;

    #endregion

    #region Initializing

    protected virtual void Awake()
    {
        platform = MapGenerator.GetPlatformFromPosition(transform.position);
        platform.objAtPlatform = this;
        platform.walkable = false;
    }

    protected virtual void Start()
    {
        health = startingHealth;

        inventorySize = 3 + workerLevel / 2;
        inventoryCellSize = 10 + (workerLevel - 1) * 5;
        workerFails = messages.GetWorkerFails();
        pathfinder = FindObjectOfType<PathFinding>();

        inventory = new Inventory(inventorySize);
        opController = GetComponent<OptionPaneController>();
        panelController = GetComponent<PanelController>();
        MouseManager.GUIElementClicked += OnGUIElementClicked;
        coManager = GetComponent<CoroutineManager>();
    }

    #endregion

    #region SelectionHandlers

    public virtual void PanelMessageHandler(object sender, EventArgs message)
    {
        if (sender is LoadingPanel)
        {
            OnLoadingPanelFinished(message as TradeMessage);
        }
    }

    public virtual void OptionpaneMessageHandler(object sender, EventArgs message)
    {
        if (message is WareHouseOptionPaneMessage msg)
        {
            OnWareHouse(msg);
        }
    }

    void OnGUIElementClicked(object sender, EventArgs e)
    {
        if (selected)
            if (sender is BuildMenu)
            {
                BuildMenuEventArgs args = e as BuildMenuEventArgs;
                if (args.BuildingType != typeof(BuildMenu))
                {
                    if (interacting)
                        CancelInteraction();
                    UnSubscribe();
                }
            }
    }

    public virtual void HandleMouseMessage(MouseMessage msg)
    {
        Platform platform = msg.Platform;
        if (msg.Sender is Ether)
            if (PlatformIsNull(platform)) return;
        object sender = platform.objAtPlatform;

        if (sender as Worker == this) return;

        if (interacting)
            CancelInteraction();
        else if (sender == null)
        {
            UnSubscribe();
            MoveOn(platform, MoveMode.StartToEnd);
        }
        else if (sender is IInteractable interactable)
            InteractWith(interactable);
        else
        {
            UnSubscribe();
            messages.Create(GetMessagePosition(), "This platform is not interactable for a " + GetType().Name + ".");
        }
    }

    protected virtual void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (!GameManager.ValidSelection()) goto Skip;

        if (selected)
        {
            if (interacting) CancelInteraction();
            UnSubscribe();
        }
        else if (GameManager.currSelected == null)
        {
            OnSelection();
        }
        else if (GameManager.currSelected.ValidSwitch(this))
        {
            GameManager.currSelected.Switch();
            OnSelection();
        }

    Skip:

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, platform));
    }

    #endregion

    #region SelectionUtilitys

    protected virtual void OnSelection()
    {
        GameManager.currSelected = this;
        selected = true;
        MouseManager.MouseClicked += HandleMouseMessage;
        inventory.Display(true);
        pointerArrow = Instantiate(pointerArrowPrefab, transform.position, Quaternion.identity);
        print("Worker selected.");
    }

    protected virtual void InteractWith(IInteractable interactable)
    {
        if (interactable is WareHouse wareHouse)
        {
            interacting = true;
            interactable.InteractedBy(this);
            opController.CreateMenu(typeof(WareHouseOptionPane), wareHouse.transform.position);
            selectedPlatform = wareHouse.Platform;
            interacted = wareHouse;
            inventory.Display(false);
        }
    }

    protected virtual void CancelInteraction()
    {
        interacting = false;

        if (interacted is WareHouse)
        {
            if (panelController.IsActive)
            {
                panelController.Disable();
                pointerArrow = Instantiate(pointerArrowPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                opController.HideMenu(typeof(WareHouseOptionPane));
            }
        }

        inventory.Display(true);
        selectedPlatform = null;
        interacted.CancelInteractionWith(this);
        interacted = null;
    }

    protected virtual void UnSubscribe()
    {
        selected = false;
        if (!switchOccured)
            GameManager.currSelected = null;
        else switchOccured = false;
        inventory.Display(false);
        MouseManager.MouseClicked -= HandleMouseMessage;
        Destroy(pointerArrow);
    }

    public virtual bool ValidSwitch(object newSelection)
    {
        if (newSelection is Worker &&
            !interacting &&
           newSelection as Worker != this)
        {
            return true;
        }
        return false;
    }

    public virtual void Switch()
    {
        switchOccured = true;
        UnSubscribe();
    }

    protected bool PlatformIsNull(Platform platform)
    {
        if (platform == null)
        {
            UnSubscribe();
            print("Worker deselected.");
            return true;
        }
        return false;
    }

    public void CallOnMouseUpAsButton()
    {
        OnMouseUpAsButton();
    }

    #endregion

    #region OptionpaneHandlers

    void OnWareHouse(WareHouseOptionPaneMessage msg)
    {
        opController.HideCurrentMenu();
        Destroy(pointerArrow);
        inventory.Display(false);
        Inventory traderInvs = ((ITrade)interacted).Inventory;
        traderInvs.Display(false);

        if (msg.TradeFlow == TradeFlow.Export)
        {
            panelController.SetActive(
                typeof(LoadingPanel),
                inventory.Clone(),     // do not swap
                traderInvs.Clone(),    //
                TradeFlow.Export);
        }
        else
        {
            panelController.SetActive(
                typeof(LoadingPanel),
                traderInvs.Clone(),   // do not swap
                inventory.Clone(),    //
                TradeFlow.Import);
        }
    }

    #endregion

    #region Loading

    void OnLoadingPanelFinished(TradeMessage msg)
    {
        if (msg.Result != null)
        {
            msg.Trader = (ITrade)interacted;
            StartCoroutine(
                           MoveThenTrade(selectedPlatform, msg));
        }
        CancelInteraction();
        UnSubscribe();
    }

    IEnumerator MoveThenTrade(Platform platform, TradeMessage t_Args)
    {
        List<Platform> path;
        MoveOn(platform, MoveMode.Interacting, out path);
        if (path == null) yield break;

        yield return new WaitWhile(() => state == States.Move);
        TradeInv(t_Args);
    }

    public void TradeInv(TradeMessage eventArgs)
    {
        if (eventArgs.Result != null)
            for (int i = 0; i < eventArgs.Result.Size; i++)
                if (eventArgs.Result[i] != null)
                    Loading(eventArgs.Result[i], eventArgs.Result[i].Quantity, eventArgs.Trader, eventArgs.TradeFlow);
    }

    public void Loading(Item item, int amount, ITrade exchanger, TradeFlow tradeFlow)
    {
        if (tradeFlow == TradeFlow.Export)
        {
            int avaibleSpace = exchanger.PrepareImport(item, amount);
            if (avaibleSpace != 0)
            {
                Item itemToExport = Export(item, avaibleSpace);
                if (itemToExport != null)
                {
                    exchanger.Import(itemToExport);
                    print("The UnLoading was successed.");
                }
                else
                    print("There is no " + item.GetType() + " what you can store in.");
            }
            else
                print("The " + exchanger.GetType() + " is full.");
        }
        else
        {
            int avaibleSpace = PrepareImport(item, amount);
            if (avaibleSpace != 0)
            {
                Item importItem = exchanger.Export(item, avaibleSpace);
                if (importItem != null)
                {
                    inventory.AddItem(importItem);
                    print("The LoadIn was successed.");
                }
                else
                    print("There is no " + item.GetType() + " what you can take out.");
            }
            else
                print("The inventory is full.");
        }
    }

    #endregion

    #region Import Export

    public void Import(Item item)
    {
        if (item == null ||
            item.Quantity < 0 ||
            inventory.IsFull ||
            inventory.CheckCapacity(item, item.Quantity) < item.Quantity)

            throw new Exception("Invalid Import!");

        inventory.AddItem(item);
    }

    public Item Export(Item item, int amount)
    {
        int avaibleQuantity = PrepareExport(item, amount);

        if (avaibleQuantity != 0)

            return
            inventory.GetItem(item, avaibleQuantity);

        return null;
    }

    public int PrepareImport(Item item, int amount)
    {
        int avaibleSpace = inventory.CheckCapacity(item, amount);
        return avaibleSpace;
    }

    public int PrepareExport(Item item, int amount)
    {
        int avaibleQuantity = inventory.CheckQuantity(item, amount);
        return avaibleQuantity;
    }

    #endregion

    #region Move

    public virtual void Move(Platform platform)
    {
        MoveOn(platform, MoveMode.StartToEnd, out List<Platform> path);
        if (path != null) print("Worker moved.");
    }

    public void MoveHandler(int resultCode)
    {
        if (resultCode == -1)
            print("Worker moved.");
        else
            print(workerFails[resultCode]);

    }

    public virtual void TryMove(Platform platform, out int resultCode)
    {
        if (platform.objAtPlatform != null)
        {
            if (platform.objAtPlatform as Worker != this)
            {
                resultCode = -1;
            }
            else
            {
                resultCode = 1;
            }
        }
        else
        {
            resultCode = -2;
        }
    }

    public virtual void MoveOn(Platform targetPlatform, MoveMode moveMode)
    {
        List<Platform> path = pathfinder.Findpath(transform.position, targetPlatform.Vector_3, diagonalMove);

        if (path == null)
        {
            unreachablePlatformEncountered = true;
            messages.Create(GetMessagePosition(), "The platform is unreachable.");
            return;
        }

        if (moveMode == MoveMode.Interacting)
        {
            path.RemoveAt(path.Count - 1);
        }
        coManager.Add(nameof(Move), Move(path.ToArray()));
    }

    public virtual void MoveOn(Platform targetPlatform, MoveMode moveMode, out List<Platform> path)
    {
        path = pathfinder.Findpath(transform.position, targetPlatform.Vector_3, diagonalMove);

        if (path == null)
        {
            unreachablePlatformEncountered = true;
            return;
        }

        if (moveMode == MoveMode.Interacting)
        {
            path.RemoveAt(path.Count - 1);
        }
        coManager.Add(nameof(Move), Move(path.ToArray()));
    }

    public virtual void MoveOn(MoveEventArgs args)
    {
        List<Platform> path = pathfinder.Findpath(transform.position, args.TargetPlatform.Vector_3, diagonalMove);

        if (path == null)
        {
            unreachablePlatformEncountered = true;
            return;
        }

        if (args.MoveMode == MoveMode.Interacting)
        {
            path.RemoveAt(path.Count - 1);
        }
        coManager.Add(nameof(Move), Move(path.ToArray()));
    }

    protected virtual IEnumerator Move(Platform[] path)
    {
        state = States.Move;
        for (int i = 0; i < path.Length; i++)
        {
            Administritation(path[i]);

            float percent = 0;
            Vector3 startPos = transform.position;
            Vector3 endPos = MapGenerator.GetPositionFromCoord(path[i].Coord) + offsetFromGround;

            while (percent < 1f)
            {
                percent += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, percent);
                yield return null;
            }
        }
        state = States.Idle;
        coManager.Stop(nameof(Move));
    }

    EventArgs CreateDefault(Type type)
    {
        if (type == typeof(MoveEventArgs))
        {
            return new MoveEventArgs(transform.position, selectedPlatform, moveSpeed, diagonalMove, MoveMode.StartToEnd);
        }
        return default;
    }

    void Administritation(Platform targetPlatform)
    {
        platform.objAtPlatform = null;
        platform.walkable = true;
        platform = targetPlatform;
        platform.objAtPlatform = this;
        platform.walkable = false;
    }

    #endregion

    public virtual void TakeDamage(int dmg)
    {
        health -= dmg;

        if (health <= 0 && !dead)
        {
            dead = true;
            Die();
        }
    }

    protected void Die()
    {
        print(GetType() + " is dead.");
        Destroy(gameObject);
    }

    public void Hit(GameObject objectToHit, int hitAmount)
    {

    }

    protected Vector3 GetMessagePosition() => transform.position + Vector3.up * transform.localScale.y;
}