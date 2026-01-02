using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages UI panels and listens to game events to update displays.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject difficultySelectPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject gameOverPanel;
    
    [Header("Game UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI requestedChordText;
    
    [Header("Settings")]
    [SerializeField] private float timerWarningThreshold = 10f;
    [SerializeField] private Color timerNormalColor = Color.white;
    [SerializeField] private Color timerWarningColor = Color.red;
    
    private string currentPanel = "";
    
    void OnEnable()
    {
        GameEvents.OnScoreChanged += UpdateScore;
        GameEvents.OnTimerTick += UpdateTimer;
        GameEvents.OnComboChanged += UpdateCombo;
        GameEvents.OnGameStart += HandleGameStart;
        GameEvents.OnGameOver += HandleGameOver;
        GameEvents.OnPanelRequested += ShowPanel;
        GameEvents.OnChordRequested += UpdateRequestedChord;
    }
    
    void OnDisable()
    {
        GameEvents.OnScoreChanged -= UpdateScore;
        GameEvents.OnTimerTick -= UpdateTimer;
        GameEvents.OnComboChanged -= UpdateCombo;
        GameEvents.OnGameStart -= HandleGameStart;
        GameEvents.OnGameOver -= HandleGameOver;
        GameEvents.OnPanelRequested -= ShowPanel;
        GameEvents.OnChordRequested -= UpdateRequestedChord;
    }
    
    void Start()
    {
        ShowPanel("MainMenu");
    }
    
    // ===== PANEL MANAGEMENT =====
    
    public void ShowPanel(string panelName)
    {
        // Hide all panels
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(difficultySelectPanel, false);
        SetPanelActive(gamePanel, false);
        SetPanelActive(optionsPanel, false);
        SetPanelActive(gameOverPanel, false);
        
        // Show requested panel
        currentPanel = panelName;
        
        switch (panelName)
        {
            case "MainMenu":
                SetPanelActive(mainMenuPanel, true);
                break;
            case "DifficultySelect":
                SetPanelActive(difficultySelectPanel, true);
                break;
            case "Game":
                SetPanelActive(gamePanel, true);
                break;
            case "Options":
                SetPanelActive(optionsPanel, true);
                break;
            case "GameOver":
                SetPanelActive(gameOverPanel, true);
                break;
        }
    }
    
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
    
    // ===== BUTTON CALLBACKS =====
    // Assign these to your UI buttons in the Inspector
    
    public void OnPlayButton()
    {
        ShowPanel("DifficultySelect");
    }
    
    public void OnOptionsButton()
    {
        ShowPanel("Options");
    }
    
    public void OnBackButton()
    {
        switch (currentPanel)
        {
            case "DifficultySelect":
            case "Options":
                ShowPanel("MainMenu");
                break;
            case "GameOver":
                ShowPanel("MainMenu");
                break;
        }
    }
    
    public void OnRestartButton()
    {
        // GameController will handle this
        GameEvents.FirePanelRequested("Game");
        GameEvents.FireGameStart();
    }
    
    // ===== UI UPDATES =====
    
    private void UpdateScore(int score)
    {
        Debug.Log($"UpdateScore called with: {score}, scoreText is {(scoreText != null ? "assigned" : "NULL")}");
        if (scoreText != null)
            scoreText.text = score.ToString();
    }
    
    private void UpdateTimer(float timeRemaining)
    {
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
            timerText.color = timeRemaining <= timerWarningThreshold 
                ? timerWarningColor 
                : timerNormalColor;
        }
    }
    
    private void UpdateCombo(int combo, int multiplier)
    {
        if (comboText != null)
        {
            if (combo > 0)
            {
                comboText.gameObject.SetActive(true);
                comboText.text = $"x{multiplier}";
            }
        }
    }
    
    private void UpdateRequestedChord(string chordName)
    {
        if (requestedChordText != null)
            requestedChordText.text = chordName;
    }
    
    private void HandleGameStart()
    {
        ShowPanel("Game");
        
        // Reset UI
        UpdateScore(0);
        UpdateCombo(0, 1);
    }
    
    private void HandleGameOver(int finalScore)
    {
        ShowPanel("GameOver");
        
        if (finalScoreText != null)
            finalScoreText.text = $"Score: {finalScore}";
    }
}
