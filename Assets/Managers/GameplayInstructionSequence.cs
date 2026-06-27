using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameplayInstructionSequence : MonoBehaviour
{
    [Header("Instruction Text")]
    [SerializeField] private Text mainInstruction;

    [Header("Startup Message")]
    [SerializeField] private string startupMessage = "The enemy boss is about to spawn";
    [SerializeField, Min(0f)] private float startupMessageDuration = 10f;

    [Header("Main Message")]
    [SerializeField, TextArea] private string mainMessage;

    private Coroutine _sequenceRoutine;

    private void Reset()
    {
        mainInstruction = GetComponent<Text>();
    }

    private void Awake()
    {
        if (mainInstruction == null)
            mainInstruction = GetComponent<Text>();

        if (mainInstruction != null)
            mainMessage = mainInstruction.text;
    }

    private void OnEnable()
    {
        if (mainInstruction == null)
        {
            Debug.LogWarning($"GameplayInstructionSequence: falta asignar Main Instruction en {gameObject.name}.", this);
            return;
        }

        if (_sequenceRoutine != null)
            StopCoroutine(_sequenceRoutine);

        _sequenceRoutine = StartCoroutine(InstructionSequenceRoutine());
    }

    private void OnDisable()
    {
        if (_sequenceRoutine == null)
            return;

        StopCoroutine(_sequenceRoutine);
        _sequenceRoutine = null;
    }

    private IEnumerator InstructionSequenceRoutine()
    {
        mainInstruction.gameObject.SetActive(true);
        mainInstruction.text = startupMessage;

        yield return new WaitForSeconds(startupMessageDuration);

        mainInstruction.text = mainMessage;
        mainInstruction.gameObject.SetActive(true);
        _sequenceRoutine = null;
    }
}
