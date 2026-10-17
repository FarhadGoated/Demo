// Procedural infernal skybox: churning smoke, molten horizon, distant fire glow and drifting embers.
// Pure math (no textures) so it is license-free and cheap on integrated GPUs.
Shader "HellSlice/Hell Sky"
{
    Properties
    {
        _ZenithColor ("Zenith", Color) = (0.035, 0.012, 0.016, 1)
        _MidColor ("Smoke", Color) = (0.20, 0.035, 0.02, 1)
        _HorizonColor ("Horizon glow", Color) = (1.0, 0.32, 0.06, 1)
        _GroundColor ("Below horizon", Color) = (0.06, 0.012, 0.008, 1)
        _HorizonIntensity ("Horizon intensity", Range(0, 6)) = 2.4
        _CloudScale ("Cloud scale", Range(0.5, 8)) = 2.6
        _CloudSpeed ("Cloud speed", Range(0, 0.2)) = 0.025
        _CloudContrast ("Cloud contrast", Range(0.5, 4)) = 1.8
        _FireColumns ("Distant fire columns", Range(0, 2)) = 0.9
        _EmberDensity ("Sky embers", Range(0, 1)) = 0.55
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _ZenithColor, _MidColor, _HorizonColor, _GroundColor;
                half _HorizonIntensity, _CloudScale, _CloudSpeed, _CloudContrast, _FireColumns, _EmberDensity;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 dir : TEXCOORD0; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.dir = v.positionOS.xyz;
                return o;
            }

            float hash21(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
            float noise(float2 p)
            {
                float2 i = floor(p), f = frac(p);
                float2 u = f * f * (3 - 2 * f);
                return lerp(lerp(hash21(i), hash21(i + float2(1, 0)), u.x), lerp(hash21(i + float2(0, 1)), hash21(i + 1), u.x), u.y);
            }
            float fbm(float2 p)
            {
                float v = 0, a = .5;
                [unroll] for (int k = 0; k < 5; k++) { v += a * noise(p); p = p * 2.03 + float2(1.7, 9.2); a *= .5; }
                return v;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 d = normalize(i.dir);
                float t = _Time.y;
                float h = d.y;
                // Spherical-ish cloud plane coordinates; stretched toward the horizon for depth.
                float2 uv = d.xz / (abs(h) + .32) * _CloudScale;
                float2 flow = float2(t * _CloudSpeed, t * _CloudSpeed * .6);
                float warp = fbm(uv * .6 + flow);
                float clouds = fbm(uv + warp * 1.6 - flow * 1.4);
                clouds = saturate(pow(clouds, _CloudContrast) * 1.9);

                float horizon = exp(-abs(h) * 9.0);
                float upper = saturate(h);
                half3 col = lerp(_MidColor.rgb, _ZenithColor.rgb, smoothstep(0, .75, upper));
                // Clouds are lit from below by the fire.
                half3 underlit = _HorizonColor.rgb * (.25 + .75 * horizon);
                col = lerp(col, col * .35 + underlit * .55, clouds * (1 - upper * .6));
                col += _HorizonColor.rgb * horizon * _HorizonIntensity * (.55 + .45 * clouds);

                // Distant pillars of fire flickering on the horizon.
                float az = atan2(d.z, d.x);
                float columns = pow(saturate(noise(float2(az * 7, t * .4)) * 1.25 - .25), 3) * horizon;
                col += _HorizonColor.rgb * columns * _FireColumns * 3 * (.7 + .3 * sin(t * 3 + az * 20));

                // Tiny drifting embers in the upper sky.
                float2 eu = d.xz / (h + 1.2) * 40 + float2(0, -t * .35);
                float2 cell = floor(eu);
                float2 sp = frac(eu) - .5 + (float2(hash21(cell), hash21(cell + 7)) - .5) * .7;
                float ember = smoothstep(.06, 0, length(sp)) * step(1 - _EmberDensity * .12, hash21(cell + 3));
                ember *= .5 + .5 * sin(t * 4 + hash21(cell) * 40);
                col += half3(1, .45, .12) * ember * 2.5 * saturate(h * 3 + .3);

                // Below the horizon fade into a dark molten haze.
                col = lerp(col, _GroundColor.rgb + _HorizonColor.rgb * horizon * .6, saturate(-h * 6));
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
