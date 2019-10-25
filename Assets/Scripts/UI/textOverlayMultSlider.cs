using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class textOverlayMultSlider : MonoBehaviour
    {

        [SerializeField]
        private TextMeshProUGUI OverlayText;

        public void SetText(string text)
        {
            OverlayText.text = text;
        }
    }
}
