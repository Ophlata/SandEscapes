using UnityEngine;
using UnityEngine.SceneManagement;
 
public class MainMenu : MonoBehaviour
{
    [Tooltip("Название сцены с игрой (точно как в Build Settings)")]
    public string gameSceneName = "GameScene";
 
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
 
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit (в редакторе не работает)");
    }
}