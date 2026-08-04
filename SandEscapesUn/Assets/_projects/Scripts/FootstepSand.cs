using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepSand : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public AudioClip footstepLoop;

    [Header("Movement")]
    public float minInput = 0.1f;

    [Header("Pitch")]
    public float minPitch = 0.9f;
    public float maxPitch = 1.4f;
    public float maxSpeed = 7f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Invoke(nameof(FixVolume), 0.1f);

        void FixVolume()
        {
            Debug.Log("Volume before fix = " + audioSource.volume);
            audioSource.volume = 1f;
            Debug.Log("Volume after fix = " + audioSource.volume);
        }
        if (controller == null)
            controller = GetComponent<CharacterController>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;      // Для проверки делаем 2D звук
        audioSource.volume = 1f;
        audioSource.clip = footstepLoop;

        Debug.Log("Footstep initialized");
        Debug.Log("Clip = " + footstepLoop);
    }

    void Update()
    {
        if (controller == null)
        {
            Debug.Log("CharacterController missing");
            return;
        }

        if (footstepLoop == null)
        {
            Debug.Log("Footstep clip missing");
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool hasInput = Mathf.Abs(h) > minInput || Mathf.Abs(v) > minInput;
        bool shouldPlay = hasInput && controller.isGrounded;

        float speed = controller.velocity.magnitude;

        audioSource.pitch = Mathf.Lerp(
            minPitch,
            maxPitch,
            Mathf.Clamp01(speed / maxSpeed));

        if (shouldPlay)
        {
            if (!audioSource.isPlaying)
            {
                Debug.Log("PLAY");
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                Debug.Log("STOP");
                audioSource.Stop();
            }
        }
    }
}