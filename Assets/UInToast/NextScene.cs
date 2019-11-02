using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class NextScene : MonoBehaviour
{
    public string sceneName;

    AsyncOperation async;

    bool waitForLoad;
    float timer;
    float timerWait;

    // Start is called before the first frame update
    void Start()
    {
        waitForLoad = false;
        timerWait = 5.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("space"))
        {
            waitForLoad = true;
            async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;
            
        }

        if(waitForLoad)
        {
            timer += Time.deltaTime;
            if(timer >= timerWait) /* || async.progress >= 0.9f */
            {
                waitForLoad = false;
                async.allowSceneActivation = true;
            }
        }
    }
}
