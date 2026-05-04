using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class AsyncLoadScreen : MonoBehaviour
{
    [SerializeField] private Image _loadingBar;
    [SerializeField] private Text _loadingText;

    [SerializeField] private string _targetScreen = "Level";

    private void Start()
    {   
        _targetScreen = PlayerPrefs.GetString("TargetScreen");
        StartCoroutine(LoadScreenAsync());
    }

    IEnumerator LoadScreenAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(_targetScreen);
        operation.allowSceneActivation = false;
        _loadingText.text = "Loading...";

        while (!operation.isDone)
        {
            _loadingBar.fillAmount = operation.progress;

            if (operation.progress >= 0.9f)
            {
                _loadingText.text = "Press SPACE to continue";

                Keyboard keyboard = Keyboard.current;
                if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                    operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
