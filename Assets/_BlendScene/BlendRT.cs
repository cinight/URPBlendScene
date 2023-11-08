using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

[CreateAssetMenu]
public class BlendRT : ScriptableRendererFeature
{
	public BlendRT()
	{
	}

	public override void Create()
	{
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var evt = RenderPassEvent.AfterRenderingGbuffer;
        
        var pass = new BlendRTPass(evt);
        renderer.EnqueuePass(pass);
    }

    //-------------------------------------------------------------------------

	class BlendRTPass : ScriptableRenderPass
	{
        public BlendRTPass(RenderPassEvent evt)
        {
            this.renderPassEvent = evt;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RTCollection.ConfigureRT(cmd,cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

			CommandBuffer cmd = CommandBufferPool.Get(camera.name + " - BlendRTPass");

            //Blend Depth first
            cmd.Blit( renderingData.cameraData.renderer.cameraColorTargetHandle , RTCollection.Blended_Depth.tex , RTCollection.mat_Blend_Depth );
            cmd.SetGlobalTexture( RTCollection.Blended_Depth.nameId, RTCollection.Blended_Depth.tex );
            cmd.SetGlobalTexture( "_CameraDepthTexture", RTCollection.Blended_Depth.tex );
            cmd.Blit( RTCollection.Blended_Depth.tex , renderingData.cameraData.renderer.cameraDepthTargetHandle , RTCollection.mat_Blend_Depth );
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            cmd.Blit( renderingData.cameraData.renderer.cameraColorTargetHandle , RTCollection.Blended_ShadowMain.tex , RTCollection.mat_Blend_ShadowMain );
            cmd.SetGlobalTexture("_MainLightShadowmapTexture", RTCollection.Blended_ShadowMain.tex );

            cmd.Blit( renderingData.cameraData.renderer.cameraColorTargetHandle , RTCollection.Blended_ShadowAdd.tex , RTCollection.mat_Blend_ShadowAdd );
            cmd.SetGlobalTexture("_AdditionalLightsShadowmapTexture", RTCollection.Blended_ShadowAdd.tex );

            //Blend Colors
            cmd.Blit( renderingData.cameraData.renderer.cameraDepthTargetHandle , RTCollection.Blended_GBuffer0.tex , RTCollection.mat_Blend_GBuffer0 );
            cmd.SetGlobalTexture( "_GBuffer0", RTCollection.Blended_GBuffer0.tex );
            
            cmd.Blit( renderingData.cameraData.renderer.cameraDepthTargetHandle , RTCollection.Blended_GBuffer1.tex , RTCollection.mat_Blend_GBuffer1 );
            cmd.SetGlobalTexture( "_GBuffer1", RTCollection.Blended_GBuffer1.tex );

            cmd.Blit( renderingData.cameraData.renderer.cameraDepthTargetHandle , RTCollection.Blended_GBuffer2.tex , RTCollection.mat_Blend_GBuffer2 );
            cmd.SetGlobalTexture( "_GBuffer2", RTCollection.Blended_GBuffer2.tex );

            cmd.Blit( renderingData.cameraData.renderer.cameraDepthTargetHandle , RTCollection.Blended_GBuffer3.tex , RTCollection.mat_Blend_GBuffer3 );
            cmd.SetGlobalTexture( "_GBuffer3", RTCollection.Blended_GBuffer3.tex );
            cmd.Blit( RTCollection.Blended_GBuffer3.tex, renderingData.cameraData.renderer.cameraColorTargetHandle );

            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}