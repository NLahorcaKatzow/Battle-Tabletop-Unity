using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class BattleController : BaseController
{
    //TODO: manejo de turnos, dice de quien es turno y quien puede mover

    public static BattleController Instance;
    public int currentTurn = 0;

    public event Action OnTurnChange;

    [Button]
    public void NextTurn()
    {
        currentTurn++;
        Debug.Log("BattleController: NextTurn, currentTurn: " + currentTurn);

        OnTurnChange?.Invoke();
        TabletopController.Instance.SetTurn(currentTurn);
        
    }

    void Awake()
    {
        Instance = this;
    }

    [Button]
    public void InitializeCombat(int newTurn = 0)
    {
        //TODO: Initialize combat
        currentTurn = newTurn;
        TabletopController.Instance.Initializate();
        TabletopController.Instance.SetTurn(currentTurn);
    }

    public int GetCurrentTurn()
    {
        return currentTurn;
    }

    public void OnTimeUp()
    {
        //TODO: tiempo agotado
        NextTurn();
    }



    public override void Initializate()
    {
        Debug.Log("BattleController: Initializated");
        InitializeCombat();
    }
}
