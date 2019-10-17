using TMPro;
using UnityEngine;

public class textOverlayMultSlider : MonoBehaviour
{

    [SerializeField]
    private TextMeshProUGUI OverlayText;

    public void SetText(string text)
    {
        OverlayText.text = text;
    }
}
