using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Transition : MonoBehaviour
{
    [SerializeField] private GameObject audioGameObjectPrefab; // Prefab to instantiate
    private GameObject instantiatedAudioObject; // Reference to the instantiated object
    private Material revealMaterial;
    public float revealTime = 1.5f;
    private float transitionValue = 0f;

    public UnityEvent OnStartTransition;

    private void Start()
    {
        revealMaterial = GetComponent<Renderer>().material;
    }

    private void OnEnable()
    {
        if (OnStartTransition != null)
            OnStartTransition.AddListener(() => StartTransition());
    }

    private void OnDisable()
    {
        if (OnStartTransition != null)
            OnStartTransition.RemoveListener(() => StartTransition());
    }

    public void StartTransition(System.Action OnTransitionComplete = null)
    {
        if (instantiatedAudioObject == null && audioGameObjectPrefab != null)
        {
            instantiatedAudioObject = Instantiate(audioGameObjectPrefab, Vector3.zero, Quaternion.identity);
        }

        StartCoroutine(TransitionCoroutine(OnTransitionComplete));
    }

    IEnumerator TransitionCoroutine(System.Action OnTransitionComplete)
    {
        Color originalColor = revealMaterial.GetColor("_Color");
        Color occludingColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f); // Fully opaque
        revealMaterial.SetColor("_Color", occludingColor);

        while (transitionValue < 1f)
        {
            transitionValue += Time.deltaTime / revealTime;
            revealMaterial.SetFloat("_Height", transitionValue);

            // Move audioGameObject upwards in sync with the transition
            if (instantiatedAudioObject != null)
            {
                instantiatedAudioObject.transform.position = new Vector3(0, transitionValue * 4f, 0);
            }

            yield return null;
        }

        // Restore original material color
        revealMaterial.SetColor("_Color", originalColor);

        // Destroy audioGameObject once transition completes
        if (instantiatedAudioObject != null)
        {
            Destroy(instantiatedAudioObject);
            instantiatedAudioObject = null; // Clear reference
        }

        OnTransitionComplete?.Invoke();
    }
}



