using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScaleText : MonoBehaviour
{
    public TextMeshPro text;

    private UnityEngine.Coroutine updateEachSecond;
    private float currentDistance;
    private float prevDistance = 0;


    // Update is called once per frame
    void Update()
    {
        currentDistance = Vector3.Distance(transform.position, Vector3.zero);
        var distance = currentDistance - prevDistance;

        if (Mathf.Abs(distance) > 0.1)
        {
            prevDistance = currentDistance;

            float multiplier = 1;

            if (currentDistance > 1)
            {
                multiplier = Mathf.Log(currentDistance, 10) + 1;
                Debug.Log(currentDistance);
                //Debug.Log("multiplier: " + multiplier);
                //Debug.Log("multiplier * font.size: " + (multiplier * text.fontSize));

            }

            text.fontSize = 0.4f * multiplier;
            //text.fontSize = 1.5f;

            Debug.Log($"Font size: {text.fontSize} Distance: {distance}");
        }

    }

}
