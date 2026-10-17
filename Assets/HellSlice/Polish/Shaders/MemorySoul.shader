Shader "HellSlice/Memory Soul"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Soul Tint", Color) = (0.44, 0.62, 0.72, 0.65)
        _EmissionColor("Soul Glow", Color) = (0.02, 0.045, 0.065, 1)
        _MinimumLight("Minimum Soul Lighting", Range(0, 2)) = 0.85
        [Toggle] _AlphaClip("Texture Cutout", Float) = 0
        _Cutoff("Texture Cutoff", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="TransparentCutout" "Queue"="AlphaTest" }
        Pass
        {
            Name "SoulForward"
            Tags { "LightMode"="UniversalForward" }
            ZWrite On
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half4 _EmissionColor;
                half _AlphaClip;
                half _Cutoff;
                half _MinimumLight;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                if (_AlphaClip > 0.5h) clip(tex.a - _Cutoff);

                // Every body layer uses the same screen-space mask. Visible face
                // pixels write depth, hiding teeth/eyes instead of blending them through.
                const float thresholds[16] = {
                    0, 8, 2, 10, 12, 4, 14, 6,
                    3, 11, 1, 9, 15, 7, 13, 5
                };
                uint2 pixel = (uint2)input.positionCS.xy;
                float threshold = (thresholds[(pixel.y & 3u) * 4u + (pixel.x & 3u)] + 0.5) / 16.0;
                clip(_BaseColor.a - threshold);

                half3 normal = normalize(input.normalWS);
                Light light = GetMainLight();
                half3 lighting = max(SampleSH(normal), half3(_MinimumLight, _MinimumLight, _MinimumLight));
                lighting += light.color * saturate(dot(normal, light.direction)) * light.distanceAttenuation;
                return half4(tex.rgb * _BaseColor.rgb * lighting + _EmissionColor.rgb, 1);
            }
            ENDHLSL
        }
    }
}
