using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UI_PauseMenu : MonoBehaviour
{
    private const string MENU_SCENE_NAME = "Menu";

    [Header("Pause UI")]
    [SerializeField] private GameObject _pauseRoot;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _backToMenuButton;

    private void Start()
    {
        BindButtons();
        SetPauseMenuVisible(false);
    }

    private void OnDestroy()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(ResumeGame);

        if (_backToMenuButton != null)
            _backToMenuButton.onClick.RemoveListener(LoadMenu);
    }

    public void SetPauseMenuVisible(bool isVisible)
    {
        if (_pauseRoot != null)
            _pauseRoot.SetActive(isVisible);

        if (isVisible && _pauseRoot != null)
            _pauseRoot.transform.SetAsLastSibling();
    }

    private void BindButtons()
    {
        if (_resumeButton != null)
        {
            _resumeButton.onClick.RemoveListener(ResumeGame);
            _resumeButton.onClick.AddListener(ResumeGame);
        }

        if (_backToMenuButton != null)
        {
            _backToMenuButton.onClick.RemoveListener(LoadMenu);
            _backToMenuButton.onClick.AddListener(LoadMenu);
        }
    }

    private void ResumeGame()
    {
        if (GameManager.instance != null)
            GameManager.instance.SetPaused(false);
        else
            SetPauseMenuVisible(false);
    }

    private void LoadMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(MENU_SCENE_NAME);
    }
}
