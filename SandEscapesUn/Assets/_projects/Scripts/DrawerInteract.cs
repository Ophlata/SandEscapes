using UnityEngine;
using SandEscapes.Interaction;

namespace SandEscapes
{
    [RequireComponent(typeof(AudioSource))]
    public class DrawerInteract : MonoBehaviour, IInteractable, IInteractablePromptProvider
    {
        [Header("Drawer")]
        [SerializeField] private Transform drawer;
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 0f, -0.45f);
        [SerializeField] private float speed = 6f;

        [Header("Prompt")]
        [SerializeField] private string openPrompt = "E - Открыть";
        [SerializeField] private string closePrompt = "E - Закрыть";

        [Header("Sound")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] [Range(0, 1)] private float volume = 1f;

        private AudioSource audioSource;

        private Vector3 closedPos;
        private Vector3 targetPos;
        private bool opened;

        void Awake()
        {
            if (drawer == null)
                drawer = transform;

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
        }

        void Start()
        {
            closedPos = drawer.localPosition;
            targetPos = closedPos;
        }

        void Update()
        {
            drawer.localPosition = Vector3.Lerp(
                drawer.localPosition,
                targetPos,
                Time.deltaTime * speed);
        }

        public void OnInteract(GameObject interactor)
        {
            opened = !opened;

            targetPos = opened
                ? closedPos + openOffset
                : closedPos;

            AudioClip clip = opened ? openSound : closeSound;

            if (clip != null)
                audioSource.PlayOneShot(clip, volume);
        }

        public string GetPromptText()
        {
            return opened ? closePrompt : openPrompt;
        }
    }
}