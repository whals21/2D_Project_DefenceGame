Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size", Float) = 1.0
        _Threshold ("Outline Threshold", Float) = 0.3
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OutlineColor;
            float _OutlineSize;
            float _Threshold;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 offset = float2(_OutlineSize/_ScreenParams.x, _OutlineSize/_ScreenParams.y);
                fixed4 col = tex2D(_MainTex, i.texcoord);
                fixed4 outline = _OutlineColor;
                fixed4 neighbor1 = tex2D(_MainTex, i.texcoord + float2(offset.x, 0));
                fixed4 neighbor2 = tex2D(_MainTex, i.texcoord + float2(-offset.x, 0));
                fixed4 neighbor3 = tex2D(_MainTex, i.texcoord + float2(0, offset.y));
                fixed4 neighbor4 = tex2D(_MainTex, i.texcoord + float2(0, -offset.y));

                float diff = max(
                    max(distance(col.rgb, neighbor1.rgb), distance(col.rgb, neighbor2.rgb)),
                    max(distance(col.rgb, neighbor3.rgb), distance(col.rgb, neighbor4.rgb))
                );
                
                if (diff > _Threshold) return _OutlineColor;

                // if (col.a == 0 &&
                //     (tex2D(_MainTex, i.texcoord + offset).a > 0 ||
                //      tex2D(_MainTex, i.texcoord - offset).a > 0 ||
                //      tex2D(_MainTex, i.texcoord + float2(offset.x, -offset.y)).a > 0 ||
                //      tex2D(_MainTex, i.texcoord + float2(-offset.x, offset.y)).a > 0))
                // {
                //     return outline;
                // }

                return col;
            }
            ENDCG
        }
    }
}