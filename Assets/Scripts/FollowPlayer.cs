using Unity.VisualScripting;
using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour
{
    Transform player;
    [SerializeField] float offset = 0.5f;
    [SerializeField] float heightOffset = -0.2f;
    [SerializeField] float positionThreshold = 0.1f;  // Distance threshold when aligned
    [SerializeField] float viewAngleThreshold = 120f;  // FOV threshold for realignment
    [SerializeField] float moveSpeed = 2f;
    
    private Coroutine moveCoroutine;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isMoving = false;
    private Vector3 currentForward;  // Track our own forward direction

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera").transform;
        currentForward = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;  // Initialize with flat forward
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);  // Ensure we start flat
    }

    void Update()
    {
        Vector3 directionToObj = (transform.position - player.position).normalized;
        directionToObj.y = 0;  // Flatten for angle calculation
        Vector3 flatPlayerForward = new Vector3(player.forward.x, 0, player.forward.z).normalized;
        float viewAngle = Vector3.Angle(flatPlayerForward, directionToObj);
        
        // Calculate if we need to move based on thresholds
        bool needsMove = false;
        bool needsRotation = false;
        
        if (viewAngle <= viewAngleThreshold * 0.5f)  // We're looking at the object
        {
            // Calculate ideal position maintaining current orientation
            Vector3 idealPosition = player.position + (currentForward * offset);
            idealPosition.y = player.position.y + heightOffset;  // Maintain height relative to player
            
            // Check if we're too far from ideal position
            float positionDiff = Vector3.Distance(transform.position, idealPosition);
            float playerDistance = Vector3.Distance(player.position, transform.position);
            float totalOffset = Mathf.Sqrt(Mathf.Pow(offset, 2) + Mathf.Pow(heightOffset, 2));
            if (positionDiff > positionThreshold && playerDistance > totalOffset + positionThreshold)     //don't move if player is leaning in
            {
                needsMove = true;
                targetPosition = idealPosition;
                
                // Always face away from player on any movement
                Vector3 directionToPlayer = (player.position - targetPosition).normalized;
                directionToPlayer.y = 0;  // Flatten direction
                float yRotation = Mathf.Atan2(directionToPlayer.x, directionToPlayer.z) * Mathf.Rad2Deg + 180f;
                targetRotation = Quaternion.Euler(0, yRotation, 0);
            }
        }
        else  // Object is out of view
        {
            needsMove = true;
            needsRotation = true;
            // Full realignment - update both position and orientation
            currentForward = new Vector3(player.forward.x, 0, player.forward.z).normalized;  // Update our tracked forward direction
            targetPosition = player.position + (currentForward * offset);
            targetPosition.y = player.position.y + heightOffset;
            
            // Calculate rotation to face away from player
            Vector3 directionToPlayer = (player.position - targetPosition).normalized;
            directionToPlayer.y = 0;  // Flatten direction
            float yRotation = Mathf.Atan2(directionToPlayer.x, directionToPlayer.z) * Mathf.Rad2Deg + 180f;
            targetRotation = Quaternion.Euler(0, yRotation, 0);
        }

        if (needsMove || needsRotation)
        {
            // If we're already moving, interrupt and restart the coroutine
            if (isMoving && moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }
            
            moveCoroutine = StartCoroutine(MoveToTarget());
        }
    }

    IEnumerator MoveToTarget()
    {
        isMoving = true;
        
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f ||
               Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        isMoving = false;
    }
}
