Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1.5
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                c.rgb *= c.a;

                // 외곽선 검사
                float outline = 0;
                float pixelWidth = _MainTex_TexelSize.x * _OutlineWidth;
                float pixelHeight = _MainTex_TexelSize.y * _OutlineWidth;

                // 8방향 체크
                outline += tex2D(_MainTex, i.uv + float2(pixelWidth, 0)).a;
                outline += tex2D(_MainTex, i.uv + float2(-pixelWidth, 0)).a;
                outline += tex2D(_MainTex, i.uv + float2(0, pixelHeight)).a;
                outline += tex2D(_MainTex, i.uv + float2(0, -pixelHeight)).a;
                outline += tex2D(_MainTex, i.uv + float2(pixelWidth, pixelHeight)).a;
                outline += tex2D(_MainTex, i.uv + float2(-pixelWidth, pixelHeight)).a;
                outline += tex2D(_MainTex, i.uv + float2(pixelWidth, -pixelHeight)).a;
                outline += tex2D(_MainTex, i.uv + float2(-pixelWidth, -pixelHeight)).a;

                // 외곽선 적용
                if (c.a == 0 && outline > 0)
                {
                    c = _OutlineColor;
                }

                return c;
            }
            ENDCG
        }
    }
}
