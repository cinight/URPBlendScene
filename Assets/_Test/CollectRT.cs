using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

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

    //-------------------------------------------------------------------------

	class CollectRTPass : ScriptableRenderPass
	{
        private bool cam1;

        public CollectRTPass(RenderPassEvent evt, bool cam1)
        {
            this.cam1 = cam1;
            this.renderPassEvent = evt;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RTCollection.ConfigureRT(cmd,cameraTextureDescriptor);
        }

        private void DoCollectRT( CommandBuffer cmd, RenderTargetIdentifier from, RenderTargetIdentifier to, int name, Material mat, bool isDepth = false)
        {
            if(isDepth) cmd.Blit( from , to );
            cmd.Blit( from , to , mat );
            cmd.SetGlobalTexture( name , to );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

			CommandBuffer cmd = CommandBufferPool.Get(camera.name + " - CollectRTContentPass");

            if(cam1)
            {
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_GBuffer0 , RTCollection.cam1_GBuffer0Id , RTCollection.mat_Collect_GBuffer0 );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_GBuffer1 , RTCollection.cam1_GBuffer1Id , RTCollection.mat_Collect_GBuffer1 );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_GBuffer2 , RTCollection.cam1_GBuffer2Id , RTCollection.mat_Collect_GBuffer2 );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_GBuffer3 , RTCollection.cam1_GBuffer3Id , RTCollection.mat_Collect_GBuffer3 );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_Depth , RTCollection.cam1_DepthId , RTCollection.mat_Collect_Depth , true );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_ShadowMain , RTCollection.cam1_ShadowMainId , RTCollection.mat_Collect_ShadowMain );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_ShadowAdd , RTCollection.cam1_ShadowAddId , RTCollection.mat_Collect_ShadowAdd );
            }
            else
            {
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_GBuffer0 , RTCollection.cam2_GBuffer0Id , RTCollection.mat_Collect_GBuffer0 );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_GBuffer1 , RTCollection.cam2_GBuffer1Id , RTCollection.mat_Collect_GBuffer1 );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_GBuffer2 , RTCollection.cam2_GBuffer2Id , RTCollection.mat_Collect_GBuffer2 );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_GBuffer3 , RTCollection.cam2_GBuffer3Id , RTCollection.mat_Collect_GBuffer3 );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_Depth , RTCollection.cam2_DepthId , RTCollection.mat_Collect_Depth , true );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_ShadowMain , RTCollection.cam2_ShadowMainId , RTCollection.mat_Collect_ShadowMain );
                DoCollectRT(cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_ShadowAdd , RTCollection.cam2_ShadowAddId , RTCollection.mat_Collect_ShadowAdd );
            }

            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}