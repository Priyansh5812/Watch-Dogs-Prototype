Shader "Unlit/TexturePainter"
{
    Properties
    {
        _UVScale("UvScale" , float) = 1
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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float4 GetColor(float3 pos)
            {
                float m = distance(float3(0,0,0) , pos);
                return float4(1,0,0, ceil(1- (m/0.1)));
            }

            float _UVScale;

            v2f vert (appdata v)
            {   
                v2f o;
                o.worldPos = mul(UNITY_MATRIX_M , v.vertex);
                o.uv = v.uv;
				float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2( 2, 2) - float2(1, 1)); //Normalized Coordinate System Conversion (-1,-1 bottom Left ; 1,1 Top right)
				o.vertex = uv * _UVScale; 
                return o;
                
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return GetColor(i.worldPos);
            }
            ENDCG
        }
    }
}
