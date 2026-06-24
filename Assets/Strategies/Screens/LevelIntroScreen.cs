using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class LevelIntroScreen : MonoBehaviour
{
    [Header("Story")]
    [SerializeField] private Sprite _storySprite;
    [SerializeField] private string _continueMessage = "Press SPACE to continue";

    [Header("References")]
    [SerializeField] private WakeUpBlinkManager _wakeUpBlinkManager;
    [SerializeField] private Canvas _targetCanvas;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float _minDisplayTime = 4f;
    [SerializeField, Min(0f)] private float _fadeInDuration = 0.6f;
    [SerializeField, Min(0f)] private float _fadeOutDuration = 1f;

    private GameObject _storyRoot;
    private Image _storyImage;
    private Text _continueText;
    private CanvasGroup _storyCanvasGroup;

    private void Start()
    {
        if (_wakeUpBlinkManager == null)
            _wakeUpBlinkManager = FindFirstObjectByType<WakeUpBlinkManager>();

        if (_storySprite == null)
        {
            Debug.LogWarning("LevelIntroScreen: no hay story sprite asignado. Se salta la intro.", this);
            BeginWakeUp();
            return;
        }

        StartCoroutine(PlayStoryIntro());
    }

    private IEnumerator PlayStoryIntro()
    {
        EnsureStoryUi();
        _wakeUpBlinkManager?.SetPlayerControl(false);
        ShowStoryUi();

        if (_fadeInDuration > 0f)
            yield return FadeStory(0f, 1f, _fadeInDuration);
        else if (_storyCanvasGroup != null)
            _storyCanvasGroup.alpha = 1f;

        yield return WaitForContinueInput();

        yield return FadeStory(1f, 0f, _fadeOutDuration);

        HideStoryUi();
        BeginWakeUp();
    }

    private IEnumerator WaitForContinueInput()
    {
        float elapsedTime = 0f;

        // Evita que el SPACE de AsyncLoad salte la intro al entrar al nivel.
        while (IsSpacePressed())
            yield return null;

        while (elapsedTime < _minDisplayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (_continueText != null)
            _continueText.text = _continueMessage;

        while (!WasSpacePressedThisFrame())
            yield return null;
    }

    private static bool IsSpacePressed()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.spaceKey.isPressed;
    }

    private static bool WasSpacePressedThisFrame()
    {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
    }

    private void EnsureStoryUi()
    {
        if (_storyRoot != null)
            return;

        Canvas canvas = _targetCanvas != null ? _targetCanvas : FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("LevelIntroScreen: no se encontro Canvas para mostrar la historia.", this);
            return;
        }

        _storyRoot = new GameObject("StoryIntro", typeof(RectTransform), typeof(CanvasGroup));
        _storyRoot.transform.SetParent(canvas.transform, false);
        _storyRoot.transform.SetAsLastSibling();

        RectTransform rootRect = _storyRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        _storyCanvasGroup = _storyRoot.GetComponent<CanvasGroup>();
        _storyCanvasGroup.alpha = 1f;
        _storyCanvasGroup.blocksRaycasts = true;

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(_storyRoot.transform, false);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image backgroundImage = background.GetComponent<Image>();
        backgroundImage.color = Color.black;
        backgroundImage.raycastTarget = true;

        GameObject imageObject = new GameObject("StoryImage", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(_storyRoot.transform, false);
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = new Vector2(0.05f, 0.12f);
        imageRect.anchorMax = new Vector2(0.95f, 0.88f);
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        _storyImage = imageObject.GetComponent<Image>();
        _storyImage.sprite = _storySprite;
        _storyImage.preserveAspect = true;
        _storyImage.color = Color.white;
        _storyImage.raycastTarget = false;

        GameObject textObject = new GameObject("ContinueText", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(_storyRoot.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0f);
        textRect.anchorMax = new Vector2(0.5f, 0f);
        textRect.pivot = new Vector2(0.5f, 0f);
        textRect.anchoredPosition = new Vector2(0f, 36f);
        textRect.sizeDelta = new Vector2(900f, 60f);

        _continueText = textObject.GetComponent<Text>();
        _continueText.text = string.Empty;
        _continueText.alignment = TextAnchor.MiddleCenter;
        _continueText.color = Color.white;
        _continueText.fontSize = 28;
        _continueText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _continueText.raycastTarget = false;
    }

    private void ShowStoryUi()
    {
        if (_storyRoot != null)
            _storyRoot.SetActive(true);

        if (_storyCanvasGroup != null)
            _storyCanvasGroup.alpha = 1f;
    }

    private void HideStoryUi()
    {
        if (_storyRoot != null)
            _storyRoot.SetActive(false);
    }

    private IEnumerator FadeStory(float from, float to, float duration)
    {
        if (_storyCanvasGroup == null || duration <= 0f)
            yield break;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float time = Mathf.Clamp01(elapsedTime / duration);
            _storyCanvasGroup.alpha = Mathf.Lerp(from, to, time);
            yield return null;
        }

        _storyCanvasGroup.alpha = to;
    }

    private void BeginWakeUp()
    {
        if (_wakeUpBlinkManager != null)
            _wakeUpBlinkManager.BeginWakeUp();
    }
}
