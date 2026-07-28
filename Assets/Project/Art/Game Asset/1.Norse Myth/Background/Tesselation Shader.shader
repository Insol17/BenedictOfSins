Shader "Custom/URP_POM_Floor"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _HeightMap ("Height Map", 2D) = "black" {}
        _HeightScale ("Height Scale", Range(0, 1)) = 0.1

        _Steps ("POM Steps", Range(4, 64)) = 32
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 viewDirTS   : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_HeightMap);      SAMPLER(sampler_HeightMap);

            float _HeightScale;
            int _Steps;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 tangentWS = TransformObjectToWorldDir(IN.tangentOS.xyz);
                float3 bitangentWS = cross(normalWS, tangentWS) * IN.tangentOS.w;

                float3 viewDirWS = normalize(_WorldSpaceCameraPos - positionWS);
                float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);
                OUT.viewDirTS = mul(TBN, viewDirWS);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float2 ParallaxOcclusionMapping(float2 uv, float3 viewDirTS)
            {
                const int MAX_STEPS = 64; // 실제 최대 반복 횟수
                float stepSize = 1.0 / _Steps;
                float layerHeight = 1.0;
                float2 deltaUV = viewDirTS.xy * (_HeightScale * 1.5) / (abs(viewDirTS.z) + 0.0001);


                float2 uvOffset = uv;
                float height = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, uvOffset, 0).r;


                // 고정된 반복문 안에서 조건적 종료
                [loop]
                for (int i = 0; i < MAX_STEPS; i++)
                {
                    if (i >= _Steps) break;                // 설정된 반복 수만큼만 진행
                    if (height < layerHeight) break;

                    uvOffset -= deltaUV * stepSize;
                    height = SAMPLE_TEXTURE2D_LOD(_HeightMap, sampler_HeightMap, uvOffset, 0).r;

                    layerHeight -= stepSize * 1.25; // 또는 1.5

                }

                return uvOffset;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                float3 viewDirTS = normalize(IN.viewDirTS);
                float2 uv = ParallaxOcclusionMapping(IN.uv, viewDirTS);

                float3 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;

                return float4(baseColor, 1.0);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
