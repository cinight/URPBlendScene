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

        private void DoCollectRT( ScriptableRenderContext context, CommandBuffer cmd, RenderTargetIdentifier from, RenderTexture to, string name, Material mat )
        {
            cmd.Blit( from , to , mat );
            cmd.SetGlobalTexture( name , to );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

			CommandBuffer cmd = CommandBufferPool.Get(camera.name + " - CollectRTContentPass");

            if(cam1)
            {
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_GBuffer0.tex , RTCollection.cam1_GBuffer0.name , RTCollection.mat_Collect_GBuffer0 );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_GBuffer1.tex , RTCollection.cam1_GBuffer1.name , RTCollection.mat_Collect_GBuffer1 );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_GBuffer2.tex , RTCollection.cam1_GBuffer2.name , RTCollection.mat_Collect_GBuffer2 );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_GBuffer3.tex , RTCollection.cam1_GBuffer3.name , RTCollection.mat_Collect_GBuffer3 );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_Depth.tex , RTCollection.cam1_Depth.name , RTCollection.mat_Collect_Depth );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_ShadowMain.tex , RTCollection.cam1_ShadowMain.name , RTCollection.mat_Collect_ShadowMain );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam1_ShadowAdd.tex , RTCollection.cam1_ShadowAdd.name , RTCollection.mat_Collect_ShadowAdd );
            }
            else
            {
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_GBuffer0.tex , RTCollection.cam2_GBuffer0.name , RTCollection.mat_Collect_GBuffer0 );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_GBuffer1.tex , RTCollection.cam2_GBuffer1.name , RTCollection.mat_Collect_GBuffer1 );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_GBuffer2.tex , RTCollection.cam2_GBuffer2.name , RTCollection.mat_Collect_GBuffer2 );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_GBuffer3.tex , RTCollection.cam2_GBuffer3.name , RTCollection.mat_Collect_GBuffer3 );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_Depth.tex , RTCollection.cam2_Depth.name , RTCollection.mat_Collect_Depth );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_ShadowMain.tex , RTCollection.cam2_ShadowMain.name , RTCollection.mat_Collect_ShadowMain );
                DoCollectRT(context, cmd, renderingData.cameraData.renderer.cameraColorTarget , RTCollection.cam2_ShadowAdd.tex , RTCollection.cam2_ShadowAdd.name , RTCollection.mat_Collect_ShadowAdd );
            }

            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}