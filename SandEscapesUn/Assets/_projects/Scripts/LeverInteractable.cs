using UnityEngine;

namespace SandEscapes
{
    public class LeverInteractable : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private GateController gate;

        [Header("Анимация рычага")]
        [SerializeField] private Transform leverHandle; // сама палка рычага
        [SerializeField] private Vector3 rotationActivated = new Vector3(40f, 0f, 0f);
        [SerializeField] private Vector3 rotationDefault = new Vector3(0f, 0f, 0f);
        [SerializeField] private float rotationSpeed = 5f;

        private bool isActivated = false;
        private Quaternion targetRotation;

        void Start()
        {
            targetRotation = Quaternion.Euler(rotationDefault);
        }

        void Update()
        {
            // Плавно поворачиваем рычаг
            leverHandle.localRotation = Quaternion.Lerp(
                leverHandle.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }

        public void Interact()
        {
            isActivated = !isActivated;
            targetRotation = Quaternion.Euler(isActivated ? rotationActivated : rotationDefault);

            if (isActivated)
                gate.Open();
            else
                gate.Close();
        }
    }
}