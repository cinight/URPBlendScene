using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class RTCollection
{
    public static CamBufferSet cam1;
    public static CamBufferSet cam2;

    //Use in CollectRT pass
    public static void AllocateRT(ref RTSet rtset, RenderGraph rg, ref TextureHandle src, RenderTextureDescriptor desc, string name)
    {
        var texDesc = rg.GetTextureDesc(src);
        //Debug.Log(name + " " + desc.graphicsFormat +desc.width + " " + desc.height + " " + desc.depthBufferBits+ " tex = " + texDesc.colorFormat + " "+ texDesc.width + " " + texDesc.height + " " + texDesc.depthBufferBits);
        if(texDesc.colorFormat != GraphicsFormat.None) desc.graphicsFormat = texDesc.colorFormat;
        if(texDesc.width > 0) desc.width = texDesc.width;
        if(texDesc.height > 0) desc.height = texDesc.height;
        desc.depthBufferBits = 0;
        rtset.desc = desc;
        RenderingUtils.ReAllocateIfNeeded(ref rtset.rt, desc, FilterMode.Point, TextureWrapMode.Clamp, name: name);
    }
    
    //Use in BlendRT pass
    public static void AllocateRTWithDesc(ref RTSet rtset, RenderTextureDescriptor desc, string name)
    {
        RenderingUtils.ReAllocateIfNeeded(ref rtset.rt, desc, FilterMode.Point, TextureWrapMode.Clamp, name: name);
    }
    
    public static void CleanUp ()
    {
        if(cam1!=null) cam1.Cleanup();
        if(cam2!=null) cam2.Cleanup();
    }
}

public struct RTSet
{
    public RTHandle rt;
    public RenderTextureDescriptor desc;
}

public class CamBufferSet
{
    public RTSet GBuffer0;
    public RTSet GBuffer1;
    public RTSet GBuffer2;
    public RTSet GBuffer3;
    public RTSet GBuffer4; //Depth
    public RTSet ShadowMain;
    public RTSet ShadowAdd;

    public CamBufferSet()
    {
        
    }

    public void Cleanup()
    {
        GBuffer0.rt?.Release();
        GBuffer1.rt?.Release();
        GBuffer2.rt?.Release();
        GBuffer3.rt?.Release();
        GBuffer4.rt?.Release();
        ShadowMain.rt?.Release();
        ShadowAdd.rt?.Release();
    }
}

