using UnityEngine;

namespace SandEscapes.Interaction
{
    public static class InteractionUtility
    {
        public static bool TryGetInteractable(Collider collider, out IInteractable interactable)
        {
            interactable = null;
            if (collider == null)
                return false;

            var behaviours = collider.GetComponentsInParent<MonoBehaviour>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IInteractable found)
                {
                    interactable = found;
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetInteractablePromptProvider(Collider collider, out IInteractablePromptProvider provider)
        {
            provider = null;
            if (collider == null)
                return false;

            var behaviours = collider.GetComponentsInParent<MonoBehaviour>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IInteractablePromptProvider found)
                {
                    provider = found;
                    return true;
                }
            }

            return false;
        }
    }
}
