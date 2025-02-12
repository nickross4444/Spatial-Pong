using Oculus.Interaction;
using UnityEngine;

public class PaddlePlane : MonoBehaviour
{
    [SerializeField]
    private GameObject paddle;
    private RayInteractable rayInteractable;
    private IPointable pointable;
    private float paddleSpeed;
    private GameManager gameManager;
    bool limitPaddleSpeed = true;

    Vector3 targetPos;

    public void Initialize(GameObject paddle)
    {
        this.paddle = paddle;
        paddleSpeed = PlayerPrefs.GetFloat("PaddleSpeed", 1f);
        limitPaddleSpeed = PlayerPrefs.GetInt("LimitPaddleSpeed") == 1;
        rayInteractable = GetComponent<RayInteractable>();
        pointable = GetComponent<IPointable>();
        rayInteractable.WhenStateChanged += HandleStateChanged;
        pointable.WhenPointerEventRaised += HandlePointerEvent;
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void HandleStateChanged(InteractableStateChangeArgs args)
    {
        if (args.NewState != InteractableState.Hover && paddle != null)
        {
            //targetPos = paddle.transform.position;      //set tp current position
        }
    }

    private void HandlePointerEvent(PointerEvent evt)
    {
        if (evt.Type == PointerEventType.Hover || evt.Type == PointerEventType.Move)
        {
            targetPos = evt.Pose.position;
        }
    }

    void Update()
    {
        if (targetPos != null && !gameManager.IsPaused)
        {
            if (limitPaddleSpeed)
            {
                paddle.transform.position = Vector3.MoveTowards(paddle.transform.position, targetPos, paddleSpeed * Time.deltaTime);
            }
            else
            {
                paddle.transform.position = targetPos;
            }
        }
    }

    private void OnDestroy()
    {
        if (rayInteractable != null)
        {
            rayInteractable.WhenStateChanged -= HandleStateChanged;
        }
        if (pointable != null)
        {
            pointable.WhenPointerEventRaised -= HandlePointerEvent;
        }
    }
}
