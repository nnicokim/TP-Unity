using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool _isGameOver = false;
    [SerializeField] private bool _isVictory = false;

    public bool isGamePause => _isGamePause;
    [SerializeField] private bool _isGamePause = false;

    #region SINGLETON
    static public GameManager instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
    }
    #endregion

    #region UNITY_EVENTS
    private void Start()
    {
        GameoverSuscribe();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            _isGamePause = !_isGamePause;
        }
    }

    private void OnDestroy()
    {
        GameoverUnsuscribe();
    }
    #endregion

    #region ACTION_GAMEOVER
    private void GameoverSuscribe() => ActionsManager.instance.OnGameover += OnGameover;
    private void GameoverUnsuscribe() => ActionsManager.instance.OnGameover -= OnGameover;

    private void OnGameover(bool isVictory)
    {
        _isGameOver = true;
        _isVictory = isVictory;
    }
    #endregion
}
