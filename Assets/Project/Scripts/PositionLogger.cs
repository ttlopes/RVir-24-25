using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class PositionLogger : MonoBehaviour
{

    public GameObject targetObject;
    public bool showOnConsole;
    private string csvFilePath;
    private string sceneName;

    private void Start()
    {

        csvFilePath = Path.Combine(Application.persistentDataPath, "PositionLogs.csv");
        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;

        // Mantém o ficheiro anterior (dá append)
        if (!File.Exists(csvFilePath))
        {
            File.WriteAllText(csvFilePath, "Task,Timestamp,Position\n");
        }

        // Cria um ficheiro novo a cada execução
        //using (StreamWriter writer = File.CreateText(csvFilePath))
        //{
        //    writer.WriteLine("Timestamp,Position");
        //}

        StartCoroutine(LogPositionEverySecond());
    }

    private IEnumerator LogPositionEverySecond()
    {
        while (true)
        {
            if (targetObject != null)
            {
                // Get the current position of the target object
                string position = targetObject.transform.position.ToString();

                // Create the log entry
                string logEntry = $"{sceneName},{System.DateTime.Now},{position}\n";

                // Append the log entry to the CSV file
                File.AppendAllText(csvFilePath, logEntry);

                // Optionally, log the entry to the console for debugging
                if (showOnConsole) { Debug.Log($"Logged Position: {logEntry}"); }
            }

            // Wait for 1 second before repeating
            yield return new WaitForSeconds(1f);
        }
    }
}
