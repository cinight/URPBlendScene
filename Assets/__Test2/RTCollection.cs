using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class RTCollection
{
    public static bool initialized = false;

    public static RTSet cam1_GBuffer0;
    public static RTSet cam1_GBuffer1;
    public static RTSet cam1_GBuffer2;
    public static RTSet cam1_GBuffer3;
    public static RTSet cam1_Depth;
    public static RTSet cam1_ShadowMain;
    public static RTSet cam1_ShadowAdd;

    public static RTSet cam2_GBuffer0;
    public static RTSet cam2_GBuffer1;
    public static RTSet cam2_GBuffer2;
    public static RTSet cam2_GBuffer3;
    public static RTSet cam2_Depth;
    public static RTSet cam2_ShadowMain;
    public static RTSet cam2_ShadowAdd;

    public static RTSet Blended_GBuffer0;
    public static RTSet Blended_GBuffer1;
    public static RTSet Blended_GBuffer2;
    public static RTSet Blended_GBuffer3;
    public static RTSet Blended_Depth;
    public static RTSet Blended_ShadowMain;
    public static RTSet Blended_ShadowAdd;

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
            cam1_GBuffer0 = new RTSet( "Cam1_GBuffer0" , descriptor);
            cam2_GBuffer0 = new RTSet( "Cam2_GBuffer0" , descriptor);
            Blended_GBuffer0 = new RTSet( "Blended_GBuffer0" , descriptor);

            //GBuffer1
            descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
            cam1_GBuffer1 = new RTSet( "Cam1_GBuffer1" , descriptor);
            cam2_GBuffer1 = new RTSet( "Cam2_GBuffer1" , descriptor);
            Blended_GBuffer1 = new RTSet( "Blended_GBuffer1" , descriptor);

            //GBuffer2
            descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_SNorm;
            cam1_GBuffer2 = new RTSet( "Cam1_GBuffer2" , descriptor);
            cam2_GBuffer2 = new RTSet( "Cam2_GBuffer2" , descriptor);
            Blended_GBuffer2 = new RTSet( "Blended_GBuffer2" , descriptor);

            //GBuffer3
            descriptor.graphicsFormat = GraphicsFormat.B10G11R11_UFloatPack32;
            cam1_GBuffer3 = new RTSet( "Cam1_GBuffer3" , descriptor);
            cam2_GBuffer3 = new RTSet( "Cam2_GBuffer3" , descriptor);
            Blended_GBuffer3 = new RTSet( "Blended_GBuffer3" , descriptor);

            //Depth
            descriptor.graphicsFormat = GraphicsFormat.None;
            cam1_Depth = new RTSet( "Cam1_CameraDepthTexture" , descriptor);
            cam2_Depth = new RTSet( "Cam2_CameraDepthTexture" , descriptor);
            Blended_Depth = new RTSet( "Blended_CameraDepthTexture" , descriptor);

            //Shadow
            descriptor.graphicsFormat = GraphicsFormat.None;
            descriptor.width = 2048;
            descriptor.height = 1024;
            cam1_ShadowMain = new RTSet( "Cam1_ShadowTexture" , descriptor);
            cam1_ShadowAdd = new RTSet( "Cam1_ShadowAddTexture" , descriptor);
            cam2_ShadowMain = new RTSet( "Cam2_ShadowTexture" , descriptor);
            cam2_ShadowAdd = new RTSet( "Cam2_ShadowAddTexture" , descriptor);
            Blended_ShadowMain = new RTSet( "Blended_ShadowTexture" , descriptor);
            Blended_ShadowAdd = new RTSet( "Blended_ShadowAddTexture" , descriptor);

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
        cam1_GBuffer0.Cleanup();
        cam1_GBuffer1.Cleanup();
        cam1_GBuffer2.Cleanup();
        cam1_GBuffer3.Cleanup();
        cam1_Depth.Cleanup();
        cam1_ShadowMain.Cleanup();
        cam1_ShadowAdd.Cleanup();

        cam2_GBuffer0.Cleanup();
        cam2_GBuffer1.Cleanup();
        cam2_GBuffer2.Cleanup();
        cam2_GBuffer3.Cleanup();
        cam2_Depth.Cleanup();
        cam2_ShadowMain.Cleanup();
        cam2_ShadowAdd.Cleanup();

        Blended_GBuffer0.Cleanup();
        Blended_GBuffer1.Cleanup();
        Blended_GBuffer2.Cleanup();
        Blended_GBuffer3.Cleanup();
        Blended_Depth.Cleanup();
        Blended_ShadowMain.Cleanup();
        Blended_ShadowAdd.Cleanup();

        initialized = false;
        
        RenderPipelineManager.endFrameRendering -= CleanUpRT;
    }
}

public class RTSet
{
    public string name;
    public int nameId;
    public RenderTexture tex;

    public RTSet(string n, RenderTextureDescriptor desc)
    {
        name = n;
        nameId = Shader.PropertyToID(name);
        tex = RenderTexture.GetTemporary(desc);
        tex.name = name;
    }

    public void Cleanup()
    {
        RenderTexture.ReleaseTemporary(tex);
    }
}

