using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent

public class Transition : MonoBehaviour
{
    private Material revealMaterial;
    public float revealTime = 1.5f;
    private float transitionValue = 0f;

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
        Color originalColor = revealMaterial.GetColor("_Color");
        Color occludingColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);        //fully opaque
        revealMaterial.SetColor("_Color", occludingColor);
        while (transitionValue < 1f)
        {

            transitionValue += Time.deltaTime / revealTime;
            revealMaterial.SetFloat("_Height", transitionValue);
            yield return null;
        }
        revealMaterial.SetColor("_Color", originalColor);   //stop fully occluding passthrough
        if (OnTransitionComplete != null)
            OnTransitionComplete();
    }

}

