Shader "Hidden/Custom/URP/PixelArtPostProcess"
{
    Properties
    {
        [Header(Pixelation)]
        _PixelResolutionX ("Pixel Resolution X", Float) = 320.0
        _PixelResolutionY ("Pixel Resolution Y", Float) = 180.0
        
        [Header(Outlines)]
        _OutlineColor ("Outline Color", Color) = (0.1, 0.1, 0.1, 1.0)
        _HighlightColor ("Highlight Color", Color) = (1.0, 1.0, 1.0, 0.5)
        _OutlineThickness ("Outline Thickness", Float) = 1.0
        _DepthThreshold ("Depth Threshold", Float) = 0.05
        _NormalThreshold ("Normal Threshold", Float) = 0.5
        
        [Header(Debug)]
        [KeywordEnum(Off, DepthEdge, NormalEdge, Combined)] _DebugMode ("Debug Mode", Float) = 0
        [Toggle(_ENABLE_PIXELATION)] _EnablePixelation ("Enable Pixelation", Float) = 1.0
        [Toggle(_ENABLE_OUTLINES)] _EnableOutlines ("Enable Outlines", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "PixelArtPostProcess"

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag
            
            // Optimization: Compile different shader variants so if statements are completely removed at runtime.
            #pragma shader_feature_local_fragment _ _ENABLE_PIXELATION
            #pragma shader_feature_local_fragment _ _ENABLE_OUTLINES
            #pragma shader_feature_local_fragment _ _DEBUGMODE_DEPTHEDGE _DEBUGMODE_NORMALEDGE _DEBUGMODE_COMBINED
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings FullscreenVert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            TEXTURE2D_X(_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float _PixelResolutionX;
                float _PixelResolutionY;
                
                float4 _OutlineColor;
                float4 _HighlightColor;
                float _OutlineThickness;
                float _DepthThreshold;
                float _NormalThreshold;
            CBUFFER_END

            half4 frag (Varyings input) : SV_Target
            {
                // 1. Pixelate the UV
                float2 res = float2(_PixelResolutionX, _PixelResolutionY);
                res = max(res, float2(1, 1)); 
                
                float2 pixelatedUV = input.uv;
                
#if defined(_ENABLE_PIXELATION)
                pixelatedUV = floor(input.uv * res) / res;
                pixelatedUV += (0.5 / res);
#else
                // If pixelation is off, the outline thickness should be relative to screen resolution
                res = _ScreenParams.xy; 
#endif

                // Sample the main color
                half4 color = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_PointClamp, pixelatedUV);
                half3 finalColor = color.rgb;

                // 2. Outlines
#if defined(_ENABLE_OUTLINES)
                float2 offset = (1.0 / res) * _OutlineThickness;

                // DEPTH EDGE (Dark Outline)
                float2 uv0 = pixelatedUV + float2(-offset.x, -offset.y); // BL
                float2 uv1 = pixelatedUV + float2( offset.x, -offset.y); // BR
                float2 uv2 = pixelatedUV + float2(-offset.x,  offset.y); // TL
                float2 uv3 = pixelatedUV + float2( offset.x,  offset.y); // TR

                float d0 = SampleSceneDepth(uv0);
                float d1 = SampleSceneDepth(uv1);
                float d2 = SampleSceneDepth(uv2);
                float d3 = SampleSceneDepth(uv3);
                
                // Roystan's Roberts Cross for Depth
                float depthFiniteDifference0 = d1 - d2;
                float depthFiniteDifference1 = d0 - d3;
                float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 100.0;

                // Center depth & normal for threshold modulation
                float centerDepth = SampleSceneDepth(pixelatedUV);
                float3 centerNormal = SampleSceneNormals(pixelatedUV);

                #if UNITY_REVERSED_Z
                    float realDepth = centerDepth;
                #else
                    float realDepth = lerp(UNITY_NEAR_CLIP_VALUE, 1, centerDepth);
                #endif
                float3 worldPos = ComputeWorldSpacePosition(pixelatedUV, realDepth, UNITY_MATRIX_I_VP);
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);

                // Modulate depth threshold by NdotV to prevent artifacts on angled planes
                float NdotV = 1.0 - saturate(dot(centerNormal, viewDir));
                float normalThreshold01 = saturate((NdotV - 0.5) / (1.0 - 0.5));
                float normalThreshold = normalThreshold01 * 7.0 + 1.0;
                
                float finalDepthThreshold = max(_DepthThreshold * centerDepth * normalThreshold, 0.0001);
                float isDepthEdge = step(finalDepthThreshold, edgeDepth);

                // Find foreground color for the outline
                float minDepth = min(min(d0, d1), min(d2, d3));
                float2 foregroundUV = uv0;
                foregroundUV = lerp(foregroundUV, uv1, step(d1, minDepth));
                foregroundUV = lerp(foregroundUV, uv2, step(d2, minDepth));
                foregroundUV = lerp(foregroundUV, uv3, step(d3, minDepth));
                
                half3 foregroundColor = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_PointClamp, foregroundUV).rgb;

                // NORMAL EDGE (Highlight)
                float3 n0 = SampleSceneNormals(uv0);
                float3 n1 = SampleSceneNormals(uv1);
                float3 n2 = SampleSceneNormals(uv2);
                float3 n3 = SampleSceneNormals(uv3);

                float3 normalFiniteDifference0 = n1 - n2;
                float3 normalFiniteDifference1 = n0 - n3;
                float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
                float isNormalEdge = step(max(_NormalThreshold, 0.0001), edgeNormal);

                // NORMAL EDGE (Highlight) blending
                half3 adaptiveHighlight = color.rgb + (_HighlightColor.rgb * (color.rgb * 2.0 + 0.1));
                finalColor = lerp(finalColor, adaptiveHighlight, isNormalEdge * _HighlightColor.a);
                
                // DEPTH EDGE (Dark Outline) blending
                half3 toneSteppedOutline = max(foregroundColor * _OutlineColor.rgb, _OutlineColor.rgb * 0.5); 
                finalColor = lerp(finalColor, toneSteppedOutline, isDepthEdge * _OutlineColor.a);
                
                // DEBUG MODE OUTPUT
#if defined(_DEBUGMODE_DEPTHEDGE)
                return half4(isDepthEdge.xxx, 1.0);
#elif defined(_DEBUGMODE_NORMALEDGE)
                return half4(isNormalEdge.xxx, 1.0);
#elif defined(_DEBUGMODE_COMBINED)
                return half4(max(isDepthEdge, isNormalEdge).xxx, 1.0);
#endif

#endif // _ENABLE_OUTLINES

                return half4(finalColor, color.a);
            }
            ENDHLSL
        }
    }
}
