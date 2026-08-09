using UnityEngine;
using SandEscapes.Interaction;

public class SceneDoor : MonoBehaviour, IInteractable
{
    [Header("=== SCENE ===")]

    [Tooltip("Точное название сцены")]
    [SerializeField] private string targetScene;

    [Tooltip("ID точки появления игрока в новой сцене")]
    [SerializeField] private string spawnPointID;

    [Header("=== DEBUG ===")]

    [SerializeField] private bool debugLogs = true;

    private bool loading;

    public void OnInteract(GameObject interactor)
    {
        if (loading)
            return;

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError(
                "[SceneDoor] SceneTransitionManager не найден в сцене!"
            );

            return;
        }

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogError(
                "[SceneDoor] Target Scene не указана!",
                this
            );

            return;
        }

        if (string.IsNullOrEmpty(spawnPointID))
        {
            Debug.LogError(
                "[SceneDoor] Spawn Point ID не указан!",
                this
            );

            return;
        }

        loading = true;

        if (debugLogs)
        {
            Debug.Log(
                $"[SceneDoor] Переход: {targetScene} | Spawn: {spawnPointID}",
                this
            );
        }

        SceneTransitionManager.Instance.LoadScene(
            targetScene,
            spawnPointID
        );
    }
}