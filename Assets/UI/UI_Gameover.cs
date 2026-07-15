using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Gameover : MonoBehaviour
{
    private const string MENU_SCENE_NAME = "Menu";
    private const string SCORE_SCROLL_VIEW_NAME = "VictoryStatsScrollView";

    [SerializeField] private Sprite _victory;
    [SerializeField] private Sprite _defeat;
    [SerializeField] private Image _gameoverImage;

    [Header("Gameover actions")]
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _backToMenuButton;

    private GameObject _scoreScrollView;
    private Text _scoreText;

    #region UNITY_EVENTS
    private void Start()
    {
        if (_gameoverImage != null)
            _gameoverImage.enabled = false;

        if (_retryButton != null)
        {
            _retryButton.gameObject.SetActive(false);
            _retryButton.onClick.AddListener(RetryLevel);
        }

        if (_backToMenuButton != null)
        {
            _backToMenuButton.gameObject.SetActive(false);
            _backToMenuButton.onClick.AddListener(LoadMenu);
        }

        GameoverSuscribe();
    }

    private void OnDestroy()
    {
        GameoverUnsuscribe();

        if (_retryButton != null)
            _retryButton.onClick.RemoveListener(RetryLevel);

        if (_backToMenuButton != null)
            _backToMenuButton.onClick.RemoveListener(LoadMenu);
    }
    #endregion

    #region ACTION_GAMEOVER
    private void GameoverSuscribe() => ActionsManager.instance.OnGameover += OnGameover;
    private void GameoverUnsuscribe() => ActionsManager.instance.OnGameover -= OnGameover;

    private void OnGameover(bool isVictory)
    {
        if (_gameoverImage != null)
        {
            _gameoverImage.enabled = true;
            _gameoverImage.sprite = isVictory ? _victory : _defeat;
        }

        if (isVictory)
            ShowVictoryScore();
        else
            SetVictoryScoreVisible(false);

        if (_retryButton != null)
            _retryButton.gameObject.SetActive(true);

        if (_backToMenuButton != null)
            _backToMenuButton.gameObject.SetActive(true);
    }
    #endregion

    #region GAMEOVER_ACTIONS
    private void RetryLevel()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;
        GameplayStatsManager.ResetStatsForNewRun();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene(MENU_SCENE_NAME);
    }
    #endregion

    #region VICTORY_SCORE
    private void ShowVictoryScore()
    {
        EnsureVictoryScoreUi();

        if (_scoreText != null)
            _scoreText.text = GameplayStatsManager.BuildVictoryScoreText();

        SetVictoryScoreVisible(true);
    }

    private void SetVictoryScoreVisible(bool isVisible)
    {
        if (_scoreScrollView != null)
            _scoreScrollView.SetActive(isVisible);
    }

    private void EnsureVictoryScoreUi()
    {
        if (_scoreScrollView != null)
            return;

        Transform parent = GetScoreUiParent();
        if (parent == null)
        {
            Debug.LogWarning("UI_Gameover: no se encontro un parent valido para crear VictoryStatsScrollView.", this);
            return;
        }

        _scoreScrollView = new GameObject(SCORE_SCROLL_VIEW_NAME, typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        _scoreScrollView.transform.SetParent(parent, false);
        _scoreScrollView.transform.SetAsLastSibling();
        _scoreScrollView.layer = parent.gameObject.layer;

        RectTransform scrollRectTransform = _scoreScrollView.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchoredPosition = new Vector2(0f, 0f);
        scrollRectTransform.sizeDelta = new Vector2(620f, 220f);

        Image background = _scoreScrollView.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.62f);
        background.raycastTarget = true;

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(_scoreScrollView.transform, false);
        viewport.layer = _scoreScrollView.layer;

        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = new Vector2(22f, 16f);
        viewportRect.offsetMax = new Vector2(-22f, -16f);

        Image viewportImage = viewport.GetComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
        viewportImage.raycastTarget = true;

        Mask viewportMask = viewport.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(Text), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        content.layer = _scoreScrollView.layer;

        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        _scoreText = content.GetComponent<Text>();
        _scoreText.font = GetDefaultFont();
        _scoreText.fontSize = 30;
        _scoreText.lineSpacing = 1.15f;
        _scoreText.alignment = TextAnchor.UpperLeft;
        _scoreText.color = new Color(0.95f, 0.91f, 0.78f, 1f);
        _scoreText.raycastTarget = false;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = _scoreScrollView.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 20f;
    }

    private Transform GetScoreUiParent()
    {
        if (_retryButton != null && _retryButton.transform.parent != null)
            return _retryButton.transform.parent;

        if (_backToMenuButton != null && _backToMenuButton.transform.parent != null)
            return _backToMenuButton.transform.parent;

        if (_gameoverImage != null && _gameoverImage.transform.parent != null)
            return _gameoverImage.transform.parent;

        return transform;
    }

    private static Font GetDefaultFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
            return font;

        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
    #endregion
}
