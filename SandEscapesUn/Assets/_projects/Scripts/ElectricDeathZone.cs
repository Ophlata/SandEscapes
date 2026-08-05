using System.Collections;
using UnityEngine;
using SandEscapes.Survival;

public class ElectricDeathZone : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private AudioClip electricSound;
    [SerializeField] private float shockTime = 2f;
    [SerializeField] private float shakeStrength = 0.15f;
    [SerializeField] private float rotationStrength = 15f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        StartCoroutine(Electrocute(other.gameObject));
    }

    private IEnumerator Electrocute(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        PlayerStatsSystem stats = player.GetComponent<PlayerStatsSystem>();

        if (controller != null)
            controller.enabled = false;

        // Проигрываем звук отдельным источником
        if (electricSound != null)
            AudioSource.PlayClipAtPoint(electricSound, player.transform.position);

        Vector3 startPos = player.transform.position;
        Quaternion startRot = player.transform.rotation;

        float timer = 0f;

        while (timer < shockTime)
        {
            timer += Time.deltaTime;

            player.transform.position =
                startPos + Random.insideUnitSphere * shakeStrength;

            player.transform.rotation =
                startRot * Quaternion.Euler(
                    Random.Range(-rotationStrength, rotationStrength),
                    Random.Range(-rotationStrength, rotationStrength),
                    Random.Range(-rotationStrength, rotationStrength));

            yield return null;
        }

        player.transform.position = startPos;
        player.transform.rotation = startRot;

        if (stats != null)
            stats.TakeDamage(9999);
    }
}