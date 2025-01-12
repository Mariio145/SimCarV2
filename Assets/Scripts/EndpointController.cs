using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndpointController : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var controller = other.GetComponent<CarReference>().carController;
            try
            {
                if (controller.checkpoints[controller.checkpoints.Length - 1])
                {
                    controller.LapFinished();
                }
            }
            catch 
            {}
        }
    }
}
