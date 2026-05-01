using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Gameover : MonoBehaviour
{
    [SerializeField] private Sprite _victory;
    [SerializeField] private Sprite _defeat;
    [SerializeField] private Image _gameoverImage;

    #region UNITY_EVENTS
    private void Start()
    {
        _gameoverImage.enabled = false;

        GameoverSuscribe();
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
        _gameoverImage.enabled = true;
        _gameoverImage.sprite = isVictory ? _victory : _defeat;
    }
    #endregion
}
