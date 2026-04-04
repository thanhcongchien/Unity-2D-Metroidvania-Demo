using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioDatabaseSO audioDatabase;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float regularBgmFadeDuration = 1f;
    [SerializeField] private float bossBgmFadeDuration = 1.5f;
    [SerializeField] private float stopBgmFadeDuration = 1f;

    private AudioSource secondaryBgmSource;
    private AudioSource activeBgmSource;
    private AudioSource inactiveBgmSource;
    private AudioClip lastMusicPlayed;
    private Transform player;
    private string currentBGMgroupName;
    private Coroutine currentBGMCo;
    [SerializeField] private bool bgmShouldPlay;
    private bool bossMusicOverrideActive;
    private string bossMusicGroupName;
    private string previousMusicGroupName;
    private float defaultBgmVolume = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeBgmSources();
        PreloadMusicClips();
        TryResolvePlayer();
    }

    private void Update()
    {
        if (bgmShouldPlay == false || currentBGMCo != null || string.IsNullOrEmpty(currentBGMgroupName))
            return;

        if (IsAnyBgmPlaying())
            return;

        float fadeDuration = bossMusicOverrideActive ? bossBgmFadeDuration : regularBgmFadeDuration;
        StartSwitchMusic(currentBGMgroupName, bossMusicOverrideActive, fadeDuration);
    }

    public void NextBGM(string musicGroup)
    {
        if (bossMusicOverrideActive && musicGroup != bossMusicGroupName)
            return;

        bgmShouldPlay = true;
        currentBGMgroupName = musicGroup;

        StartSwitchMusic(musicGroup, false, regularBgmFadeDuration);
    }

    public void StartBGM(string musicGroup)
    {
        if (bossMusicOverrideActive && musicGroup != bossMusicGroupName)
            return;

        bgmShouldPlay = true;

        if (musicGroup == currentBGMgroupName && IsAnyBgmPlaying())
            return;

        NextBGM(musicGroup);
    }

    public void StopBGM()
    {
        bgmShouldPlay = false;
        StopCurrentBgmTransition();
        currentBGMCo = StartCoroutine(FadeOutAndStopAllBgmCo(stopBgmFadeDuration));
    }

    public void StartBossBGM(string bossMusicGroup)
    {
        if (string.IsNullOrWhiteSpace(bossMusicGroup))
            return;

        if (bossMusicOverrideActive && currentBGMgroupName == bossMusicGroup && IsAnyBgmPlaying())
            return;

        if (bossMusicOverrideActive == false)
            previousMusicGroupName = currentBGMgroupName;

        bossMusicOverrideActive = true;
        bossMusicGroupName = bossMusicGroup;
        bgmShouldPlay = true;
        currentBGMgroupName = bossMusicGroup;

        StartSwitchMusic(bossMusicGroup, true, bossBgmFadeDuration);
    }

    public void StopBossBGM(bool resumePreviousBGM = true)
    {
        if (bossMusicOverrideActive == false)
            return;

        bossMusicOverrideActive = false;
        bossMusicGroupName = null;

        if (resumePreviousBGM && string.IsNullOrWhiteSpace(previousMusicGroupName) == false)
        {
            string musicToResume = previousMusicGroupName;
            previousMusicGroupName = null;
            StartBGM(musicToResume);
            return;
        }

        previousMusicGroupName = null;
        StopBGM();
    }

    private void InitializeBgmSources()
    {
        if (bgmSource == null)
            return;

        defaultBgmVolume = Mathf.Clamp01(bgmSource.volume);
        if (defaultBgmVolume <= 0f)
            defaultBgmVolume = 1f;

        // 运行时复制一个 BGM 音源，用来和主音源做交叉淡入淡出。
        secondaryBgmSource = bgmSource.gameObject.AddComponent<AudioSource>();
        CopyAudioSourceSettings(bgmSource, secondaryBgmSource);
        secondaryBgmSource.playOnAwake = false;
        secondaryBgmSource.volume = 0f;

        activeBgmSource = bgmSource;
        inactiveBgmSource = secondaryBgmSource;
    }

    private void CopyAudioSourceSettings(AudioSource sourceToCopy, AudioSource targetSource)
    {
        if (sourceToCopy == null || targetSource == null)
            return;

        targetSource.outputAudioMixerGroup = sourceToCopy.outputAudioMixerGroup;
        targetSource.mute = sourceToCopy.mute;
        targetSource.bypassEffects = sourceToCopy.bypassEffects;
        targetSource.bypassListenerEffects = sourceToCopy.bypassListenerEffects;
        targetSource.bypassReverbZones = sourceToCopy.bypassReverbZones;
        targetSource.priority = sourceToCopy.priority;
        targetSource.volume = sourceToCopy.volume;
        targetSource.pitch = sourceToCopy.pitch;
        targetSource.panStereo = sourceToCopy.panStereo;
        targetSource.spatialBlend = sourceToCopy.spatialBlend;
        targetSource.reverbZoneMix = sourceToCopy.reverbZoneMix;
        targetSource.dopplerLevel = sourceToCopy.dopplerLevel;
        targetSource.spread = sourceToCopy.spread;
        targetSource.rolloffMode = sourceToCopy.rolloffMode;
        targetSource.minDistance = sourceToCopy.minDistance;
        targetSource.maxDistance = sourceToCopy.maxDistance;
        targetSource.ignoreListenerPause = sourceToCopy.ignoreListenerPause;
        targetSource.ignoreListenerVolume = sourceToCopy.ignoreListenerVolume;
        targetSource.velocityUpdateMode = sourceToCopy.velocityUpdateMode;
        targetSource.spatialize = sourceToCopy.spatialize;
        targetSource.spatializePostEffects = sourceToCopy.spatializePostEffects;
        targetSource.playOnAwake = false;
        targetSource.loop = false;
    }

    private void PreloadMusicClips()
    {
        if (audioDatabase == null)
            return;

        // 提前加载音乐数据，避免第一次触发 boss 或切场景时临时解码造成卡顿。
        PreloadClipGroup(audioDatabase.mainMenuMusic);
        PreloadClipGroup(audioDatabase.levelMusic);
    }

    private void PreloadClipGroup(List<AudioClipData> groups)
    {
        if (groups == null)
            return;

        foreach (var group in groups)
        {
            if (group == null || group.clips == null)
                continue;

            foreach (var clip in group.clips)
            {
                if (clip == null)
                    continue;

                clip.LoadAudioData();
            }
        }
    }

    private void StartSwitchMusic(string musicGroup, bool loop, float fadeDuration)
    {
        StopCurrentBgmTransition();
        currentBGMCo = StartCoroutine(SwitchMusicCo(musicGroup, loop, fadeDuration));
    }

    private void StopCurrentBgmTransition()
    {
        if (currentBGMCo == null)
            return;

        StopCoroutine(currentBGMCo);
        currentBGMCo = null;
    }

    private IEnumerator SwitchMusicCo(string musicGroup, bool loop, float fadeDuration)
    {
        if (audioDatabase == null || bgmSource == null)
        {
            currentBGMCo = null;
            yield break;
        }

        AudioClipData data = audioDatabase.Get(musicGroup);
        if (data == null || data.clips == null || data.clips.Count == 0)
        {
            currentBGMCo = null;
            yield break;
        }

        AudioClip nextMusic = GetNextMusicClip(data);
        if (nextMusic == null)
        {
            currentBGMCo = null;
            yield break;
        }

        yield return EnsureClipLoadedCo(nextMusic);

        // 始终让一个音源负责当前音乐，另一个音源负责下一首音乐，方便做 crossfade。
        AudioSource outgoingSource = GetLoudestPlayingBgmSource();
        AudioSource incomingSource = GetOtherBgmSource(outgoingSource);

        if (incomingSource == null)
            incomingSource = inactiveBgmSource != null ? inactiveBgmSource : bgmSource;

        ResetBgmSource(incomingSource);
        incomingSource.clip = nextMusic;
        incomingSource.loop = loop;
        incomingSource.volume = 0f;
        incomingSource.Play();

        if (outgoingSource != null && outgoingSource.isPlaying)
        {
            yield return CrossfadeSourcesCo(outgoingSource, incomingSource, defaultBgmVolume, fadeDuration);
            ResetBgmSource(outgoingSource);
        }
        else
        {
            yield return FadeSingleSourceCo(incomingSource, defaultBgmVolume, fadeDuration);
        }

        lastMusicPlayed = nextMusic;
        activeBgmSource = incomingSource;
        inactiveBgmSource = GetOtherBgmSource(incomingSource);
        currentBGMCo = null;
    }

    private AudioClip GetNextMusicClip(AudioClipData data)
    {
        AudioClip nextMusic = data.GetRandomClip();
        if (nextMusic == null)
            return null;

        if (data.clips.Count > 1)
        {
            while (nextMusic == lastMusicPlayed)
                nextMusic = data.GetRandomClip();
        }

        return nextMusic;
    }

    private IEnumerator EnsureClipLoadedCo(AudioClip clip)
    {
        if (clip == null)
            yield break;

        if (clip.loadState == AudioDataLoadState.Loaded)
            yield break;

        clip.LoadAudioData();

        // 等到音频真正进入 Loaded 状态后再播放，避免切歌瞬间卡一下。
        float timeout = 5f;
        while (clip.loadState == AudioDataLoadState.Loading && timeout > 0f)
        {
            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private IEnumerator CrossfadeSourcesCo(AudioSource outgoingSource, AudioSource incomingSource, float targetIncomingVolume, float duration)
    {
        if (incomingSource == null)
            yield break;

        // 旧音乐同步淡出，新音乐同步淡入，听感会比先停后播自然很多。
        float safeDuration = Mathf.Max(duration, 0.01f);
        float time = 0f;
        float startOutgoingVolume = outgoingSource != null ? outgoingSource.volume : 0f;

        while (time < safeDuration)
        {
            time += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(time / safeDuration);

            if (outgoingSource != null)
                outgoingSource.volume = Mathf.Lerp(startOutgoingVolume, 0f, progress);

            incomingSource.volume = Mathf.Lerp(0f, targetIncomingVolume, progress);
            yield return null;
        }

        if (outgoingSource != null)
            outgoingSource.volume = 0f;

        incomingSource.volume = targetIncomingVolume;
    }

    private IEnumerator FadeSingleSourceCo(AudioSource source, float targetVolume, float duration)
    {
        if (source == null)
            yield break;

        float safeDuration = Mathf.Max(duration, 0.01f);
        float time = 0f;
        float startVolume = source.volume;

        while (time < safeDuration)
        {
            time += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, Mathf.Clamp01(time / safeDuration));
            yield return null;
        }

        source.volume = targetVolume;
    }

    private IEnumerator FadeOutAndStopAllBgmCo(float duration)
    {
        float safeDuration = Mathf.Max(duration, 0.01f);
        float time = 0f;
        float primaryStartVolume = bgmSource != null ? bgmSource.volume : 0f;
        float secondaryStartVolume = secondaryBgmSource != null ? secondaryBgmSource.volume : 0f;

        while (time < safeDuration)
        {
            time += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(time / safeDuration);

            if (bgmSource != null)
                bgmSource.volume = Mathf.Lerp(primaryStartVolume, 0f, progress);

            if (secondaryBgmSource != null)
                secondaryBgmSource.volume = Mathf.Lerp(secondaryStartVolume, 0f, progress);

            yield return null;
        }

        ResetBgmSource(bgmSource);
        ResetBgmSource(secondaryBgmSource);
        currentBGMCo = null;
    }

    private void ResetBgmSource(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        source.clip = null;
        source.loop = false;
        source.volume = 0f;
    }

    private bool IsAnyBgmPlaying()
    {
        return (bgmSource != null && bgmSource.isPlaying) || (secondaryBgmSource != null && secondaryBgmSource.isPlaying);
    }

    private AudioSource GetLoudestPlayingBgmSource()
    {
        bool primaryPlaying = bgmSource != null && bgmSource.isPlaying;
        bool secondaryPlaying = secondaryBgmSource != null && secondaryBgmSource.isPlaying;

        if (primaryPlaying && secondaryPlaying)
            return bgmSource.volume >= secondaryBgmSource.volume ? bgmSource : secondaryBgmSource;

        if (primaryPlaying)
            return bgmSource;

        if (secondaryPlaying)
            return secondaryBgmSource;

        return null;
    }

    private AudioSource GetOtherBgmSource(AudioSource source)
    {
        if (source == null)
            return activeBgmSource != null ? activeBgmSource : bgmSource;

        if (source == bgmSource)
            return secondaryBgmSource;

        if (source == secondaryBgmSource)
            return bgmSource;

        return bgmSource;
    }

    public void PlaySFX(string soundName, AudioSource sfxSource, float minDistanceToHear = 5, bool playOneShot = true)
    {
        if (audioDatabase == null || sfxSource == null || string.IsNullOrEmpty(soundName))
            return;

        var data = audioDatabase.Get(soundName);
        if (data == null)
            return;

        var clip = data.GetRandomClip();

        if (clip == null)
            return;

        float maxVolume = data.volume;
        float targetVolume = maxVolume;

        if (TryResolvePlayer() && minDistanceToHear > 0f)
        {
            float distance = Vector2.Distance(player.position, sfxSource.transform.position);
            float t = Mathf.Clamp01(1 - (distance / minDistanceToHear));
            targetVolume = Mathf.Lerp(0, maxVolume, t * t);
        }

        sfxSource.volume = targetVolume;
        if (playOneShot)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            sfxSource.clip = clip;
            sfxSource.Play();
        }
    }

    public void PlayGlobalSFX(string soundName)
    {
        var data = audioDatabase.Get(soundName);
        if (data == null)
            return;

        var clip = data.GetRandomClip();
        if (clip == null)
            return;

        sfxSource.pitch = Random.Range(.95f, 1.1f);
        sfxSource.volume = data.volume;
        sfxSource.PlayOneShot(clip);
    }

    private bool TryResolvePlayer()
    {
        if (player != null)
            return true;

        if (Player.instance != null)
            player = Player.instance.transform;

        return player != null;
    }
}
