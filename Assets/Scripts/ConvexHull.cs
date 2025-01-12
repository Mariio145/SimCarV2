using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BezierExtrudeTool;
using Unity.VisualScripting;

public class ConvexHull : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Track Settings")]
    [SerializeField]BezierExtrude racingTrack;
    [Range(500f, 1000f)] public float ExtensionLimit = 750f;
    [Range(0f, 500f)] public float SideVariation = 5f;
    [Range(0f, 10f)] public float HighVariation = 1f;
    public bool RandomTrack;
    public int Seed;
    List<Vector3> points;
    [SerializeField] CarController[] cars;
    [HideInInspector] public EndpointController endpoint;
    [HideInInspector] public List<CheckpointController> checkpoints;
    void Awake()
    {
        points = new List<Vector3>();
        checkpoints = new List<CheckpointController>();
        foreach (var car in cars)
        {
            car.convexHull = this;
        }
        racingTrack.edgeRingCount = 32;

        if (RandomTrack) Seed = System.DateTime.Now.Millisecond + System.DateTime.Now.Second;
        

        Random.InitState(Seed);

        for (int i = 0; i < ExtensionLimit/10; i++)
        {
            Vector3 position = new Vector3(Random.Range(-ExtensionLimit, ExtensionLimit), 0, Random.Range(-ExtensionLimit, ExtensionLimit));
            points.Add(position);
        }

        points = GetConvexHull(points);

        for (int i = 0; i < points.Count; i++)
        {
            points[i] += new Vector3(Random.Range(-SideVariation, SideVariation), Random.Range(-HighVariation, HighVariation), Random.Range(-SideVariation, SideVariation));
        }

        ModifyTrackLenght(points);
        GenerateTrack(points);

        StartCoroutine(LateInitializeCar());
    }

    IEnumerator LateInitializeCar()
    {
        yield return new WaitForSeconds(0.01f);
        foreach (CarController car in cars) 
        {
            car.StartTrack(points[0] + Vector3.up, Quaternion.LookRotation(points[1] - points[points.Count - 2]), checkpoints.Count);
        }
    }

    private List<Vector3> GetConvexHull(List<Vector3> listPoints)
    {

        List<Vector3> convexList = new List<Vector3>();
        Vector3 startPoint = listPoints[0];
        
        List<Vector3> notStartingPoints = new List<Vector3> (listPoints);
        //Encontramos el punto con menor Z (en caso de empate, menor X)
        for (int i = 1; i < listPoints.Count; i++)
        {
            if (startPoint.z > listPoints[i].z) startPoint = listPoints[i];
            else if (startPoint.z == listPoints[i].z && startPoint.x > listPoints[i].x) startPoint = listPoints[i];
        }

        for (int i = 0; i < listPoints.Count; i++)
        {
            if (notStartingPoints[i] == startPoint) 
            {
                notStartingPoints.RemoveAt(i);
                break;
            }
        }

        // Ordenamos la lista de puntos teniendo en cuenta los angulos generados entre la recta que forman el startingPoint y los demás puntos con el eje X
        GetPointsOrderedByAngle(notStartingPoints, startPoint, 0, notStartingPoints.Count-1);

        convexList.Add(startPoint);
        Vector3 ultimo, penultimo, antepenultimo;

        //Recorremos todos los puntos y creamos la envolvente convexa usando el Metodo de Graham
        foreach (var point in notStartingPoints)
        {
            convexList.Add(point);
            //Mientras sea giro a la derecha Y haya elementos a eliminar
            while (convexList.Count > 3)
            {
                ultimo = convexList[convexList.Count - 1];
                penultimo = convexList[convexList.Count - 2];
                antepenultimo = convexList[convexList.Count - 3];

                if (CrossProduct(ultimo, penultimo, antepenultimo) < 0) break;
                convexList.RemoveAt(convexList.Count - 2);
            }
        }
        return convexList;
    }

    private void ModifyTrackLenght(List<Vector3> trackPoints)
    {
        int i = 0;
        float threshold = ExtensionLimit - 250f;
        Vector3 direction;
        float numberPoints;
        bool leftBefore = false;

        // Añadimos puntos intermedios si la distancia entre dos puntos es mayor que el threshold
        while (i < trackPoints.Count)
        {
            if (i == trackPoints.Count - 1) break;
            if (Vector3.Distance(trackPoints[i], trackPoints[i+1]) > threshold)
            {
                numberPoints = Mathf.FloorToInt(Vector3.Distance(trackPoints[i], trackPoints[i+1])/threshold);
                var nextPoint = trackPoints[i+1];

                for (float j = 1; j < numberPoints + 1; j++)
                {
                    Vector3 newPoint = ((1-(j/(numberPoints+1)))* trackPoints[i]) +  (nextPoint * (j/(numberPoints+1)));
                    if (!leftBefore) newPoint -= Vector3.Cross(Vector3.up, trackPoints[i+1] - newPoint).normalized * Random.Range(50f, 225f);
                    else newPoint += Vector3.Cross(Vector3.up, trackPoints[i+1] - newPoint).normalized * Random.Range(50f, 225f);
                    trackPoints.Insert(i+1, newPoint);
                    i++;
                }
                leftBefore = !leftBefore;
                i++;
                continue;
            }
            leftBefore = false;
            i++;
        }

        i = 1;
        threshold = 100f;
        int minimunAngle = 25;
        float movementFactor = 5f;

        // A eliminar los puntos que se encuentra muy próximos, o si no a desplazarlos
        while (i < trackPoints.Count)
        {
            
            if (i >= trackPoints.Count - 2) break;
            Vector3 AB = (trackPoints[i] - trackPoints[i-1]).normalized;
            Vector3 BC = (trackPoints[i+1] - trackPoints[i]).normalized;
            Vector3 CrossProduct = Vector3.Cross(AB, BC);

            // Comprobamos de que el punto no esté alineado, ya que entonces no aporta información alguna
            if ((Vector3.Distance(trackPoints[i], trackPoints[i-1]) < threshold && Vector3.Distance(trackPoints[i], trackPoints[i+1]) < threshold) || CrossProduct == Vector3.zero)
            {
                trackPoints.RemoveAt(i);
                continue;
            }

            // Desplazamos el punto en la direccion opuesta a la que se encuentra el punto más cercano
            if (Vector3.Distance(trackPoints[i], trackPoints[i-1]) < threshold)
            {
                direction = (trackPoints[i] - trackPoints[i-i]).normalized;
                trackPoints[i] += direction * Random.Range(threshold - Vector3.Distance(trackPoints[i], trackPoints[i-1]), threshold);
                continue;
            }

            if (Vector3.Distance(trackPoints[i], trackPoints[i+1]) < threshold)
            {
                direction = (trackPoints[i] - trackPoints[i+i]).normalized;
                trackPoints[i] += direction * Random.Range(threshold - Vector3.Distance(trackPoints[i], trackPoints[i+1]), threshold);
                continue;
            }

            // Si el angulo formado entre los otros dos puntos es menor que el angulo mínimo, desplazamos el punto perpendicularmente para aumentar el angulo
            if (Vector3.Angle(AB, BC) < minimunAngle)
            {
                Vector3 normal = Vector3.Cross(AB, BC).normalized;
                trackPoints[i] += movementFactor * normal;
                continue;
            }

            i++;
        }
    }

    private void GenerateTrack(List<Vector3> points)
    {
        Quaternion rotation;
        int curve = 50;

        //El script de bezier funciona con dos puntos ya definidos, así que en vez de añadir puntos movemos los ya existentes y lo seliminamos de la lista
        racingTrack.ChangeControlPointData(0, points[0], Quaternion.LookRotation(points[1] - points[points.Count - 1]), new Vector3(curve, 1, curve));
        racingTrack.ChangeControlPointData(1, points[1], Quaternion.LookRotation(points[2] - points[0]), new Vector3(curve, 1, curve));
        racingTrack.GenerateMeshAsset();

        for (int i = 2; i < points.Count; i++)
        {
            if (i == points.Count - 1) rotation = Quaternion.LookRotation(points[0] - points[i-1]);
            else rotation = Quaternion.LookRotation(points[i+1] - points[i-1]);
            racingTrack.AddControlPoint(points[i], rotation, new Vector3(curve, 1, curve)); // Añadimos los puntos a la malla
        }

        for (int i = 0; i < points.Count; i++)
        {
            var temp = racingTrack.GetControlPoint(i);
            var coll = temp.AddComponent<BoxCollider>();
            coll.isTrigger = true;
            coll.size = new Vector3(0.5f, 10, 0.1f);
            coll.center = new Vector3(0, 2.25f, 0);
            if (i == 0) endpoint = temp.AddComponent<EndpointController>();
            else 
            {
                var checkpointScript = temp.AddComponent<CheckpointController>();
                checkpointScript.checkpointIndex = i - 1;
                checkpoints.Add(checkpointScript);
            }
        }

        racingTrack.AddControlPoint(points[0], Quaternion.LookRotation(points[1] - points[points.Count - 1]), new Vector3(curve, 1, curve));
    }

    private void GetPointsOrderedByAngle(List<Vector3> listPoints, Vector3 startPoint, int leftIndex, int rightIndex)
    {
        if (leftIndex >= rightIndex) return;

        float anglePivot =  Mathf.Atan2(listPoints[rightIndex].z - startPoint.z, listPoints[rightIndex].x - startPoint.x);
        float angle;
        int i = leftIndex;

        for (int j = leftIndex; j < rightIndex; j++)
        {
            angle = Mathf.Atan2(listPoints[j].z - startPoint.z, listPoints[j].x - startPoint.x);

            if (angle <= anglePivot)
            {
                (listPoints[i], listPoints[j]) = (listPoints[j], listPoints[i]); // Intercambio en una sola línea
                i++;
            }
        }

        (listPoints[i], listPoints[rightIndex]) = (listPoints[rightIndex], listPoints[i]); // Coloca el pivote en su lugar

        GetPointsOrderedByAngle(listPoints, startPoint, leftIndex, i - 1);  // Ordena la parte izquierda
        GetPointsOrderedByAngle(listPoints, startPoint, i + 1, rightIndex); // Ordena la parte derecha
    }

    private float CrossProduct(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p2.x - p1.x) * (p3.z - p1.z) - (p2.z - p1.z) * (p3.x - p1.x);
    }
}
