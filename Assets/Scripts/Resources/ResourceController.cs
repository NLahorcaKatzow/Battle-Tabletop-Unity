using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;

public class ResourceController : MonoBehaviour
{
    public static ResourceController Instance { get; private set; }
    [SerializeField] private bool logs;
    [ShowInInspector]public List<PieceDataClass> piecesData;
    [ShowInInspector]public List<SavedPosition> savedPositions;
    public event Action OnCompleteResourcesLoaded;
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        InitializateResources();
    }

    private void InitializateResources()
    {
        Debug.Log("Initializating Resources");
        try{
            piecesData = new List<PieceDataClass>();
            savedPositions = new List<SavedPosition>();
            LoadPiecesData();
            LoadSavedPositions();
            OnCompleteResourcesLoaded?.Invoke();
        }
        catch(Exception e){
            Debug.LogError($"ResourceController Error: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    public PieceDataClass GetPieceDatabyId(int id)
    {
        return piecesData.FirstOrDefault(piece => piece.id == id);
    }
    
    public Dictionary<int, Vector2Int> GetSavedPositions()
    {
        Dictionary<int, Vector2Int> savedPositionsDict = new Dictionary<int, Vector2Int>();
        foreach(var position in savedPositions){
            if (!savedPositionsDict.ContainsKey(position.id))
            {
                savedPositionsDict.Add(position.id, new Vector2Int(position.x, position.y));
            }
            else
            {
                Debug.LogWarning($"Duplicate key found: {position.id}. Skipping duplicate entry.");
            }
        }
        return savedPositionsDict;
    }
    
    #region Internal Methods

    private void LoadPiecesData()
    {
        string json = Resources.Load<TextAsset>("PiecesData").text;
        piecesData = JsonConvert.DeserializeObject<List<PieceDataClass>>(json);
        Debug.Log("PiecesData loaded");
        Debug.Log(JsonConvert.SerializeObject(piecesData));
        
    }
    
    private void LoadSavedPositions()
    {
        string json = Resources.Load<TextAsset>("PlayerSavedPositions").text;
        savedPositions = JsonConvert.DeserializeObject<List<SavedPosition>>(json);
        Debug.Log("SavedPositions loaded");
        Debug.Log(JsonConvert.SerializeObject(savedPositions));
    }
    
    #endregion

}
