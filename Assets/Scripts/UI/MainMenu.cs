using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void ActivateScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // Activa la escena cuando esté lista
    }
}
