Shader "Custom/Toon"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _RampTex ("Toon Ramp", 2D) = "gray" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0, 0.05)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // -------------------------------
        // OUTLINE PASS
        // -------------------------------
        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode"="Always" }

            Cull Front
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _OutlineColor;
            float _OutlineWidth;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f { float4 pos : SV_POSITION; };

            v2f vert(appdata v)
            {
                v2f o;

                // inflate mesh along normals (object space)
                float3 posOS = v.vertex.xyz + normalize(v.normal) * _OutlineWidth;
                o.pos = UnityObjectToClipPos(float4(posOS, 1));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // -------------------------------
        // TOON LIT PASS
        // -------------------------------
        Pass
        {
            Name "TOON"
            Tags { "LightMode"="ForwardBase" }

            Cull Back
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _RampTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldN   : TEXCOORD2;

                SHADOW_COORDS(3)
                UNITY_FOG_COORDS(4)
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldN = UnityObjectToWorldNormal(v.normal);

                TRANSFER_SHADOW(o);
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 albedo = tex2D(_MainTex, i.uv).rgb;

                fixed3 N = normalize(i.worldN);
                fixed3 L = normalize(UnityWorldSpaceLightDir(i.worldPos));

                fixed ndotl = saturate(dot(N, L));

                // shadow factor (0..1)
                fixed shadowAtten = SHADOW_ATTENUATION(i);

                // ramp lookup: use ndotl as x coordinate
                fixed ramp = tex2D(_RampTex, float2(ndotl, 0.5)).r;

                // simple ambient + toon direct
                fixed3 ambient = ShadeSH9(float4(N, 1.0));
                fixed3 direct = _LightColor0.rgb * ramp * shadowAtten;

                fixed3 col = albedo * (ambient + direct);

                fixed4 outCol = fixed4(col, 1);
                UNITY_APPLY_FOG(i.fogCoord, outCol);
                return outCol;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
