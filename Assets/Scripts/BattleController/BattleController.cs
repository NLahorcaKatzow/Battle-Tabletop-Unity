using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class BattleController : BaseController
{
    //TODO: manejo de turnos, dice de quien es turno y quien puede mover
    
    public static BattleController Instance;
    public int currentTurn = 0;
    
    public event Action OnRoundChange;
    public event Action OnTurnChange;
    
    [Button]
    public void NextTurn(){
        currentTurn++;
        OnTurnChange?.Invoke();
    }

    void Awake()
    {
        Instance = this;
    }
    [Button]
    public void ChangeRound()
    {
        OnRoundChange?.Invoke();
        //TODO: cambio de ronda
        //TODO: resetear los valores por defecto de las piezas
        NextTurn();
    }
    
    
    
    public void InitializeCombat(int newTurn = 0){
        //TODO: Initialize combat
        currentTurn = newTurn;
        if(currentTurn % 2 == 0){
            //TODO: es turno del jugador 1
        }else{
            //TODO: es turno del jugador 2
        }
    }
    
    public int GetCurrentTurn(){
        return currentTurn;
    }
    
    
    
    public override void Initializate()
    {
        Debug.Log("BattleController: Initializated");
        TabletopController.Instance.Initializate();
    }
}
