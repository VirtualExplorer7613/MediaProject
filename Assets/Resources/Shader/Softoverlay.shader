Shader "Custom/SoftGlowOverlay"
{
    Properties
    {
        _Color ("Tint Color", Color) = (1,1,1,0.2)
        _GlowStrength ("Glow Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _GlowStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);  // 화면 중심에서 거리
                float glow = smoothstep(0.4, 0.9, dist); // 바깥쪽으로 부드럽게 fade out
                float fade = 1.0 - glow;

                fixed4 col = _Color;
                col.rgb += fade * _GlowStrength; // 빛 번짐 효과
                return col;
            }
            ENDCG
        }
    }
}
