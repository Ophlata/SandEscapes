using System.Collections;
using UnityEngine;

public class NoteViewer : MonoBehaviour
{
    public static NoteViewer Instance;

    [Header("UI")]
    [SerializeField] private RectTransform paper;

    [Header("Animation")]
    [SerializeField] private float animationTime = 0.25f;
    [SerializeField] private float paperScale = 2.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openSound;
    [SerializeField] private AudioClip closeSound;

    private bool opened;

    void Awake()
    {
        Instance = this;

        gameObject.SetActive(true);
        paper.gameObject.SetActive(false);
    }

    public void Open()
    {
        if (opened)
        {
            Close();
            return;
        }

        opened = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        paper.gameObject.SetActive(true);

        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);

        StopAllCoroutines();
        StartCoroutine(OpenAnimation());
    }

    public void Close()
    {
        if (!opened)
            return;

        opened = false;

        if (audioSource != null && closeSound != null)
            audioSource.PlayOneShot(closeSound);

        StopAllCoroutines();
        StartCoroutine(CloseAnimation());
    }

    IEnumerator OpenAnimation()
    {
        paper.localScale = Vector3.zero;
        paper.localRotation = Quaternion.Euler(0, 0, -12);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / animationTime;

            paper.localScale = Vector3.Lerp(
                Vector3.zero,
                Vector3.one * paperScale,
                t);

            paper.localRotation = Quaternion.Lerp(
                Quaternion.Euler(0, 0, -12),
                Quaternion.identity,
                t);

            yield return null;
        }

        paper.localScale = Vector3.one * paperScale;
    }

    IEnumerator CloseAnimation()
    {
        Vector3 startScale = paper.localScale;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / animationTime;

            paper.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        paper.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!opened)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
            Close();
    }
}