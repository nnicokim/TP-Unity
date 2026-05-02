using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ExitDoor : MonoBehaviour
{
    private const Key INTERACTION_KEY = Key.O;

    [Header("Door State")]
    [SerializeField] private bool _isLocked = true;
    [SerializeField] private bool _isOpen;

    [Header("Animation")]
    [SerializeField] private Animation _animation;
    [SerializeField] private string _openAnimationName;

    [Header("Victory")]
    [SerializeField] private float _victoryDelay = 3f;

    private bool _isPlayerInRange;
    private bool _victoryScheduled;

    private void Start()
    {
        ResolveAnimation();
    }

    private void Update()
    {
        if (!_isPlayerInRange || _isLocked || _isOpen || IsGamePaused())
            return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[INTERACTION_KEY].wasPressedThisFrame)
            Open();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other))
            _isPlayerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
            _isPlayerInRange = false;
    }

    public void Unlock()
    {
        _isLocked = false;
        Debug.Log($"{name} desbloqueada.");
    }

    public void Open()
    {
        if (_isLocked || _isOpen)
            return;

        _isOpen = true;
        PlayOpenAnimation();

        if (!_victoryScheduled)
            StartCoroutine(ShowVictoryAfterDelay());
    }

    private IEnumerator ShowVictoryAfterDelay()
    {
        _victoryScheduled = true;
        yield return new WaitForSeconds(_victoryDelay);

        if (ActionsManager.instance != null)
            ActionsManager.instance.ActionGameover(true);
    }

    private void ResolveAnimation()
    {
        if (_animation == null)
            _animation = GetComponentInChildren<Animation>();
    }

    private void PlayOpenAnimation()
    {
        if (_animation == null)
            return;

        string animationName = GetAnimationName(_openAnimationName);
        if (string.IsNullOrEmpty(animationName))
            return;

        _animation.CrossFade(animationName);
    }

    private string GetAnimationName(string preferredName)
    {
        if (!string.IsNullOrEmpty(preferredName))
            return preferredName;

        return _animation.clip != null ? _animation.clip.name : null;
    }

    private bool IsPlayer(Collider other)
    {
        if (other.GetComponentInParent<PlayerHealth>() != null)
            return true;

        return other.GetComponentInChildren<PlayerHealth>() != null;
    }

    private bool IsGamePaused()
    {
        return GameManager.instance != null && GameManager.instance.isGamePause;
    }
}
