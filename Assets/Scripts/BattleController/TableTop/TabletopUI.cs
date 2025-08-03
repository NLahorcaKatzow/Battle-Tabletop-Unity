using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class TabletopUI : BaseController
{

    [SerializeField] private GameObject tabletopUI;
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject piecePrefab;


    public void InitializateUI(Dictionary<int, Vector2Int> savedPositions)
    {
        foreach (var piece in ResourceController.Instance.piecesData)
        {
            if (!savedPositions.ContainsKey(piece.id)) continue;
            Log("TabletopController", "Instantiating piece: " + piece.id);
            Vector3 position = grid.GetCellCenterWorld(new Vector3Int(savedPositions[piece.id].x, 0, savedPositions[piece.id].y));
            var pieceGO = Instantiate(piecePrefab, position, Quaternion.identity, grid.transform);
            var pieceController = pieceGO.GetComponent<PieceController>();
            pieceController.SetPieceData(piece);
            pieceController.SetPosition(savedPositions[piece.id]);
            TabletopController.Instance.GetCurrentPlayerPieces().Add(piece.id, new PieceDataGO
            {
                pieceController = pieceController,
                go = pieceGO
            });
        }
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

    public void ShowPieceInfo(PieceDataClass pieceData){
    //TODO: Show piece info
    }
    
    public void ShowPieceActions(PieceDataClass pieceData){
        //TODO: Show piece actions
        
    }







    public override void Initializate() { }
}