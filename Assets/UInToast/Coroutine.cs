using System.Collections;
using UnityEngine;

// https://danielilett.com/2019-08-20-unity-tips-2-coroutines/

public class Coroutine : MonoBehaviour
{

    private UnityEngine.Coroutine updateEachSecond;

    private void OnEnable()
    {
        updateEachSecond = StartCoroutine(UpdateEachSecond());
    }

    private void OnDisable()
    {
        StopCoroutine(updateEachSecond);
        updateEachSecond = null;
        Debug.Log("App disabled");
    }

    private IEnumerator UpdateEachSecond()
    {
        var wait = new WaitForSecondsRealtime(2.0f);

        while (true)
        {
            // Do expensive operations here.
            SSTools.ShowMessage("Coroutining!", SSTools.Position.bottom, SSTools.Time.oneSecond);
            yield return wait;
        }
    }
}
