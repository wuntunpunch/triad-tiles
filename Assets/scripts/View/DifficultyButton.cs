using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to difficulty selection buttons.
/// </summary>
public class DifficultyButton : MonoBehaviour
{
    [SerializeField] private int difficultyIndex;
    [SerializeField] private TextMeshProUGUI buttonText;
    
    public void OnClick()
    {
        if (GameController.Instance != null)
        {
            GameController.Instance.SelectDifficulty(difficultyIndex);
        }
    }
}
