using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent

public class Transition : MonoBehaviour
{
    private Material revealMaterial;
    public float revealSpeed = 1.5f;
    private float transitionValue = -5f;

    // Define a UnityEvent for starting the transition
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

    // Public method to trigger the transition
    public void StartTransition(System.Action OnTransitionComplete = null)
    {
        StartCoroutine(TransitionCoroutine(OnTransitionComplete));
    }


    IEnumerator TransitionCoroutine(System.Action OnTransitionComplete)
    {
        while (transitionValue < 5)
        {

            transitionValue += revealSpeed * Time.deltaTime;
            revealMaterial.SetFloat("_Height", transitionValue);
            yield return null;
        }
        if (OnTransitionComplete != null)
            OnTransitionComplete();
    }
}

