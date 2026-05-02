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
    [SerializeField] private float _victoryDelay = 2f;

    private bool _isPlayerInRange;
    private bool _victoryScheduled;

    private void Start()
    {
        ResolveAnimation();
        ConfigureAnimation();
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
}
