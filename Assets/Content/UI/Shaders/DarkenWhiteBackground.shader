Shader "Hidden/DarkenWhiteBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Threshold ("White Threshold", Range(0, 1)) = 0.9
        _Darkness ("Darkness Amount", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Threshold;
            float _Darkness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // Если пиксель близок к белому, затемняем его
                if (col.r > _Threshold && col.g > _Threshold && col.b > _Threshold)
                {
                    col.rgb *= _Darkness;
                }
                return col;
            }
            ENDCG
        }
    }
}