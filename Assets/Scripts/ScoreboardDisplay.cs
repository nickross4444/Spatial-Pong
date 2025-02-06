using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreboardDisplay : MonoBehaviour
{
    private Canvas scoreboardCanvas;
    private TextMeshProUGUI playerScoreText;
    private TextMeshProUGUI botScoreText;
    
    private Transform playerCamera;
    
    [SerializeField] private float heightOffset = 2.0f;
    [SerializeField] private float distanceFromPlayer = 3.0f;
    [SerializeField] private bool followPlayer = true;
    [SerializeField] private float scoreboardScale = 0.005f; 
    
    private void Awake()
    {
        SetupScoreboardCanvas();
        CreateScoreDisplays();
    }
    
    private void SetupScoreboardCanvas()
    {
        GameObject canvasObj = new GameObject("ScoreboardCanvas");
        canvasObj.transform.SetParent(transform);
        
        scoreboardCanvas = canvasObj.AddComponent<Canvas>();
        scoreboardCanvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;
        
        GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
        
        canvasObj.transform.localPosition = Vector3.zero;
        canvasObj.transform.localScale = Vector3.one * scoreboardScale;
    }
    
    private void CreateScoreDisplays()
    {
        GameObject container = new GameObject("ScoreContainer");
        container.transform.SetParent(scoreboardCanvas.transform);
        RectTransform containerRect = container.AddComponent<RectTransform>();
        containerRect.anchoredPosition = Vector2.zero;
        
        HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 50f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        
        playerScoreText = CreateScoreText("PlayerScore", container.transform);
        
        TextMeshProUGUI separator = CreateScoreText("Separator", container.transform);
        separator.text = "-";
        botScoreText = CreateScoreText("BotScore", container.transform);
        containerRect.sizeDelta = new Vector2(400f, 100f);
    }
    
    private TextMeshProUGUI CreateScoreText(string name, Transform parent)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(100f, 100f);
        
        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 72;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = "0";
        tmp.color = Color.white;
        
        return tmp;
    }
    
    private void Start()
    {
        playerCamera = Camera.main.transform;
        UpdatePosition();
        
        UpdateScore(0, 0);
    }
    
    private void LateUpdate()
    {
        if (followPlayer && playerCamera != null)
        {
            UpdatePosition();
            transform.LookAt(playerCamera);
            transform.Rotate(0, 180, 0); 
        }
    }
    
    private void UpdatePosition()
    {
        Vector3 cameraForward = playerCamera.forward;
        cameraForward.y = 0; 
        
        Vector3 targetPosition = playerCamera.position + 
            (cameraForward.normalized * distanceFromPlayer) + 
            (Vector3.up * heightOffset);
        
        transform.position = targetPosition;
    }
    
    public void UpdateScore(int playerScore, int botScore)
    {
        if (playerScoreText != null && botScoreText != null)
        {
            playerScoreText.text = playerScore.ToString();
            botScoreText.text = botScore.ToString();
        }
    }
}