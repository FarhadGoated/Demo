// Flowing molten lava: cooling crust plates over bright cracks, slow churn, HDR glow for bloom.
Shader "HellSlice/Lava"
{
    Properties
    {
        _HotColor ("Hot core", Color) = (1.0, 0.55, 0.12, 1)
        _MidColor ("Molten", Color) = (0.9, 0.16, 0.02, 1)
        _CrustColor ("Crust", Color) = (0.05, 0.015, 0.012, 1)
        _Glow ("Glow (HDR)", Range(0, 12)) = 4.5
        _Scale ("World scale", Range(0.05, 2)) = 0.32
        _Flow ("Flow direction", Vector) = (0.04, 0.09, 0, 0)
        _Crust ("Crust amount", Range(0, 1)) = 0.55
        _Pulse ("Pulse", Range(0, 1)) = 0.35
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Lava"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _HotColor, _MidColor, _CrustColor;
                half _Glow, _Scale, _Crust, _Pulse;
                float4 _Flow;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; half fog : TEXCOORD1; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            float2 hash22(float2 p) { p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3))); return frac(sin(p) * 43758.5453); }
            float hash21(float2 p) { return frac(sin(dot(p, float2(41.3, 289.1))) * 15731.743); }
            float noise(float2 p)
            {
                float2 i = floor(p), f = frac(p); float2 u = f * f * (3 - 2 * f);
                return lerp(lerp(hash21(i), hash21(i + float2(1, 0)), u.x), lerp(hash21(i + float2(0, 1)), hash21(i + 1), u.x), u.y);
            }
            float fbm(float2 p) { float v = 0, a = .5; [unroll] for (int k = 0; k < 4; k++) { v += a * noise(p); p *= 2.07; a *= .5; } return v; }
            // Distance to the nearest Voronoi edge: produces crack lines between crust plates.
            float voronoiEdge(float2 p, float t)
            {
                float2 n = floor(p), f = frac(p);
                float2 mg, mr; float md = 8;
                [unroll] for (int j = -1; j <= 1; j++) [unroll] for (int i = -1; i <= 1; i++)
                {
                    float2 g = float2(i, j); float2 o = hash22(n + g);
                    o = .5 + .5 * sin(t + 6.2831 * o);
                    float2 r = g + o - f; float d = dot(r, r);
                    if (d < md) { md = d; mr = r; mg = g; }
                }
                md = 8;
                [unroll] for (int j2 = -2; j2 <= 2; j2++) [unroll] for (int i2 = -2; i2 <= 2; i2++)
                {
                    float2 g = mg + float2(i2, j2); float2 o = hash22(n + g);
                    o = .5 + .5 * sin(t + 6.2831 * o);
                    float2 r = g + o - f;
                    if (dot(mr - r, mr - r) > .00001) md = min(md, dot(.5 * (mr + r), normalize(r - mr)));
                }
                return md;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float t = _Time.y;
                float2 p = i.positionWS.xz * _Scale + _Flow.xy * t;
                float warp = fbm(p * 1.3 + t * .05);
                float2 q = p + warp * .6;
                float edge = voronoiEdge(q * 1.6, t * .25);
                float crack = 1 - smoothstep(0, .12 + _Crust * .1, edge);
                float heat = fbm(q * 2.4 - t * .08);
                float molten = saturate(crack + (1 - _Crust) * heat * .9);
                float pulse = 1 + _Pulse * sin(t * 1.7 + heat * 6.2831) * .5;

                half3 col = lerp(_CrustColor.rgb * (.6 + heat), _MidColor.rgb * _Glow * .5, molten);
                col = lerp(col, _HotColor.rgb * _Glow, pow(molten, 3) * pulse);
                col = MixFog(col, i.fog * .4); // lava punches through fog
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Unlit"
}
