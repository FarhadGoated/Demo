// Shadow -> human transformation.
// _Reveal 0 = a living shadow (near-black body, cold smoky rim). _Reveal 1 = fully lit human colours.
// The human surface spreads upward from the feet with a burning ember seam at the boundary.
Shader "HellSlice/Shadow Reveal"
{
    Properties
    {
        _BaseMap ("Albedo", 2D) = "white" {}
        _BaseColor ("Colour", Color) = (1, 1, 1, 1)
        _Reveal ("Reveal", Range(0, 1)) = 0
        _ShadowColor ("Shadow body", Color) = (0.012, 0.012, 0.022, 1)
        _RimColor ("Shadow rim", Color) = (0.35, 0.45, 0.95, 1)
        _RimPower ("Rim power", Range(0.5, 8)) = 3
        _EdgeColor ("Ember seam (HDR)", Color) = (4, 1.2, 0.25, 1)
        _EdgeWidth ("Seam width", Range(0.005, 0.2)) = 0.06
        _Height ("Body height (object units)", Float) = 1.8
        _BaseY ("Feet height (object units)", Float) = 0
        _NoiseScale ("Dissolve noise", Range(0.5, 20)) = 7
        _Hit ("Hit flash", Range(0, 1)) = 0
        _HitColor ("Hit flash colour", Color) = (1, 0.25, 0.1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor, _ShadowColor, _RimColor, _EdgeColor, _HitColor;
            half _Reveal, _RimPower, _EdgeWidth, _NoiseScale, _Hit;
            float _Height, _BaseY;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile _ _FORWARD_PLUS _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2; float3 positionOS : TEXCOORD3; half fog : TEXCOORD4; half4 color : COLOR;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.positionOS = v.positionOS.xyz;
                o.fog = ComputeFogFactor(o.positionCS.z);
                o.color = v.color;
                return o;
            }

            float hash31(float3 p) { p = frac(p * .3183099 + .1); p *= 17; return frac(p.x * p.y * p.z * (p.x + p.y + p.z)); }
            float noise3(float3 x)
            {
                float3 i = floor(x), f = frac(x); f = f * f * (3 - 2 * f);
                return lerp(lerp(lerp(hash31(i), hash31(i + float3(1,0,0)), f.x), lerp(hash31(i + float3(0,1,0)), hash31(i + float3(1,1,0)), f.x), f.y),
                            lerp(lerp(hash31(i + float3(0,0,1)), hash31(i + float3(1,0,1)), f.x), lerp(hash31(i + float3(0,1,1)), hash31(i + 1), f.x), f.y), f.z);
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 viewDir = GetWorldSpaceNormalizeViewDir(i.positionWS);
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;

                // Surface lighting (lambert + soft wrap so low-poly reads well in dark scenes).
                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light main = GetMainLight(shadowCoord);
                half ndl = saturate(dot(n, main.direction) * .7 + .3);
                half3 lit = albedo.rgb * (SampleSH(n) + main.color * ndl * main.shadowAttenuation);
                #if defined(_ADDITIONAL_LIGHTS)
                uint count = GetAdditionalLightsCount();
                #if USE_FORWARD_PLUS || USE_CLUSTER_LIGHT_LOOP
                InputData inputData = (InputData)0; inputData.positionWS = i.positionWS; inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                #endif
                LIGHT_LOOP_BEGIN(count)
                    Light l = GetAdditionalLight(lightIndex, i.positionWS);
                    lit += albedo.rgb * l.color * l.distanceAttenuation * saturate(dot(n, l.direction) * .6 + .4);
                LIGHT_LOOP_END
                #endif

                // Shadow body: near-black with a smoky, flowing cold rim.
                float t = _Time.y;
                half fres = pow(1 - saturate(dot(n, viewDir)), _RimPower);
                float smoke = noise3(i.positionWS * 3 + float3(0, -t * .8, 0));
                half3 shadowBody = _ShadowColor.rgb + _RimColor.rgb * fres * (.6 + smoke * .8);

                // Reveal front climbs from the feet. Noise breaks the line into burning fragments.
                float h = saturate((i.positionOS.y - _BaseY) / max(_Height, .0001));
                float dn = noise3(i.positionOS * _NoiseScale) * .35;
                float front = _Reveal * 1.25 - .1;
                float d = front - (h * .85 + dn);
                half revealed = step(0, d);
                if (_Reveal >= .999) revealed = 1;
                if (_Reveal <= .001) revealed = 0;
                half seam = (1 - saturate(abs(d) / _EdgeWidth)) * step(.001, _Reveal) * step(_Reveal, .999);

                half3 col = lerp(shadowBody, lit, revealed);
                col += _EdgeColor.rgb * seam * (.7 + .3 * sin(t * 6 + h * 30));
                col = lerp(col, _HitColor.rgb * 2, _Hit * .6);
                col = MixFog(col, i.fog);
                return half4(col, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            float3 _LightDirection; float3 _LightPosition;
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            float4 vert (Attributes v) : SV_POSITION
            {
                float3 ws = TransformObjectToWorld(v.positionOS.xyz);
                float3 nws = TransformObjectToWorldNormal(v.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 ld = normalize(_LightPosition - ws);
                #else
                float3 ld = _LightDirection;
                #endif
                float4 cs = TransformWorldToHClip(ApplyShadowBias(ws, nws, ld));
                #if UNITY_REVERSED_Z
                cs.z = min(cs.z, UNITY_NEAR_CLIP_VALUE);
                #else
                cs.z = max(cs.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return cs;
            }
            half4 frag () : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask R
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4 vert (float4 positionOS : POSITION) : SV_POSITION { return TransformObjectToHClip(positionOS.xyz); }
            half4 frag () : SV_Target { return 0; }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
