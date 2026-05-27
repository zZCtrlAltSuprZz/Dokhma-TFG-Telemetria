Shader "Hidden/Vignette"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Float) = 0.4
        _Softness ("Softness", Float) = 0.5
        _Color ("Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Intensity;
            float _Softness;
            fixed4 _Color;

            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 uv = i.uv - 0.5;
                float dist = dot(uv, uv);
                float vignette = smoothstep(_Intensity, _Intensity - _Softness, dist);
                col.rgb = lerp(_Color.rgb, col.rgb, vignette);
                return col;
            }
            ENDCG
        }
    }
}