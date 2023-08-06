Shader "PostProcessing/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Spread("Standard Deviation (Spread)", Float) = 0
        _GridSize("Grid Size", Integer) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #define E 2.71828f

        Texture2D _MainTex;
        SamplerState sampler_MainTex;

        Texture2D _FirstBlurTex;
        SamplerState sampler_FirstBlurTex;

        Texture2D _SecondBlurTex;
        SamplerState sampler_SecondBlurTex;

        Texture2D _H;
        SamplerState sampler_H;

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            uint _GridSize;
            float _Spread;
        CBUFFER_END

        float gaussian(int x)
        {
            float sigmaSqu = _Spread * _Spread;
            return (1 / sqrt(TWO_PI * sigmaSqu)) * pow(E, -(x * x) / (2 * sigmaSqu));
        }

        struct appdata
        {
            float4 positionOS : Position;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 positionCS : SV_Position;
            float2 uv : TEXCOORD0;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
            o.uv = v.uv;
            return o;
        }

        ENDHLSL

        Pass
        {
            Name "Horizontal"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag_horizontal

            float4 frag_horizontal (v2f i) : SV_Target
            {
                float3 col = float3(0.0f, 0.0f, 0.0f);
                float gridSum = 0.0f;

                int upper = ((_GridSize - 1) / 2);
                int lower = -upper;

                for (int x = lower; x <= upper; ++x)
                {
                    float gauss = gaussian(x);
                    gridSum += gauss;
                    float2 uv = i.uv + float2(_MainTex_TexelSize.x * x, 0.0f);
                    col += gauss * _MainTex.Sample(sampler_MainTex, uv).rgb;
                }

                col /= gridSum;
                return float4(col, 1.0f);
            }

            ENDHLSL
        }

        Pass
        {
            Name "Vertical"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag_vertical

            float4 frag_vertical (v2f i) : SV_Target
            {
                float3 col = float3(0.0f, 0.0f, 0.0f);
                float gridSum = 0.0f;

                int upper = (_GridSize - 1) / 2;
                int lower = -upper;

                for (int y = lower; y <= upper; ++y)
                {
                    float gauss = gaussian(y);
                    gridSum += gauss;
                    float2 uv = i.uv + float2(0.0f, _MainTex_TexelSize.y * y);
                    col += gauss * _MainTex.Sample(sampler_MainTex, uv).rgb;
                }

                col /= gridSum;
                return float4(col, 1.0f);
            }

            ENDHLSL
        }

        Pass
        {
            Name "Subtraction"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag_subtraction

            float4 frag_subtraction (v2f i) : SV_Target
            {
                float3 colFirst = _FirstBlurTex.Sample(sampler_FirstBlurTex, i.uv).rgb;
                float3 colSecond = _SecondBlurTex.Sample(sampler_SecondBlurTex, i.uv).rgb;
                float3 hatch = _H.Sample(sampler_H, i.uv).rgb;

                float3 result = colSecond - colFirst;

                if((result.r + result.g + result.b) * 20.0f < 0.1f){
                    // calculate the brightness of the resulting color
                    float brightness = (colFirst.r + colFirst.g + colFirst.b) * 0.5f;
                    // interpolate between white and black based on brightness
                    float3 color = lerp(colFirst / 0.1f, float3(0, 0, 0), 1.1f - brightness);

                    return float4(color, 1.0f);

                    
                }else{
                    return float4(0.0f, 0.0f, 0.0f, 1.0f);
                }

            }

            ENDHLSL
        }
    }
}
