using UnityEngine;
using DG.Tweening;

/// <summary>
/// Character controller for cube surface movement with WASD controls and Rigidbody physics.
/// Allows movement on different faces of a 3D cube where each face acts as an independent 2D level.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer = 1;

    [Header("Movement Physics")]
    [SerializeField] private float acceleration = 8f; // Reduced for lerp-based system
    [SerializeField] private float friction = 8f; // Reduced for smoother deceleration
    [SerializeField] private float airFriction = 2f; // Increased for better air control
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float airMaxSpeed = 12f;
    [SerializeField] private float minMovementThreshold = 0.1f;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float maxSurfaceSearchDistance = 15f;

    [Header("Artificial Gravity")]
    [SerializeField] private float gravityRelative = 9.81f;

    // Core components
    private Rigidbody playerRigidbody;

    // Surface orientation system - Z points perpendicular to surface, XY is movement plane
    private Vector3 surfaceNormal = Vector3.up;
    private Vector3 currentGravityDirection = Vector3.down;
    private bool isGrounded = false;

    // Momentum-based landing system
    private Vector3 velocityBeforeLanding = Vector3.zero;
    private float significantVelocityThreshold = 2f;

    #region Unity Lifecycle

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeComponents();
        InitializeDOTween();
        currentGravityDirection = -transform.forward;
    }

    private void Update()
    {
        CaptureVelocityBeforeLanding();
        DetectSurface();
        ProcessInput();
    }

    private void FixedUpdate()
    {
        ApplyArtificialGravity();
    }

    private void OnDestroy()
    {
        // Cleanup if needed
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initialize required components and validate setup
    /// </summary>
    private void InitializeComponents()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        if (playerRigidbody == null)
            Debug.LogError("PlayerController requires a Rigidbody component!");
    }

    /// <summary>
    /// Configure DOTween for optimal performance
    /// </summary>
    private void InitializeDOTween()
    {
        DOTween.SetTweensCapacity(200, 50);
    }

    #endregion

    #region Surface Detection & Alignment

    /// <summary>
    /// Detect cube surface using raycast towards origin and align player orientation
    /// </summary>
    private void DetectSurface()
    {
        Vector3 directionToOrigin = (Vector3.zero - transform.position).normalized;
        float searchDistance = isGrounded ? maxSurfaceSearchDistance : maxSurfaceSearchDistance * 2f;

        if (Physics.Raycast(transform.position, directionToOrigin, out RaycastHit hitInfo, searchDistance, groundLayer))
        {
            float distanceToSurface = Vector3.Distance(transform.position, hitInfo.point);
            isGrounded = distanceToSurface <= groundCheckDistance;
            UpdateSurfaceAlignment(hitInfo.normal);
        }
        else
        {
            // Fallback: calculate normal based on position relative to origin
            Vector3 calculatedNormal = (transform.position - Vector3.zero).normalized;
            UpdateSurfaceAlignment(calculatedNormal);
            isGrounded = false;
        }
    }

    /// <summary>
    /// Capture horizontal velocity while airborne for momentum-based landing system
    /// </summary>
    private void CaptureVelocityBeforeLanding()
    {
        if (!isGrounded)
        {
            Vector3 currentVelocity = playerRigidbody.linearVelocity;
            (Vector3 horizontalVel, Vector3 _) = VectorialUtil.SeparateVelocityComponents(currentVelocity, surfaceNormal);
            velocityBeforeLanding = horizontalVel;
        }
    }

    public Vector3 GetSurfaceNormal()
    {
        return surfaceNormal;
    }

    /// <summary>
    /// Update player orientation to align with surface normal
    /// </summary>
    /// <param name="newSurfaceNormal">Normal vector of the detected surface</param>
    private void UpdateSurfaceAlignment(Vector3 newSurfaceNormal)
    {
        surfaceNormal = newSurfaceNormal;

        // Player forward direction points along surface normal
        Vector3 forwardDirection = surfaceNormal;
        Vector3 upReference = Vector3.up;

        // Handle edge case when normal is parallel to world up
        if (Mathf.Abs(Vector3.Dot(forwardDirection, Vector3.up)) > 0.9f)
        {
            upReference = Vector3.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(forwardDirection, upReference);
        transform.rotation = targetRotation;

        // Gravity is perpendicular to surface (opposite to normal)
        currentGravityDirection = -surfaceNormal;
    }

    #endregion

    #region Input Processing

    /// <summary>
    /// Process player input for movement and jumping
    /// </summary>
    private void ProcessInput()
    {
        Vector2 inputVector = GetMovementInput();
        //bool jumpInput = GetJumpInput();

        HandleMovement(inputVector);
        //HandleJump(jumpInput);
    }

    /// <summary>
    /// Get normalized movement input from WASD keys
    /// </summary>
    /// <returns>2D input vector for XY plane movement</returns>
    private Vector2 GetMovementInput()
    {
        return new Vector2(
            -Input.GetAxis("Horizontal"), // A/D keys -> X local movement
            Input.GetAxis("Vertical")     // W/S keys -> Y local movement
        );
    }

    /// <summary>
    /// Get jump input from spacebar
    /// </summary>
    /// <returns>True if jump key was pressed this frame</returns>
    private bool GetJumpInput()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    #endregion

    #region Movement System

    /// <summary>
    /// Handle player movement in XY plane relative to surface
    /// </summary>
    /// <param name="input">2D input vector</param>
    private void HandleMovement(Vector2 input)
    {
        Vector3 worldMovement = Vector3.zero;

        if (input.magnitude >= 0.1f)
        {
            worldMovement = VectorialUtil.ConvertInputToXYPlaneMovement(input, transform);
        }

        ApplyMovementForces(worldMovement, input.magnitude);
    }

    /// <summary>
    /// Apply movement forces based on input and current state (grounded/airborne)
    /// </summary>
    /// <param name="direction">World space movement direction</param>
    /// <param name="inputMagnitude">Magnitude of input for acceleration scaling</param>
    private void ApplyMovementForces(Vector3 direction, float inputMagnitude)
    {
        // No need for braking cancellation in new system

        Vector3 currentVelocity = playerRigidbody.linearVelocity;
        (Vector3 surfaceParallelVelocity, Vector3 surfacePerpendicularVelocity) =
            VectorialUtil.SeparateVelocityComponents(currentVelocity, surfaceNormal);

        if (inputMagnitude > 0.1f)
        {
            ApplyDirectionalMovement(direction, inputMagnitude, surfaceParallelVelocity, surfacePerpendicularVelocity);
        }
        else
        {
            ApplyFrictionAndBraking(surfaceParallelVelocity, surfacePerpendicularVelocity);
        }

        LimitSurfaceVelocity();
    }

    /// <summary>
    /// Apply direct movement using velocity interpolation instead of forces
    /// </summary>
    private void ApplyDirectionalMovement(Vector3 direction, float inputMagnitude, Vector3 surfaceParallelVelocity, Vector3 surfacePerpendicularVelocity)
    {
        // Clear captured velocity if grounded
        if (isGrounded)
        {
            velocityBeforeLanding = Vector3.zero;
        }

        // Calculate target velocity
        float currentMaxSpeed = isGrounded ? maxSpeed : airMaxSpeed;
        Vector3 targetVelocity = direction * movementSpeed * inputMagnitude;
        targetVelocity = Vector3.ClampMagnitude(targetVelocity, currentMaxSpeed);

        // Use lerp for smooth transitions instead of additive forces
        float lerpSpeed = isGrounded ? acceleration * Time.fixedDeltaTime : acceleration * Time.fixedDeltaTime * 0.7f;
        Vector3 newParallelVelocity = Vector3.Lerp(surfaceParallelVelocity, targetVelocity, lerpSpeed);

        // Apply the new velocity directly
        Vector3 finalVelocity = newParallelVelocity + surfacePerpendicularVelocity;
        playerRigidbody.linearVelocity = finalVelocity;
    }

    /// <summary>
    /// Apply friction and braking when no input is provided
    /// </summary>
    private void ApplyFrictionAndBraking(Vector3 surfaceParallelVelocity, Vector3 surfacePerpendicularVelocity)
    {
        if (isGrounded)
        {
            // Check if player had significant momentum before landing
            bool hadSignificantMomentum = velocityBeforeLanding.magnitude > significantVelocityThreshold;

            if (hadSignificantMomentum)
            {
                // Apply light friction to maintain fluidity
                ApplyVelocityFriction(surfaceParallelVelocity, surfacePerpendicularVelocity, airFriction);
                velocityBeforeLanding = Vector3.zero;
            }
            else
            {
                // Apply stronger ground friction
                ApplyVelocityFriction(surfaceParallelVelocity, surfacePerpendicularVelocity, friction);
            }
        }
        else
        {
            // Light friction in air for fluid control
            ApplyVelocityFriction(surfaceParallelVelocity, surfacePerpendicularVelocity, airFriction);
        }
    }

    /// <summary>
    /// Apply velocity-based friction directly to rigidbody velocity
    /// </summary>
    private void ApplyVelocityFriction(Vector3 surfaceParallelVelocity, Vector3 surfacePerpendicularVelocity, float frictionRate)
    {
        if (surfaceParallelVelocity.magnitude > minMovementThreshold)
        {
            // Apply friction using velocity lerp for smooth deceleration
            float frictionFactor = 1f - (frictionRate * Time.fixedDeltaTime);
            frictionFactor = Mathf.Clamp01(frictionFactor);

            Vector3 newParallelVelocity = surfaceParallelVelocity * frictionFactor;
            Vector3 finalVelocity = newParallelVelocity + surfacePerpendicularVelocity;

            playerRigidbody.linearVelocity = finalVelocity;
        }
        else
        {
            // Stop immediately if velocity is very low
            playerRigidbody.linearVelocity = surfacePerpendicularVelocity;
        }
    }

    /// <summary>
    /// Limit surface velocity to maximum allowed speed
    /// </summary>
    private void LimitSurfaceVelocity()
    {
        Vector3 currentVelocity = playerRigidbody.linearVelocity;
        (Vector3 surfaceParallelVelocity, Vector3 surfacePerpendicularVelocity) =
            VectorialUtil.SeparateVelocityComponents(currentVelocity, surfaceNormal);

        float currentMaxSpeed = isGrounded ? maxSpeed : airMaxSpeed;

        if (surfaceParallelVelocity.magnitude > currentMaxSpeed)
        {
            Vector3 limitedParallelVelocity = surfaceParallelVelocity.normalized * currentMaxSpeed;
            Vector3 newVelocity = limitedParallelVelocity + surfacePerpendicularVelocity;
            playerRigidbody.linearVelocity = newVelocity;
        }
    }

    #endregion

    #region Jump System

    /// <summary>
    /// Handle jump input and execution
    /// </summary>
    /// <param name="shouldJump">Whether jump input was detected</param>
    private void HandleJump(bool shouldJump)
    {
        if (shouldJump && isGrounded)
        {
            ExecuteJump();
        }
    }

    /// <summary>
    /// Execute jump while preserving horizontal momentum
    /// </summary>
    private void ExecuteJump()
    {
        Vector3 jumpDirection = surfaceNormal.normalized;
        Vector3 currentVelocity = playerRigidbody.linearVelocity;

        // Separate horizontal and vertical components
        (Vector3 horizontalVelocity, Vector3 _) =
            VectorialUtil.SeparateVelocityComponents(currentVelocity, surfaceNormal);

        // Apply jump while preserving horizontal movement
        Vector3 jumpVelocity = jumpDirection * jumpForce;
        Vector3 newVelocity = horizontalVelocity + jumpVelocity;

        playerRigidbody.linearVelocity = newVelocity;

        // Visual feedback
        transform.DOKill();
        transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1, 0.5f);

        Debug.Log("Jump executed - preserving horizontal momentum");
    }

    #endregion

    #region Gravity System

    /// <summary>
    /// Apply artificial gravity perpendicular to current surface
    /// </summary>
    private void ApplyArtificialGravity()
    {
        float gravityMultiplier = isGrounded ? 0.1f : 1f; // Reduced gravity when grounded
        Vector3 gravityForce = VectorialUtil.CalculateGravityForce(currentGravityDirection, gravityRelative * gravityMultiplier);
        playerRigidbody.AddForce(gravityForce, ForceMode.Acceleration);
    }

    #endregion
}
