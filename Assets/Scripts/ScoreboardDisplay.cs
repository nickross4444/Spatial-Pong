using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreboardDisplay : MonoBehaviour
{
    private Canvas scoreboardCanvas;
    private TextMeshProUGUI playerScoreText;
    private TextMeshProUGUI botScoreText;
    
    [SerializeField] private Vector2 scoreboardSize = new Vector2(400f, 100f);
    [SerializeField] private float fontSize = 72f; 
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private float topOffset = 100f; 
    
    [SerializeField] private bool debugMode = true;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.5f); 
    
    private void Awake()
    {
        DontDestroyOnLoad(gameObject); 
        SetupScoreboardCanvas();
        CreateScoreDisplays();
        
        if (debugMode)
        {
            Debug.Log("Scoreboard initialized");
        }
    }
    
    private void SetupScoreboardCanvas()
    {
        GameObject canvasObj = new GameObject("ScoreboardCanvas");
        canvasObj.transform.SetParent(transform);
        
        scoreboardCanvas = canvasObj.AddComponent<Canvas>();
        scoreboardCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        scoreboardCanvas.sortingOrder = 1000; 
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        if (debugMode)
        {
            Debug.Log($"Canvas created with sorting order {scoreboardCanvas.sortingOrder}");
        }
    }
    
    private void CreateScoreDisplays()
    {
        GameObject backgroundPanel = new GameObject("Background");
        backgroundPanel.transform.SetParent(scoreboardCanvas.transform);
        RectTransform backgroundRect = backgroundPanel.AddComponent<RectTransform>();
        Image backgroundImage = backgroundPanel.AddComponent<Image>();
        backgroundImage.color = backgroundColor;
        
        backgroundRect.anchorMin = new Vector2(0.5f, 1f);
        backgroundRect.anchorMax = new Vector2(0.5f, 1f);
        backgroundRect.pivot = new Vector2(0.5f, 1f);
        backgroundRect.anchoredPosition = new Vector2(0, -topOffset);
        backgroundRect.sizeDelta = new Vector2(scoreboardSize.x + 40f, scoreboardSize.y + 20f);
        
        GameObject container = new GameObject("ScoreContainer");
        container.transform.SetParent(backgroundPanel.transform);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = scoreboardSize;
        
        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 50f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        
        playerScoreText = CreateScoreText("PlayerScore", container.transform);
        TextMeshProUGUI separator = CreateScoreText("Separator", container.transform);
        separator.text = "-";
        botScoreText = CreateScoreText("BotScore", container.transform);
        
        if (debugMode)
        {
            Debug.Log("Score displays created");
        }
    }
    
    private TextMeshProUGUI CreateScoreText(string name, Transform parent)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(100f, 100f);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = "0";
        tmp.color = textColor;
        tmp.font = Resources.Load<TMP_FontAsset>("Fonts/Arial SDF"); 
        
        return tmp;
    }
    
    public void UpdateScore(int playerScore, int botScore)
    {
        if (playerScoreText != null && botScoreText != null)
        {
            playerScoreText.text = playerScore.ToString();
            botScoreText.text = botScore.ToString();
            
            if (debugMode)
            {
                Debug.Log($"Score updated - Player: {playerScore}, Bot: {botScore}");
            }
        }
        else if (debugMode)
        {
            Debug.LogWarning("Score text components are null!");
        }
    }
    
    private void OnEnable()
    {
        if (debugMode)
        {
            Debug.Log("Scoreboard enabled");
        }
    }
    
    private void OnDisable()
    {
        if (debugMode)
        {
            Debug.Log("Scoreboard disabled");
        }
    }
}