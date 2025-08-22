using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TabletopUI : BaseController
{

    [SerializeField] private GameObject tabletopUI;
    [SerializeField] private GameObject tabletopGO;
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private GameObject actionCellPrefab;
    [SerializeField] private GameObject pieceInfoPanel;
    [SerializeField] private GameObject pieceActionsPanel;
    [SerializeField] private Button moveButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private GameObject attackCellPrefab;
    Dictionary<Vector2Int, GameObject> actionCells = new Dictionary<Vector2Int, GameObject>();
    Dictionary<Vector2Int, GameObject> attackCells = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] private PieceController currentPiece;

    public void InitializateUI(Dictionary<int, SavedPosition> savedPositions, Dictionary<int, SavedPosition> enemyPositions)
    {
        foreach(var savedPosition in savedPositions){
            if(TabletopController.Instance.GetCurrentPlayerPieces().ContainsKey(savedPosition.Key)) continue;
            Log("TabletopController", "Instantiating piece: " + savedPosition.Key);
            var piece = Instantiate(piecePrefab, grid.GetCellCenterWorld(new Vector3Int(savedPosition.Value.x, 0, savedPosition.Value.y)), Quaternion.identity, grid.transform);
            piece.GetComponent<PieceController>().SetPieceData(ResourceController.Instance.GetPieceDatabyId(savedPosition.Value.type));
            piece.GetComponent<PieceController>().SetPosition(new Vector2Int(savedPosition.Value.x, savedPosition.Value.y));
            TabletopController.Instance.GetCurrentPlayerPieces().Add(savedPosition.Key, new PieceDataGO
            {
                pieceController = piece.GetComponent<PieceController>(),
                go = piece
            });
        }
    
        foreach(var enemyPosition in enemyPositions){
            if(TabletopController.Instance.GetCurrentPlayerPieces().ContainsKey(enemyPosition.Key)) continue;
            Log("TabletopController", "Instantiating enemy piece: " + enemyPosition.Key);
            var enemyPiece = Instantiate(piecePrefab, grid.GetCellCenterWorld(new Vector3Int(enemyPosition.Value.x, 0, enemyPosition.Value.y)), Quaternion.identity, grid.transform);
            enemyPiece.GetComponent<PieceController>().SetPieceData(ResourceController.Instance.GetPieceDatabyId(enemyPosition.Value.type), true);
            enemyPiece.GetComponent<PieceController>().SetPosition(new Vector2Int(enemyPosition.Value.x, enemyPosition.Value.y));
            TabletopController.Instance.GetCurrentPlayerPieces().Add(enemyPosition.Key, new PieceDataGO
            {
                pieceController = enemyPiece.GetComponent<PieceController>(),
                go = enemyPiece
            });
        }
        tabletopUI.SetActive(true);
        tabletopGO.SetActive(true);
    }
    
    [Button]
    public void DebugGrid(){
        for(int x = 0; x < 5; x++){
            for(int z = 0; z < 5; z++){
                var debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                debugCube.transform.position = grid.GetCellCenterWorld(new Vector3Int(x, 0, z));
                debugCube.transform.localScale = new Vector3(1f, 2f, 1f);
                debugCube.transform.parent = grid.transform;
                debugCube.name = $"DebugCube_{x}_{z}";
                debugCube.GetComponent<Renderer>().material.color = Color.green;
            }
        }
    }

    public void ShowPieceInfo(PieceController pieceController){
        //TODO: Show piece info
        pieceInfoPanel.SetActive(true);
    }
    
    public void ShowPieceActions(PieceController pieceController){

        //TODO: Show piece actions
        ClearActionCells();
        if(pieceController.GetMovementBool() && pieceController.GetAttackBool()) return;
        pieceActionsPanel.SetActive(true);
        
        // Convert world position to screen position and move UI
        Vector3 worldPosition = pieceController.transform.position;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        
        // Get the RectTransform of the pieceActionsPanel
        RectTransform rectTransform = pieceActionsPanel.GetComponent<RectTransform>();
        
        // Set the position of the UI panel to the screen position
        rectTransform.position = screenPosition;
        
        moveButton.onClick.RemoveAllListeners();
        attackButton.onClick.RemoveAllListeners();
        moveButton.onClick.AddListener(() => ShowMovementActions(pieceController));
        attackButton.onClick.AddListener(() => ShowAttackActions(pieceController));
        
        if(!pieceController.GetMovementBool()) moveButton.gameObject.SetActive(true);
        else moveButton.gameObject.SetActive(false);
        
        if(!pieceController.GetAttackBool()) attackButton.gameObject.SetActive(true);
        else attackButton.gameObject.SetActive(false);
    }

    public void HidePieceInfo(){
        pieceInfoPanel.SetActive(false);
    }
    
    public void HidePieceActions(){
        pieceActionsPanel.SetActive(false);
    }
    
    public void HideAll(){
        HidePieceInfo();
        HidePieceActions();
        ClearActionCells();
        ClearAttackCells();
    }
    
    public void HideTabletop(){
        tabletopUI.SetActive(false);
        HideAll();
        tabletopGO.SetActive(false);
    }
    
    
    public void ShowMovementActions(PieceController pieceController){
        Debug.Log("ShowMovementActions, movementType: " + pieceController.pieceData.movementType.ToString());
        currentPiece = pieceController;
        
        CalculateActions(pieceController.pieceData.movementType, pieceController.GetPosition(), pieceController.pieceData.movementLength);
    }
    
    public void ShowAttackActions(PieceController pieceController){
        //TODO: Show attack actions
        Debug.Log("ShowAttackActions, attackType: " + pieceController.pieceData.movementType.ToString());
        currentPiece = pieceController;
        CalculateActions(pieceController.pieceData.movementType, pieceController.GetPosition(), pieceController.pieceData.movementLength, true);
    }

    internal void CalculateActions(MovementType movementType, Vector2Int currentPosition, int movementLength, bool isAttack = false){
        // Clear previous action cells
        ClearActionCells();
        Debug.Log("CalculateMovementActions, movementType: " + movementType.ToString());
        if(isAttack) TabletopController.Instance.SetAllPiecesIntangible(currentPiece.generatedId);
        switch(movementType){
            case MovementType.KING:
                CalculateKingMovementActions(currentPosition, movementLength, isAttack);
                break;
            case MovementType.BISHOP:
                break;
            case MovementType.TOWER:
                break;
            case MovementType.QUEEN:
                break;
            case MovementType.PAWN:
                break;
            case MovementType.HORSE:
                break;
            case MovementType.NONE:
                break;
            default:
                break;
        }
    }
    
    private void CalculateKingMovementActions(Vector2Int currentPosition, int movementLength, bool isAttack = false)
    {
        // King can move in all 8 directions (including diagonals) with movementLength = 1
        for (int x = -movementLength; x <= movementLength; x++)
        {
            for (int z = -movementLength; z <= movementLength; z++)
            {
                // Skip the center position (current position)
                if (x == 0 && z == 0) continue;
                
                Vector2Int newPosition = new Vector2Int(currentPosition.x + x, currentPosition.y + z);
                
                // Check if position is within board bounds (5x5)
                if (IsValidPosition(newPosition, isAttack))
                {
                    Vector3 worldPosition = grid.GetCellCenterWorld(new Vector3Int(newPosition.x, 0, newPosition.y));
                    GameObject actionCell;
                    if(isAttack) {
                        actionCell = Instantiate(attackCellPrefab, worldPosition, Quaternion.identity, grid.transform);
                        attackCells.Add(newPosition, actionCell);
                    }
                    else {
                        actionCell = Instantiate(actionCellPrefab, worldPosition, Quaternion.identity, grid.transform);
                        actionCells.Add(newPosition, actionCell);
                    }
                    
                    Log("TabletopUI", $"Created action cell at position: {newPosition}");
                }
            }
        }
    }
    
    public void SelectActionCell(int id){
        //TODO: Select action cell
        if(currentPiece == null) return;
        var newPosition = actionCells.FirstOrDefault(cell => cell.Value.GetInstanceID() == id).Key;
        currentPiece.SetPosition(newPosition);
        currentPiece.SetMovementBool(true);
        currentPiece.MoveToPosition(grid.GetCellCenterWorld(new Vector3Int(newPosition.x, 0, newPosition.y)));
        currentPiece.Deselect();
        HideAll();
        ClearActionCells();
        
        TabletopController.Instance.ShowPieceInfo(currentPiece.generatedId);
        TabletopController.Instance.RecalculatePositions();
        currentPiece = null;
    }
    
    public void SelectAttackCell(int id){
        //TODO: Select attack cell
        if(currentPiece == null) return;
        var newPosition = attackCells.FirstOrDefault(cell => cell.Value.GetInstanceID() == id).Key;
        currentPiece.SetAttackBool(true);
        currentPiece.Deselect();
        HideAll();
        ClearAttackCells();
        TabletopController.Instance.AttackPiece(newPosition.x, newPosition.y, currentPiece.pieceData.damage);
        TabletopController.Instance.SetAllPiecesTangible();
        currentPiece = null;
    }
    
    private bool IsValidPosition(Vector2Int position, bool isAttack = false)
    {
        // Check if position is within 5x5 board bounds
        
        return position.x >= 0 && 
        position.x < 5 && 
        position.y >= 0 && 
        position.y < 5 && 
        (TabletopController.Instance.IsPositionEmpty(position) || isAttack);
    }
    
    private void ClearActionCells()
    {
        foreach (var cell in actionCells)
        {
            if (cell.Value != null)
            {
                Destroy(cell.Value);
            }
        }
        actionCells.Clear();
    }
    
    private void ClearAttackCells()
    {
        foreach (var cell in attackCells)
        {
            if (cell.Value != null)
            {   
                Destroy(cell.Value);
            }
        }
        attackCells.Clear();
    }



    public override void Initializate() { }
}