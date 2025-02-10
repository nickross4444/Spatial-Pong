using System.Collections;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    GameObject ball, botPaddle, playerPaddle, playerGoal, botGoal;
    [Header("Gameplay Settings")]
    [SerializeField] float ballKickDelay = 1.5f;
    [SerializeField] float kickForce = 1;
    [SerializeField] float bounceBoostSpeed = 1.02f;
    [SerializeField] float paddleBoostSpeed = 1.1f;
    [SerializeField] int playerScore = 0;
    [SerializeField] int botScore = 0;
    public int PlayerScore => playerScore;      //this allows public access, but private set, while staying serializable
    public int BotScore => botScore;
    int maxScore = 11;
    bool isPaused = false;
    public bool IsPaused => isPaused;      //expose pause state for other components to check
    private Vector3 storedBallVelocity;
    Vector3 ballStartPos;

    [Header("Win Events")]
    public UnityEvent onPlayerWin;
    public UnityEvent onBotWin;
    public UnityEvent onPause;
    public UnityEvent onResume;

    [Header("References")]
    public GameObject pauseMenu;


    void Start()

    {

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
            ResetBall();
        }
        else if (collision.gameObject == botGoal)
        {
            Debug.Log("Player scored!");
            playerScore++;
            ResetBall();
        }
        else
        {
            Debug.Log("Ball collided with: " + collision.gameObject.name);
            //Vector3 normal = collision.contacts[0].normal;
            //float force = collision.gameObject.CompareTag("Paddle") ? paddleBoostForce : bounceBoostForce;
            //ball.GetComponent<Rigidbody>().AddForce(normal * force, ForceMode.Impulse);
            ball.GetComponent<Rigidbody>().linearVelocity *= collision.gameObject.CompareTag("Paddle") ? paddleBoostSpeed : bounceBoostSpeed;
        }
        if (playerScore == maxScore)
        {
            Debug.Log("Player wins!");
            onPlayerWin?.Invoke();
            ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            ResetBall(false);
        }
        else if (botScore == maxScore)

        {
            Debug.Log("Bot wins!");
            onBotWin?.Invoke();
            ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            ResetBall(false);
        }
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
    [Button]
    void IncreaseBallSpeed()
    {
        if (ball != null)
        {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            rb.linearVelocity *= 5f;
        }
    }
    IEnumerator KickAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        Vector3[] corners = new Vector3[] {
            new Vector3(1, 1, 1),
            new Vector3(1, 1, -1),
            new Vector3(1, -1, 1),
            new Vector3(1, -1, -1),
            new Vector3(-1, 1, 1),
            new Vector3(-1, 1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(-1, -1, -1)
        };
        Vector3 kickAngle = corners[Random.Range(0, corners.Length)].normalized;
        rb.AddForce(kickAngle * kickForce, ForceMode.Impulse);
        //rb.AddForce(new Vector3(0, 0, 1) * kickForce, ForceMode.Impulse);
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
