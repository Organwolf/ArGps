using ARLocation;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Text LocationAccText;

    public void onLocationUpdated(LocationReading loc)
    {
        LocationAccText.text = string.Format("Locoation acc: {0}", loc.accuracy.ToString()); 
    }
}
