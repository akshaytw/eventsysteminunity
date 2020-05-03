Shader "OutlineToolkit/EncodedDepth" {
	SubShader{
		Tags{ "RenderType" = "Opaque" }
		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float4 worldPos : NORMAL;
			};

			float4 _EdgeDetectDepthArgs;
			float3 projectOnVector(float3 A, float3 B);

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			half4 frag(v2f i) : SV_Target{
				float camFarPlane = _EdgeDetectDepthArgs.z;

				float3 viewDir = UNITY_MATRIX_V[2].xyz;
				viewDir = normalize(viewDir);

				float dist = length(projectOnVector(i.worldPos - _WorldSpaceCameraPos, viewDir));

				return EncodeFloatRGBA(dist / camFarPlane).xyzw;
			}

			float3 projectOnVector(float3 B, float3 A) {
				return dot(A, B) / length(A);
			}

			ENDCG
		}
	}
}
