using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using Sirenix.OdinInspector;

public class PieceDataGO
{

    public PieceController pieceController;
    public GameObject go;
}

public class TabletopController : MonoBehaviour
{
    public static TabletopController Instance { get; private set; }


    [ShowInInspector] private Dictionary<int, PieceDataGO> currentPiecesInTabletop;
    [SerializeField] private TabletopUI tabletopUI;

    public delegate void PieceSelected(int id);
    public event PieceSelected OnPieceSelected;

    //guardado de posiciones
    // calculo de movimiento
    private void Awake()
    {
        Instance = this;
    }

    public void Initializate()
    {
        Debug.Log("TabletopController: Initializated");
        Debug.Log("TabletopController: " + ResourceController.Instance.piecesData);
        currentPiecesInTabletop = new Dictionary<int, PieceDataGO>();
        Dictionary<int, SavedPosition> savedPositions = ResourceController.Instance.GetSavedPositions();
        Dictionary<int, SavedPosition> enemyPositions = ResourceController.Instance.GetEnemyPositions();
        tabletopUI.InitializateUI(savedPositions, enemyPositions);
    }

    public void SetTurn(int turn)
    {
        Debug.Log("TabletopController: SetTurn, turn: " + turn);
        tabletopUI.HideAll();
        IAController.Instance.FinishTurn();

        if (turn % 2 == 1)
        {
            SetAllPlayerPiecesInactives();

            var enemyPieces = currentPiecesInTabletop.Values.Where(piece => piece.pieceController.isEnemy && !piece.pieceController.isDead).ToList();
            var playerPieces = currentPiecesInTabletop.Values.Where(piece => !piece.pieceController.isEnemy && !piece.pieceController.isDead).ToList();
            IAController.Instance.StartCoroutine(IAController.Instance.Initialize(enemyPieces, playerPieces));
            return;
        }
        SetAllPlayerPiecesActives();
    }

    public void SetAllPlayerPiecesInactives()
    {
        foreach (var piece in currentPiecesInTabletop)
        {
            if (piece.Value.pieceController.isEnemy)
            {
                piece.Value.pieceController.SetPieceActive();
                continue;
            }
            piece.Value.pieceController.SetPieceInactive();
        }
    }


    public void SetAllPlayerPiecesActives()
    {
        foreach (var piece in currentPiecesInTabletop)
        {
            if (piece.Value.pieceController.isEnemy)
            {
                piece.Value.pieceController.SetPieceInactive();
                continue;
            }
            piece.Value.pieceController.SetPieceActive();
        }
    }

    public void CalculateMovements()
    {
        if (!CanMovePieces())
        {
            BattleController.Instance.NextTurn();
            return;
        }
    }

    public bool CanMovePieces()
    {
        return currentPiecesInTabletop.Values.Any(piece => !piece.pieceController.isDead && (!piece.pieceController.isMoving || !piece.pieceController.isAttacking));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            // Check if click is over UI element
            if (EventSystem.current.IsPointerOverGameObject())
            {
                //Log("TabletopController", "Click is over UI, ignoring raycast");
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Piece")))
            {
                Debug.Log("TabletopController: Clicked on piece: " + hit.collider.name);
                if (hit.collider.tag == "Piece") OnPieceSelected?.Invoke(hit.collider.gameObject.GetInstanceID());
                else if (hit.collider.tag == "ActionCell") tabletopUI.SelectActionCell(hit.collider.gameObject.GetInstanceID());
                else if (hit.collider.tag == "AttackCell") tabletopUI.SelectAttackCell(hit.collider.gameObject.GetInstanceID());
            }
            else
            {
                tabletopUI.HideAll();
                SetAllPiecesTangible();
                OnPieceSelected?.Invoke(-1);
            }
        }
    }
    public PieceController GetPieceController(int id)
    {
        return currentPiecesInTabletop.FirstOrDefault(piece => piece.Value.pieceController.generatedId == id).Value.pieceController;
    }

    public Dictionary<int, PieceDataGO> GetCurrentPlayerPieces()
    {
        return currentPiecesInTabletop;
    }

    public PieceDataClass GetPieceData(int id)
    {
        return currentPiecesInTabletop.FirstOrDefault(piece => piece.Value.pieceController.generatedId == id).Value.pieceController.pieceData;
    }

    public void ShowPieceInfo(int id)
    {
        tabletopUI.ShowPieceInfo(GetPieceController(id));
        Debug.Log("TabletopController: Showing piece info: " + id);
    }

    public void ShowPieceActions(int id)
    {
        tabletopUI.ShowPieceActions(GetPieceController(id));
        Debug.Log("TabletopController: Showing piece actions: " + id);
    }

    public bool IsPositionEmpty(Vector2Int position)
    {
        return currentPiecesInTabletop.FirstOrDefault(piece => piece.Value.pieceController.GetPosition() == position && !piece.Value.pieceController.isDead).Value == null;
    }

    public Grid GetGrid()
    {
        return tabletopUI.grid;
    }


    public void AttackPiece(int x, int y, int damage)
    {
        //TODO: ataque a un espacio
        //TODO: obtener una pieza por su posicion en x,y
        var piece = currentPiecesInTabletop.FirstOrDefault(piece => !piece.Value.pieceController.isDead && piece.Value.pieceController.GetPosition() == new Vector2Int(x, y));
        if (piece.Value == null)
        {
            Debug.Log("TabletopController: No piece found at: " + x + ", " + y);

            return;
        }
        //TODO: quitar vida a la pieza
        Debug.Log("TabletopController: Attacking piece at: " + x + ", " + y);
        piece.Value.pieceController.ApplyDamage(damage);
    }
    [Button]
    public void DestroyPiece(int id)
    {
        Debug.Log("TabletopController: Destroying piece: " + id);
        var piece = currentPiecesInTabletop.FirstOrDefault(piece => piece.Value.pieceController.generatedId == id).Value;
        if (piece == null)
        {
            Debug.Log("TabletopController: Piece " + id + " not found");
            return;
        }
        piece.pieceController.DestroyPiece();
    }
    [Button]
    public void RevivePiece(int id)
    {
        //TODO: Revivir pieza, seteo de nueva posicion
        Debug.Log("TabletopController: Reviving piece: " + id);
        var piece = currentPiecesInTabletop.FirstOrDefault(piece => piece.Value.pieceController.generatedId == id).Value;
        piece.pieceController.RevivePiece();
    }

    public void SetAllPiecesIntangible(int currentPieceId)
    {
        foreach (var piece in currentPiecesInTabletop)
        {
            if (piece.Value.pieceController.generatedId == currentPieceId || piece.Value.pieceController.isDead) continue;
            piece.Value.go.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }
    public void SetAllPiecesTangible()
    {
        foreach (var piece in currentPiecesInTabletop)
        {
            piece.Value.go.layer = LayerMask.NameToLayer("Piece");
        }
    }


    public void RecalculatePositions()
    {
        //TODO:
    }


    #region Internal Methods
    internal void ClearTabletop()
    {
        foreach (var piece in currentPiecesInTabletop)
        {
            Destroy(piece.Value.go);
        }
        currentPiecesInTabletop.Clear();
        tabletopUI.HideTabletop();
    }
    #endregion


}
