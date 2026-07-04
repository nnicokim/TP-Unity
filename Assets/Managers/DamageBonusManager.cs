using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DamageBonusManager : MonoBehaviour
{
    public static DamageBonusManager instance;

    [Header("Damage Bonus")]
    [SerializeField, Min(1f)] private float damageMultiplier = 2f;

    [Header("UI")]
    [SerializeField] private Text damageBonusText;
    [SerializeField] private bool createUiIfMissing = true;
    [SerializeField] private string messageFormat = "X2 damage - Time left: {0}";
    [SerializeField] private Vector2 uiAnchoredPosition = new Vector2(20f, 20f);

    private Coroutine _bonusRoutine;
    private float _activeMultiplier = 1f;

    public static float CurrentMultiplier => instance != null ? instance._activeMultiplier : 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        ResolveUi();
        SetUiVisible(false);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public static void ActivateDamageBonus(float duration)
    {
        DamageBonusManager manager = instance != null ? instance : FindFirstObjectByType<DamageBonusManager>();
        if (manager == null)
        {
            GameObject managerObject = new GameObject("DamageBonusManager");
            manager = managerObject.AddComponent<DamageBonusManager>();
        }

        manager.Activate(duration);
    }

    public static int ApplyWeaponDamageMultiplier(int baseDamage)
    {
        return Mathf.Max(0, Mathf.RoundToInt(baseDamage * CurrentMultiplier));
    }

    public void Activate(float duration)
    {
        if (_bonusRoutine != null)
            StopCoroutine(_bonusRoutine);

        _bonusRoutine = StartCoroutine(BonusRoutine(Mathf.Max(0f, duration)));
    }

    private IEnumerator BonusRoutine(float duration)
    {
        ResolveUi();
        _activeMultiplier = damageMultiplier;
        SetUiVisible(true);

        float timeLeft = duration;
        while (timeLeft > 0f)
        {
            UpdateUi(timeLeft);
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        UpdateUi(0f);
        _activeMultiplier = 1f;
        SetUiVisible(false);
        _bonusRoutine = null;
    }

    private void ResolveUi()
    {
        if (damageBonusText != null || !createUiIfMissing)
            return;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("DamageBonusManager: no se encontro Canvas para crear DamageBonusUI.", this);
            return;
        }

        Transform uiParent = canvas.transform.Find("Game UI");
        if (uiParent == null)
            uiParent = canvas.transform;

        GameObject container = new GameObject("DamageBonusUI", typeof(RectTransform));
        container.transform.SetParent(uiParent, false);

        RectTransform containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.zero;
        containerRect.pivot = Vector2.zero;
        containerRect.anchoredPosition = uiAnchoredPosition;
        containerRect.sizeDelta = new Vector2(360f, 40f);

        GameObject textObject = new GameObject("DamageBonusText", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(container.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        damageBonusText = textObject.GetComponent<Text>();
        damageBonusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (damageBonusText.font == null)
            damageBonusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        damageBonusText.fontSize = 20;
        damageBonusText.alignment = TextAnchor.MiddleLeft;
        damageBonusText.color = Color.white;
    }

    private void UpdateUi(float timeLeft)
    {
        if (damageBonusText == null)
            return;

        damageBonusText.text = string.Format(messageFormat, Mathf.CeilToInt(timeLeft));
    }

    private void SetUiVisible(bool isVisible)
    {
        if (damageBonusText != null)
            damageBonusText.gameObject.SetActive(isVisible);
    }
}
