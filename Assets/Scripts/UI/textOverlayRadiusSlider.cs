using TMPro;
using UnityEngine;

public class textOverlayRadiusSlider : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI OverlayText;

    public void SetText(string text)
    {
        OverlayText.text = text;
    }
}
