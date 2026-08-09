using System.Collections;
using UnityEngine;
using SandEscapes.Interaction;
using SandEscapes.Inventory;
using SandEscapes.Items;

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

        [Header("Lock")]
        [SerializeField] private bool requiresKey = false;
        [SerializeField] private ItemData requiredKey;
        [SerializeField] private bool consumeKey = false;
        [SerializeField] private string lockedPrompt = "Дверь заперта";
        [SerializeField] private AudioClip lockedSound;

        [Header("Locked Animation")]
        [SerializeField] private float shakeDuration = 0.25f;
        [SerializeField] private float shakeAngle = 6f;
        [SerializeField] private float pushDistance = 0.03f;

        [Header("Prompt")]
        [SerializeField] private string promptClosed = "E - Открыть дверь";
        [SerializeField] private string promptOpen = "E - Закрыть дверь";

        [Header("Sound")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] [Range(0f,1f)] private float volume = 1f;

        private AudioSource audioSource;

        private bool isOpen;
        private bool unlocked;
        private bool shaking;

        private float currentAngle;
        private float targetAngle;

        private Vector3 originalLocalPos;

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

            originalLocalPos = doorHinge.localPosition;

            ApplyRotation();
        }

        void Update()
        {
            if (!shaking)
            {
                currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * openSpeed);
                ApplyRotation();
            }
        }

        void ApplyRotation()
        {
            Vector3 rot = doorHinge.localEulerAngles;
            doorHinge.localRotation = Quaternion.Euler(rot.x, currentAngle, rot.z);
        }

        public void OnInteract(GameObject interactor)
        {
            if (requiresKey && !unlocked)
            {
                InventorySystem inventory = interactor.GetComponent<InventorySystem>();

                if (inventory == null)
                    return;

                if (!inventory.HasAtLeast(requiredKey, 1))
                {
                    if (lockedSound != null)
                        audioSource.PlayOneShot(lockedSound, volume);

                    if (!shaking)
                        StartCoroutine(LockedShake());

                    return;
                }

                if (consumeKey)
                    inventory.TryRemoveItem(requiredKey, 1);

                unlocked = true;
            }

            isOpen = !isOpen;
            targetAngle = isOpen ? openAngle : closedAngle;

            AudioClip clip = isOpen ? openSound : closeSound;

            if (clip != null)
                audioSource.PlayOneShot(clip, volume);
        }

        public string GetPromptText()
        {
            if (requiresKey && !unlocked)
                return lockedPrompt;

            return isOpen ? promptOpen : promptClosed;
        }

        IEnumerator LockedShake()
        {
            shaking = true;

            float timer = 0f;

            while (timer < shakeDuration)
            {
                timer += Time.deltaTime;

                float k = Mathf.Sin(timer * 35f);

                currentAngle = closedAngle - k * shakeAngle;

                doorHinge.localPosition =
                    originalLocalPos
                    - doorHinge.forward * (Mathf.Abs(k) * pushDistance)
                    + doorHinge.right * (k * pushDistance * 0.35f);

                ApplyRotation();

                yield return null;
            }

            currentAngle = closedAngle;
            targetAngle = closedAngle;

            doorHinge.localPosition = originalLocalPos;

            ApplyRotation();

            shaking = false;
        }
    }
}