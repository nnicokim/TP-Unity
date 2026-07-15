using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public bool isGameOver => _isGameOver;
    [SerializeField] private bool _isGameOver = false;
    [SerializeField] private bool _isVictory = false;

    public bool isGamePause => _isGamePause;
    [SerializeField] private bool _isGamePause = false;

    [SerializeField] private UI_PauseMenu _pauseMenu;

    #region SINGLETON
    static public GameManager instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;

        Time.timeScale = 1f;
        ResolvePauseMenu();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion

    #region UNITY_EVENTS
    private void Start()
    {
        GameoverSuscribe();
        _isGamePause = false;
        Time.timeScale = 1f;

        ResolvePauseMenu();
        if (_pauseMenu != null)
            _pauseMenu.SetPauseMenuVisible(false);
    }

    private void Update()
    {
        if (_isGameOver)
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            SetPaused(!_isGamePause);
    }

    private void OnDestroy()
    {
        GameoverUnsuscribe();
    }
    #endregion

    #region PAUSE
    public void SetPaused(bool isPaused)
    {
        if (_isGameOver)
            return;

        _isGamePause = isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        ResolvePauseMenu();
        if (_pauseMenu != null)
            _pauseMenu.SetPauseMenuVisible(isPaused);
        else if (isPaused)
            Debug.LogWarning("GameManager: no hay UI_PauseMenu asignado. Crea el menu de pausa en el Canvas y asignalo.", this);
    }

    private void ResolvePauseMenu()
    {
        if (_pauseMenu != null)
            return;

        _pauseMenu = GetComponent<UI_PauseMenu>();
        if (_pauseMenu == null)
            _pauseMenu = FindFirstObjectByType<UI_PauseMenu>();
    }
    #endregion

    #region ACTION_GAMEOVER
    private void GameoverSuscribe()
    {
        if (ActionsManager.instance != null)
            ActionsManager.instance.OnGameover += OnGameover;
    }

    private void GameoverUnsuscribe()
    {
        if (ActionsManager.instance != null)
            ActionsManager.instance.OnGameover -= OnGameover;
    }

    private void OnGameover(bool isVictory)
    {
        _isGameOver = true;
        _isVictory = isVictory;
        _isGamePause = false;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (_pauseMenu != null)
            _pauseMenu.SetPauseMenuVisible(false);
    }
    #endregion
}
