using UnityEngine;

namespace SandEscapes
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private float interactRange = 2.5f;
        private Camera playerCamera;

        void Start()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        void Update()
        {
            if (!Input.GetKeyDown(interactKey)) return;

            Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                // Рычаг
                var lever = hit.collider.GetComponentInParent<LeverInteractable>();
                if (lever != null)
                {
                    lever.Interact();
                    return;
                }

                // Дверь
                var door = hit.collider.GetComponentInParent<DoorTeleport>();
                if (door != null)
                {
                    door.Teleport();
                    return;
                }
            }
        }
    }
}