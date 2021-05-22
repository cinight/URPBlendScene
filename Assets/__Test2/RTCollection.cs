using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class RTCollection
{
    public static bool initialized = false;

    public static int cam1_GBuffer0Id = Shader.PropertyToID("Cam1_GBuffer0");
    public static int cam1_GBuffer1Id = Shader.PropertyToID("Cam1_GBuffer1");
    public static int cam1_GBuffer2Id = Shader.PropertyToID("Cam1_GBuffer2");
    public static int cam1_GBuffer3Id = Shader.PropertyToID("Cam1_GBuffer3");
    public static int cam1_DepthId = Shader.PropertyToID("Cam1_CameraDepthTexture");
    public static int cam1_ShadowMainId = Shader.PropertyToID("Cam1_ShadowTexture");
    public static int cam1_ShadowAddId = Shader.PropertyToID("Cam1_ShadowAddTexture");

    public static int cam2_GBuffer0Id = Shader.PropertyToID("Cam2_GBuffer0");
    public static int cam2_GBuffer1Id = Shader.PropertyToID("Cam2_GBuffer1");
    public static int cam2_GBuffer2Id = Shader.PropertyToID("Cam2_GBuffer2");
    public static int cam2_GBuffer3Id = Shader.PropertyToID("Cam2_GBuffer3");
    public static int cam2_DepthId = Shader.PropertyToID("Cam2_CameraDepthTexture");
    public static int cam2_ShadowMainId = Shader.PropertyToID("Cam2_ShadowTexture");
    public static int cam2_ShadowAddId = Shader.PropertyToID("Cam2_ShadowAddTexture");

    public static int Blended_GBuffer0Id = Shader.PropertyToID("Blended_GBuffer0");
    public static int Blended_GBuffer1Id = Shader.PropertyToID("Blended_GBuffer1");
    public static int Blended_GBuffer2Id = Shader.PropertyToID("Blended_GBuffer2");
    public static int Blended_GBuffer3Id = Shader.PropertyToID("Blended_GBuffer3");
    public static int Blended_DepthId = Shader.PropertyToID("Blended_CameraDepthTexture");
    public static int Blended_ShadowMainId = Shader.PropertyToID("Blended_ShadowTexture");
    public static int Blended_ShadowAddId = Shader.PropertyToID("Blended_ShadowAddTexture");

    public static RenderTargetIdentifier cam1_GBuffer0 = new RenderTargetIdentifier(cam1_GBuffer0Id);
    public static RenderTargetIdentifier cam1_GBuffer1 = new RenderTargetIdentifier(cam1_GBuffer1Id);
    public static RenderTargetIdentifier cam1_GBuffer2 = new RenderTargetIdentifier(cam1_GBuffer2Id);
    public static RenderTargetIdentifier cam1_GBuffer3 = new RenderTargetIdentifier(cam1_GBuffer3Id);
    public static RenderTargetIdentifier cam1_Depth = new RenderTargetIdentifier(cam1_DepthId);
    public static RenderTargetIdentifier cam1_ShadowMain = new RenderTargetIdentifier(cam1_ShadowMainId);
    public static RenderTargetIdentifier cam1_ShadowAdd = new RenderTargetIdentifier(cam1_ShadowAddId);

    public static RenderTargetIdentifier cam2_GBuffer0 = new RenderTargetIdentifier(cam2_GBuffer0Id);
    public static RenderTargetIdentifier cam2_GBuffer1 = new RenderTargetIdentifier(cam2_GBuffer1Id);
    public static RenderTargetIdentifier cam2_GBuffer2 = new RenderTargetIdentifier(cam2_GBuffer2Id);
    public static RenderTargetIdentifier cam2_GBuffer3 = new RenderTargetIdentifier(cam2_GBuffer3Id);
    public static RenderTargetIdentifier cam2_Depth = new RenderTargetIdentifier(cam2_DepthId);
    public static RenderTargetIdentifier cam2_ShadowMain = new RenderTargetIdentifier(cam2_ShadowMainId);
    public static RenderTargetIdentifier cam2_ShadowAdd = new RenderTargetIdentifier(cam2_ShadowAddId);

    public static RenderTargetIdentifier Blended_GBuffer0 = new RenderTargetIdentifier(Blended_GBuffer0Id);
    public static RenderTargetIdentifier Blended_GBuffer1 = new RenderTargetIdentifier(Blended_GBuffer1Id);
    public static RenderTargetIdentifier Blended_GBuffer2 = new RenderTargetIdentifier(Blended_GBuffer2Id);
    public static RenderTargetIdentifier Blended_GBuffer3 = new RenderTargetIdentifier(Blended_GBuffer3Id);
    public static RenderTargetIdentifier Blended_Depth = new RenderTargetIdentifier(Blended_DepthId);
    public static RenderTargetIdentifier Blended_ShadowMain = new RenderTargetIdentifier(Blended_ShadowMainId);
    public static RenderTargetIdentifier Blended_ShadowAdd = new RenderTargetIdentifier(Blended_ShadowAddId);

    public static Material mat_Collect_GBuffer0;
    public static Material mat_Collect_GBuffer1;
    public static Material mat_Collect_GBuffer2;
    public static Material mat_Collect_GBuffer3;
    public static Material mat_Collect_Depth;
    public static Material mat_Collect_ShadowMain;
    public static Material mat_Collect_ShadowAdd;

    public static Material mat_Blend_GBuffer0;
    public static Material mat_Blend_GBuffer1;
    public static Material mat_Blend_GBuffer2;
    public static Material mat_Blend_GBuffer3;
    public static Material mat_Blend_Depth;
    public static Material mat_Blend_ShadowMain;
    public static Material mat_Blend_ShadowAdd;

    public static void ConfigureRT( CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor )
    {
        if(!initialized)
        {
            RenderTextureDescriptor descriptor = cameraTextureDescriptor;
            descriptor.depthBufferBits = 24;

            //GBuffer0
            descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
            cmd.GetTemporaryRT(cam1_GBuffer0Id, descriptor);
            cmd.GetTemporaryRT(cam2_GBuffer0Id, descriptor);
            cmd.GetTemporaryRT(Blended_GBuffer0Id, descriptor);

            //GBuffer1
            descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
            cmd.GetTemporaryRT(cam1_GBuffer1Id, descriptor);
            cmd.GetTemporaryRT(cam2_GBuffer1Id, descriptor);
            cmd.GetTemporaryRT(Blended_GBuffer1Id, descriptor);

            //GBuffer2
            descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_SNorm;
            cmd.GetTemporaryRT(cam1_GBuffer2Id, descriptor);
            cmd.GetTemporaryRT(cam2_GBuffer2Id, descriptor);
            cmd.GetTemporaryRT(Blended_GBuffer2Id, descriptor);

            //GBuffer3
            descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
            cmd.GetTemporaryRT(cam1_GBuffer3Id, descriptor);
            cmd.GetTemporaryRT(cam2_GBuffer3Id, descriptor);
            cmd.GetTemporaryRT(Blended_GBuffer3Id, descriptor);

            //Depth
            descriptor.graphicsFormat = GraphicsFormat.DepthAuto;
            cmd.GetTemporaryRT(cam1_DepthId, descriptor);
            cmd.GetTemporaryRT(cam2_DepthId, descriptor);
            cmd.GetTemporaryRT(Blended_DepthId, descriptor);

            //Shadow
            descriptor.graphicsFormat = GraphicsFormat.ShadowAuto;
            descriptor.width = 2048;
            descriptor.height = 1024;
            cmd.GetTemporaryRT(cam1_ShadowMainId, descriptor);
            cmd.GetTemporaryRT(cam1_ShadowAddId, descriptor);
            cmd.GetTemporaryRT(cam2_ShadowMainId, descriptor);
            cmd.GetTemporaryRT(cam2_ShadowAddId, descriptor);
            cmd.GetTemporaryRT(Blended_ShadowMainId, descriptor);
            cmd.GetTemporaryRT(Blended_ShadowAddId, descriptor);

            //Register cleanup
            RenderPipelineManager.endFrameRendering += CleanUpRT;

            initialized = true;
        }

        if(mat_Collect_GBuffer0 == null) mat_Collect_GBuffer0 = new Material(Shader.Find("Custom/CollectRT_GBuffer0"));
        if(mat_Collect_GBuffer1 == null) mat_Collect_GBuffer1 = new Material(Shader.Find("Custom/CollectRT_GBuffer1"));
        if(mat_Collect_GBuffer2 == null) mat_Collect_GBuffer2 = new Material(Shader.Find("Custom/CollectRT_GBuffer2"));
        if(mat_Collect_GBuffer3 == null) mat_Collect_GBuffer3 = new Material(Shader.Find("Custom/CollectRT_GBuffer3"));
        if(mat_Collect_Depth == null) mat_Collect_Depth = new Material(Shader.Find("Custom/CollectRT_Depth"));
        if(mat_Collect_ShadowMain == null) mat_Collect_ShadowMain = new Material(Shader.Find("Custom/CollectRT_Shadow"));
        if(mat_Collect_ShadowAdd == null) mat_Collect_ShadowAdd = new Material(Shader.Find("Custom/CollectRT_ShadowAdd"));

        if(mat_Blend_GBuffer0 == null) mat_Blend_GBuffer0 = new Material(Shader.Find("Custom/BlendRT_GBuffer0"));
        if(mat_Blend_GBuffer1 == null) mat_Blend_GBuffer1 = new Material(Shader.Find("Custom/BlendRT_GBuffer1"));
        if(mat_Blend_GBuffer2 == null) mat_Blend_GBuffer2 = new Material(Shader.Find("Custom/BlendRT_GBuffer2"));
        if(mat_Blend_GBuffer3 == null) mat_Blend_GBuffer3 = new Material(Shader.Find("Custom/BlendRT_GBuffer3"));
        if(mat_Blend_Depth == null) mat_Blend_Depth = new Material(Shader.Find("Custom/BlendRT_Depth"));
        if(mat_Blend_ShadowMain == null) mat_Blend_ShadowMain = new Material(Shader.Find("Custom/BlendRT_Shadow"));
        if(mat_Blend_ShadowAdd == null) mat_Blend_ShadowAdd = new Material(Shader.Find("Custom/BlendRT_ShadowAdd"));
    }

    public static void BlendMaterial(float blend)
    {
        if(mat_Blend_GBuffer0 != null) mat_Blend_GBuffer0.SetFloat("_Blend",blend);
        if(mat_Blend_GBuffer1 != null) mat_Blend_GBuffer1.SetFloat("_Blend",blend);
        if(mat_Blend_GBuffer2 != null) mat_Blend_GBuffer2.SetFloat("_Blend",blend);
        if(mat_Blend_GBuffer3 != null) mat_Blend_GBuffer3.SetFloat("_Blend",blend);
        if(mat_Blend_Depth != null) mat_Blend_Depth.SetFloat("_Blend",blend);
        if(mat_Blend_ShadowMain != null) mat_Blend_ShadowMain.SetFloat("_Blend",blend);
        if(mat_Blend_ShadowAdd != null) mat_Blend_ShadowAdd.SetFloat("_Blend",blend);
    }

    public static void CleanUpRT (ScriptableRenderContext context, Camera[] cameras)
    {
        CommandBuffer cmd = CommandBufferPool.Get("CleanUp Temp RTs");

        cmd.ReleaseTemporaryRT(cam1_GBuffer0Id);
        cmd.ReleaseTemporaryRT(cam1_GBuffer1Id);
        cmd.ReleaseTemporaryRT(cam1_GBuffer2Id);
        cmd.ReleaseTemporaryRT(cam1_GBuffer3Id);
        cmd.ReleaseTemporaryRT(cam1_DepthId);
        cmd.ReleaseTemporaryRT(cam1_ShadowMainId);
        cmd.ReleaseTemporaryRT(cam1_ShadowAddId);

        cmd.ReleaseTemporaryRT(cam2_GBuffer0Id);
        cmd.ReleaseTemporaryRT(cam2_GBuffer1Id);
        cmd.ReleaseTemporaryRT(cam2_GBuffer2Id);
        cmd.ReleaseTemporaryRT(cam2_GBuffer3Id);
        cmd.ReleaseTemporaryRT(cam2_DepthId);
        cmd.ReleaseTemporaryRT(cam2_ShadowMainId);
        cmd.ReleaseTemporaryRT(cam2_ShadowAddId);

        cmd.ReleaseTemporaryRT(Blended_GBuffer0Id);
        cmd.ReleaseTemporaryRT(Blended_GBuffer1Id);
        cmd.ReleaseTemporaryRT(Blended_GBuffer2Id);
        cmd.ReleaseTemporaryRT(Blended_GBuffer3Id);
        cmd.ReleaseTemporaryRT(Blended_DepthId);
        cmd.ReleaseTemporaryRT(Blended_ShadowMainId);
        cmd.ReleaseTemporaryRT(Blended_ShadowAddId);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);

        initialized = false;
        
        RenderPipelineManager.endFrameRendering -= CleanUpRT;
    }
}

