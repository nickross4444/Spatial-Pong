using UnityEngine;
using TMPro;
using System.Collections;

public class FadeTMPText : MonoBehaviour
{
    public TMP_Text tmpText;
    public float fadeDuration = 1.5f;
    public GameObject particleFX;
    private AudioSource audioSource;

    [Header("Text Follow Settings")]
    private Transform targetCamera;
    public float smoothSpeed = 5f; // Adjust for smoother delay
    private Vector3 targetPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // Find the main camera
        FindMainCamera();

        // Initialize target position
        if (targetCamera != null)
        {
            targetPosition = new Vector3(transform.position.x, transform.position.y, targetCamera.position.z);
        }

        StartCoroutine(FadeInText());
    }

    void Update()
    {
        if (targetCamera == null)
        {
            FindMainCamera(); // Ensure the camera is found
            return;
        }

        // Compute new target position (keeping only Y-axis rotation)
        Vector3 directionToCamera = targetCamera.position - transform.position;
        directionToCamera.y = 0; // Ignore X and Z rotation

        Vector3 desiredPosition = transform.position + directionToCamera.normalized;
        targetPosition = Vector3.Lerp(targetPosition, desiredPosition, smoothSpeed * Time.deltaTime);

        // Make the text face the camera smoothly
        transform.LookAt(new Vector3(targetPosition.x, transform.position.y, targetPosition.z));
    }

    private void FindMainCamera()
    {
        if (Camera.main != null)
        {
            targetCamera = Camera.main.transform;
            return;
        }

        Camera[] cameras = FindObjectsOfType<Camera>();
        if (cameras.Length > 0)
        {
            targetCamera = cameras[0].transform;
            Debug.LogWarning("No MainCamera found! Using first available camera.");
        }
        else
        {
            Debug.LogError("No camera found in the scene!");
        }
    }

    IEnumerator FadeInText()
    {
        yield return new WaitForSeconds(1f);
        particleFX.SetActive(true);
        audioSource.Play();
        yield return new WaitForSeconds(1.5f);

        float elapsedTime = 0f;
        Color color = tmpText.color;
        color.a = 0f;
        tmpText.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            tmpText.color = color;
            yield return null;
        }

        color.a = 1f;
        tmpText.color = color;
    }
}



