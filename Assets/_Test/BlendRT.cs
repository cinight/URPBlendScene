using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CreateAssetMenu]
public class BlendRT : ScriptableRendererFeature
{
    public Material material;
    public RenderPassEvent evt;
    
	public BlendRT()
	{
	}

	public override void Create()
	{
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var cameraColorTarget = renderer.cameraColorTarget;
        var pass = new BlendRTPass(evt,material,cameraColorTarget);
        renderer.EnqueuePass(pass);
    }

    //-------------------------------------------------------------------------

	// class DrawUIIntoRTPass : ScriptableRenderPass
	// {
    //     private RenderTargetIdentifier colorHandle;

    //     //The temporary UI texture
    //     public static int m_uiRTid = Shader.PropertyToID("_UITexture");
    //     public static RenderTargetIdentifier m_uiRT = new RenderTargetIdentifier(m_uiRTid);

    //     public DrawUIIntoRTPass(RenderPassEvent renderPassEvent, RenderTargetIdentifier colorHandle)
    //     {
    //         this.colorHandle = colorHandle;
    //         this.renderPassEvent = renderPassEvent;
    //     }

    //     public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
    //     {
    //         RenderTextureDescriptor descriptor = cameraTextureDescripor;
    //         descriptor.colorFormat = RenderTextureFormat.Default;
    //         cmd.GetTemporaryRT(m_uiRTid, descriptor);
    //     }

    //     public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    //     {
	// 		CommandBuffer cmd = CommandBufferPool.Get("Draw UI Into RT Pass");

    //         cmd.SetRenderTarget(m_uiRT);
    //         cmd.ClearRenderTarget(true,true,Color.clear);

    //         context.ExecuteCommandBuffer(cmd);
	// 		CommandBufferPool.Release(cmd);
	// 	}

	// 	public override void FrameCleanup(CommandBuffer cmd)
	// 	{
	// 		if (cmd == null)
	// 			throw new ArgumentNullException("cmd");

	// 		base.FrameCleanup(cmd);
	// 	}
	// }

    //-------------------------------------------------------------------------

	class BlendRTPass : ScriptableRenderPass
	{
        private RenderTargetIdentifier colorHandle;
        //private BuiltinRenderTextureType rtType;
        private Material material;

        public BlendRTPass(RenderPassEvent evt, Material mat, RenderTargetIdentifier colorHandle)
        {
            this.material = mat;
            this.colorHandle = colorHandle;
            this.renderPassEvent = evt;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if(material == null) return;
            var camera = renderingData.cameraData.camera;

			CommandBuffer cmd = CommandBufferPool.Get(camera.name + " - BlendRTPass");
            
            //Forward Renderer
            //cmd.Blit(null,colorHandle,material);

            //Deferred Renderer
            // if(camera.cameraType == CameraType.SceneView)
            // {
            //     material.DisableKeyword("FLIP_Y");
            // }
            // else
            // {
            //     material.EnableKeyword("FLIP_Y");
            // }
                       
            //works for deferred
            //cmd.Blit(null,BuiltinRenderTextureType.CameraTarget,material);
            //cmd.Blit(BuiltinRenderTextureType.CameraTarget,colorHandle);

            //cmd.Blit(colorHandle,BuiltinRenderTextureType.CameraTarget,material);
            //cmd.Blit(BuiltinRenderTextureType.CameraTarget,colorHandle);

            //if we do not need _CameraColorTexture
            cmd.Blit(null,renderingData.cameraData.renderer.cameraColorTarget,material);

            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		public override void FrameCleanup(CommandBuffer cmd)
		{
			if (cmd == null) throw new ArgumentNullException("cmd");

            //cmd.ReleaseTemporaryRT(DrawUIIntoRTPass.m_uiRTid);

			base.FrameCleanup(cmd);
		}
	}

    //-------------------------------------------------------------------------
}