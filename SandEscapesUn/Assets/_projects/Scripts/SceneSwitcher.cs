using UnityEngine;
using UnityEngine.SceneManagement;

namespace _projects.Scripts
{
    public class SceneSwitcher : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SceneManager.LoadScene("Menu");

            if (Input.GetKeyDown(KeyCode.Alpha2))
                SceneManager.LoadScene("TestMechanics");

            if (Input.GetKeyDown(KeyCode.Alpha3))
                SceneManager.LoadScene("Game");
        }

        public void LoadGameMenu()
        {
            SceneManager.LoadScene("Menu");
        }

        public void LoadTestMechanics()
        {
            SceneManager.LoadScene("TestMechanics");
        }

        public void LoadGame()
        {
            SceneManager.LoadScene("Game");
        }

        public void QuitGame()
        {
            Application.Quit();
            Debug.Log("Game closed");
        }
    }
}