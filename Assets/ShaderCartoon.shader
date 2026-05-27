Shader "Custom/CartoonShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _AmbientColor ("Ambient Color", Color) = (0.4, 0.4, 0.4, 1)
        _SpecularColor ("Specular Color", Color) = (0.9, 0.9, 0.9, 1)
        _Glossiness ("Glossiness", Float) = 32
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimAmount ("Rim Amount", Range(0,1)) = 0.716
        _RimThreshold ("Rim Threshold", Range(0,1)) = 0.1
    }

    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float3 worldNormal : NORMAL;
                float3 viewDir  : TEXCOORD1;
                SHADOW_COORDS(2)
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _Color;
            float4    _AmbientColor;
            float4    _SpecularColor;
            float     _Glossiness;
            float4    _RimColor;
            float     _RimAmount;
            float     _RimThreshold;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos         = UnityObjectToClipPos(v.vertex);
                o.uv          = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir     = WorldSpaceViewDir(v.vertex);
                TRANSFER_SHADOW(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 normal  = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);

                // Luz difusa con cel shading (3 bandas)
                float NdotL    = dot(normal, _WorldSpaceLightPos0.xyz);
                float shadow   = SHADOW_ATTENUATION(i);
                float lightInt = NdotL * shadow;
                float light = lightInt < 0.0 ? 0.0 : lightInt < 0.3 ? 0.2 : lightInt < 0.6 ? 0.6 : 1.0;
                float4 diffuse = light * _LightColor0;

                // Specular
                float3 halfVec   = normalize(_WorldSpaceLightPos0.xyz + viewDir);
                float  NdotH     = dot(normal, halfVec);
                float  specInt   = pow(NdotH * light, _Glossiness * _Glossiness);
                float  specSmooth = smoothstep(0.005, 0.01, specInt);
                float4 specular  = specSmooth * _SpecularColor;

                // Rim light
                float rimDot    = 1 - dot(viewDir, normal);
                float rimInt    = rimDot * pow(NdotL, _RimThreshold);
                float rimSmooth = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimInt);
                float4 rim      = rimSmooth * _RimColor;

                float4 tex = tex2D(_MainTex, i.uv) * _Color;

                return tex * (_AmbientColor + diffuse + specular + rim);
            }
            ENDCG
        }

        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}