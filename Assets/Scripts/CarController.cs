using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;
using Unity.MLAgents.Policies;
using TMPro;
using NUnit.Framework.Constraints;
using UnityEngine.SceneManagement;

public class CarController : Agent
{
    public Rigidbody rb;
    public Transform leftFrontWheel, rightFrontWheel;
    [HideInInspector] public ConvexHull convexHull;
    [Header("Car Settings")]
    public bool isBot;
    public float forwardAccel = 35;
    public float reverseAccel = 15, turnStrength = 180, gravityForce = 20, dragOnGround = 3, maxWheelTurn = 25;
    private float speedInput, turnInput;
    public bool grounded;
    [Header("Ground Check")]
    public LayerMask groundLayer;
    private float rayLength = 0.7f;
    public Transform groundRayPoint;
    private float groundTimer;
    [HideInInspector] public bool[] checkpoints;
    [HideInInspector] public int numberLaps;
    [HideInInspector] public float lapTime;
    [HideInInspector] public int botDifficulty = 0;
    [Header("Car Stats")]
    public MetricsSaver metricsSaver;
    [SerializeField] private TextMeshProUGUI lapText;
    [SerializeField] private GameObject TimeExposer;
    private bool isFreezed = true;

    
    // Start is called before the first frame update
    public override void Initialize()
    {
        rb.transform.parent = null;
        if (!isBot)
        {
            m_PolicyFactory.BehaviorType = BehaviorType.HeuristicOnly;
            lapText.text = "Lap 0";
        }
        //m_PolicyFactory.BehaviorType = BehaviorType.HeuristicOnly;
    }

    internal void StartTrack(Vector3 position, Quaternion rotation, int numCheckpoints, float startVariation)
    {
        transform.rotation = rotation;
        checkpoints = new bool[numCheckpoints];
        rb.transform.position = position + (Vector3.up*5) + transform.right * startVariation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isFreezed)lapTime += Time.deltaTime;
        transform.position = rb.transform.position - (Vector3.up * .45f);

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
        if (!isBot)StartCoroutine(ShowTime());

        AddReward(1f - (lapTime/10000f));
    }

    IEnumerator ShowTime()
    {
        TimeExposer.SetActive(true);

        TimeExposer.GetComponentInChildren<TextMeshProUGUI>().text = GlobalVariables.FormatTime(lapTime);
        yield return new WaitForSeconds(1.5f);

        TimeExposer.SetActive(false);
    }

    internal void LapFinished(int position)
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
        if (!isBot)
        {
            lapText.text = "Lap " + numberLaps;
            StartCoroutine(ShowTime());
        }
        if (numberLaps >= 2)
        {
            Freeze();
            if (!isBot)
            {
                GlobalVariables.position = position;
                GlobalVariables.timeLap = lapTime;
                foreach (CarController car in convexHull.cars)
                    car.Freeze();
                StartCoroutine(Exit());
                metricsSaver.SaveMetrics(convexHull.cars[1].botDifficulty, lapTime, position);
            }
        }
        
        AddReward(10f);
    }

    IEnumerator Exit()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("EndScene");
    }

    internal void Respawn()
    {
        if (isFreezed)
            return;
        AddReward(-1f);
        for (int i = checkpoints.Length - 1; i >= 0; i--)
        {
            if (checkpoints[i])
            {
                rb.transform.position = convexHull.checkpoints[i].transform.position + (Vector3.up * 5) + transform.forward * 5;
                transform.position = rb.transform.position - (Vector3.up * .5f);
                transform.rotation = convexHull.checkpoints[i].transform.rotation;
                return;
            }
        }

        rb.transform.position = convexHull.endpoint.transform.position + (Vector3.up * 5) + transform.forward * 5;
        transform.position = rb.transform.position - (Vector3.up * .5f);
        transform.rotation = convexHull.endpoint.transform.rotation;
        rb.velocity = Vector3.zero;
    }
    internal void SetBotDifficulty(int botDifficulty)
    {
        this.botDifficulty = botDifficulty;
        switch (botDifficulty)
        {
            case 1:
                forwardAccel = 25;
                reverseAccel = 10;
                turnStrength = 150;
                break;
            case 2:
                forwardAccel = 35;
                reverseAccel = 15;
                turnStrength = 180;
                break;
            case 3:
                forwardAccel = 40;
                reverseAccel = 20;
                turnStrength = 200;
                break;
        }
    }

    internal void Unfreeze()
    {
        isFreezed = false;
    }

    internal void Freeze()
    {
        isFreezed = true;
    }

    public override void OnEpisodeBegin()
    {
        //numberLaps = 0;
        //StartTrack(convexHull.endpoint.transform.position, convexHull.endpoint.transform.rotation, convexHull.checkpoints.Count, 0);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var spInput = Input.GetAxis("Vertical");
        var tuInput = Input.GetAxis("Horizontal");

        // Añade los valores a la lista de acciones
        actionsOut.ContinuousActions.Array[0] = spInput;
        actionsOut.ContinuousActions.Array[1] = tuInput;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isFreezed)
            return;
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
        if (isFreezed)
        {
            sensor.AddObservation(new float[6]);
            return;
        }

        var currentCheckpoint = 0;
        for (int i = 0; i < checkpoints.Length; i++)
        {
            currentCheckpoint = i;
            if (!checkpoints[i])
                break;
        }

        // Observa la rotacion del agente
        sensor.AddObservation(new Vector3 (transform.forward.x, 0, transform.forward.z).normalized);

        
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
