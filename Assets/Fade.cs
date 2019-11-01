/*
 *   https://learn.unity.com/tutorial/beautiful-transitions?projectId=5d0763baedbc2a001ebefa9f#5d076080edbc2a0020234c92
 */

using UnityEngine;

public class Fade : MonoBehaviour
{

    public bool show;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (show) show = false;
            else show = true;
            GetComponentInChildren<Animator>().SetBool("show", show);
        }
    }
}
