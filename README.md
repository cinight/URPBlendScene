# URPBlendScene

Unity 2021.2.0a18 \
URP Deferred

Collect GBuffer/Depth/Shadowmaps from 2 cameras and blend them, \
put them back to MainCamera's GBuffer/Depth/Shadowmaps for final rendering. \
Note: This is an experiment, very not optimal because the rendering of the 2 cameras are wasted.

[![Watch the video](https://img.youtube.com/vi/CU1drWhDj0I/hqdefault.jpg)](https://youtu.be/CU1drWhDj0I)

### Backup Ref:
[GBufferPass.cs](https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.universal/Runtime/Passes/GBufferPass.cs) \
[UniversalRenderer.cs](https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.universal/Runtime/UniversalRenderer.cs) \
[ScriptableRenderPass.cs](https://github.com/Unity-Technologies/Graphics/blob/master/com.unity.render-pipelines.universal/Runtime/Passes/ScriptableRenderPass.cs)
