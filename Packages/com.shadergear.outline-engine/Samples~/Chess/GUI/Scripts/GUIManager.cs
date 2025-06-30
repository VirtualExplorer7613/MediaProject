using UnityEngine;
using UnityEngine.UIElements;

namespace SHADERGEAR.OutlineEngine.Samples.Chess
{
    public class GUIManager : MonoBehaviour
    {
        [SerializeField] UIDocument document;
        [SerializeField] AudioManager audioManager;
        [SerializeField] CursorManager cursorManager;
        [SerializeField] OrbitCamera orbitCamera;

        void Start()
        {
            ShowIntro();
            RegisterButtons();
            RegisterEffects();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                OnStoreButtonClicked();
            }

            if (Time.frameCount % 60 == 0)
            {
                var label = document.rootVisualElement.Q<Label>("fps-label");
                var fps = Mathf.RoundToInt(1f / Time.smoothDeltaTime);
                label.text = $"FPS: {fps}";
            }
        }

        void ShowIntro()
        {
            var root = document.rootVisualElement;
            root.Q("intro").style.display = DisplayStyle.Flex;
        }

        void RegisterButtons()
        {
            var root = document.rootVisualElement;
            root.Q<Button>("store-btn").clicked += OnStoreButtonClicked;
            root.Q<Button>("start-btn").clicked += OnStartButtonClicked;
        }

        void RegisterEffects()
        {
            var root = document.rootVisualElement;

            root.Query<Button>().ForEach(button =>
            {
                button.clicked += () =>
                {
                    audioManager.PlayClick();
                };
            });

            root.Query<Slider>().ForEach(button =>
            {
                button.RegisterValueChangedCallback(ev =>
                {
                    audioManager.PlaySlide();
                });
            });
        }

        void OnStoreButtonClicked()
        {
            Application.OpenURL("https://u3d.as/3wiF");
        }

        void OnStartButtonClicked()
        {
            var root = document.rootVisualElement;
            root.Q("intro").style.display = DisplayStyle.None;
            cursorManager.enabled = true;
            orbitCamera.enabled = true;
        }
    }
}