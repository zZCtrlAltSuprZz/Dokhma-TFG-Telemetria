Shader "Custom/AltarFill"
{
    Properties
    {
        _FillAmount ("Fill Amount", Range(0,1)) = 0
        _BloodColor ("Blood Color", Color) = (0.4, 0.02, 0.02, 1)
        _GlowColor ("Glow Color", Color) = (0.8, 0.05, 0.05, 1)
        _EmissionIntensity ("Emission Intensity", Float) = 2.5
        _EdgeWidth ("Edge Width", Range(0, 0.15)) = 0.04
        _WaveSpeed ("Wave Speed", Float) = 0.8
        _WaveStrength ("Wave Strength", Range(0, 0.05)) = 0.015
        _BubbleSpeed ("Bubble Speed", Float) = 1.2
        _BubbleScale ("Bubble Scale", Float) = 8.0
        _DepthDarkness ("Depth Darkness", Range(0,1)) = 0.6
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float _FillAmount;
            float4 _BloodColor;
            float4 _GlowColor;
            float _EmissionIntensity;
            float _EdgeWidth;
            float _WaveSpeed;
            float _WaveStrength;
            float _BubbleSpeed;
            float _BubbleScale;
            float _DepthDarkness;

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash(i), hash(i + float2(1,0)), f.x),
                    lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), f.x),
                    f.y
                );
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float t = _Time.y;

                float wave1 = sin(uv.x * 18.0 + t * _WaveSpeed) * _WaveStrength;
                float wave2 = sin(uv.x * 9.0 - t * _WaveSpeed * 0.6) * (_WaveStrength * 0.5);
                float surface = _FillAmount + wave1 + wave2;

                if (uv.y > surface) discard;

                float2 bubbleUV = uv * _BubbleScale;
                bubbleUV.y -= t * _BubbleSpeed;
                float bubbles = noise(bubbleUV);
                bubbles = smoothstep(0.72, 0.78, bubbles) * 0.4;

                float depth = 1.0 - (uv.y / max(surface, 0.001));
                float darkness = lerp(1.0, 1.0 - _DepthDarkness, depth);

                float edgeDist = surface - uv.y;
                float edge = smoothstep(_EdgeWidth, 0.0, edgeDist);

                float3 baseColor = _BloodColor.rgb * darkness;
                baseColor += _GlowColor.rgb * bubbles;

                float3 glowColor = _GlowColor.rgb * _EmissionIntensity;
                float3 finalColor = lerp(baseColor, glowColor, edge);

                float emissionMask = saturate(edge + bubbles * 0.5);
                float3 emission = _GlowColor.rgb * _EmissionIntensity * emissionMask;
                finalColor += emission;

                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }

    FallBack "Standard"
}