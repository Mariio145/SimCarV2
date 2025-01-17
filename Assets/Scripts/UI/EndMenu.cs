using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI placeText, placeText2, IDText, IDText2, timeText, timeText2;
    [SerializeField] private MeshRenderer background;
    [SerializeField] private Material lostMaterial;

    public void ContinueButton()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void Start()
    {
        switch(GlobalVariables.position)
        {
            case 1:
                placeText.text = "1st place";
                placeText2.text = "1st place";
                break;
            case 2:
                placeText.text = "2nd place";
                placeText2.text = "2nd place";
                break;
            case 3:
                background.material = lostMaterial;
                placeText.text = "3rd place";
                placeText2.text = "3rd place";
                break;
            default:
                background.material = lostMaterial;
                placeText.text = $"{GlobalVariables.position}th place";
                placeText2.text = $"{GlobalVariables.position}th place";
                break;
        }

        IDText.text = "ID: " + GlobalVariables.Seed;
        IDText2.text = "ID: " + GlobalVariables.Seed;

        timeText.text = "Time: " + GlobalVariables.FormatTime(GlobalVariables.timeLap);
        timeText2.text = "Time: " + GlobalVariables.FormatTime(GlobalVariables.timeLap);
    }
}
