using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class TimedSliderUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private float durationSeconds = 2f;
    [SerializeField] private float startValue = 1f;
    [SerializeField] private float endValue = 0f;
    [SerializeField] private Ease ease = Ease.Linear;

    private RectTransform rectTransform;
    private Tween activeTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>(true);
        }
    }

    public void Spawn(Vector2 screenPosition)
    {
        PositionAtScreenPoint(screenPosition);
        PlayTween();
    }

    public void Spawn(Vector2 screenPosition, float duration, float fromValue, float toValue, Ease easeOverride = Ease.Unset)
    {
        durationSeconds = duration;
        startValue = fromValue;
        endValue = toValue;
        if (easeOverride != Ease.Unset)
        {
            ease = easeOverride;
        }
        PositionAtScreenPoint(screenPosition);
        PlayTween();
    }

    private void PositionAtScreenPoint(Vector2 screenPosition)
    {
        var canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("TimedSliderUI requires to be under a Canvas to position in screen space.");
            rectTransform.position = screenPosition;
            return;
        }

        Vector2 localPoint;
        var canvasRect = canvas.transform as RectTransform;
        var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, camera, out localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
        else
        {
            rectTransform.position = screenPosition;
        }
    }

    private void PlayTween()
    {
        if (slider == null)
        {
            Debug.LogError("TimedSliderUI requires a Slider assigned or present in children.");
            Destroy(gameObject);
            return;
        }

        slider.value = startValue;

        activeTween?.Kill();
        activeTween = slider.DOValue(endValue, durationSeconds)
            .SetEase(ease)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

    private void OnDestroy()
    {
        activeTween?.Kill();
        activeTween = null;
    }
}


