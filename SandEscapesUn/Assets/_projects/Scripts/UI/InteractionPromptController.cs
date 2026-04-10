using SandEscapes.Interaction;
using UnityEngine;

namespace SandEscapes.UI
{
    /// <summary>
    /// Reads the current interactable under the crosshair and shows a prompt when applicable.
    /// </summary>
    public class InteractionPromptController : MonoBehaviour
    {
        [SerializeField] CameraInteractionController interaction;
        [SerializeField] InteractionPromptUIView promptView;
        [SerializeField] string defaultInteractPrompt = "E - Interact";

        void Reset()
        {
            interaction = GetComponent<CameraInteractionController>();
        }

        void Awake()
        {
            if (interaction == null)
                interaction = GetComponent<CameraInteractionController>();
        }

        void Update()
        {
            if (promptView == null || interaction == null)
                return;

            if (interaction.PlayerCamera == null)
            {
                promptView.Hide();
                return;
            }

            if (!interaction.TryGetLookHit(out var hit))
            {
                promptView.Hide();
                return;
            }

            if (InteractionUtility.TryGetInteractablePromptProvider(hit.collider, out var promptProvider))
            {
                promptView.Show(promptProvider.GetPromptText());
                return;
            }

            if (InteractionUtility.TryGetInteractable(hit.collider, out _))
            {
                promptView.Show(defaultInteractPrompt);
                return;
            }

            promptView.Hide();
        }
    }
}
