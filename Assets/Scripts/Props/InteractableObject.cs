using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private InteractableIndicator indicator;

    public void ShowIndicator()
    {
        if (indicator != null)
        {
            indicator.Show();
        }
    }

    public void HideIndicator()
    {
        if (indicator != null)
        {
            indicator.Hide();
        }
    }
}
