using UnityEngine;

public class WaterSoundZone : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource swimSource;
    [SerializeField] private AudioClip swimClip;
    [SerializeField] private float minMoveSpeed = 0.2f;
    [SerializeField] private float stepInterval = 0.6f;

    private float stepTimer;

    private PlayerController player;
    private CharacterController playerController;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        player = other.GetComponent<PlayerController>();
        playerController = other.GetComponent<CharacterController>();

        if (swimSource != null && swimClip != null)
        {
            swimSource.clip = swimClip;
            swimSource.loop = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        StopSwimmingSound();
        player = null;
        playerController = null;
    }

    private void Update()
    {
        if (player == null || playerController == null) return;

        Vector3 horizontalVelocity = playerController.velocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;

        if (speed > minMoveSpeed)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlaySwimSound();
                stepTimer = stepInterval;
            }
        }
        else
        {
            // стоит — тишина
            StopSwimmingSound();
            stepTimer = 0f;
        }
    }

    private void PlaySwimSound()
    {
        if (swimSource == null || swimClip == null) return;

        if (!swimSource.isPlaying)
            swimSource.Play();
    }

    private void StopSwimmingSound()
    {
        if (swimSource == null) return;

        if (swimSource.isPlaying)
            swimSource.Stop();
    }
}