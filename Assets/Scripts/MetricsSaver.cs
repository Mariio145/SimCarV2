using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class MetricsSaver : MonoBehaviour
{
    private static string csvPath; // Ruta del archivo CSV

    void Start()
    {
        // Configurar la ruta del archivo CSV --> C:\Users\<user>\AppData\LocalLow\<company name>\ice only.
        // EL nombre de la compañia por defecto es DefaultCompany
        csvPath = Path.Combine(Application.persistentDataPath, "MetricsLog.csv");

        // Crear archivo CSV si no existe
        if (!File.Exists(csvPath))
        {
            File.WriteAllText(csvPath, "BotDifficult,LapTime,FinalPosition\n");
        }
    }

    public void SaveMetrics(int botDifficult, float lapTime, int finalPosition)
    {

        string line = string.Format("{0},{1},{2}\n", botDifficult, lapTime.ToString(CultureInfo.InvariantCulture), finalPosition);

        Debug.Log("Guardando");

        File.AppendAllText(csvPath, line);
    }

    public static int CalculateBotDifficulty()
    {
        // 1 = Fácil, 2 = Medio, 3 = Difícil
        if (!File.Exists(csvPath))
        {
            return 2;
        }

        string[] lines = File.ReadAllLines(csvPath);
        if (lines.Length <= 1)
        {
            return 2;
        }

        float totalTime = 0;
        int totalbotDifficult = 0;
        int totalPosition= 0;
        int count = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');

            if (columns.Length < 3)
            {
                continue;
            }


            int botDifficult;
            float timeSpent;
            int position;
            

            // Si los valores no son válidos, los saltamos
            if (!int.TryParse(columns[1], out botDifficult) ||
                !float.TryParse(columns[2], out timeSpent) ||
                !int.TryParse(columns[3], out position))
            {
                continue;
            }

            // Sumar los valores para calcular el promedio más tarde
            totalTime += timeSpent;
            totalbotDifficult += botDifficult;
            totalPosition += position;

            count++;
        }

        // Si hay datos válidos, calcular los promedios
        if (count > 0)
        {
            //He cogido el averageTime por si lo necesitaba, pero al pensar que son pistas aleatorias, es muy complicado medir el tiempo medio
            float averageTime = totalTime / count;
            float averageBotDifficult = totalbotDifficult / (float)count;
            float averagePosition = totalPosition / count;

            if (averageBotDifficult >= 2f && averagePosition > 1.75f)
            {
                return 3;
            }
            else if (averageBotDifficult <= 2f && averagePosition < 2.25f)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        return 2;
    }
}
