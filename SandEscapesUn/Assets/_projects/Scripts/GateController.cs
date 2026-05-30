using UnityEngine;

namespace SandEscapes
{
    public class GateController : MonoBehaviour
    {
        [SerializeField] private float openHeight = 3f;
        [SerializeField] private float moveSpeed = 2f;

        [Header("Звук")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;

        private Vector3 closedPosition;
        private Vector3 openPosition;
        private Vector3 targetPosition;

        void Start()
        {
            closedPosition = transform.position;
            openPosition = closedPosition + Vector3.up * openHeight;
            targetPosition = closedPosition;

            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
        }

        void Update()
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
        }

        public void Open()
        {
            targetPosition = openPosition;
            if (openSound != null) audioSource.PlayOneShot(openSound);
        }

        public void Close()
        {
            targetPosition = closedPosition;
            if (closeSound != null) audioSource.PlayOneShot(closeSound);
        }
    }
}