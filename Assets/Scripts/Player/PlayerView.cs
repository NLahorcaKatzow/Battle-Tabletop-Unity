using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool smoothRotation = true;
    
    [Header("Constraints")]
    [SerializeField] private bool constrainRotation = false;
    [SerializeField] private float minAngle = -45f;
    [SerializeField] private float maxAngle = 45f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLines = true;
    
    private Camera playerCamera;
    private Vector3 mouseWorldPosition;
    private float targetAngle;
    
    // Surface orientation - obtained from PlayerController
    private Vector3 surfaceRight = Vector3.right;
    private Vector3 surfaceUp = Vector3.forward;
    
    void Start()
    {
        // Get the main camera or find it
        playerCamera = Camera.main;
        
        if (playerCamera == null)
        {
            Debug.LogError("PlayerView: No camera found in the scene!");
        }
    }
    
    void Update()
    {
        UpdateSurfaceOrientation();
        HandleMouseLook();
    }
    
    /// <summary>
    /// Update surface orientation based on player's current surface normal
    /// </summary>
    private void UpdateSurfaceOrientation()
    {
        // Get surface normal from PlayerController
        Vector3 surfaceNormal = PlayerController.Instance.GetSurfaceNormal();
        
        // Calculate surface-aligned coordinate system
        Vector3 worldUp = Vector3.up;
        
        // Handle edge case when surface normal is parallel to world up
        if (Mathf.Abs(Vector3.Dot(surfaceNormal, Vector3.up)) > 0.9f)
        {
            worldUp = Vector3.forward;
        }
        
        // Create orthogonal basis for the surface plane
        surfaceRight = Vector3.Cross(surfaceNormal, worldUp).normalized;
        surfaceUp = Vector3.Cross(surfaceRight, surfaceNormal).normalized;
    }
    
    private void HandleMouseLook()
    {
        if (playerCamera == null) return;
        
        // Get surface normal from PlayerController
        Vector3 surfaceNormal = PlayerController.Instance.GetSurfaceNormal();
        
        // Get mouse position and project it onto the surface plane
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseWorldPosition = GetMousePositionOnSurface(mouseScreenPosition);
        
        // Calculate direction from player to mouse within the surface plane
        Vector3 directionToMouse = mouseWorldPosition - transform.position;
        
        // Project direction onto surface plane (remove component along surface normal)
        Vector3 surfaceDirection = Vector3.ProjectOnPlane(directionToMouse, surfaceNormal);
        
        if (surfaceDirection.magnitude > 0.01f)
        {
            // Calculate angle in surface coordinate system
            float angleInSurface = Mathf.Atan2(
                Vector3.Dot(surfaceDirection, surfaceUp),
                -Vector3.Dot(surfaceDirection, surfaceRight)
            ) * Mathf.Rad2Deg;
            
            targetAngle = angleInSurface;
            
            // Apply angle constraints if enabled
            if (constrainRotation)
            {
                targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);
            }
            
            // Create target rotation that maintains surface alignment
            // First, align with surface normal, then rotate on surface plane
            Quaternion surfaceAlignment = Quaternion.LookRotation(surfaceNormal, surfaceUp);
            Quaternion surfaceRotation = Quaternion.AngleAxis(targetAngle, surfaceNormal);
            Quaternion targetRotation = surfaceRotation * surfaceAlignment;
            
            // Apply rotation
            if (smoothRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
        
        // Debug visualization
        if (showDebugLines)
        {
            Debug.DrawLine(transform.position, mouseWorldPosition, Color.red);
            Debug.DrawRay(transform.position, surfaceNormal * 2f, Color.blue);
            Debug.DrawRay(transform.position, surfaceRight * 1f, Color.green);
            Debug.DrawRay(transform.position, surfaceUp * 1f, Color.yellow);
        }
    }
    
    /// <summary>
    /// Project mouse position onto the surface plane where the player is standing
    /// </summary>
    /// <param name="mouseScreenPosition">Mouse position in screen coordinates</param>
    /// <returns>Mouse position projected onto the surface plane</returns>
    private Vector3 GetMousePositionOnSurface(Vector3 mouseScreenPosition)
    {
        // Get surface normal from PlayerController
        Vector3 surfaceNormal = PlayerController.Instance.GetSurfaceNormal();
        
        Ray cameraRay = playerCamera.ScreenPointToRay(mouseScreenPosition);
        
        // Create a plane on the surface where the player is standing
        Plane surfacePlane = new Plane(surfaceNormal, transform.position);
        
        if (surfacePlane.Raycast(cameraRay, out float distance))
        {
            return cameraRay.GetPoint(distance);
        }
        
        // Fallback: project mouse position at player's distance from camera
        Vector3 cameraToPlayer = transform.position - playerCamera.transform.position;
        float playerDistance = cameraToPlayer.magnitude;
        
        Vector3 mouseWorldPos = playerCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, playerDistance)
        );
        
        // Project onto surface plane
        return Vector3.ProjectOnPlane(mouseWorldPos - transform.position, surfaceNormal) + transform.position;
    }
    
    /// <summary>
    /// Get the current mouse world position on the surface
    /// </summary>
    /// <returns>Mouse position in world coordinates projected onto surface</returns>
    public Vector3 GetMouseWorldPosition()
    {
        return mouseWorldPosition;
    }
    
    /// <summary>
    /// Get the angle towards the mouse position relative to surface orientation
    /// </summary>
    /// <returns>Angle in degrees on the surface plane</returns>
    public float GetTargetAngle()
    {
        return targetAngle;
    }
    
    /// <summary>
    /// Get the current surface normal vector from PlayerController
    /// </summary>
    /// <returns>Surface normal vector</returns>
    public Vector3 GetSurfaceNormal()
    {
        return PlayerController.Instance.GetSurfaceNormal();
    }
    
    /// <summary>
    /// Get the surface right vector (X-axis of surface coordinate system)
    /// </summary>
    /// <returns>Surface right vector</returns>
    public Vector3 GetSurfaceRight()
    {
        return surfaceRight;
    }
    
    /// <summary>
    /// Get the surface up vector (Y-axis of surface coordinate system)
    /// </summary>
    /// <returns>Surface up vector</returns>
    public Vector3 GetSurfaceUp()
    {
        return surfaceUp;
    }
    
    /// <summary>
    /// Set whether to use smooth rotation or instant rotation
    /// </summary>
    /// <param name="smooth">True for smooth rotation, false for instant</param>
    public void SetSmoothRotation(bool smooth)
    {
        smoothRotation = smooth;
    }
    
    /// <summary>
    /// Set the rotation speed for smooth rotation
    /// </summary>
    /// <param name="speed">New rotation speed</param>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(0f, speed);
    }
}
