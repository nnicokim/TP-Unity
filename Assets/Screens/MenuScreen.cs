using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScreen : MonoBehaviour
{
    const string GAME_SCENE_NAME = "GameLevel";
    const string HELP_SCENE_NAME = "Help";
    const string SETTINGS_SCENE_NAME = "Settings";
    const string CREDITS_SCENE_NAME = "Credits";
    const string ASYNC_LOAD_SCENE_NAME = "AsyncLoad";

    [SerializeField] private Button _play, _help, _credits, _quit;

    private void Start()
    {
        _play.onClick.AddListener(LoadGame);
        _help.onClick.AddListener(LoadHelp);
        _credits.onClick.AddListener(LoadCredits);
        _quit.onClick.AddListener(Quit);
    }

    public void LoadGame()
    {
        PlayerPrefs.SetString("TargetScreen", GAME_SCENE_NAME);
        PlayerPrefs.Save();

        SceneManager.LoadScene(ASYNC_LOAD_SCENE_NAME);
    }
    public void LoadHelp() => SceneManager.LoadScene(HELP_SCENE_NAME);
    public void LoadSettings() => SceneManager.LoadScene(SETTINGS_SCENE_NAME);
    public void LoadCredits() => SceneManager.LoadScene(CREDITS_SCENE_NAME);
    public void Quit() => Application.Quit();
}
