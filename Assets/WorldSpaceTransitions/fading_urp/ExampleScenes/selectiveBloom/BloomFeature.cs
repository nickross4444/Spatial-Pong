using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloomFeature : ScriptableRendererFeature
{
    
    class BloomPass : ScriptableRenderPass
    {
        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("BloomPass");
        CommandBufferBlur m_Blur;
        private RTHandle m_BlurTemp1Handle, m_BlurTemp2Handle;// m_ColorHandle;
        Material m_MaskedBrightnessBlit;
        Material m_AdditiveBlit;
        BloomSettings settings;
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
            colorDesc.depthBufferBits = 0;

            RenderTextureDescriptor opaqueDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDescriptor.depthBufferBits = 0;
            opaqueDescriptor.height = opaqueDescriptor.height / settings.downsample;
            opaqueDescriptor.width = opaqueDescriptor.width / settings.downsample;

            // Set up temporary color buffer (for blit)
            RenderingUtils.ReAllocateIfNeeded(ref m_BlurTemp1Handle, Vector2.one / settings.downsample, opaqueDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_Temp1");
            RenderingUtils.ReAllocateIfNeeded(ref m_BlurTemp2Handle, Vector2.one / settings.downsample, opaqueDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_Temp2");
            ConfigureTarget(m_BlurTemp1Handle, m_BlurTemp2Handle);
            m_Blur = new CommandBufferBlur(settings.blurStrength);
        }

        public BloomPass(Material m_Masked, Material m_Additive, BloomSettings bs)
        {
            this.m_MaskedBrightnessBlit = m_Masked;
            this.m_AdditiveBlit = m_Additive;
            this.settings = bs;
        }

  
        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera.cameraType != CameraType.Game)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();

            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            // Copy all values about our brightness and inside our mask to a temp buffer
            m_MaskedBrightnessBlit.SetFloat("_BloomThreshold", settings.bloomThreshold);
            Blitter.BlitCameraTexture(cmd, source, m_BlurTemp1Handle, m_MaskedBrightnessBlit, 0);

            // Setup command for blurring the buffer
            m_Blur.SetupCommandBuffer(cmd, m_BlurTemp1Handle, m_BlurTemp2Handle);

            // Blit the blurred brightness back into the color buffer, optionally increasing the brightness
            m_AdditiveBlit.SetFloat("_AdditiveAmount", settings.bloomAmount);
            Blitter.BlitCameraTexture(cmd, m_BlurTemp1Handle, source, m_AdditiveBlit, 0);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public void Dispose()
        {
            m_BlurTemp1Handle?.Release();
            m_BlurTemp2Handle?.Release();
        }

    }

    [System.Serializable]
    public class BloomSettings
    {
        [Range(0f, 1f), Tooltip("Bloom Threshold")]
        public float bloomThreshold = 0.5f;

        [Range(0f, 5f), Tooltip("Bloom Amount")]

        public float bloomAmount = 1.1f;
        
        [Range(1, 5)]
        public int downsample = 1;

        [Range(0, 10)]
        public int blurStrength = 5;
    }

    public BloomSettings settings = new BloomSettings();
    Material m_MaskedBrightnessBlit;
    Material m_AdditiveBlit;
    BloomPass bloomPass;
    //RenderTargetHandle bloomTexture;

    public override void Create()
    {
        bloomPass = new BloomPass(m_MaskedBrightnessBlit, m_AdditiveBlit, settings);

        m_MaskedBrightnessBlit = CoreUtils.CreateEngineMaterial("Hidden/MaskedBrightnessBlit");
        m_MaskedBrightnessBlit.SetFloat("_BloomThreshold", settings.bloomThreshold);
        m_AdditiveBlit = CoreUtils.CreateEngineMaterial("Hidden/AdditiveBlit");
        m_AdditiveBlit.SetFloat("_AdditiveAmount", settings.bloomAmount);
        bloomPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //bloomPass.Setup(renderer.cameraColorTarget, RenderTargetHandle.CameraTarget);
        renderer.EnqueuePass(bloomPass);
    }

    protected override void Dispose(bool disposing)
    {
        bloomPass.Dispose();
    }
}


