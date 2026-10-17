// Procedural soft particle for fire, embers, ash and wandering spirits. No texture required.
// _Shape: 0 = round glow, 1 = flame tongue (tapered, flickering), 2 = wisp (soft smoky streak).
Shader "HellSlice/Glow Particle"
{
    Properties
    {
        _Tint ("Tint (HDR)", Color) = (1, 0.5, 0.15, 1)
        _Shape ("Shape", Range(0, 2)) = 0
        _Softness ("Softness", Range(0.5, 6)) = 2
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst blend (One = additive)", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha [_DstBlend]
        ZWrite Off Cull Off
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint; half _Shape, _Softness; float _DstBlend;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; half fog : TEXCOORD1; };
            Varyings vert (Attributes v)
            {
                Varyings o; o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv; o.color = v.color; o.fog = ComputeFogFactor(o.positionCS.z); return o;
            }
            float hash(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
            float noise(float2 p) { float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1,0)), f.x), lerp(hash(i + float2(0,1)), hash(i + 1), f.x), f.y); }
            half4 frag (Varyings i) : SV_Target
            {
                float2 c = i.uv * 2 - 1;
                float a;
                if (_Shape < .5) a = saturate(1 - length(c));
                else if (_Shape < 1.5)
                {
                    float t = _Time.y * 3;
                    float w = lerp(.85, .15, i.uv.y) * (.8 + .4 * noise(float2(i.uv.y * 4 - t, t * .3)));
                    a = saturate(1 - abs(c.x) / max(w, .01)) * saturate(1 - abs(c.y) * .9) * smoothstep(1, .55, i.uv.y);
                }
                else
                {
                    float n = noise(i.uv * 3 + _Time.y * .4);
                    a = saturate(1 - length(c * float2(1.6, .8))) * (.5 + n);
                }
                a = pow(saturate(a), _Softness) * i.color.a * _Tint.a;
                half3 col = _Tint.rgb * i.color.rgb;
                col = MixFogColor(col, half3(0, 0, 0), i.fog);
                return half4(col, a);
            }
            ENDHLSL
        }
    }
}
