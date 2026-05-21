using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepSand : MonoBehaviour
{
    public CharacterController controller;
    public AudioClip footstepLoop;

    public float minInput = 0.1f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (controller == null)
            controller = GetComponent<CharacterController>();

        audioSource.clip = footstepLoop;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (controller == null || footstepLoop == null) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool hasInput = Mathf.Abs(h) > minInput || Mathf.Abs(v) > minInput;
        bool shouldPlay = hasInput && controller.isGrounded;

        if (shouldPlay)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}