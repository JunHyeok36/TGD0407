Shader "Custom/URP/ToonShader"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        
        [Header(Toon Colors)]
        _HighlightColor("Highlight Color", Color) = (1, 1, 1, 1)
        _MidtoneColor("Midtone Color", Color) = (0.7, 0.7, 0.7, 1)
        _ShadowColor("Shadow Color", Color) = (0.3, 0.3, 0.3, 1)
        
        [Header(Thresholds)]
        _HighlightThreshold("Highlight Threshold", Range(0, 1)) = 0.8
        _ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.2
        _BlendSmoothness("Blend Smoothness", Range(0, 0.5)) = 0.01

        [Header(Cloud Canopy Shadows)]
        _CloudNoise("Cloud Noise", 2D) = "white" {}
        _CloudScale("Cloud Scale", Float) = 0.1
        _CloudSpeed("Cloud Speed", Vector) = (0.5, 0.5, 0, 0)
        _CloudShadowIntensity("Cloud Shadow Intensity", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            TEXTURE2D(_CloudNoise);
            SAMPLER(sampler_CloudNoise);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _HighlightColor;
                float4 _MidtoneColor;
                float4 _ShadowColor;
                float _HighlightThreshold;
                float _ShadowThreshold;
                float _BlendSmoothness;
                
                float _CloudScale;
                float4 _CloudSpeed;
                float _CloudShadowIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                half4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                half3 albedo = baseTex.rgb * _BaseColor.rgb;

                float3 normalWS = normalize(IN.normalWS);
                
                // Get main light
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(IN.positionWS));
                
                // 1. Compute Base NdotL
                float NdotL = dot(normalWS, mainLight.direction);
                float lightIntensity = NdotL * 0.5 + 0.5; // remap to 0-1
                
                // Apply shadow attenuation from cast shadows
                lightIntensity *= mainLight.shadowAttenuation;
                
                // 2. Procedural Cloud/Canopy Shadows
                float2 cloudUV = IN.positionWS.xz * _CloudScale + _Time.y * _CloudSpeed.xy;
                float cloudNoise = SAMPLE_TEXTURE2D(_CloudNoise, sampler_CloudNoise, cloudUV).r;
                // Darken intensity where clouds exist
                lightIntensity *= lerp(1.0, cloudNoise, _CloudShadowIntensity);

                // 3. 3-Tone Toon Shading
                // Smoothly blend between Shadow, Midtone, and Highlight
                float shadowToMid = smoothstep(_ShadowThreshold - _BlendSmoothness, _ShadowThreshold + _BlendSmoothness, lightIntensity);
                float midToHigh = smoothstep(_HighlightThreshold - _BlendSmoothness, _HighlightThreshold + _BlendSmoothness, lightIntensity);
                
                half3 toonColor = lerp(_ShadowColor.rgb, _MidtoneColor.rgb, shadowToMid);
                toonColor = lerp(toonColor, _HighlightColor.rgb, midToHigh);
                
                // Multiply with light color and base albedo
                half3 finalColor = albedo * mainLight.color * toonColor;
                
                // Add Ambient (slightly)
                half3 ambient = SampleSH(normalWS) * albedo;
                finalColor += ambient * 0.2; // Keep ambient low to preserve toon contrast

                return half4(finalColor, baseTex.a * _BaseColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
