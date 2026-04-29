using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AsyncLoadScreen : MonoBehaviour
{
    [SerializeField] private Image _progressBar;
    [SerializeField] private Text _progressText;

    [SerializeField] private string _targetScreen = "Game";

    public void Start()
    {
        _targetScreen = PlayerPrefs.GetString("TargetScreen");
        StartCoroutine(LoadScreenAsync());
    }

    IEnumerator LoadScreenAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(_targetScreen);
        operation.allowSceneActivation = false;
        float progress = 0;

        while (!operation.isDone)
        {
            progress = operation.progress;

            _progressBar.fillAmount = progress;
            _progressText.text = $"{progress * 100} %";

            if (progress >= .9f)
            {
                _progressText.text = "Press SPACE to continue!";

                if (Input.GetKeyDown(KeyCode.Space))
                    operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
