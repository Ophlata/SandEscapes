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

        [Header("Материал здания (один конкретный объект)")]
        [SerializeField] private Renderer buildingRenderer;
        [SerializeField] private Material buildingGlowMaterial;
        [SerializeField] private Material buildingNormalMaterial;

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
            audioSource.spatialBlend = 0f;
        }

        void Start()
        {
            targetRotation = Quaternion.Euler(rotationDefault);

            isActivated = false;
            ToggleCityLights(false);
            ToggleWindows(false);
            ToggleBuildingMaterial(false);
        }

        void Update()
        {
            leverHandle.localRotation = Quaternion.Lerp(
                leverHandle.localRotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }

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

            PlayLeverSound();
            ToggleCityLights(isActivated);
            ToggleWindows(isActivated);
            ToggleBuildingMaterial(isActivated);
        }

        private void PlayLeverSound()
        {
            if (leverSound == null || audioSource == null)
                return;

            audioSource.clip = leverSound;
            audioSource.volume = leverSoundVolume;
            audioSource.loop = false;
            audioSource.Play();
        }

        private void ToggleCityLights(bool state)
        {
            if (string.IsNullOrEmpty(lightTag) || !TagExists(lightTag))
                return;

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
            if (string.IsNullOrEmpty(windowTag) || !TagExists(windowTag))
                return;

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

        private void ToggleBuildingMaterial(bool state)
        {
            if (buildingRenderer == null) return;

            Material targetMat = state ? buildingGlowMaterial : buildingNormalMaterial;
            if (targetMat != null)
                buildingRenderer.material = targetMat;
        }

        private bool TagExists(string tag)
        {
            try
            {
                GameObject.FindGameObjectsWithTag(tag);
                return true;
            }
            catch (UnityException)
            {
                return false;
            }
        }

        void OnMouseDown()
        {
            if (allowMouseClick)
                ToggleLever();
        }
    }
}