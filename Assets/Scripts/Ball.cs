using UnityEngine;

public class Ball : MonoBehaviour
{
    private GameManager gameManager;
    [SerializeField] float minSpeed = 0.1f;
    Rigidbody rb;


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
    }

    void Update()
    {
        if (rb.linearVelocity.magnitude < minSpeed && rb.linearVelocity.magnitude > 0)      //don't affect 0 velocity
        {
            rb.linearVelocity = rb.linearVelocity.normalized * minSpeed;
        }
    }
}
