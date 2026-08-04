using UnityEngine;
using SandEscapes.Interaction;

namespace SandEscapes
{
    [RequireComponent(typeof(AudioSource))]
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

        [Header("Звук рычага")]
        [SerializeField] private AudioClip leverSound;
        [SerializeField] [Range(0f, 1f)] private float leverSoundVolume = 1f;

        private bool isActivated = false;
        private Quaternion targetRotation;
        private AudioSource audioSource;

        void Awake()
        {
            if (leverHandle == null)
                leverHandle = transform;

            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D-звук, чтобы не зависеть от позиции слушателя
        }

        void Start()
        {
            targetRotation = Quaternion.Euler(rotationDefault);

            // Свет по умолчанию выключен, независимо от состояния в сцене
            isActivated = false;
            ToggleCityLights(false);
            ToggleWindows(false);
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
            PlayLeverSound();
        }

        private void PlayLeverSound()
        {
            if (leverSound == null)
            {
                Debug.LogWarning("[LeverLightSwitch] Lever Sound не назначен в инспекторе.");
                return;
            }

            audioSource.clip = leverSound;
            audioSource.volume = leverSoundVolume;
            audioSource.loop = false;
            audioSource.Play();

            Debug.Log("[LeverLightSwitch] Звук должен был проиграться: " + leverSound.name);
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

        void OnMouseDown()
        {
            if (allowMouseClick)
                ToggleLever();
        }
    }
}