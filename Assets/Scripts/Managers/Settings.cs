using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Settings : MonoBehaviour
{
    [SerializeField] InputField RadiusInputField;
    [SerializeField] InputField OffsetInputField;
    
    public void LoadMain()
    {
        var inputRadius = Convert.ToInt32(RadiusInputField.text);
        var inputOffset = Convert.ToInt32(OffsetInputField.text);
        PlayerPrefs.SetInt("Radius", inputRadius);
        PlayerPrefs.SetInt("Offset", inputOffset);
        SceneManager.LoadScene("MainScene");
    }
}
