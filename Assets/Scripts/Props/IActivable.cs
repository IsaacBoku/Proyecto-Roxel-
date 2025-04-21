using UnityEngine;

public interface IActivable
{
    void Toggle(bool state);
    void SetIgnoreTrigger(bool ignore);
}
