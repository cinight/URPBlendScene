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
    public static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);

    //Use in CollectRT pass
    public static bool AllocateRT(ref RTSet rtset, RenderGraph rg, ref TextureHandle src, RenderTextureDescriptor desc, string name, bool forDepth, bool forShadow)
    {
        var texDesc = rg.GetTextureDesc(src);
        //Debug.Log(name + " format=" + desc.graphicsFormat + " depthStencil=" + desc.depthStencilFormat + " " +desc.width + "x" + desc.height + " bit=" + desc.depthBufferBits+ " tex=" + texDesc.colorFormat + " "+ texDesc.width + "x" + texDesc.height + " bit=" + texDesc.depthBufferBits);
        if(texDesc.colorFormat != GraphicsFormat.None) desc.graphicsFormat = texDesc.colorFormat;
        //if (forShadow) desc.depthStencilFormat = GraphicsFormat.D16_UNorm;
        if(!forDepth && texDesc.width > 0) desc.width = texDesc.width;
        if(!forDepth && texDesc.height > 0) desc.height = texDesc.height;
        if(!forDepth && (!forShadow || texDesc.depthBufferBits == DepthBits.None) ) desc.depthBufferBits = 0;
        rtset.desc = desc;
        RenderingUtils.ReAllocateIfNeeded(ref rtset.rt, desc, texDesc.filterMode, texDesc.wrapMode, isShadowMap: forShadow, name: name);
        
        // true if the RT is depth (shadow texture requires depth), need special handling in CollectRT pass
        return desc.depthBufferBits != 0;
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
    public RTSet GBuffer4;
    public RTSet Depth;
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
        Depth.rt?.Release();
        ShadowMain.rt?.Release();
        ShadowAdd.rt?.Release();
    }
}

