interface InputStateMachine
{
    void ChangeState(InputState to);
    // void ChangeState(string name);
    void ResetState();

    // TODO: Async fun.
    // returns success, may need coroutine bullshit.
    // bool SubmitAction(Action action);
}