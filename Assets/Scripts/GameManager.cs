using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    GameObject ball, botPaddle, playerPaddle, playerGoal, botGoal;
    [SerializeField] float kickForce = 1;
    [SerializeField] float bounceBoostSpeed = 1.02f;
    [SerializeField] float paddleBoostSpeed = 1.1f;
    Vector3 ballStartPos;
    void Start()
    {

    }
    void Update()
    {

    }

    public void StartGame(GameObject _ball, GameObject _playerPaddle, GameObject _botPaddle, GameObject _playerGoal, GameObject _botGoal)
    {
        ball = _ball;
        botPaddle = _botPaddle;
        playerPaddle = _playerPaddle;
        playerGoal = _playerGoal;
        botGoal = _botGoal;
        ballStartPos = ball.transform.position;
        botPaddle.GetComponent<PaddleBot>().StartBot(ball, botGoal.GetComponent<MeshFilter>().mesh);
        ball.GetComponent<Ball>().Initialize(this);
        StartCoroutine(KickAfterDelay(ball.GetComponent<Rigidbody>(), 1f));
    }


    public void OnBallCollision(Collision collision)
    {
        if (collision.gameObject == playerGoal)
        {
            Debug.Log("Bot scored!");
            ResetBall();
        }
        else if (collision.gameObject == botGoal)
        {
            Debug.Log("Player scored!");
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
    }
    void ResetBall()
    {
        ball.transform.position = ballStartPos;
        ball.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        StartCoroutine(KickAfterDelay(ball.GetComponent<Rigidbody>(), 1f));
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
    }
}
