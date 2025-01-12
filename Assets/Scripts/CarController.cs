using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

public class CarController : Agent
{
    public Rigidbody rb;
    public Transform leftFrontWheel, rightFrontWheel;
    [HideInInspector] public ConvexHull convexHull;
    [Header("Car Settings")]
    public float forwardAccel = 8;
    public float reverseAccel = 4, turnStrength = 180, gravityForce = 10, dragOnGround = 3, maxWheelTurn = 25;
    private float speedInput, turnInput;
    public bool grounded;
    [Header("Ground Check")]
    public LayerMask groundLayer;
    private float rayLength = 0.6f;
    public Transform groundRayPoint;
    [HideInInspector] public bool[] checkpoints;
    [HideInInspector] public int numberLaps;
    [HideInInspector] public float timeSpent;
    
    // Start is called before the first frame update
    public override void Initialize()
    {
        rb.transform.parent = null;
    }

    internal void StartTrack(Vector3 position, Quaternion rotation, int numCheckpoints)
    {
        rb.velocity = Vector3.zero;
        rb.transform.position = position + Vector3.up*5;
        transform.rotation = rotation;
        checkpoints = new bool[numCheckpoints];
    }

    // Update is called once per frame
    float groundTimer;
    void Update()
    {
        timeSpent += Time.deltaTime;
        transform.position = rb.transform.position;

        if (!grounded)
        {
            groundTimer += Time.deltaTime;
            if (groundTimer >= 3)
            {
                groundTimer = 0;
                Respawn();
            }
        }
        else
        {
            groundTimer = 0;
        }
    }

    void FixedUpdate()
    {
        grounded = false;

        RaycastHit hit;

        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, rayLength, groundLayer))
        {
            grounded = true;
            rb.drag = dragOnGround;
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }
        else
        {
            rb.drag = 0.1f;
        }
    }

    internal void ChekpointCross(int checkpointIndex)
    {
        if (checkpointIndex + 1 < checkpoints.Length)
            if (checkpoints[checkpointIndex+1])
            {
                AddReward(-10f);
                return;
            }
        if (checkpoints[checkpointIndex])
            return;
        checkpoints[checkpointIndex] = true;
        AddReward(1f - (timeSpent/10000f));
        timeSpent = 0;
    }

    internal void LapFinished()
    {
        foreach (var checkpoint in checkpoints)
        {
            if (!checkpoint)
            {
                return;
            }
        }
        checkpoints = new bool[checkpoints.Length];
        numberLaps += 1;
        AddReward(10f);
        timeSpent = 0;
    }

    internal void Respawn()
    {
        AddReward(-1f);
        rb.velocity = Vector3.zero;
        for (int i = checkpoints.Length - 1; i >= 0; i--)
        {
            if (checkpoints[i])
            {
                rb.transform.position = convexHull.checkpoints[i].transform.position + (Vector3.up * 5) + transform.forward * 5;
                transform.position = rb.transform.position;
                transform.rotation = convexHull.checkpoints[i].transform.rotation;
                return;
            }
        }

        rb.transform.position = convexHull.endpoint.transform.position + (Vector3.up * 5) + transform.forward * 5;
        transform.position = rb.transform.position;
        transform.rotation = convexHull.endpoint.transform.rotation;
    }

    public override void OnEpisodeBegin()
    {
        numberLaps = 0;
        StartTrack(convexHull.endpoint.transform.position, convexHull.endpoint.transform.rotation, convexHull.checkpoints.Count);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //speedInput = 0;
        //turnInput = 0f;
        var spInput = Input.GetAxis("Vertical");
        var tuInput = Input.GetAxis("Horizontal");

        // Añade los valores a la lista de acciones
        actionsOut.ContinuousActions.Array[0] = spInput;
        actionsOut.ContinuousActions.Array[1] = tuInput;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        speedInput = 0;
        turnInput = actions.ContinuousActions.Array[1];

        if (grounded)
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput  * turnStrength * Time.deltaTime * actions.ContinuousActions.Array[0] , 0f));

        
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localEulerAngles.x, turnInput  * maxWheelTurn, rightFrontWheel.localEulerAngles.z);
        leftFrontWheel.localRotation = rightFrontWheel.localRotation;//Quaternion.Euler(leftFrontWheel.localEulerAngles.x, (turnInput * maxWheelTurn) - 180, leftFrontWheel.localEulerAngles.z);

        if (actions.ContinuousActions.Array[0] > 0)
        {
            speedInput = actions.ContinuousActions.Array[0] * forwardAccel * 1000f;
        }
        else if (actions.ContinuousActions.Array[0] < 0)
        {
            speedInput = actions.ContinuousActions.Array[0] * reverseAccel * 1000f;
        }


        if (grounded)
        {
            if (Mathf.Abs(speedInput) > 0)
                rb.AddForce(transform.forward * speedInput);
        }
        else
        {
            rb.AddForce(transform.forward * 750f);
            rb.AddForce(Vector3.up * (-gravityForce * 100f));
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        var currentCheckpoint = 0;
        for (int i = 0; i < checkpoints.Length; i++)
        {
            currentCheckpoint = i;
            if (!checkpoints[i])
                break;
        }


        sensor.AddObservation(new Vector3 (transform.forward.x, 0, transform.forward.z).normalized);

        // Observa la rotacion del agente
        
        if (currentCheckpoint != checkpoints.Length - 1) 
        {
            var direction = (convexHull.checkpoints[currentCheckpoint].transform.position - groundRayPoint.position).normalized;
            sensor.AddObservation(direction - new Vector3(0, direction.y, 0));
        }
        else 
        {
            var direction = (convexHull.endpoint.transform.position - groundRayPoint.position).normalized;
            sensor.AddObservation(direction - new Vector3(0, direction.y, 0));
        }

        // Vector que va desde el pico hasta la flor más cercana
    }
}
