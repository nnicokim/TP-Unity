// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;

// public class GameManager : MonoBehaviour
// {
//     [SerializeField] private bool _isGameOver = false;
//     [SerializeField] private bool _isVictory = false;
//     [SerializeField] private Text _gameoverMessage;

//     private void Start()
//     {
//         EventManager.instance.OnGameOver += OnGameOver;
//         _gameoverMessage.text = string.Empty;
//     }

//     private void OnGameOver(bool isVictory)
//     {
//         _isGameOver = true;
//         _isVictory = isVictory;

//         _gameoverMessage.text = isVictory ? "Victory" : "Defeat";
//         _gameoverMessage.color = isVictory ? Color.cyan : Color.red;

//         Invoke("LoadEndgameScene", 3f);
//     }

//     private void LoadEndgameScene() => SceneManager.LoadScene(2);
// }
