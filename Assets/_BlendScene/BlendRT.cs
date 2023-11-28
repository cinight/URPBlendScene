using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu]
public class BlendRT : ScriptableRendererFeature
{
	public Material material;
	
	public BlendRT()
	{
	}

	public override void Create()
	{
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(material == null)
            return;
        
        var evt = RenderPassEvent.AfterRenderingGbuffer;
        
        var pass = new BlendRTPass(evt,material);
        renderer.EnqueuePass(pass);
    }
    
    protected override void Dispose(bool disposing)
    {
        RTCollection.CleanUp();
    }

    //-------------------------------------------------------------------------

	class BlendRTPass : ScriptableRenderPass
	{
		private Material m_Material;
        private string k_SrcName1 = "_Src1";
        private string k_SrcName2 = "_Src2";
        private static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);
        private int m_ShadowMainId = -1;
        private int m_ShadowAddId = -1;

        public BlendRTPass(RenderPassEvent evt, Material mat)
        {
            renderPassEvent = evt;
            m_Material = mat;
            
            m_ShadowMainId = Shader.PropertyToID("_MainLightShadowmapTexture");
            m_ShadowAddId = Shader.PropertyToID("_AdditionalLightsShadowmapTexture");
        }
        
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            
            //shouldn't blit from the backbuffer
            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            //null check
            if (m_Material == null)
                return;
            
            //Setup builder
            var desc = cameraData.cameraTargetDescriptor;
            
            //if (RTCollection.blended == null) RTCollection.blended = new CamBufferSet();
            resourceData.gBuffer[0] = SetupBuilder(renderGraph, ref RTCollection.cam1.GBuffer0, ref RTCollection.cam2.GBuffer0, "BlendRT_GBuffer0");
            resourceData.gBuffer[1] = SetupBuilder(renderGraph, ref RTCollection.cam1.GBuffer1, ref RTCollection.cam2.GBuffer1, "BlendRT_GBuffer1");
            resourceData.gBuffer[2] = SetupBuilder(renderGraph, ref RTCollection.cam1.GBuffer2, ref RTCollection.cam2.GBuffer2, "BlendRT_GBuffer2");
            resourceData.gBuffer[3] = SetupBuilder(renderGraph, ref RTCollection.cam1.GBuffer3, ref RTCollection.cam2.GBuffer3, "BlendRT_GBuffer3");
            resourceData.gBuffer[4] = SetupBuilder(renderGraph, ref RTCollection.cam1.GBuffer4, ref RTCollection.cam2.GBuffer4, "BlendRT_GBuffer4");
            resourceData.mainShadowsTexture = SetupBuilder(renderGraph, ref RTCollection.cam1.ShadowMain, ref RTCollection.cam2.ShadowMain, "BlendRT_ShadowMain",m_ShadowMainId);
            resourceData.additionalShadowsTexture = SetupBuilder(renderGraph, ref RTCollection.cam1.ShadowAdd, ref RTCollection.cam2.ShadowAdd, "BlendRT_ShadowAdd",m_ShadowAddId);
        }

        private class PassData
        {
            internal TextureHandle src1;
            internal TextureHandle src2;
            internal Material mat;
        }
        private TextureHandle SetupBuilder(RenderGraph rg, ref RTSet srcRT1, ref RTSet srcRT2, string passName, int urpTextureId = -1)
        {
            //Create RT
            TextureHandle src1 = rg.ImportTexture(srcRT1.rt);
            TextureHandle src2 = rg.ImportTexture(srcRT2.rt);
            TextureHandle dest = UniversalRenderer.CreateRenderGraphTexture(rg, srcRT1.desc, passName, false);
            
            //To avoid error from material preview in the scene
            if(!src1.IsValid() || !src2.IsValid() || !dest.IsValid())
                return TextureHandle.nullHandle;

            //Builder
            using (var builder = rg.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                //setup pass data
                passData.src1 = src1;
                passData.src2 = src2;
                passData.mat = m_Material;
                
                //setup builder
                builder.UseTexture(passData.src1);
                builder.UseTexture(passData.src2);
                builder.SetRenderAttachment(dest,0);
                builder.AllowPassCulling(false);
                builder.AllowGlobalStateModification(true); //in order to set global texture before blit
                //For shadow texture we override original resource with global texture
                if(urpTextureId >= 0) builder.SetGlobalTextureAfterPass(dest,urpTextureId);
                
                //render function
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    //data.mat.SetTexture(k_SrcName1, data.src1);
                    //data.mat.SetTexture(k_SrcName2, data.src2);
                    context.cmd.SetGlobalTexture(k_SrcName1, data.src1);
                    context.cmd.SetGlobalTexture(k_SrcName2, data.src2);
                    Blitter.BlitTexture(context.cmd, data.src1, scaleBias, data.mat, 0);
                });
            }
            return dest;
        }
	}
}