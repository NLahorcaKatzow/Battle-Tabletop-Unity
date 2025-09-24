using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player; // Reference to the player
    public float distanceFromOrigin = 30f; // Always 30 units from origin
    public float followSpeed = 2f; // Speed for DOTween animations
    public float rotationSpeed = 100f; // Speed for free camera rotation
    public float mouseSensitivity = 1f; // Sensitivity for free camera movement
    
    [Header("Free Camera Settings")]
    public KeyCode freeCameraKey = KeyCode.Mouse1; // Right mouse button
    
    private bool isFreeCameraMode = false;
    private Vector3 lastMousePosition;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Tween moveTween;
    private Tween rotateTween;
    private Vector3 currentPlaneNormal = Vector3.up; // Track current plane normal
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize camera position if player is assigned
        if (player != null)
        {
            PositionCameraBehindPlayer();
        }
        else
        {
            Debug.LogError("Player reference not assigned to CameraController!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        
        if (!isFreeCameraMode && player != null)
        {
            FollowPlayer();
        }
        else if (isFreeCameraMode)
        {
            HandleFreeCameraMovement();
        }
    }
    
    void HandleInput()
    {
        // Check for right mouse button - enter free camera while held down
        if (Input.GetKeyDown(freeCameraKey))
        {
            EnterFreeCameraMode();
        }
        
        // Exit free camera when right mouse button is released
        if (Input.GetKeyUp(freeCameraKey) && isFreeCameraMode)
        {
            ExitFreeCameraMode();
        }
        
        // Also exit free camera if any other mouse button is clicked while in free camera mode
        if (isFreeCameraMode && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2)))
        {
            ExitFreeCameraMode();
        }
        
        // Only exit free camera if player moved to a different plane AND we're not holding right mouse button
        if (isFreeCameraMode && !Input.GetKey(freeCameraKey) && HasPlayerMoved())
        {
            ExitFreeCameraMode();
        }
    }
    
    void FollowPlayer()
    {
        if (player == null) return;
        
        // Get the normal of the plane the player is on
        Vector3 playerNormal = GetPlayerPlaneNormal();
        
        // Always ensure camera is at the center of the current plane
        // This handles both plane changes and returning from free camera mode
        if (Vector3.Distance(playerNormal, currentPlaneNormal) > 0.1f || 
            Vector3.Distance(transform.position, Vector3.zero + playerNormal * distanceFromOrigin) > 0.1f)
        {
            currentPlaneNormal = playerNormal;
            
            // Calculate target position: 30 units away from origin in the direction of the plane normal
            Vector3 directionFromOrigin = playerNormal;
            targetPosition = Vector3.zero + directionFromOrigin * distanceFromOrigin;
            
            // Calculate target rotation: look at the center of the plane (origin)
            Vector3 directionToCenter = (Vector3.zero - targetPosition).normalized;
            targetRotation = Quaternion.LookRotation(directionToCenter);
            
            // Smooth movement with DOTween to the center of the plane
            MoveCameraSmooth(targetPosition, targetRotation);
            
            Debug.Log($"Camera repositioned to center of plane with normal: {playerNormal}");
        }
    }
    
    void EnterFreeCameraMode()
    {
        isFreeCameraMode = true;
        lastMousePosition = Input.mousePosition;
        
        // Kill any ongoing tweens
        if (moveTween != null) moveTween.Kill();
        if (rotateTween != null) rotateTween.Kill();
        
        Debug.Log("Entered free camera mode");
    }
    
    void ExitFreeCameraMode()
    {
        isFreeCameraMode = false;
        
        // Return to the center of the player's current plane
        if (player != null)
        {
            // Force repositioning to current player's plane center
            Vector3 playerNormal = GetPlayerPlaneNormal();
            currentPlaneNormal = playerNormal;
            
            // Calculate target position: center of the current plane
            Vector3 directionFromOrigin = playerNormal;
            targetPosition = Vector3.zero + directionFromOrigin * distanceFromOrigin;
            
            // Calculate target rotation: look at the center (origin)
            Vector3 directionToCenter = (Vector3.zero - targetPosition).normalized;
            targetRotation = Quaternion.LookRotation(directionToCenter);
            
            // Smooth movement back to plane center
            MoveCameraSmooth(targetPosition, targetRotation);
            
            Debug.Log($"Camera returned to center of plane with normal: {playerNormal}");
        }
        
        Debug.Log("Exited free camera mode");
    }
    
    void HandleFreeCameraMovement()
    {
        Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
        
        if (mouseDelta.magnitude > 0.1f)
        {
            // Convert mouse movement to rotation around origin with sensitivity
            float horizontalRotation = -mouseDelta.x * rotationSpeed * mouseSensitivity * Time.deltaTime;
            float verticalRotation = mouseDelta.y * rotationSpeed * mouseSensitivity * Time.deltaTime;
            
            // Rotate around origin
            transform.RotateAround(Vector3.zero, Vector3.up, horizontalRotation);
            transform.RotateAround(Vector3.zero, transform.right, verticalRotation);
            
            // Maintain distance from origin
            Vector3 directionFromOrigin = (transform.position - Vector3.zero).normalized;
            transform.position = Vector3.zero + directionFromOrigin * distanceFromOrigin;
            
            // Always look at origin in free camera mode
            transform.LookAt(Vector3.zero);
        }
        
        lastMousePosition = Input.mousePosition;
    }
    
    void MoveCameraSmooth(Vector3 newPosition, Quaternion newRotation)
    {
        // Kill previous tweens to avoid conflicts
        if (moveTween != null) moveTween.Kill();
        if (rotateTween != null) rotateTween.Kill();
        
        // Smooth position movement
        moveTween = transform.DOMove(newPosition, followSpeed).SetEase(Ease.OutCubic);
        
        // Smooth rotation
        rotateTween = transform.DORotateQuaternion(newRotation, followSpeed).SetEase(Ease.OutCubic);
    }
    
    Vector3 GetPlayerPlaneNormal()
    {
        if (player == null) return Vector3.up;
        
        // Determine which face of the cube the player is on based on position
        Vector3 playerPos = player.position;
        Vector3 absPos = new Vector3(Mathf.Abs(playerPos.x), Mathf.Abs(playerPos.y), Mathf.Abs(playerPos.z));
        
        // Find the axis with the maximum absolute value (the face normal)
        if (absPos.x >= absPos.y && absPos.x >= absPos.z)
        {
            return new Vector3(Mathf.Sign(playerPos.x), 0, 0); // X face
        }
        else if (absPos.y >= absPos.x && absPos.y >= absPos.z)
        {
            return new Vector3(0, Mathf.Sign(playerPos.y), 0); // Y face
        }
        else
        {
            return new Vector3(0, 0, Mathf.Sign(playerPos.z)); // Z face
        }
    }
    
    bool HasPlayerMoved()
    {
        if (player == null) return false;
        
        // Check if player has changed planes
        Vector3 playerNormal = GetPlayerPlaneNormal();
        
        // If we're in free camera mode, only check for plane changes, not camera position
        if (isFreeCameraMode)
        {
            return Vector3.Distance(playerNormal, currentPlaneNormal) > 0.1f;
        }
        
        // When not in free camera mode, check both plane changes and camera position
        Vector3 expectedCameraPosition = Vector3.zero + playerNormal * distanceFromOrigin;
        return Vector3.Distance(playerNormal, currentPlaneNormal) > 0.1f || 
               Vector3.Distance(transform.position, expectedCameraPosition) > 0.5f;
    }
    
    void PositionCameraBehindPlayer()
    {
        if (player == null) return;
        
        Vector3 playerNormal = GetPlayerPlaneNormal();
        currentPlaneNormal = playerNormal;
        Vector3 directionFromOrigin = playerNormal;
        Vector3 initialPosition = Vector3.zero + directionFromOrigin * distanceFromOrigin;
        
        transform.position = initialPosition;
        transform.LookAt(Vector3.zero); // Look at the center of the plane (origin)
        
        Debug.Log($"Camera initialized at center of plane with normal: {playerNormal}");
    }
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw a sphere at origin to visualize the 30-unit distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Vector3.zero, distanceFromOrigin);
        
        // Draw line from origin to camera
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, transform.position);
        
        // Show distance
        Gizmos.color = Color.white;
        Vector3 midPoint = transform.position * 0.5f;
        
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }
    }
#endif
}
