using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;


public class SettingsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Controls")]
    [SerializeField] private Slider volumeSlider;

    [Header("Ball Speed Controls")]
    [SerializeField] private Slider ballSpeedSlider;

    [Header("Passthrough Toggle")]
    [SerializeField] private Toggle passthroughToggle;
    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }
    private void Start()
    {
        // Initialize volume slider
        volumeSlider.onValueChanged.AddListener(HandleVolumeChange);

        // Initialize ball speed slider
        ballSpeedSlider.onValueChanged.AddListener(HandleBallSpeedChange);

        // Initialize passthrough toggle
        passthroughToggle.onValueChanged.AddListener(HandlePassthroughToggle);
    }
    void OnEnable()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        ballSpeedSlider.value = PlayerPrefs.GetFloat("KickForce", 1f);
        passthroughToggle.isOn = PlayerPrefs.GetInt("UsePassthrough", 1) == 1;
    }

    private void HandleVolumeChange(float value)
    {
        float volume = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20f;
        audioMixer.SetFloat("MasterVolume", volume);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }


    private void HandleBallSpeedChange(float value)
    {
        gameManager.kickForce = value;
        PlayerPrefs.SetFloat("KickForce", value);
    }

    private void HandlePassthroughToggle(bool isOn)
    {
        gameManager.usePassthrough = isOn;
        GameSetup gameSetup = gameManager.GetComponent<GameSetup>();
        gameSetup.SetPongPassthrough(gameSetup.setupComplete);      //set to pong passthrough if setup is complete
        PlayerPrefs.SetInt("UsePassthrough", isOn ? 1 : 0);
    }
    public void ToggleSettings()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}