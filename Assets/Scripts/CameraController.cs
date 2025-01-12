using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
 [Header("Target Settings")]
    public Transform target; // El coche que la cámara seguirá.

    [Header("Camera Settings")]
    public Vector3 offset = new Vector3(0, 5, -10); // Posición relativa a la del coche.
    public float followSpeed = 10f; // Velocidad con la que sigue al coche.
    public float rotationSpeed = 5f; // Velocidad de rotación para alinear con el coche.

    [Header("Look Settings")]
    public bool lookAtTarget = true; // Si la cámara debe mirar al coche.

    private void LateUpdate()
    {
        if (!target) return; // Salir si no hay un objetivo asignado.

        // Movimiento suave de la cámara hacia la posición deseada
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // Opcional: Girar la cámara hacia el objetivo
        if (lookAtTarget)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Alinear la rotación de la cámara con la rotación del coche
            Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles.x, target.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
