using ARLocation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Text LocationAccText;
    [SerializeField]
    private Text CompassAccText;

    //private Assets.Mapbox.SimpleAutomaticSynchronizationContext syncContext;

    //private void Start()
    //{
    //    syncContext = new Assets.Mapbox.SimpleAutomaticSynchronizationContext();
    //}

    public void onLocationUpdated(LocationReading loc)
    {
        LocationAccText.text = string.Format("Locoation acc: {0}", loc.accuracy.ToString()); 
        
        //syncContext.AddSynchronizationNodes()
    }

    public void onCompassUpdated(HeadingReading head)
    {
        //Debug.Log(string.Format("accuracy: {0}, heading: {1}, isMagneticHeadingAvailable: {2}, magneticHeading: {3}", head.accuracy, head.heading, head.isMagneticHeadingAvailable, head.magneticHeading));
        CompassAccText.text = string.Format("Compass acc: {0}", head.accuracy.ToString());
    }
}
