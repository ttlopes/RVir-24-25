using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonClickLogger : MonoBehaviour
{

    public string nextScene;

    public void Transition()
    {
        SceneManager.LoadScene(nextScene);
    }
}
