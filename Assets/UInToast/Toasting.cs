using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Toasting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TestToToast()
    {
        SSTools.ShowMessage("Toastning", SSTools.Position.top, SSTools.Time.threeSecond);
    }
}
