using UnityEngine;
using SandEscapes.Interaction;

namespace SandEscapes
{
    public class LeverLightSwitch : MonoBehaviour, IInteractable, IInteractablePromptProvider
    {
        [Header("Свет города")]
        [SerializeField] private string lightTag = "CityLight";
        [SerializeField] private string windowTag = "CityWindow";
        [SerializeField] private Material glowMaterial;
        [SerializeField] private Material normalMaterial;

        [Header("Анимация рычага")]
        [SerializeField] private Transform leverHandle;
        [SerializeField] private Vector3 rotationActivated = new Vector3(40f, 0f, 0f);
        [SerializeField] private Vector3 rotationDefault = new Vector3(0f, 0f, 0f);
        [SerializeField] private float rotationSpeed = 5f;

        [Header("Подсказка")]
        [SerializeField] private string promptTextOff = "E - Включить свет";
        [SerializeField] private string promptTextOn = "E - Выключить свет";

        [Header("Резервный клик мышкой (можно оставить или выключить)")]
        [SerializeField] private bool allowMouseClick = true;

        private bool isActivated = false;
        private Quaternion targetRotation;

        void Awake()
        {
            if (leverHandle == null)
                leverHandle = transform;
        }

        void Start()
        {
            targetRotation = Quaternion.Euler(rotationDefault);
        }

        void Update()
        {
            leverHandle.localRotation = Quaternion.Lerp(
                leverHandle.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }

        // Вызывается системой взаимодействия по E
        public void OnInteract(GameObject interactor)
        {
            ToggleLever();
        }

        // Текст подсказки внизу экрана — меняется в зависимости от состояния
        public string GetPromptText()
        {
            return isActivated ? promptTextOn : promptTextOff;
        }

        private void ToggleLever()
        {
            isActivated = !isActivated;
            targetRotation = Quaternion.Euler(isActivated ? rotationActivated : rotationDefault);

            ToggleCityLights(isActivated);
            ToggleWindows(isActivated);
        }

        private void ToggleCityLights(bool state)
        {
            GameObject[] lights = GameObject.FindGameObjectsWithTag(lightTag);
            foreach (GameObject obj in lights)
            {
                Light lightComp = obj.GetComponent<Light>();
                if (lightComp != null)
                    lightComp.enabled = state;
            }
        }

        private void ToggleWindows(bool state)
        {
            GameObject[] windows = GameObject.FindGameObjectsWithTag(windowTag);
            Material targetMat = state ? glowMaterial : normalMaterial;
            if (targetMat == null) return;

            foreach (GameObject obj in windows)
            {
                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null)
                    rend.material = targetMat;
            }
        }

        // Резервный клик мышкой — оставлен, чтобы работало и так, и через E
        void OnMouseDown()
        {
            if (allowMouseClick)
                ToggleLever();
        }
    }
}