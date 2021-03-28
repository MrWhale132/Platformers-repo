using System;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static event System.Action EndTurn;

    public enum BattleState { Start, PlayerTurn, Enemyturn, Win, Lose }
    public static BattleState battleState;

    public static bool freeSelection = true;

    public static ISelectable currSelected;


    private void Awake()
    {
        battleState = BattleState.Start;
    }

    public static bool ValidSelection()
    {
        if (BuildManager.IsConstructing)
        {
            return false;
        }
        return true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Time.timeScale = 0;
        }
        else if (Input.GetKeyDown(KeyCode.O))
            Time.timeScale = 1;
    }
}