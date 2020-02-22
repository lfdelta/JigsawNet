/*Shader "Unlit/OutlineShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Thickness ("Thickness", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull front

            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members normal)
#pragma exclude_renderers d3d11
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL0;
            };

            float4 _Color;
            float _Thickness;

            v2f vert (appdata_base v)
            {
                v2f o;
                float4 offset;
                offset.xyz = normalize(v.normal) * _Thickness;
                offset.w = 0;
                o.vertex = UnityObjectToClipPos(v.vertex + offset);
                o.normal = -v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Color"
}*/


Shader "Unlit/OutlineShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Scale ("Scale", float) = 1.1
        _Alpha ("Alpha", float) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        LOD 100

        Pass
        {
            Cull front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _Scale;
            fixed _Alpha;

            v2f vert (appdata_base v)
            {
                v2f o;
                
                o.vertex = UnityObjectToClipPos(v.vertex * _Scale);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                clip(_Alpha - 0.01);
                return _Color;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Color"
}