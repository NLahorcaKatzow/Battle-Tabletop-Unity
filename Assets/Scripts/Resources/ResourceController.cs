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
    [ShowInInspector] public List<SavedPosition> enemyPositions;
    [ShowInInspector] public List<SavedPosition> savedPositionsNewTurn;
    [ShowInInspector] public List<SavedPosition> enemyPositionsNewTurn;
    [ShowInInspector] public GameConfigs gameConfigs;
    [ShowInInspector] public List<HealthPickUpClass> healthPickUps;
    [SerializeField] private string enemyPositionsPresetResourcePath = "Data/EnemyPositionsPreset1";
    public event Action OnCompleteResourcesLoaded;
    public bool IsReady { get; private set; }
    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
            enemyPositions = new List<SavedPosition>();
            healthPickUps = new List<HealthPickUpClass>();
            LoadPiecesData();
            LoadSavedPositions();
            LoadEnemyPositions();
            LoadSavedPositionsNewTurn();
            LoadEnemyPositionsNewTurn();
            LoadGameConfigs();
            LoadHealthPickUps();
            IsReady = true;
            OnCompleteResourcesLoaded?.Invoke();
            Debug.Log("Resources loaded");
        }
        catch(Exception e){
            Debug.LogError($"ResourceController Error: {e.Message}\nStackTrace: {e.StackTrace}");
        }
    }

    public PieceDataClass GetPieceDatabyId(int id)
    {
        return piecesData.FirstOrDefault(piece => piece.id == id);
    }    
    public PieceDataClass GetPieceDatabyId(MovementType type)
    {
        return piecesData.FirstOrDefault(piece => piece.movementType == type);
    }
    
    public Dictionary<int, SavedPosition> GetSavedPositions()
    {
        Dictionary<int, SavedPosition> savedPositionsDict = new Dictionary<int, SavedPosition>();
        foreach(var position in savedPositions){
            if (!savedPositionsDict.ContainsKey(position.id))
            {
                savedPositionsDict.Add(position.id, position);
            }
            else
            {
                Debug.LogWarning($"Duplicate key found: {position.id}. Skipping duplicate entry.");
            }
        }
        return savedPositionsDict;
    }
    public Dictionary<int, SavedPosition> GetSavedPositionsNewTurn()
    {
        Dictionary<int, SavedPosition> savedPositionsDict = new Dictionary<int, SavedPosition>();
        foreach(var position in savedPositionsNewTurn){
            if (!savedPositionsDict.ContainsKey(position.id))
            {
                savedPositionsDict.Add(position.id, position);
            }
        }
        return savedPositionsDict;
    }

    public Dictionary<int, SavedPosition> GetEnemyPositions()
    {
        Dictionary<int, SavedPosition> enemyPositionsDict = new Dictionary<int, SavedPosition>();
        foreach (var position in enemyPositions)
        {
            if (!enemyPositionsDict.ContainsKey(position.id))
            {
                enemyPositionsDict.Add(position.id, position);
            }
            else
            {
                Debug.LogWarning($"Duplicate enemy key found: {position.id}. Skipping duplicate entry.");
            }
        }
        return enemyPositionsDict;
    }
    public Dictionary<int, SavedPosition> GetEnemyPositionsNewTurn()
    {
        Dictionary<int, SavedPosition> enemyPositionsDict = new Dictionary<int, SavedPosition>();
        foreach(var position in enemyPositionsNewTurn){
            if (!enemyPositionsDict.ContainsKey(position.id))
            {
                enemyPositionsDict.Add(position.id, position);
            }
        }
        return enemyPositionsDict;
    }
    
    public GameConfigs GetGameConfigs()
    {
        return gameConfigs;
    }
    
    public List<HealthPickUpClass> GetHealthPickUps()
    {
        return healthPickUps;
    }
    
    public HealthPickUpClass GetHealthPickUpById(int id)
    {
        return healthPickUps.FirstOrDefault(pickup => pickup.id == id);
    }
    
    public List<HealthPickUpClass> GetHealthPickUpsByRarity(PickUpRarity rarity)
    {
        return healthPickUps.Where(pickup => pickup.rarity == rarity).ToList();
    }
    
    #region Internal Methods

    private void LoadPiecesData()
    {
        string json = Resources.Load<TextAsset>("Data/PiecesData").text;
        piecesData = JsonConvert.DeserializeObject<List<PieceDataClass>>(json);
        Debug.Log("PiecesData loaded");
        Debug.Log(JsonConvert.SerializeObject(piecesData));
        
    }
    
    private void LoadSavedPositions()
    {
        string json = Resources.Load<TextAsset>("Data/PlayerSavedPositions").text;
        savedPositions = JsonConvert.DeserializeObject<List<SavedPosition>>(json);
        Debug.Log("SavedPositions loaded");
        Debug.Log(JsonConvert.SerializeObject(savedPositions));
    }    
    private void LoadSavedPositionsNewTurn()
    {
        string json = Resources.Load<TextAsset>("Data/PlayerSavedPositions2").text;
        savedPositionsNewTurn = JsonConvert.DeserializeObject<List<SavedPosition>>(json);
        Debug.Log("SavedPositions loaded");
        Debug.Log(JsonConvert.SerializeObject(savedPositionsNewTurn));
    }

    private void LoadEnemyPositions()
    {
        var textAsset = Resources.Load<TextAsset>(enemyPositionsPresetResourcePath);
        if (textAsset == null)
        {
            Debug.LogError($"Enemy positions preset not found at '{enemyPositionsPresetResourcePath}'.");
            enemyPositions = new List<SavedPosition>();
            return;
        }
        enemyPositions = JsonConvert.DeserializeObject<List<SavedPosition>>(textAsset.text);
        Debug.Log($"EnemyPositions loaded from {enemyPositionsPresetResourcePath}");
        Debug.Log(JsonConvert.SerializeObject(enemyPositions));
    }
    private void LoadEnemyPositionsNewTurn()
    {
        var textAsset = Resources.Load<TextAsset>("Data/EnemyPositionsPreset2");
        if (textAsset == null)
        {
            Debug.LogError($"Enemy positions preset not found at 'EnemyPositionsPreset2'.");
            enemyPositionsNewTurn = new List<SavedPosition>();
            return;
        }
        enemyPositionsNewTurn = JsonConvert.DeserializeObject<List<SavedPosition>>(textAsset.text);
        Debug.Log($"EnemyPositions loaded from EnemyPositionsPreset2");
        Debug.Log(JsonConvert.SerializeObject(enemyPositionsNewTurn));
    }

    private void LoadGameConfigs()
    {
        var textAsset = Resources.Load<TextAsset>("Data/GameConfigs");
        gameConfigs = JsonConvert.DeserializeObject<GameConfigs>(textAsset.text);
        Debug.Log("GameConfigs loaded");
        Debug.Log(JsonConvert.SerializeObject(gameConfigs));
    }
    
    private void LoadHealthPickUps()
    {
        var textAsset = Resources.Load<TextAsset>("Data/HealthPickUps");
        if (textAsset == null)
        {
            Debug.LogError("HealthPickUps data not found at 'Data/HealthPickUps'.");
            healthPickUps = new List<HealthPickUpClass>();
            return;
        }
        healthPickUps = JsonConvert.DeserializeObject<List<HealthPickUpClass>>(textAsset.text);
        Debug.Log("HealthPickUps loaded");
        Debug.Log(JsonConvert.SerializeObject(healthPickUps));
    }
    
    #endregion

}
