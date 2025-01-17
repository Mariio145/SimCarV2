using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField seedText;
    public void ActivateScene(string sceneName)
    {
        if (seedText.text == "")
        {
            GlobalVariables.Seed = Random.Range(0, 10000);
        }
        SceneManager.LoadScene(sceneName); // Activa la escena cuando esté lista
    }
}
