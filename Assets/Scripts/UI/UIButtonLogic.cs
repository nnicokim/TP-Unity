using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_ButtonsLogic : MonoBehaviour
{
    // TODO: adaptar a nuestras pantallas
    public void LoadMenuScene() => SceneManager.LoadScene(0);
    public void LoadLevelScene() => SceneManager.LoadScene(1);
    public void LoadEndgameScene() => SceneManager.LoadScene(2);
    public void LoadInfoScene() => Debug.Log("Information scene in development!!!");
    public void LoadSettingsScene() => Debug.Log("Settings scene in development!!!");
    public void CloseGame() => Application.Quit();
}
