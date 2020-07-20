
Shader "Hidden/Metaballs2D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

           
            sampler2D _MainTex;
            int _count;
            float _cameraSize;
            
            float4 _positions[100];         
            float3 _colors[100];
            
            float4 _balls[100];
            
            float distance2(float2 a, float2 b) 
            { 
                float2 v = a - b;                
                return dot( v, v ); 
            }
            
            float4 calculateball( float2 ballPosition, float r, float3 ballColor, float2 uv)
            {     
                float d = (distance2( uv, ballPosition ));
                float dst = (r / d);                
                if (r<d)
                    return float4((ballColor*(dst)), sqrt(dst));
                else
                    return float4((ballColor*dst), dst);
            }
            
            
            float3 calculateAll(float2 uv, float4 tex)
            {
                float res = 0;
                float3 color = float3(0.0, 0.0, 0.0);
                float avdst = 0;
                for (int i = 0; i<_count; i++)
                {
                    float radius = ((_positions[i].w * _ScreenParams.y) / _cameraSize);               
                    _balls[i] = calculateball(_positions[i].xy, radius, _colors[i], uv);                   
                    res += (_balls[i].a);
                    color += max(_balls[i].rgb, _colors[i]*_balls[i].a);                                   
                }
                //res = clamp(res, 0., 1.);
                tex = clamp(tex,0.,1.);
                avdst = res/_count;
                
                if (res<=0.75) return tex;               
                color/=res;               
                color = (clamp(color, 0., 1.));                                                
                color = (clamp(color, 0., 1.))*(res-avdst);
                return color;
            }
            
                               
            float4 frag(v2f_img i) : SV_Target
            {               
                float2 uv = i.uv * _ScreenParams.xy;
                float4 tex = tex2D(_MainTex, i.uv);
                float3 color = calculateAll(uv, tex);
               
                                
                return float4(color, 1.0);
            }       
         
            ENDCG
        }
    }
}
