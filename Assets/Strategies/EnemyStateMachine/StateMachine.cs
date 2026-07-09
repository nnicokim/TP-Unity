public class StateMachine{
    public StateMachineState CurrentState { get; private set; }

    public void InitStateMachine(StateMachineState currentState)
    {
        this.CurrentState = currentState;
        CurrentState.Enter();
    }

    public void ChangeState(StateMachineState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}