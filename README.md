# URPBlendScene

Unity 2023.3.0a17+
URP Deferred + RenderGraph

The scene is not doing fancy effect but is a great exercise for learning how to handle all different textures used in the pipeline.
- GBffer0, Gbuffer1, Gbuffer2, Camera Color, Camera Depth, MainLight Shadowmap, AdditionalLight Shadowmap
- Collect and blit these textures from 2 cameras to custom RTHandles (basically make copies of them)
- And on the third camera blend these textures and make the pipeline use the blended texture as resource for deferred rendering

[![Watch the video](https://img.youtube.com/vi/CU1drWhDj0I/hqdefault.jpg)](https://youtu.be/CU1drWhDj0I)

## Details

### Scene Setup

On ProjectView, select the BlendMaterial and drag the slider to animate the blending.

There are 3 Cameras in the scene, having the exact same transform - position and rotation, so that camera matrices are the same.
  - Cam1
    - Renders a set of construction objects on specific layer
    - Renderer has the CollectRT feature
  - Cam2
    - Renders another set of construction objects on another layer
    - Renderer has the CollectRT feature
  - MainCamera
    - Renders a dummy plane object so that it triggers the same render passes as the other 2 cameras
    - Has the highest priority, so that it renders last
    - Renderer has the BlendRT feature

### RTCollection

It holds all the RTHandles (the copies of the buffers from Cam1 and Cam2)
- `RTHandles` are like temporary render textures, you manage them by yourself (create and release)
- In order to make these RTHandles available across different cameras, don't write a custom `frameData (ContextItem)`. Write your own class to keep the RTHandle references.
- In each passes do `RenderGraph.ImportTexture()` to get the TextureHandle for RenderGraph to use

To make copies of the buffers, you need to use the same RenderTextureDescriptor as the original resource. See `RTCollection.cs > AllocateRT()`.

### Gbuffer0,1,2,4 and Gbuffer3 (camera color)

**Collect**
- They are just color textures, so just the basic blit, no need special material

**Blend**
- Simply sample the 2 source textures and blend them

### Depth+Stencil

**Collect**
- Reusing the URP CopyDepthPass and CopyDepth Shader for the blit
- However CopyDepthPass does not have RecordRenderGraph() so need to write a class to wrap it

**Blend**
- Modified the URP CopyDepth Shader so that it takes 2 source textures and blend them, pretty straightforward
- For stencil values, reference to the GBuffer event on FrameDebugger and make sure the Blending shader writes the same stencil value to depth

### Main Light Shadowmap & Additional Light Shadowmap

**Collect**
- They are also depth textures but can't reuse the URP CopyDepthPass because it will use the GameView viewport for the blit which doesn't fit the shadowmap's aspect ratio, so need to write a custom pass
- Can't reuse the URP CopyDepth Shader directly because it samples the depth texture instead of _BlitTexture, so have to make a modified shader of it

**Blend**
- Same process with Depth, but 
- Deferred Pass doesn't take the shadowmap resource references directly, it takes the global texture references. So need to bind the blended shadowmaps to the same global texture names as the original shadowmaps.

### Notes

**Blit with multiple sources + multiple render graph builders in a pass**

In BlendRT feature, for each blending it needs 2 sources (Src1 and Src2) and 1 destination texture. And all these buffer blend shares one material.
I have multiple builders in 1 pass and `Material.SetTexture()` doesn't run in CommandBuffer scope so I have to do `CommandBuffer.SetGlobalTexture()` so that the blit operation has the correct source textures.

**FrameDebugger can't preview stencil if target has depth only**

This makes debugging a bit hard. Use RenderDoc instead so that you can see all the input and output with all their depth / stencil contents. (Thanks Apporva and Mikkel for the help!)

