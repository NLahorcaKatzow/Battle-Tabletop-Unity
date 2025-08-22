using Sirenix.OdinInspector.Editor;
using UnityEngine;

/// <summary>
/// Singleton UI controller that exposes a parent Transform
/// for dynamic UI instantiation.
/// </summary>
public sealed class UIController : MonoBehaviour
{
    /// <summary>
    /// Global singleton instance.
    /// </summary>
    public static UIController Instance { get; private set; }

    [Header("Parent for dynamically instantiated UI")]
    [Tooltip("Optional. If not set, this GameObject's transform will be used.")]
    [SerializeField] private Transform uiParent;

    private void Awake()
    {
        // Enforce singleton instance
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate UIController detected. Destroying this instance.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Fallback to this transform if none provided
        if (uiParent == null)
        {
            uiParent = transform;
        }
    }

    /// <summary>
    /// Returns the Transform used as parent for dynamically created UI.
    /// If no explicit parent was set, returns this component's transform.
    /// </summary>
    public Transform GetParentTransform()
    {
        return uiParent != null ? uiParent : transform;
    }

    /// <summary>
    /// Static helper to obtain the UI parent Transform from anywhere.
    /// Returns null if the singleton is not present in the scene.
    /// </summary>
    public static Transform GetParent()
    {
        return Instance != null ? Instance.GetParentTransform() : null;
    }
    
    public static Transform GetTransform()
    {
        return Instance != null ? Instance.transform : null;
    }
}


