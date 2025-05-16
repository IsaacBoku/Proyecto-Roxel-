using UnityEngine;

public interface IMenuInputHandler
{
    Vector2 NavigateInput { get; }
    bool SubmitInput { get; }
    bool CancelInput { get; }
    bool QInput { get; }
    bool RInput { get; }
    bool OptionsInput { get; }
    void UseSubmitInput();
    void UseCancelInput();
    void UseQInput();
    void UseRInput();
    void UseOptionsInput();
    void OnPause();
    void OnGame();
}
