using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

[CreateAssetMenu]
public class CollectRT : ScriptableRendererFeature
{
    public bool cam1 = true;
    public Shader copyDepthShader;
    private CollectRTPass m_pass;
    
    //Have to be in seperate passes otherwise the blit source will conflict
    private CameraDepthToTexPass m_CollectShadowMainPass;
    private CameraDepthToTexPass m_CollectShadowAddPass;

	public CollectRT()
	{
	}

	public override void Create()
	{
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var evt = RenderPassEvent.AfterRenderingGbuffer;
        
        m_pass = new CollectRTPass(evt,cam1);
        m_CollectShadowMainPass = new CameraDepthToTexPass(cam1,true, evt, copyDepthShader, false, true,false);
        m_CollectShadowAddPass = new CameraDepthToTexPass(cam1,false, evt, copyDepthShader, false, true,false);
        renderer.EnqueuePass(m_pass);
        renderer.EnqueuePass(m_CollectShadowMainPass);
        renderer.EnqueuePass(m_CollectShadowAddPass);
    }
    
    protected override void Dispose(bool disposing)
    {
        m_CollectShadowMainPass?.Dispose();
        m_CollectShadowAddPass?.Dispose();
    }

    //-------------------------------------------------------------------------

	class CollectRTPass : ScriptableRenderPass
	{
        private bool m_IsCam1;

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
        }

        private class CollectRTPassData
        {
            internal TextureHandle src;
        }
        private bool SetupBuilder(RenderGraph rg, RenderTextureDescriptor desc, TextureHandle src, ref RTSet destRT, string passName)
        {
            //Create RT
            RTCollection.AllocateRT(ref destRT, rg, ref src, desc, passName, false); //Material preview triggers depth so we need to force no depth
            TextureHandle dest = rg.ImportTexture(destRT.rt);
            
            //To avoid error from material preview in the scene
            if(!src.IsValid() || !dest.IsValid())
                return false;
            
            //Builder
            using (var builder = rg.AddRasterRenderPass<CollectRTPassData>(passName, out var passData))
            {
                //setup pass data
                passData.src = src;
            
                //setup builder
                builder.UseTexture(passData.src);
                builder.SetRenderAttachment(dest,0);
                builder.AllowPassCulling(false);
            
                //render function
                builder.SetRenderFunc((CollectRTPassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.src, RTCollection.scaleBias, 0, false);
                });
            }

            return true;
        }
    }
    
    //-------------------------------------------------------------------------
    
    class CameraDepthToTexPass :CopyDepthPass
    {
        private bool m_IsCam1;
        private bool m_IsShadowMain;
        public CameraDepthToTexPass(bool cam1, bool shadowMain, RenderPassEvent evt, Shader copyDepthShader, bool shouldClear = false,
            bool copyToDepth = false, bool copyResolvedDepth = false)
            : base(evt, copyDepthShader, shouldClear, copyToDepth, copyResolvedDepth)
        {
            m_IsCam1 = cam1;
            m_IsShadowMain = shadowMain;
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
            CamBufferSet camSet = m_IsCam1 ? RTCollection.cam1 : RTCollection.cam2;
            
            //Setup builder
            var desc = cameraData.cameraTargetDescriptor;
            if (m_IsShadowMain)
            {
                SetupBuilder(renderGraph, desc, resourceData.mainShadowsTexture, ref camSet.ShadowMain, setName+ "_CollectRT_ShadowMain", resourceData, cameraData);
            }
            else
            {
                SetupBuilder(renderGraph, desc, resourceData.additionalShadowsTexture, ref camSet.ShadowAdd, setName+ "_CollectRT_ShadowAdd", resourceData, cameraData);
            }
        }

        private void SetupBuilder(RenderGraph rg, RenderTextureDescriptor desc, TextureHandle src, ref RTSet destRT, string passName, UniversalResourceData resourceData, UniversalCameraData cameraData)
        {
            //Create RT
            RTCollection.AllocateRT(ref destRT, rg, ref src, desc, passName,true);
            TextureHandle dest = rg.ImportTexture(destRT.rt);

            //To avoid error from material preview in the scene
            if (!src.IsValid() || !dest.IsValid())
                return;
            
            Render(rg, dest, src, resourceData, cameraData, false);
        }
    }
}