using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    [SerializeField] private float _victoryDelay = 2f;

    [Header("Interaction Prompt")]
    [SerializeField] private Text _interactionText;
    [SerializeField] private bool _showPromptOnlyWhenPlayerInRange = true;
    [SerializeField] private string _lockedMessage = "Exit door is locked";
    [SerializeField] private string _unlockedMessage = "Press 'o' to open";

    private bool _isPlayerInRange;
    private bool _victoryScheduled;

    private void Start()
    {
        ResolveAnimation();
        ConfigureAnimation();
        UpdateDoorMessage();
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
        {
            _isPlayerInRange = true;
            UpdateDoorMessage();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other))
        {
            _isPlayerInRange = false;
            UpdateDoorMessage();
        }
    }

    public void Unlock()
    {
        _isLocked = false;
        Debug.Log($"{name} desbloqueada.");
        UpdateDoorMessage();
    }

    public void Open()
    {
        if (_isLocked || _isOpen)
            return;

        _isOpen = true;
        UpdateDoorMessage();
        PlayOpenAnimation();

        if (!_victoryScheduled)
            StartCoroutine(ShowVictoryAfterDelay());
    }

    private IEnumerator ShowVictoryAfterDelay()
    {
        _victoryScheduled = true;
        Debug.Log("Puerta abierta. Mostrando victoria...");
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
        {
            Debug.LogWarning($"{name} no tiene componente Animation para reproducir la apertura.");
            return;
        }

        string animationName = GetAnimationName(_openAnimationName);
        if (string.IsNullOrEmpty(animationName))
        {
            Debug.LogWarning($"{name} no tiene clip de apertura asignado.");
            return;
        }

        if (_animation[animationName] == null)
        {
            Debug.LogWarning($"{name} no contiene un clip llamado '{animationName}'.");
            return;
        }

        _animation.Stop();
        _animation.Play(animationName);
    }

    private void ConfigureAnimation()
    {
        if (_animation == null)
            return;

        _animation.playAutomatically = false;

        foreach (AnimationState state in _animation)
        {
            state.wrapMode = WrapMode.Once;
            state.speed = 1f;
        }

        _animation.Stop();
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

    private void UpdateDoorMessage()
    {
        if (_interactionText == null)
            return;

        bool shouldShow = !_isOpen && (!_showPromptOnlyWhenPlayerInRange || _isPlayerInRange);
        _interactionText.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        _interactionText.text = _isLocked ? _lockedMessage : _unlockedMessage;
    }
}
