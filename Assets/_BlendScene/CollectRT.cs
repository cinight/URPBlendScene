using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu]
public class CollectRT : ScriptableRendererFeature
{
    public bool cam1 = true;

	public CollectRT()
	{
	}

	public override void Create()
	{
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var evt = RenderPassEvent.AfterRenderingGbuffer;
        
        var pass = new CollectRTPass(evt,cam1);
        renderer.EnqueuePass(pass);
    }
    
    protected override void Dispose(bool disposing)
    {
    }

    //-------------------------------------------------------------------------

	class CollectRTPass : ScriptableRenderPass
	{
        private bool m_IsCam1;
        private static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);

        public CollectRTPass(RenderPassEvent evt, bool cam1)
        {
            m_IsCam1 = cam1;
            renderPassEvent = evt;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            
            //shouldn't blit from the backbuffer
            if (resourceData.isActiveTargetBackBuffer)
                return;
            
            //Setup RTset
            string setName = m_IsCam1? "Cam1" : "Cam2";
            if (m_IsCam1)
            {
                if (RTCollection.cam1 == null) RTCollection.cam1 = new CamBufferSet();
            }
            else
            {
                if (RTCollection.cam2 == null) RTCollection.cam2 = new CamBufferSet();
            }
            CamBufferSet camSet = m_IsCam1 ? RTCollection.cam1 : RTCollection.cam2;
            
            //Setup builder
            var desc = cameraData.cameraTargetDescriptor;
            SetupBuilder(renderGraph, desc, resourceData.gBuffer[0], ref camSet.GBuffer0, setName+ "_CollectRT_GBuffer0");
            SetupBuilder(renderGraph, desc, resourceData.gBuffer[1], ref camSet.GBuffer1, setName+ "_CollectRT_GBuffer1");
            SetupBuilder(renderGraph, desc, resourceData.gBuffer[2], ref camSet.GBuffer2, setName+ "_CollectRT_GBuffer2");
            SetupBuilder(renderGraph, desc, resourceData.gBuffer[3], ref camSet.GBuffer3, setName+ "_CollectRT_GBuffer3");
            SetupBuilder(renderGraph, desc, resourceData.gBuffer[4], ref camSet.GBuffer4, setName+ "_CollectRT_GBuffer4");
            SetupBuilder(renderGraph, desc, resourceData.mainShadowsTexture, ref camSet.ShadowMain, setName+ "_CollectRT_ShadowMain");
            SetupBuilder(renderGraph, desc, resourceData.additionalShadowsTexture, ref camSet.ShadowAdd, setName+ "_CollectRT_ShadowAdd");
        }

        private class PassData
        {
            internal TextureHandle src;
        }
        private bool SetupBuilder(RenderGraph rg, RenderTextureDescriptor desc, TextureHandle src, ref RTSet destRT, string passName)
        {
            //Create RT
            RTCollection.AllocateRT(ref destRT, rg, ref src, desc, passName);
            TextureHandle dest = rg.ImportTexture(destRT.rt);
            
            //To avoid error from material preview in the scene
            if(!src.IsValid() || !dest.IsValid())
                return false;

            //Builder
            using (var builder = rg.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                //setup pass data
                passData.src = src;
                
                //setup builder
                builder.UseTexture(passData.src);
                builder.SetRenderAttachment(dest,0);
                builder.AllowPassCulling(false);
                
                //render function
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.src, scaleBias, 0, false);
                });
            }

            return true;
        }
	}
}