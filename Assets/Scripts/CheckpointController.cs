using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public int checkpointIndex;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var controller = other.GetComponent<CarReference>().carController;
            if (checkpointIndex == 0 || controller.checkpoints[checkpointIndex - 1])
            {
                controller.ChekpointCross(checkpointIndex);
            }
        }
    }
}
