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

    [Header("Clearing options")]
    public RTClearFlags clearFlag;
    public Color clearColor;
    public float clearDepth;
    public uint clearStencil;

	public BlitToRT()
	{
	}

	public override void Create()
	{
	}

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {      
        var pass = new BlitToRTPass(evt,material,RTname,isDepth,clearFlag,clearColor,clearDepth,clearStencil);
        renderer.EnqueuePass(pass);
    }

    //-------------------------------------------------------------------------

	class BlitToRTPass : ScriptableRenderPass
	{
        private string RTname;
        private Material material;
        private bool isDepth;

        private RTClearFlags clear_flag;
        private Color clear_color;
        private float clear_depth;
        private uint clear_stencil;

        //private int tempRTid = Shader.PropertyToID("TempRTForClear");
       // private RenderTargetIdentifier tempRT;

        public BlitToRTPass(RenderPassEvent evt, Material mat, string name, bool d, RTClearFlags clearFlag, Color clearColor, float clearDepth, uint clearStencil)
        {
            this.clear_flag = clearFlag;
            this.clear_color = clearColor;
            this.clear_depth = clearDepth;
            this.clear_stencil = clearStencil;

            this.isDepth = d;
            this.material = mat;
            this.RTname = name;
            this.renderPassEvent = evt;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescripor)
        {
            //tempRT = new RenderTargetIdentifier(tempRTid);
            //cmd.GetTemporaryRT(tempRTid, cameraTextureDescripor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
			CommandBuffer cmd = CommandBufferPool.Get(camera.name + " - BlitToRTPass - " + RTname);

            RenderTargetIdentifier RT = renderingData.cameraData.renderer.cameraColorTargetHandle;
            if(isDepth) RT = renderingData.cameraData.renderer.cameraColorTargetHandle;

            //Clearing Render Target
            cmd.SetRenderTarget(RT);
            cmd.ClearRenderTarget(clear_flag,clear_color,clear_depth,clear_stencil);
            //cmd.Blit(tempRT,RT);
            //cmd.ReleaseTemporaryRT(tempRTid);

            if(material != null) cmd.Blit( null , RT , material );
            if(RTname != "") cmd.SetGlobalTexture(RTname,RT);

            context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}