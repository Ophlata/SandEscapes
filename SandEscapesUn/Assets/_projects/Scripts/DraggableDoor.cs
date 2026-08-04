using UnityEngine;
using SandEscapes.Interaction;

namespace SandEscapes
{
    [RequireComponent(typeof(AudioSource))]
    public class DraggableDoor : MonoBehaviour, IInteractable, IInteractablePromptProvider
    {
        [Header("Door")]
        [SerializeField] private Transform doorHinge;

        [SerializeField] private float closedAngle = 0f;
        [SerializeField] private float openAngle = 100f;

        [SerializeField] private float openSpeed = 5f;

        [Header("Prompt")]
        [SerializeField] private string promptClosed = "E - Открыть дверь";
        [SerializeField] private string promptOpen = "E - Закрыть дверь";

        [Header("Sound")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] [Range(0f,1f)] private float volume = 1f;

        private AudioSource audioSource;

        private bool isOpen;
        private float currentAngle;
        private float targetAngle;

        void Awake()
        {
            if (doorHinge == null)
                doorHinge = transform;

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 1f;
        }

        void Start()
        {
            currentAngle = closedAngle;
            targetAngle = closedAngle;
            ApplyRotation();
        }

        void Update()
        {
            currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * openSpeed);
            ApplyRotation();
        }

        private void ApplyRotation()
        {
            Vector3 rot = doorHinge.localEulerAngles;
            doorHinge.localRotation = Quaternion.Euler(rot.x, currentAngle, rot.z);
        }

        public void OnInteract(GameObject interactor)
        {
            isOpen = !isOpen;

            targetAngle = isOpen ? openAngle : closedAngle;

            AudioClip clip = isOpen ? openSound : closeSound;

            if (clip != null)
                audioSource.PlayOneShot(clip, volume);
        }

        public string GetPromptText()
        {
            return isOpen ? promptOpen : promptClosed;
        }
    }
}