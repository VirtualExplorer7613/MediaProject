using UnityEngine;
using UnityEngine.UIElements;

namespace SHADERGEAR.OutlineEngine.Samples.Chess
{
    public class ChessManager : MonoBehaviour
    {
        [SerializeField] GameObject[] pieces;
        [SerializeField] UIDocument document;
        [SerializeField] OutlineEngine outlineEngine;

        int selectedPiece = 0;

        void Start()
        {
            SelectPiece(0);
            RegisterEvents();
            InitializeParameters();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                SelectPiece(selectedPiece + 1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                SelectPiece(selectedPiece - 1);
            }
        }

        void RegisterEvents()
        {
            var root = document.rootVisualElement;

            root.Q<Slider>("width-slider").RegisterValueChangedCallback(OnWidthSliderValueChanged);
            root.Q<Slider>("softness-slider").RegisterValueChangedCallback(OnSoftnessSliderValueChanged);
            root.Q<Button>("piece-left-btn").clicked += OnPieceButtonLeftClicked;
            root.Q<Button>("piece-right-btn").clicked += OnPieceButtonRightClicked;
            root.Q<Button>("randomize-btn").clicked += OnRandomizeButtonClicked;
            root.Q<Slider>("hue-slider").RegisterValueChangedCallback(OnColorSliderChanged);
            root.Q<Slider>("saturation-slider").RegisterValueChangedCallback(OnColorSliderChanged);
            root.Q<Slider>("value-slider").RegisterValueChangedCallback(OnColorSliderChanged);
            root.Q<Slider>("alpha-slider").RegisterValueChangedCallback(OnColorSliderChanged);
            root.Q<EnumField>("event-dropdown").RegisterValueChangedCallback(OnEventDropdownChanged);
            root.Q<EnumField>("occlusion-dropdown").RegisterValueChangedCallback(OnOcclusionDropdownChanged);
        }

        void InitializeParameters()
        {
            var root = document.rootVisualElement;

            var h = root.Q<Slider>("hue-slider").value;
            var s = root.Q<Slider>("saturation-slider").value;
            var v = root.Q<Slider>("value-slider").value;
            var color = Color.HSVToRGB(h, s, v);
            outlineEngine.Outline.Color = color;

            var width = (int)root.Q<Slider>("width-slider").value;
            outlineEngine.Outline.Width = width;

            var softness = root.Q<Slider>("softness-slider").value;
            outlineEngine.Outline.Softness = softness;

            var ev = (RenderingEvent)root.Q<EnumField>("event-dropdown").value;
            outlineEngine.Rendering.Event = ev;

            var occlusion = (RenderingOcclusion)root.Q<EnumField>("occlusion-dropdown").value;
            outlineEngine.Rendering.Occlusion = occlusion;
        }

        public void SelectPiece(int index)
        {
            pieces[selectedPiece].SetActive(false);

            selectedPiece = index;
            if (selectedPiece >= pieces.Length)
            {
                selectedPiece = 0;
            }
            else if (selectedPiece < 0)
            {
                selectedPiece = pieces.Length - 1;
            }

            pieces[selectedPiece].SetActive(true);

            document.rootVisualElement.Q<Label>("piece-name").text = pieces[selectedPiece].name;
        }

        void OnWidthSliderValueChanged(ChangeEvent<float> changeEvent)
        {
            outlineEngine.Outline.Width = (int)changeEvent.newValue;
            outlineEngine.Refresh();
        }

        void OnSoftnessSliderValueChanged(ChangeEvent<float> changeEvent)
        {
            outlineEngine.Outline.Softness = changeEvent.newValue;
            outlineEngine.Refresh();
        }

        void OnPieceButtonLeftClicked()
        {
            SelectPiece(selectedPiece - 1);
        }

        void OnPieceButtonRightClicked()
        {
            SelectPiece(selectedPiece + 1);
        }

        void OnRandomizeButtonClicked()
        {
            outlineEngine.Outline.Width = Random.Range(0, 2) == 1 ? Random.Range(1, 6) : Random.Range(1, 257);
            outlineEngine.Outline.Softness = Random.Range(0, 2) == 1 ? Random.Range(0, 0.05f) : Random.Range(0f, 0.5f);

            var color = Random.ColorHSV();
            color.a = Random.Range(0, 5) != 1 ? 1f : Random.Range(0.5f, 1f);
            outlineEngine.Outline.Color = color;
            outlineEngine.Refresh();

            SelectPiece(Random.Range(0, pieces.Length));

            Color.RGBToHSV(color, out var h, out var s, out var v);
            var root = document.rootVisualElement;
            root.Q<Slider>("hue-slider").value = h;
            root.Q<Slider>("saturation-slider").value = s;
            root.Q<Slider>("value-slider").value = v;
            root.Q<Slider>("alpha-slider").value = color.a;
            root.Q<Slider>("width-slider").value = outlineEngine.Outline.Width;
            root.Q<Slider>("softness-slider").value = outlineEngine.Outline.Softness;
        }

        void OnColorSliderChanged(ChangeEvent<float> changeEvent)
        {
            var root = document.rootVisualElement;
            var hue = root.Q<Slider>("hue-slider").value;
            var saturation = root.Q<Slider>("saturation-slider").value;
            var value = root.Q<Slider>("value-slider").value;
            var alpha = root.Q<Slider>("alpha-slider").value;

            var color = Color.HSVToRGB(hue, saturation, value);
            color.a = alpha;

            outlineEngine.Outline.Color = color;
            outlineEngine.Refresh();
        }

        void OnEventDropdownChanged(ChangeEvent<System.Enum> changeEvent)
        {
            outlineEngine.Rendering.Event = (RenderingEvent)changeEvent.newValue;
            outlineEngine.Refresh();
        }

        void OnOcclusionDropdownChanged(ChangeEvent<System.Enum> changeEvent)
        {
            outlineEngine.Rendering.Occlusion = (RenderingOcclusion)changeEvent.newValue;
            outlineEngine.Refresh();
        }
    }
}