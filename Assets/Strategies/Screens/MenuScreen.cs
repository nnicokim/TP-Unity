using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScreen : MonoBehaviour
{
    const string GAME_SCENE_NAME = "Level";
    const string INFO_SCENE_NAME = "Info";
    const string SETTINGS_SCENE_NAME = "Settings";
    const string ASYNC_LOAD_SCENE_NAME = "AsyncLoad";

    [SerializeField] private Button _play, _info, _settings, _quit;

    private void Start()
    {
        _play.onClick.AddListener(LoadGame);
        _info.onClick.AddListener(LoadInfo);
        _settings.onClick.AddListener(LoadSettings);
        _quit.onClick.AddListener(Quit);
    }

    public void LoadGame()
    {
        PlayerPrefs.SetString("TargetScreen", GAME_SCENE_NAME);
        PlayerPrefs.Save();

        SceneManager.LoadScene(ASYNC_LOAD_SCENE_NAME);
    }

    public void LoadInfo() => SceneManager.LoadScene(INFO_SCENE_NAME);
    public void LoadSettings() => SceneManager.LoadScene(SETTINGS_SCENE_NAME);
    public void Quit() => Application.Quit();
}
