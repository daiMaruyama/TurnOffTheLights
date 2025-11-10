using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundData
    {
        public string name;
        public AudioClip audioClip;
        [HideInInspector] public float playedTime;
    }

    [SerializeField] SoundData[] _soundDatas;
    [SerializeField] int _maxAudioSources = 20;
    [SerializeField] float _playableDistance = 0.2f;
    [SerializeField] float _bgmFadeDuration = 1.5f;

    AudioSource[] _audioSources;
    AudioSource _bgmSource;
    Dictionary<string, SoundData> _soundDictionary = new Dictionary<string, SoundData>();

    static AudioManager _instance;
    public static AudioManager Instance => _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSources = new AudioSource[_maxAudioSources];
        for (int i = 0; i < _maxAudioSources; i++)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            _audioSources[i] = source;
        }

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.spatialBlend = 0f;

        foreach (var data in _soundDatas)
        {
            if (!_soundDictionary.ContainsKey(data.name))
            {
                _soundDictionary.Add(data.name, data);
            }
            else
            {
                Debug.LogWarning($"重複しているサウンド名があります: {data.name}");
            }
        }

        // シーンが変わったときにBGMを自動で変更
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーン読み込み時に自動でBGM切り替え
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string bgmName = $"BGM_{scene.name}";
        if (_soundDictionary.TryGetValue(bgmName, out var bgmData))
        {
            PlayBGM(bgmData.audioClip);
        }
        else
        {
            Debug.Log($"このシーン専用のBGMは登録されていません: {scene.name}");
        }
    }

    AudioSource GetUnusedAudioSource()
    {
        foreach (var source in _audioSources)
        {
            if (!source.isPlaying)
                return source;
        }
        return null;
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        var source = GetUnusedAudioSource();
        if (source == null || clip == null) return;

        source.clip = clip;
        source.volume = volume;
        source.Play();
    }

    public void Play(string name, float volume = 1f)
    {
        if (_soundDictionary.TryGetValue(name, out var data))
        {
            if (Time.realtimeSinceStartup - data.playedTime < _playableDistance) return;

            data.playedTime = Time.realtimeSinceStartup;
            Play(data.audioClip, volume);
        }
        else
        {
            Debug.LogWarning($"登録されていないサウンド名です: {name}");
        }
    }

    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
        StopAllCoroutines();
        StartCoroutine(FadeInBGM(clip, volume));
    }

    IEnumerator FadeInBGM(AudioClip clip, float targetVolume)
    {
        if (_bgmSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutCoroutine(_bgmFadeDuration));
        }

        _bgmSource.clip = clip;
        _bgmSource.volume = 0f;
        _bgmSource.Play();

        float time = 0f;
        while (time < _bgmFadeDuration)
        {
            time += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(0f, targetVolume, time / _bgmFadeDuration);
            yield return null;
        }
    }

    IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = _bgmSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            _bgmSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        _bgmSource.Stop();
        _bgmSource.volume = startVolume;
    }

    public void StopBGM()
    {
        _bgmSource.Stop();
    }
}
