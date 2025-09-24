using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class FloatingAnimation : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float floatDistance = 0.5f;
    [SerializeField] private float floatDuration = 2f;
    [SerializeField] private Ease floatEase = Ease.InOutSine;
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Debug")]
    [ShowInInspector, ReadOnly] private Vector3 originalPosition;
    [ShowInInspector, ReadOnly] private bool isFloating = false;
    
    private Tween floatingTween;
    private Transform targetTransform;

    private void Awake()
    {
        targetTransform = transform;
        UpdateOriginalPosition();
    }

    private void Start()
    {
        if (autoStart)
        {
            StartFloating();
        }
    }

    [Button("Start Floating")]
    public void StartFloating()
    {
        if (isFloating) return;

        StopFloating();
        
        // Actualizar la posición base antes de comenzar la animación
        UpdateOriginalPosition();
        
        Vector3 upPosition = originalPosition + Vector3.up * floatDistance;
        Vector3 downPosition = originalPosition - Vector3.up * floatDistance;

        // Crear secuencia de movimiento que siempre vuelve a la posición original
        Sequence floatingSequence = DOTween.Sequence();
        
        floatingSequence.Append(targetTransform.DOLocalMoveY(upPosition.y, floatDuration / 2f).SetEase(floatEase))
                       .Append(targetTransform.DOLocalMoveY(downPosition.y, floatDuration).SetEase(floatEase))
                       .Append(targetTransform.DOLocalMoveY(originalPosition.y, floatDuration / 2f).SetEase(floatEase))
                       .SetLoops(-1, LoopType.Restart)
                       .SetUpdate(useUnscaledTime)
                       .SetLink(gameObject);

        floatingTween = floatingSequence;
        isFloating = true;
    }

    [Button("Stop Floating")]
    public void StopFloating()
    {
        if (floatingTween != null)
        {
            floatingTween.Kill();
            floatingTween = null;
        }
        isFloating = false;
    }

    [Button("Reset Position")]
    public void ResetToOriginalPosition()
    {
        StopFloating();
        targetTransform.localPosition = originalPosition;
    }

    public void SetFloatDistance(float distance)
    {
        floatDistance = distance;
        if (isFloating)
        {
            StopFloating();
            StartFloating();
        }
    }

    public void SetFloatDuration(float duration)
    {
        floatDuration = duration;
        if (isFloating)
        {
            StopFloating();
            StartFloating();
        }
    }

    public void SetFloatEase(Ease ease)
    {
        floatEase = ease;
        if (isFloating)
        {
            StopFloating();
            StartFloating();
        }
    }

    private void OnDisable()
    {
        StopFloating();
    }

    private void OnDestroy()
    {
        StopFloating();
    }

    // Método para cambiar la posición original (útil si el objeto se mueve)
    public void UpdateOriginalPosition()
    {
        bool wasFloating = isFloating;
        StopFloating();
        originalPosition = targetTransform.localPosition;
        if (wasFloating)
        {
            StartFloating();
        }
    }
    
    // Método para mover el objeto y actualizar la posición base automáticamente
    public void MoveToPosition(Vector3 newPosition)
    {
        bool wasFloating = isFloating;
        StopFloating();
        targetTransform.localPosition = newPosition;
        originalPosition = newPosition;
        if (wasFloating)
        {
            StartFloating();
        }
    }
    
    // Método para mover solo en Y y mantener X,Z
    public void MoveToY(float newY)
    {
        Vector3 newPos = originalPosition;
        newPos.y = newY;
        MoveToPosition(newPos);
    }

    private void OnValidate()
    {
        if (floatDistance < 0f) floatDistance = 0f;
        if (floatDuration <= 0f) floatDuration = 0.1f;
    }
}
