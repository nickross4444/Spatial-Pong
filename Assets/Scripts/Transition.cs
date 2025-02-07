using JetBrains.Annotations;
using UnityEngine;

public class Transition : MonoBehaviour
{
    public Material revealMaterial;
    public float revealSpeed = 10f;
    private float transitionValue = -5f;
    public bool isChanging = false;

    private void Start()
    {
        revealMaterial = GetComponent<Renderer>().material;
    }


    void Update()
    {
       
        if (isChanging && transitionValue < 10)
        {
            transitionValue = transitionValue + revealSpeed * Time.deltaTime;
            Debug.Log(transitionValue);
            //
            revealMaterial.SetFloat("_Height", transitionValue);
        }
    }
}