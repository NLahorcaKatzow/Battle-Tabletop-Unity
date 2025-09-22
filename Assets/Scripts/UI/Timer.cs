using System;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using DG.Tweening;

public class Timer : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float maxTime = 120f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerFillImage;
    [SerializeField] private Image teamTurnImage;
    [SerializeField] private Button buttonTurn;
    private float currentTime;
    public bool isRunning = false;
    public bool isPaused = false;
    
    public float CurrentTime => currentTime;
    public float MaxTime => maxTime;
    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;
    
    private void Update()
    {
        if (isRunning && !isPaused)
        {
            UpdateTimer();
        }
    }
    
    public void InitializeTimer()
    {
        ResetTimer();
        UpdateTimerDisplay();
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = 1f;
        }
        BattleController.Instance.OnTurnChange += ResetTimer;
        buttonTurn.onClick.AddListener(BattleController.Instance.NextTurn);
    }
    [Button]
    public void StartTimer()
    {
        if (!isRunning)
        {
            isRunning = true;
            isPaused = false;
            Log("Timer", "Timer started");
        }
    }
    
    [Button]
    public void PauseTimer()
    {
        if (isRunning && !isPaused)
        {
            isPaused = true;
            Log("Timer", "Timer paused");
        }
    }
    
    [Button]
    public void ResumeTimer()
    {
        if (isRunning && isPaused)
        {
            isPaused = false;
            Log("Timer", "Timer resumed");
        }
    }
    
    [Button]
    public void StopTimer()
    {
        isRunning = false;
        isPaused = false;
        Log("Timer", "Timer stopped");
    }
    
    [Button]
    public void ResetTimer()
    {
        currentTime = maxTime;
        isRunning = false;
        isPaused = false;
        UpdateTimerDisplay();
        if (timerFillImage != null)
        {
            timerFillImage.fillAmount = 1f;
        }
        Log("Timer", "Timer reset");
        TeamTurnSelector();
    }

    private void TeamTurnSelector()
    {
        int newPos = BattleController.Instance.GetCurrentTurn() % 2 == 0 ? -130 : 130;
        Color newColor = BattleController.Instance.GetCurrentTurn() % 2 == 0 ? Color.green : Color.red;
        if (BattleController.Instance.GetCurrentTurn() % 2 == 0) buttonTurn.gameObject.SetActive(true);
        else buttonTurn.gameObject.SetActive(false);
        // Animate position and color simultaneously
        var rectTransform = teamTurnImage.GetComponent<RectTransform>();
        var image = teamTurnImage.GetComponent<Image>();

        if (rectTransform != null && image != null)
        {
            // Position animation
            rectTransform.DOAnchorPosX(newPos, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                StartTimer();
            });
            // Color animation to white
            image.DOColor(newColor, 0.5f).SetEase(Ease.InOutSine);

        }
    }

    public void SetMaxTime(float newMaxTime)
    {
        maxTime = newMaxTime;
        if (currentTime > maxTime)
        {
            currentTime = maxTime;
        }
        UpdateTimerDisplay();
        Log("Timer", $"Max time set to {newMaxTime}");
    }
    
    public float GetCurrentTime()
    {
        return currentTime;
    }
    
    public float GetTimeRemaining()
    {
        return Mathf.Max(0, currentTime);
    }
    
    public float GetTimePercentage()
    {
        return maxTime > 0 ? (currentTime / maxTime) : 0f;
    }
    
    private void UpdateTimer()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();
            
            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                BattleController.Instance.OnTimeUp();
                Log("Timer", "Time is up!");
            }
        }
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        
        if (timerFillImage != null)
        {
            float fillAmount = GetTimePercentage();
            timerFillImage.DOFillAmount(fillAmount, 0.1f);
        }
    }
    
    private void Log(string context, string message)
    {
        Debug.Log($"[{context}] {message}");
    }
}
