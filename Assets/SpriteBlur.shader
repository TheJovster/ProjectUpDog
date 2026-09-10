// Cheap separable-ish gaussian for sprites: 5 taps horizontally, 5 vertically,
// averaged. 9 samples total instead of the 25 a true 5x5 kernel would cost.
Shader "UPDOG/SpriteBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurRadius ("Blur Radius (texels)", Range(0, 16)) = 2
        _Strength ("Strength", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha   // premultiplied, matches Unity's sprite default

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _BlurRadius;
            float _Strength;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 src = tex2D(_MainTex, i.uv);

                // Gaussian weights for offsets 0, 1, 2.
                const float w0 = 0.375;
                const float w1 = 0.25;
                const float w2 = 0.0625;

                float2 texel = _MainTex_TexelSize.xy * _BlurRadius;

                fixed4 h =
                    tex2D(_MainTex, i.uv) * w0 +
                    tex2D(_MainTex, i.uv + float2( texel.x, 0)) * w1 +
                    tex2D(_MainTex, i.uv + float2(-texel.x, 0)) * w1 +
                    tex2D(_MainTex, i.uv + float2( texel.x * 2, 0)) * w2 +
                    tex2D(_MainTex, i.uv + float2(-texel.x * 2, 0)) * w2;

                fixed4 v =
                    tex2D(_MainTex, i.uv) * w0 +
                    tex2D(_MainTex, i.uv + float2(0,  texel.y)) * w1 +
                    tex2D(_MainTex, i.uv + float2(0, -texel.y)) * w1 +
                    tex2D(_MainTex, i.uv + float2(0,  texel.y * 2)) * w2 +
                    tex2D(_MainTex, i.uv + float2(0, -texel.y * 2)) * w2;

                fixed4 blurred = (h + v) * 0.5;

                // Strength 0 = untouched sprite, 1 = fully blurred.
                fixed4 col = lerp(src, blurred, _Strength);
                col *= i.color;
                col.rgb *= col.a;   // premultiply for the blend mode above
                return col;
            }
        ENDCG
        }
    }
}
