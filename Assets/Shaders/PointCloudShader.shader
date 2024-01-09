Shader "Unlit/PointCloudShader"
{
    Properties
    {
        [MainTexture] _Mask ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            //#pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"            

            struct appdata
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct paintBlob
            {
                float4 vertex;
                float4 color;
                float2 uv;
            };

            struct v2g
            {
                float4 vertex : POSITION;
                float4 rotation : NORMAL;
                half4 colorAndRadius : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            uniform StructuredBuffer<paintBlob> _VertexBuffer;

            Texture2D _Mask;
            SamplerState sampler_Mask;

            float4 qmul(float4 q1, float4 q2)
            {
                return float4(
                    q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
                    q1.w * q2.w - dot(q1.xyz, q2.xyz)
                );
            }

            float3 rotate_vector(float3 v, float4 r)
            {
                float4 r_c = r * float4(-1, -1, -1, 1);
                return qmul(r, qmul(float4(v, 0), r_c)).xyz;
            }

            v2f vert (appdata v, uint vid : SV_VertexID)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v);
                //UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                paintBlob pb = _VertexBuffer[vid];

                o.vertex = UnityObjectToClipPos(pb.vertex);
                //o.rotation = pb.rotation;
                o.color = pb.color;
                o.uv = pb.uv;
                return o;
            }
             
            [maxvertexcount(6)]
            void geom(point v2g input[1], inout TriangleStream<g2f> outStream)
            {
                v2g i = input[0];
                g2f p0, p1, p2, p3;

                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_INITIALIZE_OUTPUT(g2f, p0);
                UNITY_INITIALIZE_OUTPUT(g2f, p1);
                UNITY_INITIALIZE_OUTPUT(g2f, p2);
                UNITY_INITIALIZE_OUTPUT(g2f, p3);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(p0);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(p1);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(p2);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(p3);

                float radius = i.colorAndRadius.w;
                float4 left = float4(rotate_vector(float3(1, 0, 0), i.rotation) * radius, 0);
                float4 up = float4(rotate_vector(float3(0, 1, 0), i.rotation) * radius, 0);

                float4 v0 = i.vertex - left - up;
                float4 v1 = i.vertex - left + up;
                float4 v2 = i.vertex + left - up;
                float4 v3 = i.vertex + left + up;

                p0.vertex = UnityObjectToClipPos(v0);
                p1.vertex = UnityObjectToClipPos(v1);
                p2.vertex = UnityObjectToClipPos(v2);
                p3.vertex = UnityObjectToClipPos(v3);                

                p0.uv = float2(0, 0);
                p1.uv = float2(0, 1);
                p2.uv = float2(1, 0);
                p3.uv = float2(1, 1);

                p0.color = p1.color = p2.color = p3.color = half4(i.colorAndRadius.xyz, 1);      
                
                outStream.Append(p0);
                outStream.Append(p2);
                outStream.Append(p1);
                outStream.Append(p1);
                outStream.Append(p3);
                outStream.Append(p2);
                outStream.RestartStrip();
            }

            half4 frag (v2f i) : SV_Target
            {
                // sample the texture
                half4 maskCol = _Mask.Sample(sampler_Mask, i.uv);

                clip(maskCol.a - 0.5);

                half4 col = half4(maskCol.xyz * i.color.xyz, 1);
                return col;
            }

            ENDHLSL
        }
    }
}

