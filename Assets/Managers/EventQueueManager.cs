using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventQueueManager : MonoBehaviour
{
    #region SINGLETON
    static public EventQueueManager instance;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        instance = this;
    }
    #endregion

    #region COMMAND_WITH_LIST
    private const string TYPE_CMD_MOVE = "CmdMove";
    //private const string TYPE_CMD_ATTACK = "CmdAttack";

    // Pueden armar la getter publico
    [SerializeField] private List<ICommand> _commands = new List<ICommand>();

    // 1. Agregar comandos a la lista
    public void AddCommand(ICommand command)
    {
        if (command == null)
        {
            Debug.LogWarning("Se intento agregar un comando null a la cola.");
            return;
        }

        _commands.Add(command);
    }

    // 2. Recorrer la lista y ejecutar los comandos almacenados
    private void ExecuteStoredCommands()
    {
        for (int i = _commands.Count - 1; i >= 0; i--)
        {
            //if (GameManager.instance.isGamePause && (_commands[i].GetType().Equals(TYPE_CMD_MOVE) || _commands[i].GetType().Equals(TYPE_CMD_ATTACK)))
            //{
            //    _commands.RemoveAt(i);
            //    break;
            //}

            // TODO: agregar esto de game manager
            // if (GameManager.instance.isGamePause)
            // {
            //     _commands.RemoveAt(i);
            //     break;
            // }

            if (_commands[i] == null)
            {
                _commands.RemoveAt(i);
                continue;
            }

            _commands[i].Execute();
            _commands.RemoveAt(i);
        }
    }
    #endregion

    #region COMMAND_WITH_QUEUE
    private Queue<ICommand> _queue = new Queue<ICommand>();

    public void AddCommandToQueue(ICommand command)
    {
        if (command == null)
        {
            Debug.LogWarning("Se intento agregar un comando null a la cola.");
            return;
        }

        _queue.Enqueue(command);
    }

    private void ExecuteStoredCommandsInQueue()
    {
        while (_queue.Count != 0)
        {
            //_queue.Dequeue().Execute();

            ICommand command = _queue.Dequeue();

            // if (GameManager.instance.isGamePause)
            //     break;

            command.Execute();
        }
    }
    #endregion

    private void Update()
    {
        ExecuteStoredCommands();
        //ExecuteStoredCommandsInQueue();
    }
}
