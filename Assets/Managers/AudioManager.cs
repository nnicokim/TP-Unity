using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    #region SINGLETON
    public static AudioManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        ResolveSource();
    }
    #endregion

    #region MUSIC
    [Header("Music source")]
    [SerializeField] private AudioSource _musicSource;

    [Header("Tracks por escena")]
    [SerializeField] private List<SceneMusicEntry> _sceneMusic = new List<SceneMusicEntry>();

    [System.Serializable]
    public struct SceneMusicEntry
    {
        public string SceneName;
        public AudioClip Music;
        [Range(0f, 1f)] public float Volume;
    }
    #endregion

    #region UNITY_EVENTS
    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (_subscribedActions != null)
        {
            _subscribedActions.OnGameover -= OnGameover;
            _subscribedActions = null;
        }
    }
    #endregion

    #region SCENE_HANDLING
    private ActionsManager _subscribedActions;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (TryGetSceneMusic(scene.name, out SceneMusicEntry entry))
            PlayMusic(entry.Music, entry.Volume);

        SubscribeToGameover();
    }

    private void SubscribeToGameover()
    {
        if (_subscribedActions != null)
            _subscribedActions.OnGameover -= OnGameover;

        if (ActionsManager.instance == null)
        {
            _subscribedActions = null;
            return;
        }

        _subscribedActions = ActionsManager.instance;
        _subscribedActions.OnGameover += OnGameover;
    }

    private void OnGameover(bool isVictory)
    {
        StopMusic();
    }

    private bool TryGetSceneMusic(string sceneName, out SceneMusicEntry entry)
    {
        for (int i = 0; i < _sceneMusic.Count; i++)
        {
            if (_sceneMusic[i].SceneName == sceneName)
            {
                entry = _sceneMusic[i];
                return true;
            }
        }

        entry = default;
        return false;
    }
    #endregion

    #region API
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (_musicSource == null)
            ResolveSource();

        if (_musicSource == null)
            return;

        if (clip == null)
        {
            _musicSource.Stop();
            _musicSource.clip = null;
            return;
        }

        if (_musicSource.clip == clip && _musicSource.isPlaying)
        {
            _musicSource.volume = volume;
            return;
        }

        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.volume = volume;
        _musicSource.spatialBlend = 0f;
        _musicSource.playOnAwake = false;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        if (_musicSource != null)
            _musicSource.Stop();
    }
    #endregion

    private void ResolveSource()
    {
        if (_musicSource == null)
            _musicSource = GetComponent<AudioSource>();
    }
}
