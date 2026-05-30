using UnityEngine;
using UnityEngine.SceneManagement;

namespace SandEscapes
{
    public class DoorTeleport : MonoBehaviour
    {
        [SerializeField] private string targetScene;
        [SerializeField] private string spawnPointName; // имя точки появления в целевой сцене

        public void Teleport()
        {
            PlayerPrefs.SetString("SpawnPoint", spawnPointName);
            SceneManager.LoadScene(targetScene);
        }
    }
}