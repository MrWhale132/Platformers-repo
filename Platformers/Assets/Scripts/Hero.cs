using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(OptionPaneController))]
public class Hero : MonoBehaviour, ISelectable, IMove, IAtack, IHaveOptionPane
{
    #region Fields

    Action<Platform> action;

    public enum PrepareState { Idle = 0, Move = 1, Atack = 2 }
    [HideInInspector]
    public PrepareState prepareState;

    bool selected;
    bool prepareing;

    public bool Interacting { get => prepareing; set => prepareing = value; }
    public bool Selected { get => selected; set => selected = value; }

    protected PathFinding pathfinder;

    public int dmgAmount;

    [SerializeField]
    protected float moveSpeed;
    [SerializeField]
    protected bool diagonalMove;

    public Vector2Int[] moveField;
    public Color moveFieldColor;

    public Vector2Int[] atackField;
    public Color atackFieldColor;

    protected string[] moveFails;
    protected string[] atackFails;

    Platform platform;

    public Platform Platform { get { return platform; } }

    protected OptionPaneController optionPaneController;

    bool switchOccured;

    [SerializeField]
    protected Vector3 offSetFromGround;

    [SerializeField]
    DialogMessages messages;

    public virtual Vector3 OffsetFromGround => offSetFromGround;

    #endregion

    protected virtual void Start()
    {
        platform = MapGenerator.GetPlatformFromPosition(transform.position);
        platform.objAtPlatform = this;
        platform.walkable = false;

        moveFails = messages.GetMoveFails();
        atackFails = messages.GetAtackFails();
        pathfinder = FindObjectOfType<PathFinding>();
        optionPaneController = GetComponent<OptionPaneController>();
        MouseManager.GUIElementClicked += OnGUIElementClicked;
    }


    public void OptionpaneMessageHandler(object sender, EventArgs message)
    {
        if (message is HeroSelectionMessage msg)
        {
            string selection = msg.Selection;
            if (selection == "Atack")
                SelectAtack();
            else if (selection == "Move")
                SelectMove();
            else
                SelectSkill();
        }
    }


    public void HandleMouseMessage(MouseMessage msg)
    {
        if (msg.Sender as Hero == this) return;

        if (selected && !prepareing)
        {
            UnSubscribe();
            return;
        }

        if (prepareing)
            action(msg.Platform);

        if (msg.Sender is Ether)
            if (PlatformIsNull(msg.Platform)) return;
    }

    void OnMouseUpAsButton()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!GameManager.ValidSelection()) goto Skip;

        if (!selected && GameManager.currSelected == null)
        {
            OnSelection();
        }
        else if (!selected && GameManager.currSelected.ValidSwitch(this))
        {
            GameManager.currSelected.Switch();
            OnSelection();
        }
        else if (selected)
        {
            print(GetType().Name + " deselected.");
            if (prepareing)
                PostActionRegister();
            else UnSubscribe();
        }

    Skip:

        MouseManager.BroadCastClick(new MouseMessage(ClickType.AsButton, this, platform));
    }

    void OnGUIElementClicked(object sender, EventArgs e)
    {
        if (selected && prepareing)
        {
            PostActionRegister();
            print(GetType() + " deselected.");
        }
        else if (selected && !prepareing)
        {
            UnSubscribe();
        }
    }

    void OnSelection()
    {
        selected = true;
        GameManager.currSelected = this;
        optionPaneController.CreateMenu(typeof(HeroSelectionOptionpane), transform.position);
        MouseManager.MouseClicked += HandleMouseMessage;
    }

    void UnSubscribe()
    {
        selected = false;
        if (!switchOccured) GameManager.currSelected = null;
        else switchOccured = false;
        optionPaneController.HideCurrentMenu();
        MouseManager.MouseClicked -= HandleMouseMessage;
    }

    public bool ValidSwitch(object newSelection)
    {
        if (!prepareing &&
            newSelection as Hero != this)
        {
            return true;
        }
        return false;
    }

    public void Switch()
    {
        switchOccured = true;
        UnSubscribe();
    }

    protected bool PlatformIsNull(Platform platform)
    {
        if (platform == null)
        {
            UnSubscribe();
            print("Hero deselected.");
            return true;
        }
        return false;
    }

    public void CallOnMouseUpAsButton()
    {
        OnMouseUpAsButton();
    }

    #region SelectAction

    public void SelectMove()
    {
        action = Move;
        PaperWork();
        prepareState = PrepareState.Move;
        ShowField(true, moveField, moveFieldColor);
    }

    public void SelectAtack()
    {
        action = Atack;
        PaperWork();
        prepareState = PrepareState.Atack;
        ShowField(true, atackField, atackFieldColor);
    }

    public void SelectSkill()
    {
        print("Use skill");
        prepareing = false;
        selected = true;
        optionPaneController.HideCurrentMenu();
    }

    void PaperWork()
    {
        prepareing = true;
        optionPaneController.HideCurrentMenu();
    }

    #endregion

    #region Atack

    public virtual void Atack(Platform platform)
    {
        if (platform == null)
        {
            PostActionRegister();
            return;
        }

        TryAtack(platform, out int resultCode);

        object atackedObj = platform.objAtPlatform;

        if (resultCode == -1)
        {
            IDamagable damagable = atackedObj as IDamagable;
            damagable.TakeDamage(dmgAmount);

            print(this + " atacked.");
            PostActionRegister();
        }
        else if (platform == null || (resultCode == 0 && atackedObj as Hero == this))
        {
            print(this + " deselected");
            PostActionRegister();
        }
        else
            messages.Create(GetMessagePosition(), atackFails[resultCode]);
    }

    public virtual void TryAtack(Platform platform, out int resultCode)
    {
        object atackedObject = platform.objAtPlatform;
        if (platform != null)
            if (atackedObject != null)
                if (atackField.Contains(platform.Coord - this.platform.Coord))
                    if (atackedObject is IDamagable damagable)
                        if (!(damagable is Hero))
                            resultCode = -1;
                        else
                            resultCode = 3;
                    else
                        resultCode = 1;
                else
                    resultCode = 0;
            else
                resultCode = 2;
        else
            resultCode = 4;
    }

    #endregion

    #region Move

    public virtual void Move(Platform platform)
    {
        TryMove(platform, out int resultCode);

        if (resultCode == -1)
        {
            PostActionRegister();

            MoveOn(platform, -1);
        }
        else if (platform == null || (resultCode == 0 && platform.objAtPlatform as Hero == this))
        {
            print(this + " deselected.");
            PostActionRegister();
        }
        else
            messages.Create(GetMessagePosition(), moveFails[resultCode]);
    }

    public virtual void MoveOn(Platform targetPlatform, int resultCode)
    {
        if (resultCode < 0)
        {
            List<Platform> path = pathfinder.Findpath(transform.position, targetPlatform.Vector_3, diagonalMove);

            if (path == null)
            {
                messages.Create(GetMessagePosition(), "This platform is unreachable");
                return;
            }

            StartCoroutine(Move(path.ToArray()));
        }
    }

    public virtual void TryMove(Platform platform, out int resultCode)
    {
        if (platform != null)
            if (moveField.Contains(platform.Coord - this.platform.Coord))
                if (platform.objAtPlatform == null)
                    resultCode = -1;
                else
                    resultCode = 1;
            else
                resultCode = 0;
        else
            resultCode = 2;
    }

    IEnumerator Move(Platform[] path)
    {
        for (int i = 0; i < path.Length; i++)
        {
            Adminastritation(path[i]);

            float percent = 0;
            Vector3 startPos = transform.position;
            Vector3 endPos = MapGenerator.GetPositionFromCoord(path[i].Coord) + offSetFromGround;

            while (percent < 1f)
            {
                percent += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, percent);
                yield return null;
            }
        }
    }

    void Adminastritation(Platform targetPlatform)
    {
        platform.objAtPlatform = null;
        platform.walkable = true;
        platform = targetPlatform;
        platform.objAtPlatform = this;
        platform.walkable = false;
    }

    #endregion

    #region Utilitys

    void PostActionRegister()
    {
        MouseManager.MouseClicked -= HandleMouseMessage;
        action = null;
        selected = false;
        prepareing = false;
        switch (prepareState)
        {
            case PrepareState.Move:
                Vector2Int[] field = FieldSetUp(moveField);
                DeShowField(field);
                break;
            case PrepareState.Atack:
                DeShowField(FieldSetUp(atackField));
                break;
            case PrepareState.Idle:
                Debug.LogError("You used an actionregister for an idle case!");
                break;
        }
        prepareState = PrepareState.Idle;
        GameManager.currSelected = null;
    }

    protected void DeShowField(Vector2Int[] field)
    {
        RemoveCoordsFromList(field);
        foreach (var coord in field)
        {
            Platform curr = MapGenerator.GetPlatformFromCoord(coord);
            curr.ChangeColor(curr.StartColor);
        }
    }

    protected void ShowField(bool display, Vector2Int[] field, Color color)
    {
        Vector2Int[] fieldInGlobal = prepareState == PrepareState.Move ?
                                    FieldSetUp(field, platform => platform.walkable) :
                                    FieldSetUp(field);

        if (display)
            AddCoordsToList(fieldInGlobal);
        else
            RemoveCoordsFromList(fieldInGlobal);

        Utility.HighLightArea(fieldInGlobal, color);
    }

    public void AddCoordsToList(Vector2Int[] field)
    {
        foreach (Vector2Int coord in field)
            if (!Platform.hoverExceptions.Contains(coord) && Utility.CoordInMapRange(coord))
                Platform.hoverExceptions.Add(coord);
    }

    public void RemoveCoordsFromList(Vector2Int[] field)
    {
        foreach (Vector2Int coord in field)
            if (Platform.hoverExceptions.Contains(coord))
                Platform.hoverExceptions.Remove(coord);
    }

    Vector2Int[] FieldSetUp(Vector2Int[] fieldToShow)
    {
        Vector2Int heroCoord = MapGenerator.GetCoordFromPosition(transform.position);
        return Utility.LocalToGlobal(fieldToShow, heroCoord);
    }

    Vector2Int[] FieldSetUp(Vector2Int[] fieldToShow, Predicate<Platform> Criteria)
    {
        Vector2Int heroCoord = MapGenerator.GetCoordFromPosition(transform.position);
        return Utility.LocalToGlobal(fieldToShow, heroCoord, Criteria);
    }

    protected Vector3 GetMessagePosition() => transform.position + Vector3.up * transform.localScale.y;

    #endregion
}