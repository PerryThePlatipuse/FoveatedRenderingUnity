using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using UnityEngine.Experimental.Rendering;         // For RenderGraphContext
using UnityEngine.Rendering.RenderGraphModule;    // For RecordRenderGraph
using FoveatedRenderingVRS;                      

[DisallowMultipleRendererFeature("VrsUrpFeature")]
public class VrsUrpFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class VrsUrpFeatureSettings
    {
        public bool enableFoveatedRendering = true;
    }

    //////////////////////////////////////////////////////////////////////////
    // ENABLE PASS
    //////////////////////////////////////////////////////////////////////////
    class EnableFoveatedRenderingPass : ScriptableRenderPass
    {
        private readonly string profilerTag;

        public EnableFoveatedRenderingPass(string tag)
        {
            profilerTag = tag;
            renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        // --- LEGACY PATH ---
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(profilerTag);
            cmd.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(),
                                 (int)FoveatedEventID.ENABLE_FOVEATED_RENDERING);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // --- RENDER GRAPH PATH ---
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<SimplePassData>(profilerTag, out var passData))
            {
                builder.AllowPassCulling(false); // Does nothing, without it RenderGraph skips my feature for some reason

                passData.eventID = (int)FoveatedEventID.ENABLE_FOVEATED_RENDERING;

                builder.SetRenderFunc((SimplePassData data, RasterGraphContext rgContext) =>
                {
                    var cmd = rgContext.cmd;
                    cmd.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), data.eventID);
                });
            }
        }

        private class SimplePassData
        {
            public int eventID;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // DISABLE PASS
    //////////////////////////////////////////////////////////////////////////
    class DisableFoveatedRenderingPass : ScriptableRenderPass
    {
        private readonly string profilerTag;

        public DisableFoveatedRenderingPass(string tag)
        {
            profilerTag = tag;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        // --- LEGACY PATH ---
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(profilerTag);
            cmd.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(),
                                 (int)FoveatedEventID.DISABLE_FOVEATED_RENDERING);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // --- RENDER GRAPH PATH ---
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<SimplePassData>(profilerTag, out var passData))
            {
                // Prevent the pass from being culled
                builder.AllowPassCulling(false);

                passData.eventID = (int)FoveatedEventID.DISABLE_FOVEATED_RENDERING;

                builder.SetRenderFunc((SimplePassData data, RasterGraphContext rgContext) =>
                {
                    var cmd = rgContext.cmd;
                    cmd.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), data.eventID);
                });
            }
        }

        private class SimplePassData
        {
            public int eventID;
        }
    }

    //////////////////////////////////////////////////////////////////////////
    // FEATURE ITSELF
    //////////////////////////////////////////////////////////////////////////
    public VrsUrpFeatureSettings settings = new VrsUrpFeatureSettings();

    private EnableFoveatedRenderingPass enablePass;
    private DisableFoveatedRenderingPass disablePass;

    public override void Create()
    {
        enablePass = new EnableFoveatedRenderingPass("Enable Foveated Rendering Pass");
        disablePass = new DisableFoveatedRenderingPass("Disable Foveated Rendering Pass");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.enableFoveatedRendering)
        {
            // We enqueue both passes
            renderer.EnqueuePass(enablePass);
            renderer.EnqueuePass(disablePass);
        }
    }
}
