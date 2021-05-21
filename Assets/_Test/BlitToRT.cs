using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

[CreateAssetMenu]
public class BlitToRT : ScriptableRendererFeature
{
    public RenderPassEvent evt;
    public string RTname;
    public Material material;
    public bool isDepth;
    
	public BlitToRT()
	{
	}

	public override void Create()
	{
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {      
        var pass = new BlitToRTPass(evt,material,RTname,isDepth);
        renderer.EnqueuePass(pass);
    }

    //-------------------------------------------------------------------------

	class BlitToRTPass : ScriptableRenderPass
	{
        private string RTname;
        private Material material;
        private bool isDepth;

        public BlitToRTPass(RenderPassEvent evt, Material mat, string name, bool d)
        {
            this.isDepth = d;
            this.material = mat;
            this.RTname = name;
            this.renderPassEvent = evt;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if( material == null) return;

            var camera = renderingData.cameraData.camera;
			CommandBuffer cmd = CommandBufferPool.Get(camera.name + " - BlitToRTPass - " + RTname);
            if(isDepth)
            {
                cmd.Blit( null , renderingData.cameraData.renderer.cameraDepthTarget , material );
                if(RTname != "") cmd.SetGlobalTexture(RTname,renderingData.cameraData.renderer.cameraDepthTarget);
            }
            else
            {
                cmd.Blit( null , renderingData.cameraData.renderer.cameraColorTarget , material );
                if(RTname != "") cmd.SetGlobalTexture(RTname,renderingData.cameraData.renderer.cameraColorTarget);
            }
            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}