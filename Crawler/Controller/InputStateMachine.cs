interface InputStateMachine
{
    void ChangeState(InputState to);
    // void ChangeState(string name);
    void ResetState();
}