using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Gameover : MonoBehaviour
{
    private const string MENU_SCENE_NAME = "Menu";

    [SerializeField] private Sprite _victory;
    [SerializeField] private Sprite _defeat;
    [SerializeField] private Image _gameoverImage;

    [Header("Gameover actions")]
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _backToMenuButton;

    #region UNITY_EVENTS
    private void Start()
    {
        _gameoverImage.enabled = false;

        if (_retryButton != null)
        {
            _retryButton.gameObject.SetActive(false);
            _retryButton.onClick.AddListener(RetryLevel);
        }

        if (_backToMenuButton != null)
        {
            _backToMenuButton.gameObject.SetActive(false);
            _backToMenuButton.onClick.AddListener(LoadMenu);
        }

        GameoverSuscribe();
    }

    private void OnDestroy()
    {
        GameoverUnsuscribe();

        if (_retryButton != null)
            _retryButton.onClick.RemoveListener(RetryLevel);

        if (_backToMenuButton != null)
            _backToMenuButton.onClick.RemoveListener(LoadMenu);
    }
    #endregion

    #region ACTION_GAMEOVER
    private void GameoverSuscribe() => ActionsManager.instance.OnGameover += OnGameover;
    private void GameoverUnsuscribe() => ActionsManager.instance.OnGameover -= OnGameover;

    private void OnGameover(bool isVictory)
    {
        _gameoverImage.enabled = true;
        _gameoverImage.sprite = isVictory ? _victory : _defeat;

        if (_retryButton != null)
            _retryButton.gameObject.SetActive(true);

        if (_backToMenuButton != null)
            _backToMenuButton.gameObject.SetActive(true);
    }
    #endregion

    #region GAMEOVER_ACTIONS
    private void RetryLevel()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene(MENU_SCENE_NAME);
    }
    #endregion
}
