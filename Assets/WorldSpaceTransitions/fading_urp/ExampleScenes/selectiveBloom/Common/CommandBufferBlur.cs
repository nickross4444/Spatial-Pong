using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

// Set up a CommandBuffer to do a blur
public class CommandBufferBlur
{
    private Material m_Material;

    public CommandBufferBlur(int blurrStrength)
    {
        m_Material = CoreUtils.CreateEngineMaterial("Hidden/GlassBlur");
        m_Material.hideFlags = HideFlags.HideAndDontSave;
        m_Material.SetInt("_BlurStrength", blurrStrength);
    }

    public CommandBufferBlur()
    {
        m_Material = CoreUtils.CreateEngineMaterial("Hidden/GlassBlur");
        m_Material.hideFlags = HideFlags.HideAndDontSave;
        //m_Material.SetInt("_BlurStrength", blurrStrength);
    }

    public void SetupCommandBuffer( CommandBuffer cmd, RTHandle blurTemp1, RTHandle blurTemp2 )
    {
        /*
        // horizontal blur
        cmd.SetGlobalVector("offsets", new Vector4(1.0f, 0, 0, 0));
        Blitter.BlitCameraTexture(cmd, blurTemp1, blurTemp2, m_Material,2);
        // vertical blur
        cmd.SetGlobalVector("offsets", new Vector4(0, 1.0f, 0, 0));
        Blitter.BlitCameraTexture(cmd, blurTemp2, blurTemp1, m_Material, 2);
        // horizontal blur
        cmd.SetGlobalVector("offsets", new Vector4(2.0f, 0, 0, 0));
        Blitter.BlitCameraTexture(cmd, blurTemp1, blurTemp2, m_Material, 2);
        // vertical blur
        cmd.SetGlobalVector("offsets", new Vector4(0, 2.0f, 0, 0));
        Blitter.BlitCameraTexture(cmd, blurTemp2, blurTemp1, m_Material, 2);
        */
        Blitter.BlitCameraTexture(cmd, blurTemp1, blurTemp2, m_Material, 0);
        Blitter.BlitCameraTexture(cmd, blurTemp2, blurTemp1, m_Material, 1);
    }
}
