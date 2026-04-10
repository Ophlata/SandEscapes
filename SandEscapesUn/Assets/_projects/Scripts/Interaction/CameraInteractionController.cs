using UnityEngine;

namespace SandEscapes.Interaction
{
    /// <summary>
    /// Raycasts from the player camera center and invokes IInteractable on a hit when the interact key is pressed.
    /// </summary>
    public class CameraInteractionController : MonoBehaviour
    {
        [SerializeField] Camera playerCamera;
        [SerializeField] float interactionDistance = 3f;
        [SerializeField] LayerMask interactableLayers = ~0;
        [SerializeField] KeyCode interactKey = KeyCode.E;
        [SerializeField] QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        void Reset()
        {
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>(true);
        }

        void Awake()
        {
            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>(true);
        }

        void Update()
        {
            if (!Input.GetKeyDown(interactKey))
                return;
            if (playerCamera == null)
                return;

            if (!TryGetInteractableInView(out var hit, out var interactable))
                return;

            interactable.OnInteract(gameObject);
        }

        /// <summary>
        /// Ray from screen center; returns first <see cref="IInteractable"/> hit within range.
        /// </summary>
        public bool TryGetInteractableInView(out RaycastHit hit, out IInteractable interactable)
        {
            interactable = null;
            hit = default;
            if (playerCamera == null)
                return false;

            var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (!Physics.Raycast(ray, out hit, interactionDistance, interactableLayers, triggerInteraction))
                return false;

            return InteractionUtility.TryGetInteractable(hit.collider, out interactable);
        }

        public Camera PlayerCamera => playerCamera;
        public float InteractionDistance => interactionDistance;
        public LayerMask InteractableLayers => interactableLayers;
        public QueryTriggerInteraction TriggerInteraction => triggerInteraction;

        /// <summary>
        /// Center-screen raycast (for prompts etc.), same parameters as interaction.
        /// </summary>
        public bool TryGetLookHit(out RaycastHit hit)
        {
            hit = default;
            if (playerCamera == null)
                return false;

            var ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            return Physics.Raycast(ray, out hit, interactionDistance, interactableLayers, triggerInteraction);
        }
    }
}
