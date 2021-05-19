using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu]
public class CollectGBuffer : ScriptableRendererFeature
{
    public string[] nameList;
    public Material[] materialList;
    public GraphicsFormat[] overrideFormat;
    
	public CollectGBuffer()
	{
	}

	public override void Create()
	{
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //var cameraColorTarget = renderer.cameraColorTarget;
        var pass = new CollectGBufferPass(materialList,nameList,overrideFormat);
        renderer.EnqueuePass(pass);
    }

    //-------------------------------------------------------------------------

	class CollectGBufferPass : ScriptableRenderPass
	{
        private string[] nameList;
        private Material[] materialList;
        private RenderTargetIdentifier[] m_RTList;
        private int[] m_RTnameList;
        private GraphicsFormat[] overrideFormat;

        public CollectGBufferPass(Material[] matL, string[] nameL, GraphicsFormat[] formats)
        {
            this.materialList = matL;
            this.nameList = nameL;
            this.renderPassEvent = RenderPassEvent.AfterRenderingGbuffer;
            this.overrideFormat = formats;

            this.m_RTnameList = new int[nameList.Length];
            this.m_RTList = new RenderTargetIdentifier[nameList.Length];
            for(int i=0; i<nameList.Length; i++)
            {
                m_RTnameList[i] = Shader.PropertyToID(nameList[i]);
                m_RTList[i] = new RenderTargetIdentifier(m_RTnameList[i]);
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
        {
            for(int i=0; i<m_RTnameList.Length; i++)
            {
                RenderTextureDescriptor descriptor = cameraTextureDescripor;
                if(overrideFormat[i] != GraphicsFormat.None) descriptor.graphicsFormat = overrideFormat[i];
                cmd.GetTemporaryRT(m_RTnameList[i], descriptor);
            }
            RenderPipelineManager.endFrameRendering += OnEndFrameRendering;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            for(int i=0; i<materialList.Length; i++)
            {
                if( materialList[i] == null) return;
            }

            var camera = renderingData.cameraData.camera;

			CommandBuffer cmd = CommandBufferPool.Get(camera.name + " - CollectRTContentPass");
            
            //DeferredLights m_DeferredLights 
            //RenderTargetHandle[] gbufferAttachments = m_DeferredLights.GbufferAttachments;

            for(int i=0; i<materialList.Length; i++)
            {
                cmd.Blit( renderingData.cameraData.renderer.cameraColorTarget , m_RTList[i] , materialList[i]);
                cmd.SetGlobalTexture(m_RTnameList[i],m_RTList[i]);
            }

            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

        void OnEndFrameRendering(ScriptableRenderContext context, Camera[] cameras)
        {
            CommandBuffer cmd = CommandBufferPool.Get("CleanUp Temp RTs");
            for(int i=0; i<m_RTnameList.Length; i++)
            {
                cmd.ReleaseTemporaryRT(m_RTnameList[i]);
            }
            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
            RenderPipelineManager.endFrameRendering -= OnEndFrameRendering;
        }

		// public override void FrameCleanup(CommandBuffer cmd)
		// {
		// 	if (cmd == null) throw new ArgumentNullException("cmd");

        //     for(int i=0; i<m_RTnameList.Length; i++)
        //     {
        //         //cmd.ReleaseTemporaryRT(m_RTnameList[i]);
        //     }
        
		// 	base.FrameCleanup(cmd);
		// }
	}
}