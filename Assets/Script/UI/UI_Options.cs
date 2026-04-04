using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UI_Options : MonoBehaviour
{
    private Player player;

    [SerializeField] private Toggle healthBarToggle;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixerMultiplier = 25;

    [Header("BGM Volume Settings")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private string bgmParameter;

    [Header("SFX Volume Settings")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private string sfxParameter;

    private void Start()
    {
        player = FindFirstObjectByType<Player>();

        if (healthBarToggle == null)
        {
            healthBarToggle = GetComponentInChildren<Toggle>(true);
        }

        if (healthBarToggle == null)
        {
            
            return;
        }

        healthBarToggle.onValueChanged.AddListener(OnHealthbarToggleChaneged);
    }

    public void BGMSliderValue(float value)
    {
        float newValue = Mathf.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(bgmParameter, newValue);
    }
    public void SFXSliderValue(float value)
    {
        float newValue = Mathf.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(sfxParameter, newValue);
    }

    private void OnDestroy()
    {
        if (healthBarToggle != null)
        {
            healthBarToggle.onValueChanged.RemoveListener(OnHealthbarToggleChaneged);
        }
    }

    private void OnHealthbarToggleChaneged(bool isOn)
    {
        if (player == null || player.health == null)
        {
            Debug.LogWarning("UI_Options: Player or Player.health is missing.");
            return;
        }

        player.health.EnableHealthbar(isOn);
    }

    public void GoMainMenuBTN()=> GameManager.Instance.ChangeScene("MainMenu",RespawnType.None);

    private void OnEnable()
    {
        if (!HasValidVolumeUIReferences())
        {
            return;
        }

        sfxSlider.value=PlayerPrefs.GetFloat(sfxParameter,.6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParameter, .6f);
    }

    private void OnDisable()
    {
        if (!HasValidVolumeUIReferences())
        {
            return;
        }

        PlayerPrefs.SetFloat(sfxParameter, sfxSlider.value);
        PlayerPrefs.SetFloat(bgmParameter, bgmSlider.value);
    }
    public void LoadUpVolume()
    {
        if (!HasValidVolumeUIReferences())
        {
            return;
        }

        sfxSlider.value = PlayerPrefs.GetFloat(sfxParameter, .6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParameter, .6f);
    }

    private bool HasValidVolumeUIReferences()
    {
        if (sfxSlider == null || bgmSlider == null)
        {
            Debug.LogError($"UI_Options on '{name}' is missing slider references. Assign SFX and BGM sliders in Inspector.");
            return false;
        }

        if (string.IsNullOrEmpty(sfxParameter) || string.IsNullOrEmpty(bgmParameter))
        {
            Debug.LogError($"UI_Options on '{name}' is missing mixer parameter names. Assign SFX/BGM parameter strings in Inspector.");
            return false;
        }

        return true;
    }
}
