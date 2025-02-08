using UnityEngine;
using TMPro;
using System.Collections;

public class FadeTMPText : MonoBehaviour
{
    public TMP_Text tmpText;
    public float fadeDuration = 1.5f;
    public GameObject particleFX;
    private AudioSource audioSource;
    //add menu gameobject here
    //Disable paddles, ball and white wall smoothly 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(FadeInText());
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

