Shader "Custom/FoveatedMaskShader"
{
    Properties
    {
        _PeripheralTex ("Peripheral Texture", 2D) = "white" {}
        _FovealTex ("Foveal Texture", 2D) = "white" {}
        _Offset ("Foveal Offset", Vector) = (0,0,0,0)
        _FovealWindowSize ("Foveal Window Size", Vector) = (0.5, 0.5, 0, 0) // relative size
        _BorderColor ("Border Color", Color) = (1,0,0,1)
        _BorderThickness ("Border Thickness", Float) = 0.01
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

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
                float2 uv  : TEXCOORD0;
            };

            sampler2D _PeripheralTex;
            sampler2D _FovealTex;
            float4 _Offset;
            float4 _FovealWindowSize;
            float4 _BorderColor;
            float _BorderThickness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the peripheral texture
                fixed4 peripheralColor = tex2D(_PeripheralTex, i.uv);

                // Calculate the position relative to the center
                float2 relativePos = i.uv - 0.5;
                relativePos += _Offset.xy; // Corrected: Add offset to move foveal window in the intended direction

                // Define half window size
                float2 halfWin = _FovealWindowSize.xy * 0.5;

                // Check if within foveal window
                bool insideFoveal = (relativePos.x > -halfWin.x) && (relativePos.x < halfWin.x) &&
                                     (relativePos.y > -halfWin.y) && (relativePos.y < halfWin.y);

                fixed4 finalColor = peripheralColor;

                if (insideFoveal)
                {
                    // Map relative position to foveal texture UV
                    float2 fovealUV = (relativePos / _FovealWindowSize.xy) + 0.5;
                    fixed4 fovealColor = tex2D(_FovealTex, fovealUV);

                    finalColor = fovealColor;

                    // Draw border
                    float border = _BorderThickness;
                    bool isBorder = (abs(relativePos.x) > (halfWin.x - border)) || 
                                    (abs(relativePos.y) > (halfWin.y - border));

                    if (isBorder)
                    {
                        finalColor = _BorderColor;
                    }
                }

                return finalColor;
            }
            ENDCG
        }
    }
}
