using UnityEngine;

public class Ball : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] float minSpeed = 0.1f;
    Rigidbody rb;
    AudioSource audioSource;
    public AudioClip goalSound;
    public AudioClip bounceSound;
    public AudioClip paddleSound;

    void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
    }

    public void Initialize(GameManager manager)

    {
        gameManager = manager;
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (gameManager != null)
        {
            gameManager.OnBallCollision(collision);
        }
        if (collision.gameObject.CompareTag("Goal"))
        {
            audioSource.PlayOneShot(goalSound);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            audioSource.PlayOneShot(bounceSound);
        }
        else if (collision.gameObject.CompareTag("Paddle"))
        {
            audioSource.PlayOneShot(paddleSound);
        }
    }




    void Update()
    {
        if (rb.linearVelocity.magnitude < minSpeed && rb.linearVelocity.magnitude > 0)      //don't affect 0 velocity
        {
            rb.linearVelocity = rb.linearVelocity.normalized * minSpeed;
        }
    }
}
