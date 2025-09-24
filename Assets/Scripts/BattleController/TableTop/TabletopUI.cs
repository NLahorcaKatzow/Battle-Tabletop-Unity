using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabletopUI : BaseController
{

    [SerializeField] private GameObject tabletopUI;
    [SerializeField] private GameObject tabletopGO;
    public Grid grid;
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private GameObject actionCellPrefab;
    [SerializeField] private GameObject pieceInfoPanel;
    [SerializeField] private GameObject pieceActionsPanel;
    
    [SerializeField] private GameObject pickUpPrefab;
    
    
    [SerializeField] private Button moveButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private GameObject attackCellPrefab;
    [SerializeField] private GameObject timerGO;
    [SerializeField] private Timer timerComponent;
    
    // TextMeshProUGUI variables for piece info display
    [SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI maxHealthText;
    [SerializeField] private TextMeshProUGUI movementTypeText;
    [SerializeField] private TextMeshProUGUI movementLengthText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI specialText;
    
    Dictionary<Vector2Int, GameObject> actionCells = new Dictionary<Vector2Int, GameObject>();
    Dictionary<Vector2Int, GameObject> attackCells = new Dictionary<Vector2Int, GameObject>();
    [SerializeField] private PieceController currentPiece;
    [SerializeField] private Dictionary<Vector2Int, GameObject> pickUpGOs = new Dictionary<Vector2Int, GameObject>();
    
    
    
    

    public void InitializateUI(Dictionary<int, SavedPosition> savedPositions, Dictionary<int, SavedPosition> enemyPositions)
    {
        foreach(var savedPosition in savedPositions){
            if(TabletopController.Instance.GetCurrentPlayerPieces().ContainsKey(savedPosition.Key)) continue;
            Log("TabletopController", "Instantiating piece: " + savedPosition.Key);
            var piece = Instantiate(piecePrefab, grid.GetCellCenterWorld(new Vector3Int(savedPosition.Value.x, 0, savedPosition.Value.y)), Quaternion.identity, grid.transform);
            var pieceData = piece.GetComponent<PieceController>();
            pieceData.SetPieceData(ResourceController.Instance.GetPieceDatabyId(savedPosition.Value.type));
            pieceData.SetPosition(new Vector2Int(savedPosition.Value.x, savedPosition.Value.y));
            TabletopController.Instance.GetCurrentPlayerPieces().Add(savedPosition.Key, new PieceDataGO
            {
                pieceController = pieceData,
                go = piece
            });
        }
    
        foreach(var enemyPosition in enemyPositions){
            if(TabletopController.Instance.GetCurrentPlayerPieces().ContainsKey(enemyPosition.Key)) continue;
            Log("TabletopController", "Instantiating enemy piece: " + enemyPosition.Key);
            var enemyPiece = Instantiate(piecePrefab, grid.GetCellCenterWorld(new Vector3Int(enemyPosition.Value.x, 0, enemyPosition.Value.y)), Quaternion.identity, grid.transform);
            var pieceData = enemyPiece.GetComponent<PieceController>();
            pieceData.SetPieceData(ResourceController.Instance.GetPieceDatabyId(enemyPosition.Value.type), true);
            pieceData.SetPosition(new Vector2Int(enemyPosition.Value.x, enemyPosition.Value.y));
            pieceData.render.GetComponentInChildren<Renderer>().material.SetColor("_Mid_Color", Color.grey);
            TabletopController.Instance.GetCurrentPlayerPieces().Add(enemyPosition.Key, new PieceDataGO
            {
                pieceController = pieceData,
                go = enemyPiece
            });
        }
        tabletopUI.SetActive(true);
        tabletopGO.SetActive(true);
        InitializeTimer();
    }
    
    public void InitializatePickUps(int amount = 0){
        var pickUps = ResourceController.Instance.GetHealthPickUps();
        for(int i = 0; i < amount; i++){
        
            var randomPosition = GetRandomPosition();
            var pickUpGO = Instantiate(pickUpPrefab, grid.GetCellCenterWorld(
            new Vector3Int(randomPosition.x, 0, randomPosition.y)), Quaternion.identity, grid.transform);
            pickUpGO.transform.position = new Vector3(pickUpGO.transform.position.x, 1.45f, pickUpGO.transform.position.z);
            pickUpGO.GetComponent<HealthPickUp>().SetPickUpData(pickUps[Random.Range(0, pickUps.Count)]);
            pickUpGOs.Add(randomPosition, pickUpGO);
        }
    }

    public void ClearPickUps()
    {
        foreach(var pickUp in pickUpGOs.Values)
        {
            Destroy(pickUp);
        }
        pickUpGOs.Clear();
        Debug.Log("ClearPickUps: " + pickUpGOs.Count);
    }

    private Vector2Int GetRandomPosition()
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        for(int i = 0; i < 5; i++){
            for(int j = 0; j < 5; j++){
                if(TabletopController.Instance.IsPositionEmpty(new Vector2Int(i, j))) positions.Add(new Vector2Int(i, j));
            }
        }
        return positions[Random.Range(0, positions.Count)];
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
        if (pieceController == null || pieceController.pieceData == null) return;
        
        // Set all the text fields with piece data
        if (idText != null) idText.text = $"ID: {pieceController.pieceData.id}";
        if (nameText != null) nameText.text = $"Name: {pieceController.pieceData.name}";
        if (damageText != null) damageText.text = $"Damage: {pieceController.pieceData.damage}";
        if (maxHealthText != null) maxHealthText.text = $"Max Health: {pieceController.pieceData.maxHealth}";
        if (movementTypeText != null) movementTypeText.text = $"Movement Type: {pieceController.pieceData.movementType}";
        if (movementLengthText != null) movementLengthText.text = $"Movement Length: {pieceController.pieceData.movementLength}";
        if (rangeText != null) rangeText.text = $"Range: {pieceController.pieceData.range}";
        if (specialText != null) specialText.text = $"Special: {pieceController.pieceData.special}";
        
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
        ClearActionCells();
        ClearAttackCells();
    }
    
    public void HideAll(){
        HidePieceInfo();
        HidePieceActions();
    }
    
    public void HideTabletop(){
        tabletopUI.SetActive(false);
        HideAll();
        timerComponent.StopTimer();
        tabletopGO.SetActive(false);
    }
    
    
    public void ShowMovementActions(PieceController pieceController){
        Debug.Log("ShowMovementActions, movementType: " + pieceController.pieceData.movementType.ToString());
        currentPiece = pieceController;
        HidePieceActions();
        CalculateActions(pieceController.pieceData.movementType, pieceController.GetPosition(), pieceController.pieceData.movementLength);
    }
    
    public void ShowAttackActions(PieceController pieceController){
        //TODO: Show attack actions
        HidePieceActions();
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
                CalculateBishopMovementActions(currentPosition, movementLength, isAttack);
                break;
            case MovementType.TOWER:
                CalculateTowerMovementActions(currentPosition, movementLength, isAttack);
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
    
    private void CalculateBishopMovementActions(Vector2Int currentPosition, int movementLength, bool isAttack = false)
    {
        // Bishop moves diagonally in all 4 directions
        // Diagonal directions: (1,1), (1,-1), (-1,1), (-1,-1)
        Vector2Int[] diagonalDirections = {
            new Vector2Int(1, 1),   // Top-right
            new Vector2Int(1, -1),  // Bottom-right
            new Vector2Int(-1, 1),  // Top-left
            new Vector2Int(-1, -1)  // Bottom-left
        };
        
        foreach (Vector2Int direction in diagonalDirections)
        {
            for (int distance = 1; distance <= movementLength; distance++)
            {
                Vector2Int newPosition = new Vector2Int(
                    currentPosition.x + (direction.x * distance),
                    currentPosition.y + (direction.y * distance)
                );
                
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
                    
                    Log("TabletopUI", $"Created bishop action cell at position: {newPosition}");
                }
                else
                {
                    // If we hit an obstacle or boundary, stop checking this direction
                    break;
                }
            }
        }
    }
    
    private void CalculateTowerMovementActions(Vector2Int currentPosition, int movementLength, bool isAttack = false)
    {
        Vector2Int[] straightDirections = {
            new Vector2Int(1, 0),   // Right
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1)   // Down
        };
        
        foreach (Vector2Int direction in straightDirections)
        {
            for (int distance = 1; distance <= movementLength; distance++)
            {
                Vector2Int newPosition = new Vector2Int(
                    currentPosition.x + (direction.x * distance),
                    currentPosition.y + (direction.y * distance)
                );
                
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
                    
                    Log("TabletopUI", $"Created tower action cell at position: {newPosition}");
                }
                else
                {
                    // If we hit an obstacle or boundary, stop checking this direction
                    break;
                }
            }
        }
    }
    
    public void SelectActionCell(int id){
        //TODO: Select action cell
        if(currentPiece == null) return;
        var newPosition = actionCells.FirstOrDefault(cell => cell.Value.GetInstanceID() == id).Key;

        currentPiece.MoveToPosition(newPosition);
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

    public void InitializeTimer(){
        if (timerComponent != null)
        {
            // Load time configuration from game configs
            var gameConfigs = ResourceController.Instance.GetGameConfigs();
            if (gameConfigs != null)
            {
                timerComponent.SetMaxTime(gameConfigs.timePerPlayerRound);
                Log("TabletopUI", $"Timer initialized with {gameConfigs.timePerPlayerRound} seconds per round");
            }
            else
            {
                timerComponent.SetMaxTime(120f); // Default fallback
                Log("TabletopUI", "Game configs not found, using default 120 seconds");
            }
            
            timerComponent.InitializeTimer();
            timerGO.SetActive(true);
        }
        else
        {
            Log("TabletopUI", "Timer component not found!");
        }
    }
    
    public float GetCurrentTime()
    {
        return timerComponent != null ? timerComponent.GetCurrentTime() : 0f;
    }
    
    public float GetTimeRemaining()
    {
        return timerComponent != null ? timerComponent.GetTimeRemaining() : 0f;
    }
    





    public override void Initializate() { }
}