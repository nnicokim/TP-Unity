using System.Collections.Generic;
using UnityEngine;

public class EventQueueManager : MonoBehaviour
{
    public static EventQueueManager Instance { get; private set; }

    private readonly List<ICommand> _events = new();
    private readonly Queue<ICommand> _eventQueue = new();

    public IReadOnlyList<ICommand> Events => _events;
    //public int QueuedEventsCount => _eventQueue.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        ExecuteEvents();
        ExecuteQueuedEvents();
    }

    public void AddEvent(ICommand command)
    {
        if (command == null)
        {
            Debug.LogWarning("Se intento agregar un evento null.");
            return;
        }

        _events.Add(command);
    }

    public void AddEventToQueue(ICommand command)
    {
        if (command == null)
        {
            Debug.LogWarning("Se intento agregar un evento null al queue.");
            return;
        }

        _eventQueue.Enqueue(command);
    }

    private void ExecuteEvents()
    {
        if (_events.Count == 0) return;

        List<ICommand> eventsToExecute = new(_events);
        _events.Clear();

        foreach (ICommand command in eventsToExecute)
        {
            ExecuteCommandSafely(command);
        }
    }

    private void ExecuteQueuedEvents()
    {
        while (_eventQueue.Count > 0)
        {
            ICommand command = _eventQueue.Dequeue();
            ExecuteCommandSafely(command);
        }
    }

    private void ExecuteCommandSafely(ICommand command)
    {
        // TODO: ver el tema del juego en pause
        try
        {
            command.Execute();
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Error ejecutando comando {command.GetType().Name}: {exception.Message}");
        }
    }
}