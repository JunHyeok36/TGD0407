Shader "Custom/XRaySpriteUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1, 1, 1, 1)
        _TintColor ("Particle Tint (Optional)", Color) = (1, 1, 1, 1)
        _XRayAlphaRatio ("XRay Occluded Alpha Ratio", Range(0.1, 1.0)) = 0.45
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+100"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SRPDefaultUnlit"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _CameraDepthTexture;
            fixed4 _Color;
            fixed4 _TintColor;
            float _XRayAlphaRatio;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos       : SV_POSITION;
                float4 color     : COLOR;
                float2 uv        : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color * _TintColor;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = tex * i.color;

                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);

                #if defined(UNITY_REVERSED_Z)
                bool isOccluded = (rawDepth > i.pos.z + 0.0008);
                #else
                bool isOccluded = (rawDepth < i.pos.z - 0.0008);
                #endif

                if (isOccluded)
                {
                    col.a *= _XRayAlphaRatio;
                    col.rgb = saturate(col.rgb * 1.15 + 0.05);
                }

                return col;
            }
            ENDCG
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _CameraDepthTexture;
            fixed4 _Color;
            fixed4 _TintColor;
            float _XRayAlphaRatio;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos       : SV_POSITION;
                float4 color     : COLOR;
                float2 uv        : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color * _TintColor;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed4 col = tex * i.color;

                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);

                #if defined(UNITY_REVERSED_Z)
                bool isOccluded = (rawDepth > i.pos.z + 0.0008);
                #else
                bool isOccluded = (rawDepth < i.pos.z - 0.0008);
                #endif

                if (isOccluded)
                {
                    col.a *= _XRayAlphaRatio;
                    col.rgb = saturate(col.rgb * 1.15 + 0.05);
                }

                return col;
            }
            ENDCG
        }
    }
}
