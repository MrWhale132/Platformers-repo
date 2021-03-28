using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Farmer : Worker
{
    #region Fields

    enum WorkType { Whoing = 0, Sowing = 1, Harvesting = 2 }
    enum Designation { SquareArea = 0, Custom = 1 }
    enum WorkMode { Add = 0, Remove = 1 }

    Designation designation = Designation.SquareArea;
    WorkMode workMode = WorkMode.Add;
    WorkType workType = WorkType.Whoing;

    List<Platform> selectedPlatforms;
    List<Platform> underSelection;
    List<Platform> originalPlatofrms;

    [SerializeField]
    Color[] workingColors;
    [SerializeField]
    Color[] waitingColors;

    Action<Platform>[] handlers;
    Predicate<Platform>[] filters;
    Action<Platform>[] finisheds;

    Platform startPlatform;
    Platform endPlatform;

    [SerializeField]
    Color platformsToRemoveColor;
    [SerializeField]
    Color platformsToPlowColor;
    [SerializeField]
    Color platformsWaitingToPlow;
    [SerializeField]
    Color platformsToSowColor;
    [SerializeField]
    Color platformsWaitingToSow;
    [SerializeField]
    Color platformsToHarvestColor;
    [SerializeField]
    Color platformsWaitingToHarvest;

    [SerializeField]
    AnimationCurve animationCurve;

    [SerializeField]
    Text unfinishedDesignationText;

    bool farming;

    [SerializeField]
    WorkProgressInfoBox workInfoBoxPrefab;

    WorkProgressInfoBox workingProgress;

    [SerializeField]
    float secondsToWorkOnePlatform;

    bool keepOpenWorkingInfoBox;

    [SerializeField]
    SoilPlatform soilPlatformPrefab;

    Color selectionColor;
    Color waitingColor;

    bool designationing;

    Predicate<Platform> filter;
    Action<Platform> handler;
    Action<Platform> finished;

    #endregion

    #region Initializing

    protected override void Start()
    {
        base.Start();
        Platform test = MapGenerator.GetPlatformFromCoord(3, 1);
        Destroy(test.gameObject);
        test = Instantiate(soilPlatformPrefab, MapGenerator.GetPositionFromCoord(3, 1), Quaternion.identity);
        MapGenerator.Instance.ReplacePlatformAt(new Vector2Int(3, 1), test);
        test = MapGenerator.GetPlatformFromCoord(4, 1);
        Destroy(test.gameObject);
        test = Instantiate(soilPlatformPrefab, MapGenerator.GetPositionFromCoord(4, 1), Quaternion.identity);
        MapGenerator.Instance.ReplacePlatformAt(new Vector2Int(4, 1), test);

        {
            filters = new Predicate<Platform>[3];
            handlers = new Action<Platform>[3];
            finisheds = new Action<Platform>[3];
            filters[0] = PlowingFilter;
            filters[1] = SowingFilter;
            filters[2] = HarvestingFilter;
            handlers[0] = PlowingHandler;
            handlers[1] = SowingHandler;
            handlers[2] = HarvestingHandler;
            finisheds[0] = PlowingFinished;
            finisheds[1] = SowingFinished;
            finisheds[2] = HarvestingFinished;
        }
        inventory.AddItem(new WheatSeed(5));
    }

    #endregion

    #region StateControllers

    public override void OptionpaneMessageHandler(object sender, EventArgs message)
    {
        if (message is FarmerSelectionMessage msg)
        {
            designationing = true;
            workType = (WorkType)msg.EnumCode;
            selectionColor = workingColors[msg.EnumCode];
            waitingColor = waitingColors[msg.EnumCode];
            filter = filters[msg.EnumCode];
            handler = handlers[msg.EnumCode];
            finished = finisheds[msg.EnumCode];
            if (!coManager.Running(nameof(AreaDesignation)))
                coManager.Add(nameof(AreaDesignation), AreaDesignation());
            opController.HideCurrentMenu();
        }
        else if (message is FarmersInFarmingMessage)
            StopWorking();
        else if (message is FarmerDesignationingMessage)
            AbortDesignation();
        else base.OptionpaneMessageHandler(sender, message);
    }

    public override void PanelMessageHandler(object sender, EventArgs message)
    {
        if (message is MillPanelMessage millMsg)
        {
            HandleMillPanelMessage(millMsg);
        }
        else if (message is BakeryPanelMessage bakeryMsg) HandleBakeryPanelMessage(bakeryMsg);
        else base.PanelMessageHandler(sender, message);
    }

    public override void HandleMouseMessage(MouseMessage msg)
    {
        if (!designationing && !farming)
        {
            base.HandleMouseMessage(msg);
        }
        else if (designationing)
        {
            if (selectedPlatforms.Contains(msg.Platform))
            {
                if (underSelection.Count == 0)
                {
                    foreach (var item in selectedPlatforms)
                        item.ChangeColor(waitingColor);
                    CreateInfoBoxAndOptionPane();
                    originalPlatofrms = new List<Platform>(selectedPlatforms);
                    coManager.Add(nameof(WorkPlatforms), WorkPlatforms(selectedPlatforms));
                    UnSubscribe();
                }
            }
            else opController.HideCurrentMenu();
            //else base.HandleMouseMessage  When we will have turn base.
        }
    }

    protected override void OnMouseUpAsButton()  //Warning: no broadcast on every path
    {
        if (!selected && !farming)
        {
            base.OnMouseUpAsButton();
        }
        else if (selected && !designationing)
        {
            if (interacting) CancelInteraction();
            UnSubscribe();
        }
        else if (designationing)
        {
            opController.CreateMenu(typeof(FarmerDesignationingOptionpane), transform.position);
        }
        else if (farming)
        {
            keepOpenWorkingInfoBox = !keepOpenWorkingInfoBox;
            inventory.Display(true);
        }
    }

    #endregion

    #region StateVariables

    protected override void InteractWith(IInteractable interactable)
    {
        if (interactable is WareHouse wareHouse)
        {
            CommonInteractions(wareHouse);
            opController.CreateMenu(typeof(WareHouseOptionPane), wareHouse.transform.position);
            selectedPlatform = wareHouse.Platform;
        }
        else if (interactable is Mill mill)
        {
            CommonInteractions(mill);
            selectedPlatform = mill.Platform;
            var permission = mill.RequestClientPermission();
            permission.SetInventory(inventory.GetInventoryStatistics());
            permission.AddListener(PanelMessageHandler);
            Destroy(pointerArrow.gameObject);
        }
        else if (interactable is Bakery bakery)
        {
            CommonInteractions(bakery);
            bakery.FetchClientStatistics(inventory.GetInventoryStatistics());
            selectedPlatform = bakery.Platform;
            Destroy(pointerArrow.gameObject);
        }
    }

    protected virtual void CommonInteractions(IInteractable interactable)
    {
        interacting = true;
        interacted = interactable;
        interactable.InteractedBy(this);
        opController.HideMenu(typeof(FarmerSelectionOptionpane));
        inventory.Display(false);
    }

    protected override void CancelInteraction()
    {
        if (interacted is WareHouse)
        {
            opController.ShowMenu(typeof(FarmerSelectionOptionpane));
        }
        else if (interacted is Mill mill)
        {
            var permission = mill.RequestClientPermission();
            permission.RemoveListener(PanelMessageHandler);
            opController.ShowMenu(typeof(FarmerSelectionOptionpane));
        }

        base.CancelInteraction();
    }

    public override bool ValidSwitch(object newSelection)
    {
        if (base.ValidSwitch(newSelection) &&
            !designationing)
            return true;
        return false;
    }

    protected override void OnSelection()
    {
        base.OnSelection();
        opController.CreateMenu(typeof(FarmerSelectionOptionpane), transform.position);
    }

    protected override void UnSubscribe()
    {
        if (designationing)
        {
            coManager.Stop(nameof(AreaDesignation));
            designation = Designation.SquareArea;
            designationing = false;
        }
        underSelection = null;
        MouseManager.SuspendHoverColor(false);
        opController.HideCurrentMenu();
        base.UnSubscribe();
    }

    void ExitMillPanel()
    {
        CancelInteraction();
    }

    #endregion

    #region PanelMessageHandler

    void HandleMillPanelMessage(MillPanelMessage msg)
    {
        switch (msg.Pressed)
        {
            case MillPanelMessage.Buttons.Exit:
                ExitMillPanel(); break;
            case MillPanelMessage.Buttons.Command:
                OnCommandByMill(msg); break;
            default: break;
        }
    }

    void OnCommandByMill(MillPanelMessage msg)
    {
        Mill mill = interacted as Mill;
        coManager.Add(nameof(MoveThenInteractWithMill), MoveThenInteractWithMill(mill, msg.TradeInfos));
        CancelInteraction();
        UnSubscribe();
    }

    IEnumerator MoveThenInteractWithMill(Mill mill, MillTradeInfo[] tradeInfos)
    {
        MoveOn(mill.Platform, MoveMode.Interacting, out List<Platform> path);
        if (path == null) yield break;
        yield return new WaitWhile(() => state == States.Move);

        mill.CheckOutTradeInfos(inventory, tradeInfos);

        coManager.Stop(nameof(MoveThenInteractWithMill));
    }


    void HandleBakeryPanelMessage(BakeryPanelMessage msg)
    {
        switch (msg.Pressed)
        {
            case BakeryPanelMessage.Buttons.Exit:
                ExitBakeryPanel(); break;
            case BakeryPanelMessage.Buttons.Command:
                OnCommandByBakery(msg.BakeryLease); break;
            default: break;
        }
    }

    void OnCommandByBakery(BakeryLease lease)
    {
        coManager.Add(nameof(MoveThenInteractWithBakery), MoveThenInteractWithBakery(lease));
        CancelInteraction();
        UnSubscribe();
    }

    IEnumerator MoveThenInteractWithBakery(BakeryLease lease)
    {
        Bakery bakery = lease.GetBakery();
        MoveOn(bakery.Platform, MoveMode.Interacting, out List<Platform> path);
        if (path == null) yield break;
        yield return new WaitWhile(() => state == States.Move);

        bakery.ValidateLease(inventory, lease);

        coManager.Stop(nameof(MoveThenInteractWithBakery));
    }

    void ExitBakeryPanel()
    {
        CancelInteraction();
    }

    #endregion

    #region FarmingMethods

    IEnumerator WorkPlatforms(List<Platform> platforms)
    {
        farming = true;

        int count = platforms.Count;
        float totalTime = secondsToWorkOnePlatform * platforms.Count;
        workingProgress.SetTimer(Utility.SecondsToDate((int)totalTime));

        while (platforms.Count > 0)
        {
            if (workType == WorkType.Sowing &&
                inventory.QuantityOf(item => item is Seed) == 0)
            {
                messages.Create(GetMessagePosition(), "There is no more seed what you could sow.");
                StopWorking();
                yield break;
            }

            Platform curr = platforms[0];
            MoveOn(curr, MoveMode.StartToEnd);
            yield return new WaitWhile(() => state == States.Move);

            if (unreachablePlatformEncountered)
            {
                messages.Create(GetMessagePosition(), "Some of the platfroms are unreachable." + curr.Coord);
                curr.ChangeColor(curr.StartColor);
                platforms.RemoveAt(0);
                unreachablePlatformEncountered = false;
                continue;
            }

            float percent = 0;
            float currentProgres = 0;

            while (currentProgres < secondsToWorkOnePlatform)
            {
                currentProgres += Time.deltaTime;
                if (workingProgress.gameObject.activeSelf)
                {
                    percent = currentProgres / secondsToWorkOnePlatform;
                    workingProgress.SetCurrentPercent((int)(percent * 100));
                    workingProgress.SetCurrentBarX(percent);
                    float remainingTime = totalTime - (count - platforms.Count) * secondsToWorkOnePlatform - currentProgres;
                    workingProgress.SetTimer(Utility.SecondsToDate(Mathf.CeilToInt(remainingTime)));
                }
                yield return null;
            }

            platforms.Remove(curr);
            Platform.hoverExceptions.Remove(curr.Coord);
            finished(platform);

            if (workingProgress.gameObject.activeSelf)
            {
                percent = 1 - platforms.Count / (float)count;
                workingProgress.SetTotalPercent(Mathf.FloorToInt(percent * 100));
                workingProgress.SetCurrentPercent(0);
                workingProgress.SetCurrentBarX(0);
            }
        }
        WorkingFinished();
    }

    void StopWorking()
    {
        foreach (var item in selectedPlatforms)
        {
            item.ChangeColor(item.StartColor);
            Platform.hoverExceptions.Remove(item.Coord);
        }
        WorkingFinished();
    }

    void WorkingFinished()
    {
        coManager.Stop(nameof(WorkPlatforms));
        farming = false;
        selectedPlatforms = null;
        originalPlatofrms = null;
        keepOpenWorkingInfoBox = false;
        inventory.Display(false);
        Destroy(workingProgress.gameObject);
        opController.HideMenu(typeof(FarmersFarmingOptionpane));  // This is dangerous... what if...
    }

    void CreateInfoBoxAndOptionPane()
    {
        workingProgress = Instantiate(workInfoBoxPrefab);
        workingProgress.gameObject.SetActive(false);
        workingProgress.transform.SetParent(transform);
        opController.HideCurrentMenu();
        opController.CreateMenu(typeof(FarmersFarmingOptionpane), transform.position);
        opController.HideCurrentMenu();
        opController.SetParentCurrentMenu(transform);
    }

    #endregion

    #region DelegateMethods

    bool PlowingFilter(Platform platform)
    {
        if (workMode == WorkMode.Add)
        {
            if (platform != null &&
                platform.GetType() == typeof(Platform) &&
                (platform.objAtPlatform == null ||
                platform.objAtPlatform as Farmer == this) &&
                !selectedPlatforms.Contains(platform))

                return true;
        }
        else
            if (selectedPlatforms.Contains(platform))
            return true;
        return false;
    }

    void PlowingHandler(Platform platform)
    {
        if (workMode == WorkMode.Add)
        {
            platform.ChangeColor(selectionColor);
        }
        else platform.ChangeColor(platformsToRemoveColor);
    }

    void PlowingFinished(Platform at)
    {
        SoilPlatform fPlatform = Instantiate(soilPlatformPrefab, at.Vector_3, Quaternion.identity);
        MapGenerator.Instance.ReplacePlatformAt(fPlatform.Coord, fPlatform);
        Destroy(at.gameObject);
    }

    bool SowingFilter(Platform platform)
    {
        if (workMode == WorkMode.Add)
        {
            if (platform != null &&
                platform is SoilPlatform fPlatfrom &&
                (platform.objAtPlatform == null ||
                platform.objAtPlatform as Farmer == this) &&
                fPlatfrom.FreeToGrow &&
                !selectedPlatforms.Contains(platform))

                return true;
        }
        else
            if (selectedPlatforms.Contains(platform))
            return true;
        return false;
    }

    void SowingHandler(Platform platform)
    {
        if (workMode == WorkMode.Add)
        {
            platform.ChangeColor(selectionColor);
        }
        else platform.ChangeColor(platformsToRemoveColor);
    }

    void SowingFinished(Platform platform)
    {
        bool Criteria(Item item) => item.GetType().IsSubclassOf(typeof(Seed));

        int amount = inventory.CheckQuantity(Criteria, 1);
        if (amount == 0)
        {
            StopWorking();
            return;
        }
        Seed seed = (Seed)inventory.GetItem(Criteria, amount);

        SoilPlatform fPlatform = platform as SoilPlatform;
        fPlatform.ChangeColor(fPlatform.StartColor);
        fPlatform.SowSeeds(seed.GetCrop());
        fPlatform.GrowingCrop.GrowSeeds();
    }

    bool HarvestingFilter(Platform platform)
    {
        if (workMode == WorkMode.Add)
        {
            if (platform != null &&
                platform is SoilPlatform fPlatform &&
                (platform.objAtPlatform == null ||
                platform.objAtPlatform as Farmer == this) &&
                fPlatform.GrowingCrop != null &&
                fPlatform.GrowingCrop.IsFinished &&
                !selectedPlatforms.Contains(platform))

                return true;
        }
        else
            if (selectedPlatforms.Contains(platform))
            return true;
        return false;
    }

    void HarvestingHandler(Platform platform)
    {
        if (workMode == WorkMode.Add)
        {
            platform.ChangeColor(selectionColor);
        }
        else platform.ChangeColor(platformsToRemoveColor);
    }

    void HarvestingFinished(Platform platform)
    {
        SoilPlatform fPlatform = platform as SoilPlatform;
        Crop crop = fPlatform.GrowingCrop;
        Item itemSample = crop.GetItemSample();
        int hQuantity = crop.HarvestQuantity;
        if (inventory.CheckCapacity(itemSample, hQuantity) < hQuantity)
        {
            if (inventory.IsFull)
            {
                StopWorking();
            }
            fPlatform.ChangeColor(fPlatform.StartColor);
            return;
        }
        Item harvestedSeed = crop.GetHarvested();
        inventory.AddItem(harvestedSeed);
        fPlatform.ChangeColor(fPlatform.StartColor);
    }

    #endregion

    #region Designation

    IEnumerator AreaDesignation()
    {
        selectedPlatforms = new List<Platform>();
        underSelection = new List<Platform>();
        List<Platform> prevSelection = new List<Platform>();
        Platform prevEndPlatform = null;
        bool open = true;

        while (true)
        {
            Platform platform = MouseManager.RayCastPlatform();

            if (Input.GetMouseButtonDown(0) && !Input.GetMouseButton(1))
            {
                if (underSelection.Count == 0)
                {
                    MouseManager.SuspendHoverColor(true);
                    workMode = WorkMode.Add;
                    startPlatform = null;
                    open = true;
                }
                else open = false;
            }
            else if (Input.GetMouseButtonDown(1) && !Input.GetMouseButton(0))
            {
                if (underSelection.Count == 0)
                {
                    MouseManager.SuspendHoverColor(true);
                    workMode = WorkMode.Remove;
                    startPlatform = null;
                    open = true;
                    if (platform != null && !selectedPlatforms.Contains(platform))
                    {
                        platform.ChangeColor(platform.StartColor);
                    }
                }
                else open = false;
            }
            if (Input.GetMouseButton(0) && open && !Input.GetMouseButton(1))
            {
                if (designation == Designation.Custom)
                {
                    if (filter(platform) && !underSelection.Contains(platform))
                    {
                        underSelection.Add(platform);
                        handler(platform);
                    }
                }
                else if (designation == Designation.SquareArea)
                {
                    if (platform != null)
                    {
                        if (startPlatform == null)
                            startPlatform = platform;

                        endPlatform = platform;
                        if (endPlatform != prevEndPlatform && prevEndPlatform != null)
                        {
                            underSelection = new List<Platform>(
                                             GetPlatformsInArea(startPlatform, endPlatform,
                                                                filter, handler));

                            if (CoordsDecreased(endPlatform.Coord, prevEndPlatform.Coord))
                            {
                                ShrinkVisualArea(prevSelection.ToArray());
                            }
                            prevSelection = underSelection;
                        }
                        prevEndPlatform = endPlatform;
                    }
                }
            }
            else if (Input.GetMouseButton(1) && !Input.GetMouseButton(0))
            {
                if (platform != null)
                {
                    if (designation == Designation.Custom)
                    {
                        if (selectedPlatforms.Contains(platform) && open)
                        {
                            underSelection.Add(platform);
                            platform.ChangeColor(platformsToRemoveColor);
                        }
                        else if (underSelection.Contains(platform))
                        {
                            underSelection.Remove(platform);
                            platform.ChangeColor(selectionColor);
                        }
                    }
                    else if (designation == Designation.SquareArea)
                    {
                        if (startPlatform == null)
                            startPlatform = platform;

                        endPlatform = platform;
                        if (endPlatform != prevEndPlatform && prevEndPlatform != null && open)
                        {
                            underSelection = new List<Platform>(
                                             GetPlatformsInArea(startPlatform, endPlatform,
                                                                filter, handler));

                            if (CoordsDecreased(endPlatform.Coord, prevEndPlatform.Coord))
                            {
                                ShrinkVisualArea(prevSelection.ToArray());
                            }
                            prevSelection = underSelection;
                        }
                        prevEndPlatform = endPlatform;
                    }
                }
            }
            if (Input.GetMouseButtonUp(0) && !Input.GetMouseButton(1))
            {
                if (!open)
                {
                    coManager.Stop(nameof(PulsarEffect));

                    if (underSelection.Contains(platform))
                    {
                        if (workMode == WorkMode.Add)
                        {
                            AddCurrentSelection(underSelection);
                        }
                        else RemoveCurrentSelection(underSelection);
                    }
                    else UndoCurrentSelection(underSelection);
                }
                else
                {
                    for (int i = 0; i < underSelection.Count; i++)
                        Platform.hoverExceptions.Add(underSelection[i].Coord);

                    if (underSelection.Count > 0)
                        coManager.Add(nameof(PulsarEffect), PulsarEffect(underSelection));
                }

                MouseManager.SuspendHoverColor(false);
            }
            else if (Input.GetMouseButtonUp(1) && open && !Input.GetMouseButton(0))
            {
                if (underSelection.Count > 0)
                {
                    coManager.Add(nameof(PulsarEffect), PulsarEffect(underSelection));
                }
                MouseManager.SuspendHoverColor(false);
            }

            int diversion = (int)Input.mouseScrollDelta.y;
            if (diversion != 0)
            {
                int length = Enum.GetValues(typeof(Designation)).Length;
                int index = Utility.Circulate((int)designation, diversion, 0, length - 1);
                designation = (Designation)index;
            }

            yield return null;
        }
    }

    void RemoveCurrentSelection(List<Platform> currentSelection)
    {
        for (int i = 0; i < currentSelection.Count; i++)
        {
            var curr = currentSelection[i];
            selectedPlatforms.Remove(curr);
            curr.ChangeColor(curr.StartColor);
            Platform.hoverExceptions.Remove(curr.Coord);
        }
        startPlatform = null;
        currentSelection.Clear();
    }

    void UndoCurrentSelection(List<Platform> currentselection)
    {
        for (int i = 0; i < currentselection.Count; i++)
        {
            Platform curr = currentselection[i];
            if (selectedPlatforms.Contains(curr))
            {
                curr.ChangeColor(selectionColor);
            }
            else
            {
                curr.ChangeColor(curr.StartColor);
                Platform.hoverExceptions.Remove(curr.Coord);
            }
        }
        startPlatform = null;
        currentselection.Clear();
    }

    void AddCurrentSelection(List<Platform> currentSelection)
    {
        for (int i = 0; i < currentSelection.Count; i++)
        {
            var curr = currentSelection[i];
            selectedPlatforms.Add(curr);
            curr.ChangeColor(selectionColor);
        }
        startPlatform = null;
        currentSelection.Clear();
        MouseManager.SuspendHoverColor(false);
    }

    void ShrinkVisualArea(Platform[] prevSelection)
    {
        if (workMode == WorkMode.Remove)
        {
            for (int i = 0; i < prevSelection.Length; i++)
            {
                var curr = prevSelection[i];
                if (!underSelection.Contains(curr))
                {
                    curr.ChangeColor(selectionColor);
                }
            }
        }
        else
        {
            for (int i = 0; i < prevSelection.Length; i++)
            {
                var curr = prevSelection[i];
                if (!underSelection.Contains(curr))
                {
                    curr.ChangeColor(curr.StartColor);
                }
            }
        }
        //if (startPlatform == endPlatform)
        //{
        //    underSelection.Clear();
        //    startPlatform.ChangeColor(Color.green);
        //}
    }

    void AbortDesignation()
    {
        if (coManager.Running(nameof(PulsarEffect)))
        {
            coManager.Stop(nameof(PulsarEffect));
            UndoCurrentSelection(underSelection);
        }
        for (int i = 0; i < selectedPlatforms.Count; i++)
        {
            Platform curr = selectedPlatforms[i];
            curr.ChangeColor(curr.StartColor);
            Platform.hoverExceptions.Remove(curr.Coord);
        }
        selectedPlatforms = null;
        UnSubscribe();
    }

    IEnumerator PulsarEffect(List<Platform> platforms)
    {
        float percent = 0;
        Color currentColor = platforms[0].CurrentColor;
        while (true)
        {
            percent += Time.deltaTime;
            for (int i = 0; i < platforms.Count; i++)
            {
                var curr = platforms[i];
                curr.ChangeColor(Color.Lerp(curr.StartColor, currentColor, animationCurve.Evaluate(percent)));
            }
            yield return null;
        }
    }

    #endregion

    #region MonoMethods

    void OnMouseEnter()
    {
        if (farming)
        {
            workingProgress.transform.position = transform.position + Vector3.up * 3;
            workingProgress.gameObject.SetActive(true);
            opController.ShowCurrentMenu();
            workingProgress.SetTotalPercent(Mathf.FloorToInt(100 - (selectedPlatforms.Count / (float)originalPlatofrms.Count * 100)));
        }
    }

    void OnMouseExit()
    {
        if (farming && !keepOpenWorkingInfoBox)
        {
            opController.HideCurrentMenu();
            workingProgress.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Helpers

    Platform[] GetPlatformsInArea(Platform a, Platform b, Predicate<Platform> filter = null, Action<Platform> handler = null)
    {
        Vector2Int from = a.Coord;
        Vector2Int to = b.Coord;
        Vector2Int diff = to - from;
        Vector2Int temp = from;
        if (diff.x < 0)
        {
            from.x = to.x;
            to.x = temp.x;
        }
        if (diff.y < 0)
        {
            from.y = to.y;
            to.y = temp.y;
        }
        diff = to - from;
        diff += Vector2Int.one;
        if (filter != null)
        {
            List<Platform> platforms = new List<Platform>();
            for (int i = 0; i < diff.x; i++)
                for (int j = 0; j < diff.y; j++)
                {
                    Platform curr = MapGenerator.GetPlatformFromCoord(from.x + i, from.y + j);
                    if (filter(curr))
                    {
                        handler?.Invoke(curr);
                        platforms.Add(curr);
                    }
                }

            return platforms.ToArray();
        }
        else
        {
            Platform[] area = new Platform[diff.x * diff.y];
            for (int i = 0; i < diff.x; i++)
                for (int j = 0; j < diff.y; j++)
                {
                    Platform curr = MapGenerator.GetPlatformFromCoord(from.x + i, from.y + j);
                    handler?.Invoke(curr);
                    area[i * diff.y + j] = curr;
                }

            return area;
        }
    }

    bool CoordsDecreased(Vector2Int from, Vector2Int to)
    {
        int xDist = Mathf.Abs(from.x - to.x);
        int yDist = Mathf.Abs(from.y - to.y);

        if (xDist > 0 || yDist > 0)
            return true;
        return false;
    }

    int GetDistance(Platform platformA, Platform platformB)
    {
        int distX = Mathf.Abs(platformA.Coord.x - platformB.Coord.x);
        int distY = Mathf.Abs(platformA.Coord.y - platformB.Coord.y);

        return Mathf.Min(distY, distX) * 14 + 10 * Mathf.Abs(distY - distX);
    }

    #endregion
}