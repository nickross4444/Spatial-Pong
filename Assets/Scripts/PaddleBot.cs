using UnityEngine;
using System.Collections;

public class PaddleBot : MonoBehaviour
{
    GameObject ball;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float paddleSpeed;
    float xMax, xMin, yMax, yMin;

    public void StartBot(GameObject _ball, Mesh boundsMesh)
    {
        ball = _ball;
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        paddleSpeed = PlayerPrefs.GetFloat("PaddleSpeed", 1f);

        xMax = boundsMesh.bounds.max.x;
        xMin = boundsMesh.bounds.min.x;
        yMax = boundsMesh.bounds.max.y;
        yMin = boundsMesh.bounds.min.y;
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
            //todo: clamp targetPos to bounds
            // Move towards the projected point
            transform.position = Vector3.MoveTowards(transform.position, targetPos, paddleSpeed * Time.deltaTime);
        }
    }
}
