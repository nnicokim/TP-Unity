using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GameoverSounds : MonoBehaviour
{
    [SerializeField] private AudioClip _victory;
    [SerializeField] private AudioClip _defeat;
    private AudioSource _source;

    #region UNITY_EVENTS
    private void Start()
    {
        _source = GetComponent<AudioSource>();

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
        _source.PlayOneShot(isVictory ? _victory : _defeat);
    }
    #endregion
}
