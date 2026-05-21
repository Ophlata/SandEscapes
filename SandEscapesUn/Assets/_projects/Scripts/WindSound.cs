using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WindSound : MonoBehaviour
{
    [Header("Игрок")]
    public CharacterController player;

    [Header("Настройки ветра")]
    public float minVolume = 0.2f;
    public float maxVolume = 1f;

    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;

    [Header("Скорость игрока")]
    public float maxPlayerSpeed = 10f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (player == null)
            player = FindObjectOfType<CharacterController>();

        audioSource.loop = true;
        audioSource.playOnAwake = true;

        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 velocity = player.velocity;
        velocity.y = 0f;

        float speed = velocity.magnitude;

        float t = Mathf.Clamp01(speed / maxPlayerSpeed);

        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, t);
        audioSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);
    }
}