using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ScoreboardDisplay : MonoBehaviour
{
    public static ScoreboardDisplay Instance { get; private set; }
    private Canvas scoreboardCanvas;
    private TextMeshProUGUI playerScoreText;
    private TextMeshProUGUI botScoreText;
    
    [Header("VR UI Settings")]
    [SerializeField] private float distanceFromCamera = 2f;
    [SerializeField] private float verticalOffset = 0.5f;
    
    private Transform cameraRig;
    private bool isGameStarted = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameObject.name = "ScoreboardManager";
        
        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        DontDestroyOnLoad(gameObject);
        
        cameraRig = GameObject.Find("[BuildingBlock] Camera Rig")?.transform;
        if (cameraRig == null)
        {
            Debug.LogError("[ScoreboardDisplay] Camera Rig not found!");
            return;
        }

        InitializeScoreboard();
        // Initially hide the scoreboard
        SetScoreboardVisibility(false);
    }

    private void InitializeScoreboard()
    {
        // Create and setup canvas
        GameObject canvasObj = new GameObject("ScoreboardCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.layer = LayerMask.NameToLayer("UI");
        
        scoreboardCanvas = canvasObj.AddComponent<Canvas>();
        scoreboardCanvas.renderMode = RenderMode.WorldSpace;
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1f, 0.3f);
        
        // Don't use a canvas scaler for world space VR
        canvasObj.AddComponent<GraphicRaycaster>();

        // Set the world scale and flip the canvas to prevent mirroring
        canvasObj.transform.localScale = new Vector3(-1f, 1f, 1f);

        CreateScoreDisplays();
    }
    
    private void CreateScoreDisplays()
    {
        // Create background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(scoreboardCanvas.transform, false);
        backgroundObj.layer = LayerMask.NameToLayer("UI");
        
        RectTransform backgroundRect = backgroundObj.AddComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.8f);

        // Create text container
        GameObject textContainer = new GameObject("TextContainer");
        textContainer.transform.SetParent(backgroundObj.transform, false);
        
        RectTransform textContainerRect = textContainer.AddComponent<RectTransform>();
        textContainerRect.anchorMin = Vector2.zero;
        textContainerRect.anchorMax = Vector2.one;
        textContainerRect.offsetMin = new Vector2(10, 10);
        textContainerRect.offsetMax = new Vector2(-10, -10);

        // Create score texts
        playerScoreText = CreateScoreText("PlayerScore", 0.25f);
        CreateSeparatorText();
        botScoreText = CreateScoreText("BotScore", 0.75f);

        // Initial values
        SetScoreText("0", "0");
    }

    private TextMeshProUGUI CreateScoreText(string name, float xPosition)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(scoreboardCanvas.transform, false);
        textObj.layer = LayerMask.NameToLayer("UI");

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(xPosition - 0.2f, 0);
        rect.anchorMax = new Vector2(xPosition + 0.2f, 1);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = 0.2f;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 0.1f;
        tmp.fontSizeMax = 0.4f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.color = Color.white;
        tmp.text = "0";

        return tmp;
    }

    private void CreateSeparatorText()
    {
        GameObject sepObj = new GameObject("Separator");
        sepObj.transform.SetParent(scoreboardCanvas.transform, false);
        sepObj.layer = LayerMask.NameToLayer("UI");

        RectTransform rect = sepObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.45f, 0);
        rect.anchorMax = new Vector2(0.55f, 1);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = sepObj.AddComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        tmp.fontSize = 0.2f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.color = Color.white;
        tmp.text = "-";
    }
    
    private void LateUpdate()
    {
        if (!isGameStarted || scoreboardCanvas == null || cameraRig == null)
            return;

        Vector3 forward = cameraRig.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 position = cameraRig.position + forward * distanceFromCamera;
        position.y = cameraRig.position.y + verticalOffset;
        
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(-forward, Vector3.up);
    }

    private void SetScoreText(string playerScore, string botScore)
    {
        if (playerScoreText != null && botScoreText != null)
        {
            playerScoreText.text = playerScore;
            botScoreText.text = botScore;
            Debug.Log($"[ScoreboardDisplay] Setting scores: Player={playerScore}, Bot={botScore}");
        }
    }
    
    public void UpdateScore(int playerScore, int botScore)
    {
        SetScoreText(playerScore.ToString(), botScore.ToString());
    }

    public void OnGameStart()
    {
        isGameStarted = true;
        SetScoreboardVisibility(true);
    }

    private void SetScoreboardVisibility(bool visible)
    {
        if (scoreboardCanvas != null)
        {
            scoreboardCanvas.gameObject.SetActive(visible);
        }
    }
}