using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] private Slider extensionSlider, heightSlider;
    [SerializeField] private TextMeshProUGUI extensionText, heightText;
    [SerializeField] private TMP_InputField seedText;
    [SerializeField] private Toggle lowResToggle;
    public void SetExtension()
    {
        GlobalVariables.ExtensionLimit = extensionSlider.value;
    }

    public void SetHeight()
    {
        GlobalVariables.HighVariation = heightSlider.value;
    }

    public void SetSeed()
    {
        GlobalVariables.Seed = int.Parse(seedText.text) == 0 ? -1 : int.Parse(seedText.text);
    }

    public void SetLowRes()
    {
        GlobalVariables.lowRes = lowResToggle.isOn;
    }
}
