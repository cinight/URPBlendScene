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

            cmd.Blit( renderingData.cameraData.renderer.cameraDepthTarget , RTCollection.Blended_GBuffer0 , RTCollection.mat_Blend_GBuffer0 );
            cmd.SetGlobalTexture( "_GBuffer0", RTCollection.Blended_GBuffer0 );
            
            cmd.Blit( renderingData.cameraData.renderer.cameraDepthTarget , RTCollection.Blended_GBuffer1 , RTCollection.mat_Blend_GBuffer1 );
            cmd.SetGlobalTexture( "_GBuffer1", RTCollection.Blended_GBuffer1 );

            cmd.Blit( renderingData.cameraData.renderer.cameraDepthTarget , RTCollection.Blended_GBuffer2 , RTCollection.mat_Blend_GBuffer2 );
            cmd.SetGlobalTexture( "_GBuffer2", RTCollection.Blended_GBuffer2 );

            cmd.Blit( renderingData.cameraData.renderer.cameraDepthTarget , RTCollection.Blended_GBuffer3 , RTCollection.mat_Blend_GBuffer3 );
            cmd.SetGlobalTexture( "_GBuffer3", RTCollection.Blended_GBuffer3 );

           cmd.Blit( renderingData.cameraData.renderer.cameraColorTarget , RTCollection.Blended_Depth , RTCollection.mat_Blend_Depth );
           cmd.SetGlobalTexture( "_CameraDepthTexture", RTCollection.Blended_Depth );
           cmd.Blit( RTCollection.Blended_Depth , renderingData.cameraData.renderer.cameraDepthTarget , RTCollection.mat_Blend_Depth ); //TODO it's duplicated work

            cmd.Blit( renderingData.cameraData.renderer.cameraColorTarget , RTCollection.Blended_ShadowMain , RTCollection.mat_Blend_ShadowMain );
            cmd.SetGlobalTexture("_MainLightShadowmapTexture", RTCollection.Blended_ShadowMain );

            cmd.Blit( renderingData.cameraData.renderer.cameraColorTarget , RTCollection.Blended_ShadowAdd , RTCollection.mat_Blend_ShadowAdd );
            cmd.SetGlobalTexture("_AdditionalLightsShadowmapTexture", RTCollection.Blended_ShadowAdd );

            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}