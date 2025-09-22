using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] private string[] levelScenes;
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private float transitionDelay = 2f;
    [SerializeField] private bool debugMode = true;
    
    [Header("Transition Effects")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Ease fadeEaseIn = Ease.InOutQuad;
    [SerializeField] private Ease fadeEaseOut = Ease.InOutQuad;
    
    [Header("Death UI")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private CanvasGroup deathCanvasGroup;
    [SerializeField] private Button restartButton;
    [SerializeField] private float deathUIFadeDuration = 0.5f;

    [Header("Victory UI")]
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private CanvasGroup victoryCanvasGroup;
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private float victoryUIFadeDuration = 0.5f;
    
    public static SceneManager Instance;
    private int currentLevelIndex = 0;
    private bool isTransitioning = false;
    private bool isDeathUIActive = false;
    private bool isVictoryUIActive = false;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (debugMode)
                Debug.Log("SceneManager singleton created and set to DontDestroyOnLoad");
        }
        else if (Instance != this)
        {
            if (debugMode)
                Debug.Log("Duplicate SceneManager found, destroying...");
            Destroy(gameObject);
            return;
        }
        
        InitializeCurrentLevel();
    }
    
    void Start()
    {
        if (fadePanel == null)
        {
            SetupFadePanel();
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
    }
    
    private void InitializeCurrentLevel()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        for (int i = 0; i < levelScenes.Length; i++)
        {
            if (levelScenes[i] == currentScene)
            {
                currentLevelIndex = i;
                if (debugMode)
                    Debug.Log($"Current level index set to: {currentLevelIndex} (Scene: {currentScene})");
                return;
            }
        }
        
        if (debugMode)
            Debug.Log($"Current scene '{currentScene}' not found in level list. Starting from level 0.");
    }
    
    private void SetupFadePanel()
    {
        if (fadePanel == null)
        {
            GameObject fadeObject = new GameObject("FadePanel");
            Canvas canvas = fadeObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            fadePanel = fadeObject.AddComponent<CanvasGroup>();
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
            
            UnityEngine.UI.Image image = fadeObject.AddComponent<UnityEngine.UI.Image>();
            image.color = Color.black;
            
            DontDestroyOnLoad(fadeObject);
            
            if (debugMode)
                Debug.Log("Created default fade panel");
        }
    }
    
    private void OnRestartButtonClicked()
    {
        if (debugMode)
            Debug.Log("Restart button clicked - restarting current level");
        
        HideDeathUIAndRestart();
    }
    
    private void HideDeathUIAndRestart()
    {
        if (deathCanvasGroup != null)
        {
            deathCanvasGroup.DOFade(0f, deathUIFadeDuration)
                .OnComplete(() => {
                    deathCanvasGroup.blocksRaycasts = false;
                    deathCanvasGroup.interactable = false;
                    deathUI.SetActive(false);
                    isDeathUIActive = false;
                    RestartCurrentLevel();
                });
        }
        else
        {
            isDeathUIActive = false;
            RestartCurrentLevel();
        }
    }
    
    public void ShowDeathUI()
    {
        if (isDeathUIActive || isTransitioning)
        {
            if (debugMode)
                Debug.Log("Death UI already active or transitioning, ignoring ShowDeathUI call");
            return;
        }
        
        ShowDeathUIAnimation();
    }
    
    public void ShowVictoryUI()
    {
        if (isVictoryUIActive || isTransitioning)
        {
            if (debugMode)
                Debug.Log("Victory UI already active or transitioning, ignoring ShowVictoryUI call");
            return;
        }

        ShowVictoryUIAnimation();
    }
    
    private void ShowDeathUIAnimation()
    {
        isDeathUIActive = true;
        
        if (debugMode)
            Debug.Log("Showing death UI");
        
        if (deathUI != null && deathCanvasGroup != null)
        {
            deathUI.SetActive(true);
            deathCanvasGroup.blocksRaycasts = true;
            deathCanvasGroup.interactable = true;
            
            deathCanvasGroup.DOFade(1f, deathUIFadeDuration)
                .OnComplete(() => {
                    if (debugMode)
                        Debug.Log("Death UI fully visible");
                });
        }
    }

    private void ShowVictoryUIAnimation()
    {
        isVictoryUIActive = true;
        
        if (victoryUI != null && victoryCanvasGroup != null)
        {
            victoryUI.SetActive(true);
            victoryCanvasGroup.blocksRaycasts = true;
            victoryCanvasGroup.interactable = true;
            
            victoryCanvasGroup.DOFade(1f, victoryUIFadeDuration)
                .OnComplete(() => {
                    if (debugMode)
                        Debug.Log("Victory UI fully visible");
                });
        }
        
    }

    private void HideVictoryUIAnimation()
    {
        isVictoryUIActive = false;
        if(victoryCanvasGroup != null)
        {
            victoryCanvasGroup.DOFade(0f, victoryUIFadeDuration)
                .OnComplete(() => {
                    victoryCanvasGroup.blocksRaycasts = false;
                    victoryCanvasGroup.interactable = false;
                    victoryUI.SetActive(false);
                    isVictoryUIActive = false;
                });
        }
        else
        {
            isVictoryUIActive = false;
        }
    }
    
    public void HideDeathUI()
    {
        if (!isDeathUIActive)
        {
            if (debugMode)
                Debug.Log("Death UI not active, ignoring HideDeathUI call");
            return;
        }
        
        HideVictoryUIAnimation();
    }
    
    private void HideDeathUIAnimation()
    {
        if (debugMode)
            Debug.Log("Hiding death UI");
        
        if (deathCanvasGroup != null)
        {
            deathCanvasGroup.DOFade(0f, deathUIFadeDuration)
                .OnComplete(() => {
                    deathCanvasGroup.blocksRaycasts = false;
                    deathCanvasGroup.interactable = false;
                    deathUI.SetActive(false);
                    isDeathUIActive = false;
                    
                    if (debugMode)
                        Debug.Log("Death UI hidden");
                });
        }
        else
        {
            isDeathUIActive = false;
        }
    }
    
    public void LoadNextLevel(bool changeScene = true)
    {
        if (isTransitioning)
        {
            if (debugMode)
                Debug.Log("Already transitioning, ignoring LoadNextLevel call");
            return;
        }
        
        int nextLevelIndex = currentLevelIndex + 1;
        
        if (nextLevelIndex < levelScenes.Length)
        {
            TransitionToLevel(nextLevelIndex, changeScene);
        }
        else
        {
            if (debugMode)
                Debug.Log("No more levels available, going to main menu");
            TransitionToMainMenu();
        }
    }
    
    public void LoadLevel(int levelIndex)
    {
        if (isTransitioning)
        {
            if (debugMode)
                Debug.Log("Already transitioning, ignoring LoadLevel call");
            return;
        }
        
        if (levelIndex >= 0 && levelIndex < levelScenes.Length)
        {
            TransitionToLevel(levelIndex);
        }
        else
        {
            Debug.LogError($"Invalid level index: {levelIndex}. Valid range: 0-{levelScenes.Length - 1}");
        }
    }
    
    public void RestartCurrentLevel()
    {
        if (isTransitioning)
        {
            if (debugMode)
                Debug.Log("Already transitioning, ignoring RestartCurrentLevel call");
            return;
        }
        
        if (isDeathUIActive)
        {
            HideDeathUI();
        }
        
        TransitionToLevel(currentLevelIndex);
    }
    
    public void LoadMainMenu()
    {
        if (isTransitioning)
        {
            if (debugMode)
                Debug.Log("Already transitioning, ignoring LoadMainMenu call");
            return;
        }
        
        TransitionToMainMenu();
    }
    
    private void TransitionToLevel(int levelIndex, bool changeScene = true)
    {
        isTransitioning = true;
        
        if (debugMode)
            Debug.Log($"Transitioning to level {levelIndex}: {levelScenes[levelIndex]}");
        
        // Create DOTween sequence for transition
        Sequence transitionSequence = DOTween.Sequence();
        
        // Add transition delay
        transitionSequence.AppendInterval(transitionDelay);
        
        // Add fade out
        transitionSequence.AppendCallback(() => FadeOut(() => {
            // Load the scene after fade out completes
            currentLevelIndex = levelIndex;
            if(changeScene) UnityEngine.SceneManagement.SceneManager.LoadScene(levelScenes[levelIndex]);
            
            
            
            // Fade in after scene loads (small delay for scene initialization)
            DOVirtual.DelayedCall(0.1f, () => {
                FadeIn(() => {
                    isTransitioning = false;
                    BattleController.Instance.InitializeCombat(levelIndex - 1);
                    if (debugMode)
                        Debug.Log($"Successfully loaded level: {levelScenes[levelIndex]}");
                });
            });
        }));
    }
    
    private void TransitionToMainMenu()
    {
        isTransitioning = true;
        
        if (debugMode)
            Debug.Log("Transitioning to main menu");
        
        // Create DOTween sequence for transition
        Sequence transitionSequence = DOTween.Sequence();
        
        // Add transition delay
        transitionSequence.AppendInterval(transitionDelay);
        
        // Add fade out
        transitionSequence.AppendCallback(() => FadeOut(() => {
            // Load main menu after fade out completes
            currentLevelIndex = 0;
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
            
            // Fade in after scene loads (small delay for scene initialization)
            DOVirtual.DelayedCall(0.1f, () => {
                FadeIn(() => {
                    isTransitioning = false;
                    
                    if (debugMode)
                        Debug.Log("Successfully loaded main menu");
                });
            });
        }));
    }
    
    private void FadeOut(System.Action onComplete = null)
    {
        if (fadePanel != null)
        {
            fadePanel.blocksRaycasts = true;
            fadePanel.interactable = false;
            
            fadePanel.DOFade(1f, fadeDuration)
                .SetEase(fadeEaseOut)
                .OnComplete(() => {
                    if (debugMode)
                        Debug.Log("Fade out completed");
                    
                    onComplete?.Invoke();
                });
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    private void FadeIn(System.Action onComplete = null)
    {
        if (fadePanel != null)
        {
            fadePanel.DOFade(0f, fadeDuration)
                .SetEase(fadeEaseIn)
                .OnComplete(() => {
                    fadePanel.blocksRaycasts = false;
                    fadePanel.interactable = true;
                    
                    if (debugMode)
                        Debug.Log("Fade in completed");
                    
                    onComplete?.Invoke();
                });
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    public void OnLevelCompleted()
    {
        if (debugMode)
            Debug.Log("Level completed! Preparing to load next level...");
        
        LoadNextLevel();
    }
    
    public void OnPlayerDeath()
    {
        if (debugMode)
            Debug.Log("Player died! Showing death UI...");
        
        ShowDeathUI();
    }
    
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
