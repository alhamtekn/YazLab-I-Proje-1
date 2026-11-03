// Tapınak dokuları için üç yönlü (triplanar) Standard shader.
// Sahne kutulardan kurulduğu için UV'ler her yüzde 0-1 arasıdır; büyük bir duvarda doku gerilirdi.
// Bu shader dokuyu dünya (ya da nesne) koordinatından örnekler: _Tiling metrede bir tekrar demektir.
Shader "Lumen/TempleTriplanar"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Albedo", 2D) = "white" {}
        _EmissionMap ("Emission", 2D) = "black" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _Tiling ("Tiling (m)", Float) = 2
        [Toggle] _ObjectSpace ("Object Space (hareketli nesneler)", Float) = 0
        _Glossiness ("Smoothness", Range(0, 1)) = 0.2
        _Metallic ("Metallic", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _EmissionMap;
        fixed4 _Color;
        half4 _EmissionColor;
        float _Tiling;
        float _ObjectSpace;
        half _Glossiness;
        half _Metallic;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        float4 Triplanar(sampler2D tex, float3 p, float3 n)
        {
            float3 w = pow(abs(n), 4);
            w /= (w.x + w.y + w.z);
            return tex2D(tex, p.zy / _Tiling) * w.x
                 + tex2D(tex, p.xz / _Tiling) * w.y
                 + tex2D(tex, p.xy / _Tiling) * w.z;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Lumi ve kapı gibi hareket eden nesnelerde doku nesneyle birlikte gitsin diye nesne uzayı kullanılır.
            float3 objectPos = mul(unity_WorldToObject, float4(IN.worldPos, 1)).xyz;
            float3 objectNormal = normalize(mul((float3x3)unity_WorldToObject, IN.worldNormal));
            float3 p = lerp(IN.worldPos, objectPos, _ObjectSpace);
            float3 n = lerp(IN.worldNormal, objectNormal, _ObjectSpace);

            fixed4 albedo = Triplanar(_MainTex, p, n) * _Color;
            o.Albedo = albedo.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = Triplanar(_EmissionMap, p, n).rgb * _EmissionColor.rgb;
            o.Alpha = 1;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
