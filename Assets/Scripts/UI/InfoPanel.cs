using ARLocation;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    [SerializeField] private Text locationText;

    private void Awake()
    {
        locationText.text = "Waiting for location";
    }

    public void OnLocationEnabled(LocationReading reading)
    {
        locationText.text = $"OnLocationEnabled";
        //locationText.text = $"longitude: {reading.longitude} / latitude: {reading.latitude}";
    }

    public void OnLocationUpdated(LocationReading reading)
    {
        locationText.text = $"OnLocationUpdated";
        //locationText.text = $"longitude: {reading.longitude} / latitude: {reading.latitude}";
    }

    public void OnRawLocationUpdated(LocationReading reading)
    {
        locationText.text = $"OnRawLocationUpdated";
        //locationText.text = $"longitude: {reading.longitude} / latitude: {reading.latitude}";
    }
}
