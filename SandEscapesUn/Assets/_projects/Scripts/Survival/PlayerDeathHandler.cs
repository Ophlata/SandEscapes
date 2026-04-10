using UnityEngine;
using UnityEngine.SceneManagement;

namespace SandEscapes.Survival
{
    /// <summary>
    /// Reloads the active scene when <see cref="PlayerStatsSystem"/> reports death.
    /// </summary>
    public class PlayerDeathHandler : MonoBehaviour
    {
        [SerializeField] PlayerStatsSystem stats;
        [SerializeField] bool reloadActiveScene = true;
        [SerializeField] string sceneNameOverride = "TestMechanics";
        [SerializeField] float reloadDelaySeconds;

        void Awake()
        {
            if (stats == null)
                stats = GetComponent<PlayerStatsSystem>();
        }

        void OnEnable()
        {
            if (stats != null)
                stats.OnPlayerDied += HandleDeath;
        }

        void OnDisable()
        {
            CancelInvoke();
            if (stats != null)
                stats.OnPlayerDied -= HandleDeath;
        }

        void HandleDeath()
        {
            if (reloadDelaySeconds > 0f)
            {
                Invoke(nameof(DoReload), reloadDelaySeconds);
                return;
            }

            DoReload();
        }

        void DoReload()
        {
            if (!reloadActiveScene && !string.IsNullOrEmpty(sceneNameOverride))
            {
                SceneManager.LoadScene(sceneNameOverride);
                return;
            }

            var s = SceneManager.GetActiveScene();
            SceneManager.LoadScene(s.name);
        }
    }
}
