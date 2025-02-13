using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ScoreboardDisplay : MonoBehaviour
{
    public static ScoreboardDisplay Instance { get; private set; }
    private Canvas scoreboardCanvas;
    private TextMeshProUGUI playerScoreText;
    private TextMeshProUGUI botScoreText;

    [FormerlySerializedAs("scoreboardPosition")][SerializeField] private Vector3 scoreboardOffset = new Vector3(0f, 2f, 2f);
    [SerializeField] private Vector3 scoreboardRotation = new Vector3(15f, 0f, 0f);
    [SerializeField] private Vector2 canvasSize = new Vector2(1f, 0.3f);

    [SerializeField] private TMPro.TMP_FontAsset textFont;

    private bool isGameStarted = false;
    private Vector3 ballSpawnPosition;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Instance = null; // Reset the singleton instance when domain reloads
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize immediately but hide until game starts
        InitializeScoreboard();
        SetScoreboardVisibility(false);

        gameObject.layer = LayerMask.NameToLayer("UI");
    }

    public void SetBallSpawnPosition(Vector3 position)
    {
        ballSpawnPosition = position;
        if (scoreboardCanvas != null)
        {
            // Position the scoreboard relative to the ball spawn position
            scoreboardCanvas.transform.position = ballSpawnPosition + scoreboardOffset;
        }
    }

    private void Start()
    {
        // Position the scoreboard relative to the main camera if it exists
        Camera mainCamera = Camera.main;
        if (mainCamera != null && scoreboardCanvas != null)
        {
            UpdateScoreboardRotation(mainCamera);
        }
    }

    private void UpdateScoreboardRotation(Camera camera)
    {
        // Calculate direction from scoreboard to camera
        Vector3 directionToCamera = camera.transform.position - scoreboardCanvas.transform.position;

        // Create rotation to face camera while keeping vertical orientation
        Quaternion lookRotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);

        // Apply the base rotation to face the camera
        scoreboardCanvas.transform.rotation = lookRotation;

        // Apply the additional custom rotation
        scoreboardCanvas.transform.Rotate(scoreboardRotation);
    }

    private void InitializeScoreboard()
    {
        if (scoreboardCanvas != null)
        {
            Debug.Log("[ScoreboardDisplay] Scoreboard canvas already exists");
            return;
        }

        // Create canvas with world space render mode
        GameObject canvasObj = new GameObject("ScoreboardCanvas");
        canvasObj.transform.SetParent(transform);

        scoreboardCanvas = canvasObj.AddComponent<Canvas>();
        scoreboardCanvas.renderMode = RenderMode.WorldSpace;

        // Add necessary components
        canvasObj.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100f;

        // Set up the canvas rect transform
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = canvasSize;

        // Create the UI elements
        CreateScoreDisplays();

        Debug.Log("[ScoreboardDisplay] Scoreboard initialized successfully");
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
        tmp.font = textFont;
        tmp.fontSize = 0.2f;
        tmp.enableAutoSizing = false;
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
        tmp.font = textFont;
        tmp.fontSize = 0.2f;
        tmp.enableAutoSizing = false;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.color = Color.white;
        tmp.text = "-";
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
        if (playerScoreText == null || botScoreText == null)
        {
            Debug.LogError("[ScoreboardDisplay] Score text components are null!");
            return;
        }

        SetScoreText(playerScore.ToString(), botScore.ToString());
        Debug.Log($"[ScoreboardDisplay] Scores updated - Player: {playerScore}, Bot: {botScore}");
    }

    public void OnGameStart()
    {
        isGameStarted = true;
        SetScoreboardVisibility(true);
        Debug.Log("[ScoreboardDisplay] Game started - Scoreboard visible");

        // Initialize scores to 0
        UpdateScore(0, 0);
    }

    private void SetScoreboardVisibility(bool visible)
    {
        if (scoreboardCanvas != null)
        {
            scoreboardCanvas.gameObject.SetActive(visible);
        }
    }

    private void LateUpdate()
    {
        Camera mainCamera = Camera.main;
        if (scoreboardCanvas != null && Camera.main != null)
        {
            // Keep the same position but update rotation to face away from camera
            UpdateScoreboardRotation(mainCamera);
        }
    }
}