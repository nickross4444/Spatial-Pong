using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;



// This class sets up the bloom pass
public class BloomMaskFeature : ScriptableRendererFeature
{

    // This class implments the bloom effect
    class BloomMaskPass : ScriptableRenderPass
    {
        Material maskMaterial = null;
        Material _maskMaterial = null;
        SortingCriteria _sortingCriteria;
        readonly ProfilingSampler _profilingSampler;
        readonly List<ShaderTagId> _shaderTagIds = new List<ShaderTagId>();

        private RTHandle rtCustomColor;
        private RendererListParams rendererListParams;

        private RendererList rendererList;

        FilteringSettings m_FilteringSettings;

        public BloomMaskPass(string profilerTag, RenderQueueRange renderQueueRange, SortingCriteria sortingCriteria, LayerMask layerMask, Material material)
        {
            _profilingSampler = new ProfilingSampler(profilerTag);

            m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            this._maskMaterial = material;

            _shaderTagIds.Add(new ShaderTagId("Meta"));
            _shaderTagIds.Add(new ShaderTagId("UniversalForward"));
            _shaderTagIds.Add(new ShaderTagId("UniversalForwardOnly"));
        }

        /// <summary>
        /// This method is called by the renderer before rendering a camera
        /// Override this method if you need to to configure render targets and their clear state, and to create temporary render target textures.
        /// If a render pass doesn't override this method, this render pass renders to the active Camera's render target.
        /// You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        /// </summary>
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.colorFormat = RenderTextureFormat.RGB565;

            RenderingUtils.ReAllocateIfNeeded(ref rtCustomColor, descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_PerObjectBloomMask");
            RTHandle rtCameraDepth = renderingData.cameraData.renderer.cameraDepthTargetHandle;

            ConfigureTarget(rtCustomColor, rtCameraDepth);
            ConfigureClear(ClearFlag.All, Color.white);
        }

        /// <summary>
        /// Execute the pass. This is where custom rendering occurs. Specific details are left to the implementation
        /// </summary>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagIds, ref renderingData, _sortingCriteria);
            drawingSettings.overrideMaterial = _maskMaterial;
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, _profilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                rendererListParams.cullingResults = renderingData.cullResults;
                rendererListParams.drawSettings = drawingSettings;
                rendererListParams.filteringSettings = m_FilteringSettings;
                rendererList = context.CreateRendererList(ref rendererListParams);

                cmd.DrawRendererList(rendererList);
                cmd.SetGlobalTexture("_PerObjectBloomMask", rtCustomColor);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {

        }
        public void Dispose()
        {
            rtCustomColor?.Release();
        }
    }
    

   [System.Serializable]
    public class MaskSettings
    { 
        public LayerMask layermask;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public Shader maskShader;
    }

    public MaskSettings settings;
    [SerializeField] SortingCriteria _sortingCriteria = SortingCriteria.None;
    Material maskMaterial;
    BloomMaskPass m_perObjectPass;
    const string PassTag = "Bloom Mask Pass";

    public override void Create()
    {
        if (!isActive) Shader.SetGlobalTexture("_PerObjectBloomMask", null);
        if (settings.maskShader != null) // to explain: why always null ?
        {
            maskMaterial = CoreUtils.CreateEngineMaterial(settings.maskShader);
        }
        else
        {
            //Debug.LogWarningFormat("Missing mask Shader");
            maskMaterial = CoreUtils.CreateEngineMaterial("Hidden/Internal-StencilWrite");
        }
        m_perObjectPass = new BloomMaskPass(PassTag, RenderQueueRange.all, _sortingCriteria, settings.layermask, maskMaterial);
        m_perObjectPass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_perObjectPass);
    }
    protected override void Dispose(bool disposing)
    {
        m_perObjectPass.Dispose();
    }
}


