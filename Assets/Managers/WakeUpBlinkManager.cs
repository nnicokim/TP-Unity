using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class WakeUpBlinkManager : MonoBehaviour
{
    [Header("Eyelids")]
    [SerializeField] private RectTransform topEyelid;
    [SerializeField] private RectTransform bottomEyelid;

    [Header("Blink Settings")]
    [SerializeField, Min(1)] private int blinkCount = 4;
    [SerializeField, Min(0f)] private float initialClosedTime = 1.2f;
    [SerializeField, Min(0.01f)] private float openDuration = 0.8f;
    [SerializeField, Min(0.01f)] private float closeDuration = 0.35f;
    [SerializeField, Min(0f)] private float openPauseBetweenBlinks = 0.12f;
    [SerializeField, Min(0f)] private float closedPauseBetweenBlinks = 0.08f;

    [Header("Player Control During Wake Up")]
    [SerializeField] private MonoBehaviour playerMovement;
    [SerializeField] private MonoBehaviour mouseLook;

    private Vector2 _topClosedPosition;
    private Vector2 _bottomClosedPosition;
    private Vector2 _topOpenPosition;
    private Vector2 _bottomOpenPosition;

    private void Start()
    {
        if (!HasRequiredReferences())
            return;

        StartCoroutine(PlayWakeUpBlink());
    }

    private bool HasRequiredReferences()
    {
        if (topEyelid != null && bottomEyelid != null)
            return true;

        Debug.LogWarning(
            "WakeUpBlinkManager: faltan referencias. Asignar TopEyelid y BottomEyelid en el Inspector.",
            this);

        return false;
    }

    private IEnumerator PlayWakeUpBlink()
    {
        SetPlayerControl(false);

        Canvas.ForceUpdateCanvases();
        ConfigureClosedLayout();
        CacheAnimationPositions();
        SetEyelidsPosition(0f);

        if (initialClosedTime > 0f)
            yield return new WaitForSeconds(initialClosedTime);

        for (int i = 0; i < blinkCount; i++)
        {
            yield return AnimateEyelids(0f, 1f, openDuration);

            if (openPauseBetweenBlinks > 0f)
                yield return new WaitForSeconds(openPauseBetweenBlinks);

            yield return AnimateEyelids(1f, 0f, closeDuration);

            if (closedPauseBetweenBlinks > 0f)
                yield return new WaitForSeconds(closedPauseBetweenBlinks);
        }

        yield return AnimateEyelids(0f, 1f, openDuration);
        SetEyelidsPosition(1f);
        SetPlayerControl(true);
    }

    private void ConfigureClosedLayout()
    {
        topEyelid.anchorMin = new Vector2(0f, 0.5f);
        topEyelid.anchorMax = Vector2.one;
        topEyelid.pivot = new Vector2(0.5f, 0.5f);
        topEyelid.offsetMin = Vector2.zero;
        topEyelid.offsetMax = Vector2.zero;

        bottomEyelid.anchorMin = Vector2.zero;
        bottomEyelid.anchorMax = new Vector2(1f, 0.5f);
        bottomEyelid.pivot = new Vector2(0.5f, 0.5f);
        bottomEyelid.offsetMin = Vector2.zero;
        bottomEyelid.offsetMax = Vector2.zero;
    }

    private void CacheAnimationPositions()
    {
        _topClosedPosition = topEyelid.anchoredPosition;
        _bottomClosedPosition = bottomEyelid.anchoredPosition;

        float topOpenDistance = topEyelid.rect.height;
        float bottomOpenDistance = bottomEyelid.rect.height;

        _topOpenPosition = _topClosedPosition + Vector2.up * topOpenDistance;
        _bottomOpenPosition = _bottomClosedPosition + Vector2.down * bottomOpenDistance;
    }

    private IEnumerator AnimateEyelids(float from, float to, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float time = Mathf.Clamp01(elapsedTime / duration);
            float smoothTime = Mathf.SmoothStep(from, to, time);

            SetEyelidsPosition(smoothTime);
            yield return null;
        }

        SetEyelidsPosition(to);
    }

    private void SetEyelidsPosition(float openAmount)
    {
        topEyelid.anchoredPosition = Vector2.LerpUnclamped(_topClosedPosition, _topOpenPosition, openAmount);
        bottomEyelid.anchoredPosition = Vector2.LerpUnclamped(_bottomClosedPosition, _bottomOpenPosition, openAmount);
    }

    private void SetPlayerControl(bool isEnabled)
    {
        if (playerMovement != null)
            playerMovement.enabled = isEnabled;

        if (mouseLook != null)
            mouseLook.enabled = isEnabled;
    }
}
