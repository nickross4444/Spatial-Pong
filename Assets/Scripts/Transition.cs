using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent

public class Transition : MonoBehaviour
{
    private Material revealMaterial;
    public float revealSpeed = 1.5f;
    private float transitionValue = -5f;
    public bool isChanging = false;

    // Define a UnityEvent for starting the transition
    public UnityEvent OnStartTransition;

    private void Start()
    {
        revealMaterial = GetComponent<Renderer>().material;
    }

    private void OnEnable()
    {
        if (OnStartTransition != null)
            OnStartTransition.AddListener(StartTransition);
    }

    private void OnDisable()
    {
        if (OnStartTransition != null)
            OnStartTransition.RemoveListener(StartTransition);
    }

    // Public method to trigger the transition
    public void StartTransition()
    {
        isChanging = true;
    }

    private void Update()
    {
        if (isChanging && transitionValue < 10)
        {
            transitionValue += revealSpeed * Time.deltaTime;
            Debug.Log(transitionValue);
            revealMaterial.SetFloat("_Height", transitionValue);
        }
    }
}
