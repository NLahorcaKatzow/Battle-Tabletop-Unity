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
        TabletopController.Instance.Initializate(newTurn);
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
    
    public void FinishBattle()
    {
        //TODO: terminar la batalla
        int teamWinner = GetCurrentTurn() % 2;
        if(teamWinner == 0)
        {
            Debug.Log("BattleController: Player Team is the winner");
            //TODO: mostrar la pantalla de vencedor
            //SceneManager.Instance.ShowVictoryUI();
            SceneManager.Instance.LoadNextLevel(false);
        }
        else
        {
            Debug.Log("BattleController: Enemy Team is the winner");
            //TODO: mostrar la pantalla de derrota
            SceneManager.Instance.ShowDeathUI();
        }
    }



    public override void Initializate()
    {
        Debug.Log("BattleController: Initializated");
        //InitializeCombat();
    }
}
