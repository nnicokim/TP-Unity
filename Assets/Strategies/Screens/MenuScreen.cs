using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScreen : MonoBehaviour
{
    const string GAME_SCENE_NAME = "Level_1";
    const string INFO_SCENE_NAME = "Info";
    const string ASYNC_LOAD_SCENE_NAME = "AsyncLoad";

    [SerializeField] private Button _play, _info, _quit;

    private void Start()
    {
        _play.onClick.AddListener(LoadGame);
        _info.onClick.AddListener(LoadInfo);
        _quit.onClick.AddListener(Quit);
    }

    public void LoadGame()
    {
        PlayerPrefs.SetString("TargetScreen", GAME_SCENE_NAME);
        PlayerPrefs.Save();

        SceneManager.LoadScene(ASYNC_LOAD_SCENE_NAME);
    }

    public void LoadInfo() => SceneManager.LoadScene(INFO_SCENE_NAME);
    public void Quit() => Application.Quit();
}
