using Oculus.Interaction;
using UnityEngine;

public class PaddlePlane : MonoBehaviour
{
    [SerializeField]
    private GameObject paddle;
    private RayInteractable rayInteractable;
    private IPointable pointable;
    private float paddleSpeed;

    Vector3 targetPos;

    public void Initialize(GameObject paddle)
    {
        this.paddle = paddle;
        paddleSpeed = PlayerPrefs.GetFloat("PaddleSpeed", 1f);
        rayInteractable = GetComponent<RayInteractable>();
        pointable = GetComponent<IPointable>();
        rayInteractable.WhenStateChanged += HandleStateChanged;
        pointable.WhenPointerEventRaised += HandlePointerEvent;
    }

    private void HandleStateChanged(InteractableStateChangeArgs args)
    {
        if (args.NewState != InteractableState.Hover)
        {
            targetPos = paddle.transform.position;
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
        if (targetPos != null)
        {
            paddle.transform.position = Vector3.MoveTowards(paddle.transform.position, targetPos, paddleSpeed * Time.deltaTime);
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
