using UnityEngine;

public class Ball : MonoBehaviour
{
    private GameManager gameManager;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (gameManager != null)
        {
            gameManager.OnBallCollision(collision);
        }
    }
}
