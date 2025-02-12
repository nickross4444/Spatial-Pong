using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;
using TMPro;
using UnityEngine.UI;


public class SettingsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject worldPassthrough;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Volume Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;
    
    [Header("Ball Speed Controls")]
    [SerializeField] private Slider ballSpeedSlider;
    [SerializeField] private TextMeshProUGUI ballSpeedText;
    
    [Header("Passthrough Toggle")]
    [SerializeField] private Toggle passthroughToggle;
    
    private void Start()
    {
        // Initialize volume slider
        volumeSlider.onValueChanged.AddListener(HandleVolumeChange);
        volumeSlider.value = audioSource.volume;
        // UpdateVolumeText(volumeSlider.value);
        
        // Initialize ball speed slider
        ballSpeedSlider.onValueChanged.AddListener(HandleBallSpeedChange);
        ballSpeedSlider.value = gameManager.kickForce;
        // UpdateBallSpeedText(ballSpeedSlider.value);
        
        // Initialize passthrough toggle
        passthroughToggle.onValueChanged.AddListener(HandlePassthroughToggle);
        passthroughToggle.isOn = worldPassthrough.activeSelf;
    }
    
    private void HandleVolumeChange(float value)
    {
        audioSource.volume = value;
        // UpdateVolumeText(value);
    }
    
    // private void UpdateVolumeText(float value)
    // {
    //     volumeText.text = $"Volume: {(value * 100):F0}%";
    // }
    
    private void HandleBallSpeedChange(float value)
    {
        gameManager.kickForce = value;
        // UpdateBallSpeedText(value);
    }
    
    // private void UpdateBallSpeedText(float value)
    // {
    //     ballSpeedText.text = $"Ball Speed: {value:F1}";
    // }
    
    private void HandlePassthroughToggle(bool isOn)
    {
        worldPassthrough.SetActive(isOn);
    }
}