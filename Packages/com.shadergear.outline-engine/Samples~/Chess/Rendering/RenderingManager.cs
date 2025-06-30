using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SHADERGEAR.OutlineEngine.Samples.Chess
{
    public class RenderingManager : MonoBehaviour
    {
        [SerializeField] UniversalRenderPipelineAsset demoRenderPipeline;

        void Start()
        {
            if (GraphicsSettings.currentRenderPipeline != demoRenderPipeline)
            {
                QualitySettings.renderPipeline = demoRenderPipeline;
                Debug.Log("(Outline Engine Demo) The demo render pipeline asset has been assigned automatically.", demoRenderPipeline);
            }
        }
    }
}