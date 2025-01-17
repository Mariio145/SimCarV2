using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndpointController : MonoBehaviour
{
    int position = 1;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var controller = other.GetComponent<CarReference>().carController;
            try
            {
                if (controller.checkpoints[controller.checkpoints.Length - 1])
                {
                    if (controller.numberLaps == 1 && controller.isBot) position++;
                    controller.LapFinished(position);
                }
            }
            catch 
            {}
        }
    }
}
