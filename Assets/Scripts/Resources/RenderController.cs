using UnityEngine;
using Sirenix.OdinInspector;

public class RenderController : MonoBehaviour
{
    public static RenderController Instance { get; private set; }

    [Header("Piece Prefabs")]
    [SerializeField] private GameObject kingPrefab;
    [SerializeField] private GameObject queenPrefab;
    [SerializeField] private GameObject bishopPrefab;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private GameObject pawnPrefab;
    [SerializeField] private GameObject horsePrefab;


    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Instantiates a piece prefab based on the MovementType
    /// </summary>
    /// <param name="movementType">The type of piece to instantiate</param>
    /// <param name="position">World position where to instantiate the piece</param>
    /// <param name="rotation">Rotation of the instantiated piece</param>
    /// <returns>The instantiated GameObject, or null if prefab not found</returns>
    public GameObject InstantiatePieceByMovementType(MovementType movementType, Vector3 position, Transform parent = null)
    {
        GameObject prefabToInstantiate = GetPrefabByMovementType(movementType);

        if (prefabToInstantiate != null)
        {
            GameObject instantiatedPiece = Instantiate(prefabToInstantiate, position, Quaternion.identity, parent);

            Log("RenderController", $"Instantiated {movementType} piece at position {position}");
            return instantiatedPiece;
        }
        else
        {
            Log("RenderController", $"No prefab found for MovementType: {movementType}");
            return null;
        }
    }

    /// <summary>
    /// Gets the prefab GameObject based on MovementType without instantiating
    /// </summary>
    /// <param name="movementType">The type of piece to get</param>
    /// <returns>The prefab GameObject, or null if not found</returns>
    public GameObject GetPrefabByMovementType(MovementType movementType)
    {
        switch (movementType)
        {
            case MovementType.KING:
                return kingPrefab;
            case MovementType.QUEEN:
                return queenPrefab;
            case MovementType.BISHOP:
                return bishopPrefab;
            case MovementType.TOWER:
                return towerPrefab;
            case MovementType.PAWN:
                return pawnPrefab;
            case MovementType.HORSE:
                return horsePrefab;
            case MovementType.NONE:
            default:
                Log("RenderController", $"Unsupported MovementType: {movementType}");
                return null;
        }
    }

    private void Log(string context, string message)
    {
        Debug.Log($"[{context}] {message}");
    }
}
