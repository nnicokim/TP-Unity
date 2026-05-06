using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InfoScreen : MonoBehaviour
{
    const string MENU_SCENE_NAME = "Menu";

    [SerializeField] private Button _backToMenuButton;

    private void Start()
    {
        _backToMenuButton.onClick.AddListener(LoadMenu);
    }

    private void LoadMenu() => SceneManager.LoadScene(MENU_SCENE_NAME);
}
