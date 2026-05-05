using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Gameover : MonoBehaviour
{
    private const string MENU_SCENE_NAME = "Menu";

    [SerializeField] private Sprite _victory;
    [SerializeField] private Sprite _defeat;
    [SerializeField] private Image _gameoverImage;

    [Header("Back to menu")]
    [SerializeField] private Button _backToMenuButton;

    #region UNITY_EVENTS
    private void Start()
    {
        _gameoverImage.enabled = false;

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

        if (_backToMenuButton != null)
            _backToMenuButton.gameObject.SetActive(true);
    }
    #endregion

    #region BACK_TO_MENU
    private void LoadMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(MENU_SCENE_NAME);
    }
    #endregion
}
