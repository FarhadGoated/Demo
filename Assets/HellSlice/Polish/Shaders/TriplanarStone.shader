// World-space triplanar stone for the authored box geometry: any scaled cube tiles at real-world size
// with no UV work. Upward faces use the Top set (floor), everything else the Side set (walls, rock).
// Optional lava heat glows on surfaces close to the lava level. Lit by URP (shadows, Forward+, fog).
Shader "HellSlice/Triplanar Stone"
{
    Properties
    {
        [Header(Top faces)]
        _TopMap ("Top albedo", 2D) = "gray" {}
        [Normal] _TopNormal ("Top normal", 2D) = "bump" {}
        _TopARM ("Top AO/Rough/Metal", 2D) = "white" {}
        _TopTint ("Top tint", Color) = (1, 1, 1, 1)
        _TopTile ("Top tile size (m)", Float) = 1.8
        [Header(Side faces)]
        _SideMap ("Side albedo", 2D) = "gray" {}
        [Normal] _SideNormal ("Side normal", 2D) = "bump" {}
        _SideARM ("Side AO/Rough/Metal", 2D) = "white" {}
        _SideTint ("Side tint", Color) = (1, 1, 1, 1)
        _SideTile ("Side tile size (m)", Float) = 1.8
        [Header(Surface)]
        _NormalStrength ("Normal strength", Range(0, 2)) = 0.8
        _Sharpness ("Blend sharpness", Range(1, 16)) = 6
        _Smoothness ("Smoothness scale", Range(0, 1)) = 0.5
        _AO ("Occlusion strength", Range(0, 1)) = 0.8
        _Macro ("Large-scale variation", Range(0, 1)) = 0.35
        [Header(Lava heat)]
        [HDR] _HeatColor ("Heat glow", Color) = (1.6, 0.35, 0.06, 1)
        _HeatTop ("Glow fades out above (world Y)", Float) = -0.1
        _HeatFade ("Glow fade height (m)", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_TopMap); TEXTURE2D(_TopNormal); TEXTURE2D(_TopARM);
            TEXTURE2D(_SideMap); TEXTURE2D(_SideNormal); TEXTURE2D(_SideARM);
            SAMPLER(sampler_TopMap); SAMPLER(sampler_SideMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _TopTint, _SideTint, _HeatColor;
                float _TopTile, _SideTile, _HeatTop, _HeatFade;
                half _NormalStrength, _Sharpness, _Smoothness, _AO, _Macro;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                half fog : TEXCOORD2;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            struct Sample3 { half3 albedo; half3 normalTS; half3 arm; };

            Sample3 SampleSet(bool top, float2 uv)
            {
                Sample3 s;
                if (top)
                {
                    s.albedo = SAMPLE_TEXTURE2D(_TopMap, sampler_TopMap, uv).rgb * _TopTint.rgb;
                    s.normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_TopNormal, sampler_TopMap, uv), _NormalStrength);
                    s.arm = SAMPLE_TEXTURE2D(_TopARM, sampler_TopMap, uv).rgb;
                }
                else
                {
                    s.albedo = SAMPLE_TEXTURE2D(_SideMap, sampler_SideMap, uv).rgb * _SideTint.rgb;
                    s.normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_SideNormal, sampler_SideMap, uv), _NormalStrength);
                    s.arm = SAMPLE_TEXTURE2D(_SideARM, sampler_SideMap, uv).rgb;
                }
                return s;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 p = i.positionWS;
                half3 blend = pow(abs(n), _Sharpness); blend /= max(dot(blend, 1), 1e-4);
                half3 axisSign = sign(n + 1e-5);
                bool up = n.y > 0;

                // Projections (Ben Golus' UDN triplanar, with mirrored-face correction).
                float sideScale = 1.0 / max(_SideTile, 0.01), topScale = 1.0 / max(_TopTile, 0.01);
                float2 uvX = p.zy * sideScale; uvX.x *= axisSign.x;
                float2 uvY = p.xz * (up ? topScale : sideScale); uvY.x *= axisSign.y;
                float2 uvZ = p.xy * sideScale; uvZ.x *= -axisSign.z;

                Sample3 sx = SampleSet(false, uvX);
                Sample3 sy = SampleSet(up, uvY);
                Sample3 sz = SampleSet(false, uvZ);

                half3 tX = sx.normalTS, tY = sy.normalTS, tZ = sz.normalTS;
                tX.x *= axisSign.x; tY.x *= axisSign.y; tZ.x *= -axisSign.z;
                half3 absN = abs(n);
                tX = half3(tX.xy + n.zy, absN.x);
                tY = half3(tY.xy + n.xz, absN.y);
                tZ = half3(tZ.xy + n.xy, absN.z);
                tX.z *= axisSign.x; tY.z *= axisSign.y; tZ.z *= axisSign.z;
                float3 normalWS = normalize(tX.zyx * blend.x + tY.xzy * blend.y + tZ.xyz * blend.z);

                half3 albedo = sx.albedo * blend.x + sy.albedo * blend.y + sz.albedo * blend.z;
                half3 arm = sx.arm * blend.x + sy.arm * blend.y + sz.arm * blend.z;

                // Break up visible tiling with a slow, large-scale brightness drift.
                float2 macroUV = (up ? p.xz : p.xy + p.zy) * 0.045;
                half macro = SAMPLE_TEXTURE2D(_SideMap, sampler_SideMap, macroUV).g;
                albedo *= lerp(1, saturate(macro * 2.2), _Macro);

                InputData input = (InputData)0;
                input.positionWS = p;
                input.positionCS = i.positionCS;
                input.normalWS = normalWS;
                input.viewDirectionWS = GetWorldSpaceNormalizeViewDir(p);
                input.shadowCoord = TransformWorldToShadowCoord(p);
                input.fogCoord = i.fog;
                input.bakedGI = SampleSH(normalWS);
                input.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                input.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surf = (SurfaceData)0;
                surf.albedo = albedo;
                surf.metallic = 0;
                surf.smoothness = (1 - arm.g) * _Smoothness;
                surf.occlusion = lerp(1, arm.r, _AO);
                surf.normalTS = half3(0, 0, 1);
                surf.alpha = 1;
                // Heat from the lava below: strongest at the lava line, gone by _HeatTop.
                half heat = saturate((_HeatTop - p.y) / max(_HeatFade, 0.01));
                surf.emission = _HeatColor.rgb * heat * heat * lerp(1, arm.r, 0.6);

                half4 color = UniversalFragmentPBR(input, surf);
                color.rgb = MixFog(color.rgb, i.fog);
                return color;
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
        UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
        UsePass "Universal Render Pipeline/Lit/DEPTHNORMALS"
    }
    FallBack Off
}
