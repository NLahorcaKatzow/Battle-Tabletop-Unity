using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PieceDataGO
{

    public PieceController pieceController;
    public GameObject go;
}

public class TabletopController : BaseController{
    public static TabletopController Instance { get; private set; }


    [SerializeField] private Dictionary<int, PieceDataGO> currentPiecesInTabletop;
    [SerializeField] private TabletopUI tabletopUI;
    
    public delegate void PieceSelected(int id);
    public event PieceSelected OnPieceSelected;

    //guardado de posiciones
    // calculo de movimiento
    private void Awake(){
        Instance = this;
    }

    public override void Initializate()
    {
        Log("TabletopController", "Initializated");
        Log("TabletopController", ResourceController.Instance.piecesData);
        currentPiecesInTabletop = new Dictionary<int, PieceDataGO>();
        Dictionary<int, Vector2Int> savedPositions = ResourceController.Instance.GetSavedPositions();
        tabletopUI.InitializateUI(savedPositions);
    }
    
    private void Update(){
        if (Input.GetMouseButtonDown(0)) // Left mouse button click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Piece")))
            {
                Log("TabletopController", $"Clicked on piece: {hit.collider.name}");
                // Aquí puedes agregar la lógica para manejar el click en la pieza
                OnPieceSelected?.Invoke(hit.collider.gameObject.GetInstanceID());
            }
        }
    }
    public PieceController GetPieceController(GameObject go)
    {
        return currentPiecesInTabletop.FirstOrDefault(piece => piece.Value.go == go).Value.pieceController;
    }
    
    public Dictionary<int, PieceDataGO> GetCurrentPlayerPieces()
    {
        return currentPiecesInTabletop;
    }
    
    public PieceDataClass GetPieceData(int id)
    {
        return currentPiecesInTabletop.FirstOrDefault(piece => piece.Value.pieceController.generatedId == id).Value.pieceController.pieceData;
    }
    
    public void ShowPieceInfo(int id){
        tabletopUI.ShowPieceInfo(GetPieceData(id));
        Log("TabletopController", $"Showing piece info: {id}");
    }
    
    public void ShowPieceActions(int id){
        tabletopUI.ShowPieceActions(GetPieceData(id));
        Log("TabletopController", $"Showing piece actions: {id}");
    }
    #region Internal Methods
    internal void ClearTabletop()
    {
        foreach(var piece in currentPiecesInTabletop){
            Destroy(piece.Value.go);
        }
        currentPiecesInTabletop.Clear();
    }
    #endregion


}
