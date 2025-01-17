using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaceController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private Animator canvasAnim;
    [SerializeField] private ConvexHull convexHullScrpit;
    [SerializeField] private TextMeshProUGUI[] countdownText;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip Countdown, Music;
    void Start()
    {
        StartCoroutine(StartRace());
    }

    IEnumerator StartRace()
    {
        yield return new WaitForSeconds(1);
        audioSource.clip = Countdown;
        audioSource.Play();
        canvasAnim.SetTrigger("Start");
        foreach (var text in countdownText)
        {
            text.text = "3";
        }
        yield return new WaitForSeconds(1f);
        foreach (var text in countdownText)
        {
            text.text = "2";
        }
        yield return new WaitForSeconds(1);
        foreach (var text in countdownText)
        {
            text.text = "1";
        }
        yield return new WaitForSeconds(1);
        foreach (var text in countdownText)
        {
            text.text = "GO!";
        }
        foreach (var car in convexHullScrpit.cars)
        {
            car.Unfreeze();
        }
        yield return new WaitForSeconds(1f);
        audioSource.clip = Music;
        audioSource.pitch = 0.8f;
        audioSource.loop = true;
        audioSource.Play();
        yield return new WaitForSeconds(0.5f);
        foreach (var text in countdownText)
        {
            text.text = "";
        }

        while (audioSource.pitch < 1.05f)
        {
            yield return new WaitForSeconds(24f);
            audioSource.pitch += 0.01f;
        }
    }
}
