using UnityEngine;
using TMPro;
using System.Collections;

public class EffectSpawn : MonoBehaviour
{
    [SerializeField] private TMP_Text tmpText;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private GameObject particleFX;
    [SerializeField] private float smoothSpeed = 5f;
    
    private Transform targetCamera;
    private Vector3 targetPosition;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        FindMainCamera();
        
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
            FindMainCamera(); 
            return;
        }
        
        Vector3 directionToCamera = targetCamera.position - transform.position;
        directionToCamera.y = 0; 

        Vector3 desiredPosition = transform.position + directionToCamera.normalized;
        targetPosition = Vector3.Lerp(targetPosition, desiredPosition, smoothSpeed * Time.deltaTime);
        
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



