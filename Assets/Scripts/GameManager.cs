using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Goal Settings")]
    public int daysToSurvive = 5;
    
    [Header("References")]
    public DayNightManager dayNightManager;
    public GameObject gameOverPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public UnityEngine.UI.Button restartButton;
    public Health playerHealth;
    
    [Header("Victory/Defeat Settings")]
    public string victoryTitle = "CONGRATULATIONS!";
    public string victoryMessage = "You survived {0} days!\\n\\nWould you like to play again?";
    public Color victoryColor = Color.green;
    
    public string defeatTitle = "GAME OVER";
    public string defeatMessage = "You were defeated on Day {0}.\\n\\nWould you like to try again?";
    public Color defeatColor = Color.red;
    
    private static GameManager instance;
    private bool gameEnded = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (dayNightManager == null)
        {
            dayNightManager = FindObjectOfType<DayNightManager>();
        }
        
        if (playerHealth == null)
        {
            playerHealth = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Health>();
        }
        
        if (playerHealth != null)
        {
            playerHealth.onDeath.AddListener(OnPlayerDeath);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static GameManager Instance
    {
        get { return instance; }
    }

    public int GetDaysToSurvive()
    {
        return daysToSurvive;
    }
    
    public void TriggerVictory()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        ShowGameOverScreen(
            victoryTitle,
            string.Format(victoryMessage, daysToSurvive),
            victoryColor
        );
    }

    private void OnPlayerDeath()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        int currentDay = dayNightManager != null ? dayNightManager.GetCurrentDay() : 1;
        
        ShowGameOverScreen(
            defeatTitle,
            string.Format(defeatMessage, currentDay),
            defeatColor
        );
    }

    private void ShowGameOverScreen(string title, string message, Color titleColor)
    {
        if (gameOverPanel == null) return;
        
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = titleColor;
        }
        
        if (messageText != null)
        {
            messageText.text = message;
        }
        
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
