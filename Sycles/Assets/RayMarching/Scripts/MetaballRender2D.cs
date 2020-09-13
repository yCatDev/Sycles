using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MetaballRender2D : ScriptableRendererFeature
{
    /*[System.Serializable]
    public class MetaballRender2DSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        [Range(0f, 1f), Tooltip("Outline size.")]
        public float outlineSize = 1.0f;

        [Tooltip("Inner color.")]
        public Color innerColor = Color.white;

        [Tooltip("Outline color.")]
        public Color outlineColor = Color.white;
    }*/

    //public MetaballRender2DSettings settings;

    class MetaballRender2DPass : ScriptableRenderPass
    {
        private Material material;
        private bool isFirstRender = true;

        private RenderTargetIdentifier source;
        private string profilerTag;

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
            material = new Material(Shader.Find("Hidden/Metaballs2D"));
        }

        public MetaballRender2DPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            if(isFirstRender)
            {
                isFirstRender = false;
                cmd.SetGlobalVectorArray("_positions", new Vector4[100]);
                cmd.SetGlobalVectorArray("_colors", new Vector4[100]);
            }
            
            List<Metaball2D> metaballs = MetaballSystem2D.Get();

            var metaballPositions = new Vector4[100];
            var metaballColors = new Vector4[100];

            for(int i = 0; i < metaballs.Count; ++i)
            {
                if (metaballs[i] != null)
                {
                    Vector3 pos = renderingData.cameraData.camera.WorldToScreenPoint(metaballs[i].transform.position);
                    float radius = metaballs[i].GetRadius();
                    metaballPositions[i] = (new Vector4(pos.x, pos.y, pos.z, radius));
                    metaballColors[i] = metaballs[i].GetColor();
                }
            }

            if(metaballs.Count > 0)
            {
                cmd.SetGlobalInt("_count", metaballs.Count);
                cmd.SetGlobalVectorArray("_positions", metaballPositions);
                cmd.SetGlobalVectorArray("_colors", metaballColors);
                cmd.SetGlobalFloat("_cameraSize", renderingData.cameraData.camera.orthographicSize);

                cmd.Blit(source, source, material);

                context.ExecuteCommandBuffer(cmd);
            }
            
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }

    MetaballRender2DPass pass;

    public override void Create()
    {
        name = "Metaballs (2D)";

        pass = new MetaballRender2DPass("Metaballs2D");

        /*pass.outlineSize = settings.outlineSize;
        pass.innerColor = settings.innerColor;
        pass.outlineColor = settings.outlineColor;*/

        pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        pass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(pass);
    }
}
