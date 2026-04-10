using UnityEngine;

namespace SandEscapes.Interaction
{
    /// <summary>
    /// Implemented by world objects the player can activate via the camera interaction ray.
    /// </summary>
    public interface IInteractable
    {
        void OnInteract(GameObject interactor);
    }
}
