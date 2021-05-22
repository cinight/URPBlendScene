using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

[CreateAssetMenu]
public class CollectGBuffer : ScriptableRendererFeature
{
    public string[] nameList;
    public Material[] materialList;
    public RTtype[] rtType;
    public Vector2Int[] rtSize;
    public bool mainCam;

    public enum RTtype
    {
        GBuffer0,
        GBuffer1,
        GBuffer2,
        GBuffer3,
        Depth,
        MainShadow,
        AddShadow
    }
    
	public CollectGBuffer()
	{
	}

	public override void Create()
	{
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var evt = RenderPassEvent.AfterRenderingGbuffer;
        //var cameraDepthTarget = renderer.cameraDepthTarget;
        
        var pass = new CollectGBufferPass(evt,materialList,nameList,rtType,rtSize,mainCam);
        renderer.EnqueuePass(pass);

        // if(copyDepth)
        // {
        //     var depthPass = new CopyDepthPass(evt,copyDepthMat);
        //     renderer.EnqueuePass(depthPass);
        // }
    }

    //-------------------------------------------------------------------------

	class CollectGBufferPass : ScriptableRenderPass
	{
        private string[] nameList;
        private Material[] materialList;
        private RenderTargetIdentifier[] m_RTList;
        private int[] m_RTnameList;
        private RTtype[] rtType;
        private bool mainCam;
        private Vector2Int[] rtSize;

        public CollectGBufferPass(RenderPassEvent evt, Material[] matL, string[] nameL, RTtype[] rttype, Vector2Int[] rtSize, bool mainCam)
        {
            this.rtSize = rtSize;
            this.mainCam = mainCam;
            this.materialList = matL;
            this.nameList = nameL;
            this.renderPassEvent = evt;
            this.rtType = rttype;

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
                switch(rtType[i])
                {
                    case RTtype.GBuffer0: descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB; break;
                    case RTtype.GBuffer1: descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm; break;
                    case RTtype.GBuffer2: descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_SNorm; break;
                    case RTtype.GBuffer3: descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm; break;
                    case RTtype.Depth: descriptor.graphicsFormat = GraphicsFormat.DepthAuto; break;
                    case RTtype.MainShadow: descriptor.graphicsFormat = GraphicsFormat.ShadowAuto; break;
                    case RTtype.AddShadow: descriptor.graphicsFormat = GraphicsFormat.ShadowAuto; break;
                }

                if(rtSize[i].x > 0 && rtSize[i].y > 0)
                {
                    descriptor.width = rtSize[i].x;
                    descriptor.height = rtSize[i].y;
                }

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

                if(mainCam && rtType[i] == RTtype.Depth)
                {
                    cmd.SetGlobalTexture("_CameraDepthTexture", m_RTList[i]);
                    cmd.Blit( m_RTList[i] , renderingData.cameraData.renderer.cameraDepthTarget , materialList[i] );
                }
                else if(mainCam && rtType[i] == RTtype.MainShadow)
                {
                    cmd.SetGlobalTexture("_MainLightShadowmapTexture",m_RTList[i]);
                }
                else if(mainCam && rtType[i] == RTtype.AddShadow)
                {
                    cmd.SetGlobalTexture("_AdditionalLightsShadowmapTexture",m_RTList[i]);
                }
                else
                {
                    cmd.SetGlobalTexture(m_RTnameList[i],m_RTList[i]);
                }
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