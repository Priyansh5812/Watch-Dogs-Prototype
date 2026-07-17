

Shader "Unlit/NormalShader"
{
    Properties
    {
        _StartColorPos("StartColorPos",Range(0,1)) = 0
        _EndColorPos("EndColorPos",Range(0,1)) = 1
        _StartColor("StartColor",Color) = (0,0,0,1)
        _EndColor("EndColor",Color) = (1,1,1,1)

    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent"
        }
        LOD 100

        Pass
        {   Blend One One
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"


            float4 _StartColor;
            float4 _EndColor;
            float _StartColorPos;
            float _EndColorPos;


            struct meshData
            {
                float4 vertex : POSITION;
                float3 normal: NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f // aka Interpolators 
            {
                float3 normal : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            float InverseLerp(float a , float b , float v)
            {
                return (v-a)/(b-a);    
            }

            v2f vert (meshData v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }



            float4 frag (v2f i) : SV_Target
            {
                float t = InverseLerp(_StartColorPos, _EndColorPos , i.uv.x);
                t = clamp(t , 0 , 1);
                return lerp(_StartColor , _EndColor , t);
            }

            ENDCG
        }
    }
}
