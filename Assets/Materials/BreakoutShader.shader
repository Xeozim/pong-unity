Shader "Xeosim/BreakoutShader"
{
    Properties
    {
        _Palette ("Palette", 2D) = "white" {}

        // Minimum and maximum Y value which define how the palette is applied
        // If world Y position is less than or equal to _MinY then the palette
        // texture will be sampled at UV(0.5,0.0). If it's greater than or equal
        // to _MaxY then the palette texture will be sampled at UV(0.5,1.0)
        _MinY ("Min Y World", Float) = -1.0
        _MaxY ("Max Y World", Float) = 2.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            struct Interpolators
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            // The input render texture
            sampler2D _Palette;
            float _MinY;
            float _MaxY;

            Interpolators vert (appdata v)
            {
                Interpolators o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                // Normalize Y position to [0, 1] based on Min and Max Y
                float normalizedY = saturate((i.worldPos.y - _MinY) / (_MaxY - _MinY));

                // Use normalized Y as the V coordinate for sampling
                fixed4 color = tex2D(_Palette, float2(0.5, normalizedY));

                return color;
            }
            ENDCG
        }
    }
}