using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class FootstepSurface : MonoBehaviour
{
    [System.Serializable]
    public class Surface
    {
        public string tag;
        public AudioClip clip;
    }

    [Header("Player")]
    public CharacterController controller;

    [Header("Surface Sounds")]
    public List<Surface> surfaces = new();

    [Header("Settings")]
    public float rayDistance = 1.5f;
    public float minInput = 0.1f;
    public float fadeSpeed = 6f;
    public LayerMask groundMask = ~0;

    private AudioSource source;
    private AudioClip currentClip;
    private string currentTag = "";

    void Awake()
    {
        source = GetComponent<AudioSource>();

        if (controller == null)
            controller = GetComponent<CharacterController>();

        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.volume = 0f;
    }

    void Update()
    {
        if (controller == null)
            return;

        bool moving =
            controller.isGrounded &&
            (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > minInput ||
             Mathf.Abs(Input.GetAxisRaw("Vertical")) > minInput);

        if (!moving)
        {
            FadeOut();
            return;
        }

        if (Physics.Raycast(transform.position + Vector3.up * 0.2f,
                            Vector3.down,
                            out RaycastHit hit,
                            rayDistance,
                            groundMask))
        {
            if (hit.collider.tag != currentTag)
            {
                currentTag = hit.collider.tag;

                foreach (var s in surfaces)
                {
                    if (s.tag == currentTag)
                    {
                        ChangeClip(s.clip);
                        break;
                    }
                }
            }
        }

        if (source.clip == null)
            return;

        if (!source.isPlaying)
            source.Play();

        source.volume = Mathf.MoveTowards(source.volume, 1f, fadeSpeed * Time.deltaTime);
    }

    void ChangeClip(AudioClip clip)
    {
        if (clip == currentClip)
            return;

        currentClip = clip;

        source.Stop();
        source.clip = clip;

        if (clip != null)
            source.Play();
    }

    void FadeOut()
    {
        source.volume = Mathf.MoveTowards(source.volume, 0f, fadeSpeed * Time.deltaTime);

        if (source.volume <= 0.01f && source.isPlaying)
            source.Stop();
    }
}