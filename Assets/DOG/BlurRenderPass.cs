using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRenderPass : ScriptableRenderPass
{
    Material material;
    BlurSettings blurSettings;

    RenderTargetIdentifier source;
    RenderTargetHandle blurTex;
    int blurTexID;

    RenderTargetHandle blurTex2;
    int blurTexID2;

    RenderTargetHandle firstOut;
    int firstOutID;

    RenderTargetHandle secondOut;
    int secondOutID;

    public bool Setup(ScriptableRenderer renderer)
    {
        source = renderer.cameraColorTarget;
        blurSettings = VolumeManager.instance.stack.GetComponent<BlurSettings>();
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        if(blurSettings != null && blurSettings.IsActive())
        {
            material = new Material(Shader.Find("PostProcessing/Blur"));
            return true;
        }

        return false;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        if(blurSettings == null || !blurSettings.IsActive())
        {
            return;
        }

        blurTexID = Shader.PropertyToID("_BlurTex");
        blurTex = new RenderTargetHandle();
        blurTex.id = blurTexID;
        cmd.GetTemporaryRT(blurTex.id, cameraTextureDescriptor);

        blurTexID2 = Shader.PropertyToID("_BlurTex2");
        blurTex2 = new RenderTargetHandle();
        blurTex2.id = blurTexID2;
        cmd.GetTemporaryRT(blurTex2.id, cameraTextureDescriptor);

        firstOutID = Shader.PropertyToID("_BlurTex3");
        firstOut = new RenderTargetHandle();
        firstOut.id = firstOutID;
        cmd.GetTemporaryRT(firstOut.id, cameraTextureDescriptor);

        secondOutID = Shader.PropertyToID("_BlurTex4");
        secondOut = new RenderTargetHandle();
        secondOut.id = secondOutID;
        cmd.GetTemporaryRT(secondOut.id, cameraTextureDescriptor);

        base.Configure(cmd, cameraTextureDescriptor);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (blurSettings == null || !blurSettings.IsActive())
        {
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("Blur Post Process");

        int gridSize = Mathf.CeilToInt(blurSettings.strength.value * 6.0f);

        if (gridSize % 2 == 0)
        {
            gridSize++;
        }

        material.SetInteger("_GridSize", gridSize);

        // First Blur
        material.SetFloat("_Spread", (float)blurSettings.strength.value);
        cmd.Blit(source, blurTex.id, material, 0);
        cmd.Blit(blurTex.id, firstOut.id, material, 1);

        // Second Blur
        material.SetFloat("_Spread", (float)blurSettings.second.value);
        cmd.Blit(firstOut.id, blurTex2.id, material, 0);
        cmd.Blit(blurTex2.id, secondOut.id, material, 1);

        // Subtract Second Blur From First Blur //
        cmd.SetGlobalTexture("_FirstBlurTex", firstOut.id);
        cmd.SetGlobalTexture("_SecondBlurTex", secondOut.id);
        material.SetTexture("_H", blurSettings.hatch.value);
        cmd.Blit(source, source, material, 2);

        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);

    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(blurTexID);
        cmd.ReleaseTemporaryRT(blurTexID2);
        cmd.ReleaseTemporaryRT(firstOutID);
        cmd.ReleaseTemporaryRT(secondOutID);
        base.FrameCleanup(cmd);
    }
}
