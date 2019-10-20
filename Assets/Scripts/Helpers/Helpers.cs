using UnityEngine;

public class Helpers : MonoBehaviour
{
    public static float ConvertToLog(float sliderValue, float elevation)
    {
        if (elevation == 0) elevation = 0.0001f;

        var minp = 0f;
        var maxp = 1f;

        var minv = Mathf.Log(elevation);
        var maxv = Mathf.Log(elevation * 100);

        var scale = (maxv - minv) / (maxp - minp);

        var logarithmicExagerationOfElevation = Mathf.Exp(minv + scale * (sliderValue - minp));

        Debug.Log("Logarithmic Value: " + logarithmicExagerationOfElevation);

        return logarithmicExagerationOfElevation;
    }
}
