using System.Collections;
using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

public class MenuItem : MonoBehaviour
{
    private IPointable pointable;
    [SerializeField] UnityEvent onClick;
    [SerializeField] int alphaAdditionOnHover = 20;
    [SerializeField] float scaleOnSelect = 0.9f;
    [SerializeField] float scaleTime = 0.1f;
    [SerializeField] GameObject mesh;
    MeshRenderer meshRenderer;
    Vector3 originalScale;
    Color originalEmissionColor;
    Coroutine scaleCoroutine;
    private static readonly string EmissionProperty = "_BaseEmissionColor"; // ShaderGraph property name

    void Awake()
    {
        pointable = GetComponentInChildren<IPointable>();
        meshRenderer = mesh.GetComponent<MeshRenderer>();
    }
    void Start()
    {
        pointable.WhenPointerEventRaised += OnPointerEventRaised;
        originalEmissionColor = meshRenderer.material.GetColor(EmissionProperty);
        originalScale = mesh.transform.localScale;
    }

    private void OnPointerEventRaised(PointerEvent evt)
    {
        switch (evt.Type)
        {
            case PointerEventType.Hover:
                Color hoverColor = originalEmissionColor;
                hoverColor.a += alphaAdditionOnHover / 255f;
                meshRenderer.material.SetColor(EmissionProperty, hoverColor);
                break;
            case PointerEventType.Unhover:
                meshRenderer.material.SetColor(EmissionProperty, originalEmissionColor);
                break;
            case PointerEventType.Select:
                ScaleTo(scaleOnSelect);
                break;
            case PointerEventType.Unselect:
                ScaleTo(1f, () => onClick.Invoke());
                break;
        }
    }

    void ScaleTo(float targetScale, System.Action onComplete = null)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleMesh(targetScale, scaleTime, onComplete));
    }
    private IEnumerator ScaleMesh(float targetScale, float duration, System.Action onComplete = null)
    {
        float currentTime = 0;
        Vector3 startScale = mesh.transform.localScale;
        Vector3 endScale = targetScale * originalScale;
        while (currentTime < duration)
        {
            mesh.transform.localScale = Vector3.Lerp(startScale, endScale, currentTime / duration);
            currentTime += Time.deltaTime;
            yield return null;
        }

        mesh.transform.localScale = targetScale * originalScale;
        scaleCoroutine = null;
        
        onComplete?.Invoke();
    }
}