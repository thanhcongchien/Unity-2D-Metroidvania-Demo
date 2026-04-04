using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class EntitySFX : MonoBehaviour
{
    private const string JumpSfx = "player_Jump";
    private const string LandSfx = "player_Land";
    private const string DashSfx = "player_Dash";
    private const string SuccessfulCounterAttackSfx = "player_SuccessfulCounterAttack";

    private AudioSource audioSource;
    [SerializeField] private AudioSource footstepSource;


    [Header("SFX Names")]
    [SerializeField] private string attackHit;
    [SerializeField] private string attackMiss;
    [Space]
    [SerializeField] private float soundDistance = 15f;

    [Header("Footstep Group (Random from 1/2/3)")]
    [SerializeField] private string footstep1;
    [SerializeField] private string footstep2;
    [SerializeField] private string footstep3;
    [SerializeField] private float footstepDistance = 12f;
    [SerializeField] private float footstepInterval = 0.2f;

    private Coroutine footstepLoopCo;

    private void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
        if (footstepSource == null)
            footstepSource = audioSource;
    }

    public void PlayAttackHit()
    {
        if (AudioManager.instance == null || audioSource == null)
            return;

        AudioManager.instance.PlaySFX(attackHit, audioSource, soundDistance);
    }

    public void PlayAttackMiss()
    {
        if (AudioManager.instance == null || audioSource == null)
            return;

        AudioManager.instance.PlaySFX(attackMiss, audioSource, soundDistance);
    }

    public void PlayJump()
    {
        PlayActionSFX(JumpSfx);
    }

    public void PlayLand()
    {
        PlayActionSFX(LandSfx);
    }

    public void PlayDash()
    {
        PlayActionSFX(DashSfx);
    }

    public void PlaySuccessfulCounterAttack()
    {
        PlayActionSFX(SuccessfulCounterAttackSfx);
    }

    public void StartFootstepLoop()
    {
        if (footstepLoopCo != null || AudioManager.instance == null || footstepSource == null)
            return;

        footstepLoopCo = StartCoroutine(FootstepLoopCo());
    }

    public void StopFootstepLoop()
    {
        if (footstepLoopCo != null)
        {
            StopCoroutine(footstepLoopCo);
            footstepLoopCo = null;
        }

        if (footstepSource != null && footstepSource.isPlaying)
            footstepSource.Stop();
    }

    private IEnumerator FootstepLoopCo()
    {
        while (true)
        {
            string soundName = GetRandomFootstepName();
            if (!string.IsNullOrEmpty(soundName) && AudioManager.instance != null && footstepSource != null)
            {
                AudioManager.instance.PlaySFX(soundName, footstepSource, footstepDistance, false);
            }

            yield return new WaitForSeconds(footstepInterval);
        }
    }

    private string GetRandomFootstepName()
    {
        string[] group = { footstep1, footstep2, footstep3 };
        int validCount = 0;

        for (int i = 0; i < group.Length; i++)
        {
            if (!string.IsNullOrEmpty(group[i]))
                validCount++;
        }

        if (validCount == 0)
            return string.Empty;

        int pick = Random.Range(0, validCount);
        int current = 0;
        for (int i = 0; i < group.Length; i++)
        {
            if (string.IsNullOrEmpty(group[i]))
                continue;

            if (current == pick)
                return group[i];

            current++;
        }

        return string.Empty;
    }

    private void OnDisable()
    {
        StopFootstepLoop();
    }

    private void PlayActionSFX(string soundName)
    {
        if (AudioManager.instance == null || audioSource == null || string.IsNullOrEmpty(soundName))
            return;

        AudioManager.instance.PlaySFX(soundName, audioSource, soundDistance);
    }
}
