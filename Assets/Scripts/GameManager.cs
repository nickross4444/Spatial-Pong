using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    GameObject ball, botPaddle, playerPaddle, playerGoal, botGoal;
    [Header("Gameplay Settings")]
    [SerializeField] float paddleSpeed = 2f;
    [SerializeField] float ballKickDelay = 1.5f;
    [SerializeField] public float kickForce = 1;
    [SerializeField] float bounceBoostSpeed = 1.02f;
    [SerializeField] float paddleBoostSpeed = 1.1f;
    [SerializeField][Range(0, 90)] float kickRandomRangeDegrees = 20f;    // Controls the random angle variation in initial kick (in degrees)
    [SerializeField] float possessionForce = 2f;    // Force applied in possession direction on wall hits
    [SerializeField] float aimBoost = 1f;
    [SerializeField] bool limitPaddleSpeed = true;
    int playerScore = 0, botScore = 0;
    public int PlayerScore => playerScore;      //this allows public access, but private set, while staying serializable
    public int BotScore => botScore;
    int maxScore = 11;
    bool isPaused = false;
    public bool IsPaused => isPaused;      //expose pause state for other components to check
    private Vector3 storedBallVelocity;
    Vector3 ballStartPos;
    private bool lastWinnerWasPlayer = true;  // true = player, false = bot
    private bool playerHasPossession = true;  // Tracks who has possession of the ball
    private Vector3 playerGoalDirection;    // Cached direction from center to player goal
    private Vector3 botGoalDirection;       // Cached direction from center to bot goal

    [Header("Win Events")]
    public UnityEvent onPlayerWin;
    public UnityEvent onBotWin;
    public UnityEvent onPause;
    public UnityEvent onResume;

    [Header("References")]
    public GameObject pauseMenu;

    void Start()
    {
        PlayerPrefs.SetFloat("PaddleSpeed", paddleSpeed);
        PlayerPrefs.SetInt("LimitPaddleSpeed", limitPaddleSpeed ? 1 : 0);
    }
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Start))

        {
            if (isPaused)
            {
                ResumeGame();
                onResume?.Invoke();
                //menu is set false in inspector
            }
            else
            {
                PauseGame();
                pauseMenu.SetActive(true);
                onPause?.Invoke();
            }
        }
    }
    public void StartBall(GameObject _ball, GameObject _playerPaddle, GameObject _botPaddle, GameObject _playerGoal, GameObject _botGoal)
    {
        ball = _ball;
        botPaddle = _botPaddle;
        playerPaddle = _playerPaddle;
        playerGoal = _playerGoal;
        botGoal = _botGoal;
        ballStartPos = ball.transform.position;

        // Cache the goal directions
        playerGoalDirection = (playerGoal.transform.position - ballStartPos).normalized;
        botGoalDirection = (botGoal.transform.position - ballStartPos).normalized;

        botPaddle.GetComponent<PaddleBot>().StartBot(ball, botGoal.GetComponent<MeshFilter>().mesh);
        ball.GetComponent<Ball>().Initialize(this);
        StartCoroutine(KickAfterDelay(ball.GetComponent<Rigidbody>(), ballKickDelay));
    }
    public void RestartGame()
    {
        foreach (EffectSpawn obj in GameObject.FindObjectsByType<EffectSpawn>(FindObjectsSortMode.None))
        {
            obj.gameObject.SetActive(false);
        }
        ResetScore();
        StartCoroutine(KickAfterDelay(ball.GetComponent<Rigidbody>(), ballKickDelay));
    }
    void ResetScore()
    {
        playerScore = 0;
        botScore = 0;
    }
    public void OnBallCollision(Collision collision)
    {
        if (collision.gameObject == playerGoal)
        {
            Debug.Log("Bot scored!");
            botScore++;
            lastWinnerWasPlayer = false;
            playerHasPossession = false;  // Bot gets possession after scoring
        }
        else if (collision.gameObject == botGoal)
        {
            Debug.Log("Player scored!");
            playerScore++;
            lastWinnerWasPlayer = true;
            playerHasPossession = true;   // Player gets possession after scoring
        }
        else
        {
            Debug.Log("Ball collided with: " + collision.gameObject.name);
            if (collision.gameObject.CompareTag("Paddle"))
            {
                GameObject paddle = collision.gameObject;
                // Transfer possession to the opposite player when a paddle hits the ball
                playerHasPossession = paddle != playerPaddle;
                ball.GetComponent<Rigidbody>().linearVelocity *= paddleBoostSpeed;


                //aim the ball in the direction of the contact point from the center of the paddle
                Vector3 aimVector = (ball.transform.position - paddle.transform.position);
                Vector3 aimVector2D = Vector3.ProjectOnPlane(aimVector, paddle.transform.forward);  //remove the forward component of the aim vector(don't affect forward momentum)
                float aimMag = aimVector2D.magnitude * aimBoost;
                Vector3 aimVector2DNormalized = aimVector2D.normalized;

                ball.GetComponent<Rigidbody>().linearVelocity += aimVector2DNormalized * aimMag;
            }
            else
            {
                // Wall hit - apply possession-based force towards appropriate goal
                Vector3 possessionDirection = playerHasPossession ? playerGoalDirection : botGoalDirection;
                ball.GetComponent<Rigidbody>().AddForce(possessionDirection * possessionForce, ForceMode.Impulse);
                ball.GetComponent<Rigidbody>().linearVelocity *= bounceBoostSpeed;
            }
            return;
        }
        bool gameOver = false;
        if (playerScore == maxScore)
        {
            Debug.Log("Player wins!");
            onPlayerWin?.Invoke();
            ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            gameOver = true;
        }
        else if (botScore == maxScore)
        {
            Debug.Log("Bot wins!");
            onBotWin?.Invoke();
            ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            gameOver = true;
        }
        ResetBall(!gameOver);
    }
    void ResetBall(bool toKick = true)
    {
        ball.transform.position = ballStartPos;
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = false;
        if (toKick)
        {
            StartCoroutine(KickAfterDelay(rb, ballKickDelay));
        }
    }
    IEnumerator KickAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Get base direction based on who won
        Vector3 baseDirection = lastWinnerWasPlayer ? playerGoalDirection : botGoalDirection;

        // Convert the half-angle of the cone from degrees to radians
        float coneAngleRad = kickRandomRangeDegrees * Mathf.Deg2Rad;

        // Sample cosθ uniformly between cos(coneAngleRad) and 1 for even distribution over the cone
        float cosTheta = Mathf.Lerp(Mathf.Cos(coneAngleRad), 1.0f, Random.value);

        // Get the actual theta angle from its cosine
        float theta = Mathf.Acos(cosTheta);

        // Sample random angle around the cone uniformly from 0 to 2π
        float phi = Random.Range(0f, 2f * Mathf.PI);

        // Calculate the direction in local space where baseDirection is treated as forward
        float sinTheta = Mathf.Sin(theta);
        Vector3 localDirection = new Vector3(
            sinTheta * Mathf.Cos(phi),
            sinTheta * Mathf.Sin(phi),
            cosTheta
        );

        // Create rotation from Vector3.forward to our baseDirection
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, baseDirection);

        // Rotate our local direction to align with the base direction
        Vector3 kickDirection = rotation * localDirection;

        rb.AddForce(kickDirection * kickForce, ForceMode.Impulse);
        ball.GetComponent<Ball>().audioSource.PlayOneShot(ball.GetComponent<Ball>().kickSound);
    }
    public void QuitApp()
    {
        //!!TODO: add a fade to black here
        Application.Quit();
    }
    public void InitializeGame()
    {
        GameSetup gameSetup = GetComponent<GameSetup>();
        gameSetup.SetupWalls();
    }
    void PauseGame()
    {
        if (ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            storedBallVelocity = rb.linearVelocity;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        isPaused = true;
    }
    public void ResumeGame()
    {
        if (ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.linearVelocity = storedBallVelocity;
        }
        isPaused = false;
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("Pong");
    }
}
