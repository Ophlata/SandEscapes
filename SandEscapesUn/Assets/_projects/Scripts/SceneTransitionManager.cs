using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [Header("=== FADE ===")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("=== PLAYER ===")]
    [SerializeField] private string playerTag = "Player";

    private string targetSpawnPointID;
    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (fadeImage != null)
            SetFadeAlpha(0f);
        else
            Debug.LogWarning(
                "[SceneTransitionManager] Fade Image не назначен."
            );
    }

    public void LoadScene(
        string sceneName,
        string spawnPointID)
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "[SceneTransitionManager] sceneName пустой!"
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(spawnPointID))
        {
            Debug.LogError(
                "[SceneTransitionManager] spawnPointID пустой!"
            );
            return;
        }

        targetSpawnPointID = spawnPointID;

        StartCoroutine(
            LoadSceneRoutine(sceneName)
        );
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;

        Debug.Log(
            "[SceneTransitionManager] Начинаю загрузку: " +
            sceneName
        );

        // Затемнение
        if (fadeImage != null)
        {
            yield return StartCoroutine(
                FadeToBlack()
            );
        }

        // Проверяем сцену
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                "[SceneTransitionManager] Сцена '" +
                sceneName +
                "' не найдена в Build Settings!"
            );

            isLoading = false;

            yield break;
        }

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError(
                "[SceneTransitionManager] LoadSceneAsync вернул NULL!"
            );

            isLoading = false;

            yield break;
        }

        while (!operation.isDone)
        {
            yield return null;
        }
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        Debug.Log(
            "[SceneTransitionManager] Сцена загружена: " +
            scene.name
        );

        StartCoroutine(
            PlacePlayer()
        );
    }

    private IEnumerator PlacePlayer()
    {
        yield return null;

        GameObject player =
            GameObject.FindGameObjectWithTag(
                playerTag
            );

        if (player == null)
        {
            Debug.LogError(
                "[SceneTransitionManager] Игрок не найден! " +
                "Проверь Tag = Player."
            );

            isLoading = false;

            yield break;
        }

        if (string.IsNullOrWhiteSpace(
            targetSpawnPointID))
        {
            Debug.LogError(
                "[SceneTransitionManager] " +
                "Target Spawn Point ID пустой!"
            );

            isLoading = false;

            yield break;
        }

        SpawnPoint[] spawnPoints =
            FindObjectsByType<SpawnPoint>(
                FindObjectsSortMode.None
            );

        SpawnPoint targetPoint = null;

        foreach (SpawnPoint point in spawnPoints)
        {
            if (point == null)
                continue;

            if (point.spawnID ==
                targetSpawnPointID)
            {
                targetPoint = point;
                break;
            }
        }

        if (targetPoint == null)
        {
            Debug.LogError(
                "[SceneTransitionManager] " +
                "SpawnPoint с ID '" +
                targetSpawnPointID +
                "' не найден в сцене '" +
                SceneManager.GetActiveScene().name +
                "'!"
            );

            isLoading = false;

            yield break;
        }

        CharacterController controller =
            player.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        player.transform.position =
            targetPoint.transform.position;

        player.transform.rotation =
            targetPoint.transform.rotation;

        if (controller != null)
            controller.enabled = true;

        Debug.Log(
            "[SceneTransitionManager] Игрок помещён в SpawnPoint: " +
            targetSpawnPointID
        );

        targetSpawnPointID = "";

        if (fadeImage != null)
        {
            yield return StartCoroutine(
                FadeFromBlack()
            );
        }

        isLoading = false;
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null)
            yield break;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float alpha =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            SetFadeAlpha(alpha);

            yield return null;
        }

        SetFadeAlpha(1f);
    }

    private IEnumerator FadeFromBlack()
    {
        if (fadeImage == null)
            yield break;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float alpha =
                1f -
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            SetFadeAlpha(alpha);

            yield return null;
        }

        SetFadeAlpha(0f);
    }

    private void SetFadeAlpha(float alpha)
    {
        if (fadeImage == null)
            return;

        Color color = fadeImage.color;

        color.a = Mathf.Clamp01(alpha);

        fadeImage.color = color;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -=
            OnSceneLoaded;
    }
}