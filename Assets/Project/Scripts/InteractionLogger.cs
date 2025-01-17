using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class VRInteractionLoggerCSV : MonoBehaviour
{
    private string csvFilePath;
    public int rightSphere;
    private int counter;
    public string nextScene;
    private string sceneName;
    public AudioClip soundCorrect;
    public AudioClip soundWrong;
    private AudioSource audioSource;

    private void Start()
    {   
        audioSource = GetComponent<AudioSource>();
        csvFilePath = Path.Combine(Application.persistentDataPath, "VRInteractionLogs.csv");
        Scene currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;
        Debug.Log(sceneName);

        if (!File.Exists(csvFilePath))
        {
            File.WriteAllText(csvFilePath, "Task,Timestamp,Object Name,Interaction Type,Position,Additional Info\n");
        }
    }

    public void LogInteraction(GameObject obj, string interactionType, string additionalInfo = "")
    {
        string position = obj.transform.position.ToString();
        string logEntry = $"{sceneName},{System.DateTime.Now},{obj.name},{interactionType},{position},{additionalInfo}\n";
        File.AppendAllText(csvFilePath, logEntry);
        //Debug.Log($"Log registado: {logEntry}");
        Debug.Log(obj.name + " selecionada");

        if (obj.name == "Sphere " + rightSphere.ToString())
        {
            audioSource.PlayOneShot(soundCorrect);
            StartCoroutine(LoadSceneWithDelay(nextScene, 0.5f));
            Debug.Log("Acertou, passar � pr�xima task");
        }
        else
        {
            audioSource.PlayOneShot(soundWrong);
            counter = counter + 1;
            Debug.Log("Errou " + counter.ToString());
        }
        if (counter == 3)
        {
            audioSource.PlayOneShot(soundWrong);
            StartCoroutine(LoadSceneWithDelay(nextScene, 0.5f));
            Debug.Log("Errou 3 vezes, passar � pr�xima task");
        }
    }

    private IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
