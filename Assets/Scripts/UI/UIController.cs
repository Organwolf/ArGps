using ARLocation;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Text LocationAccText;

    public void onLocationUpdated(LocationReading loc)
    {
        var value = loc.accuracy;
        value = Math.Round(value, 2, MidpointRounding.ToEven);
        LocationAccText.text = string.Format("Locoation acc: {0}", value.ToString()); 
    }
}
