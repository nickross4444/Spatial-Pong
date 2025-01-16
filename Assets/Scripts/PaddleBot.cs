using UnityEngine;
using System.Collections;

public class PaddleBot : MonoBehaviour
{
    GameObject ball;
    [SerializeField] float speed = 1;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public void StartBot(GameObject _ball)
    {
        ball = _ball;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (ball != null)
        {
            // Get vector from paddle to ball
            Vector3 toBall = ball.transform.position - transform.position;

            // Project the ball position onto the paddle's plane
            Vector3 projectedDirection = Vector3.ProjectOnPlane(toBall, transform.forward);

            // Calculate the target position while maintaining the original position along the plane's normal
            Vector3 targetPos = transform.position + projectedDirection;

            // Move towards the projected point
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
        }
    }
}
