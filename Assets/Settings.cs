using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    [SerializeField] InputField input1;
    [SerializeField] InputField input2;
    [SerializeField] InputField input3;
    
    public void LoadMain()
    {
        //PlayerPrefs.SetString("input1", input1.text);
        //PlayerPrefs.SetString("input2", input2.text);
        //PlayerPrefs.SetString("input3", input3.text);
        SceneManager.LoadScene("MainScene");
    }
}
