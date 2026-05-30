using UnityEngine;

namespace SandEscapes
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] private string defaultSpawnPoint = "SpawnDefault";

        void Awake()
        {
            string spawnName = PlayerPrefs.GetString("SpawnPoint", defaultSpawnPoint);
            PlayerPrefs.DeleteKey("SpawnPoint");

            GameObject spawnPoint = GameObject.Find(spawnName);
            if (spawnPoint != null)
                transform.position = spawnPoint.transform.position;
        }
    }
}