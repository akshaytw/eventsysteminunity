Shader "OutlineToolkit/EdgeDetect"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile __ THICK
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex; //Depth
			sampler2D _NormalsTex;

			int _EdgedetectDebugMode;

			float4 _MainTex_TexelSize;
			float4x4 _ScreenToWorldMatrix;
			float vectorAngle(float3 A, float3 B);
			float3 projectOnVector(float3 B, float3 A);

			float4x4 _Cam2World; //only the rotation
			float4 _EdgeDetectDepthArgs; //xy is uv multiplicand (depends on FOV). z is the camera far clipping plane times a scalar

			float4 _SensitivityAndWidthArgs; //x is normals, y is depth

			fixed4 frag (v2f i) : SV_Target
			{
				float4 weight = 0;
				i.uv.y = 1 - i.uv.y;

				if (_EdgedetectDebugMode == 2) {
					return tex2D(_NormalsTex, i.uv);
				}
				else if (_EdgedetectDebugMode == 3) {
					return DecodeFloatRGBA(tex2D(_MainTex, i.uv));
				}
				else if (_EdgedetectDebugMode == 4) {
					return tex2D(_MainTex, i.uv);
				}

				float2 uvU = i.uv.xy + float2(0, _MainTex_TexelSize.y);
				float2 uvD = i.uv.xy - float2(0, _MainTex_TexelSize.y);
				float2 uvL = i.uv.xy + float2(_MainTex_TexelSize.x, 0);
				float2 uvR = i.uv.xy - float2(_MainTex_TexelSize.x, 0);

				float res = _EdgeDetectDepthArgs.z;
				float decodedU = DecodeFloatRGBA(tex2D(_MainTex, uvU)) * res;
				float decodedD = DecodeFloatRGBA(tex2D(_MainTex, uvD)) * res;
				float decodedL = DecodeFloatRGBA(tex2D(_MainTex, uvL)) * res;
				float decodedR = DecodeFloatRGBA(tex2D(_MainTex, uvR)) * res;
				float mid = DecodeFloatRGBA(tex2D(_MainTex, i.uv)) * res;

				float3 normalM = tex2D(_NormalsTex, i.uv) * 2 - 1;
				float3 normalD = tex2D(_NormalsTex, uvD) * 2 - 1;
				float3 normalR = tex2D(_NormalsTex, uvR) * 2 - 1;
				float3 normalU = tex2D(_NormalsTex, uvU) * 2 - 1;
				float3 normalL = tex2D(_NormalsTex, uvL) * 2 - 1;

				uvU -= float2(0.5, 0.5);
				uvD -= float2(0.5, 0.5);
				uvR -= float2(0.5, 0.5);
				uvL -= float2(0.5, 0.5);
				i.uv -= float2(0.5, 0.5);

				uvU *= _EdgeDetectDepthArgs.xy;
				uvD *= _EdgeDetectDepthArgs.xy;
				uvR *= _EdgeDetectDepthArgs.xy;
				uvL *= _EdgeDetectDepthArgs.xy;
				i.uv *= _EdgeDetectDepthArgs.xy;

				float3 worldSpaceU = float3(uvU.x, uvU.y, 1) * decodedU;
				float3 worldSpaceD = float3(uvD.x, uvD.y, 1) * decodedD;
				float3 worldSpaceL = float3(uvL.x, uvL.y, 1) * decodedL;
				float3 worldSpaceR = float3(uvR.x, uvR.y, 1) * decodedR;
				float3 worldSpaceM = float3(i.uv.x, i.uv.y, 1) * mid;

				if (_EdgedetectDebugMode == 5) {
					return float4 (
						fmod(worldSpaceM.x + 1024, 1),
						fmod(worldSpaceM.y + 1024, 1),
						fmod(worldSpaceM.z + 1024, 1),
						1);
				}

				worldSpaceU = mul(_Cam2World, float4(worldSpaceU, 0.0));
				worldSpaceD = mul(_Cam2World, float4(worldSpaceD, 0.0));
				worldSpaceL = mul(_Cam2World, float4(worldSpaceL, 0.0));
				worldSpaceR = mul(_Cam2World, float4(worldSpaceR, 0.0));
				worldSpaceM = mul(_Cam2World, float4(worldSpaceM, 0.0));

				//two adjacent outer points are compared to a plane formed by the other two outer points and the center point
				float3 planeNormal = normalize(cross(worldSpaceU - worldSpaceM, worldSpaceL - worldSpaceM));
				weight.y += length(projectOnVector(worldSpaceD - worldSpaceM, planeNormal));
				weight.y += length(projectOnVector(worldSpaceR - worldSpaceM, planeNormal));
				planeNormal = normalize(cross(worldSpaceU - worldSpaceM, worldSpaceR - worldSpaceM));
				weight.y += length(projectOnVector(worldSpaceD - worldSpaceM, planeNormal));
				weight.y += length(projectOnVector(worldSpaceL - worldSpaceM, planeNormal));
				planeNormal = normalize(cross(worldSpaceD - worldSpaceM, worldSpaceR - worldSpaceM));
				weight.y += length(projectOnVector(worldSpaceU - worldSpaceM, planeNormal));
				weight.y += length(projectOnVector(worldSpaceL - worldSpaceM, planeNormal));
				planeNormal = normalize(cross(worldSpaceD - worldSpaceM, worldSpaceL - worldSpaceM));
				weight.y += length(projectOnVector(worldSpaceU - worldSpaceM, planeNormal));
				weight.y += length(projectOnVector(worldSpaceR - worldSpaceM, planeNormal));
				weight.y *= _SensitivityAndWidthArgs.y / 64.0;

				float ud = length(worldSpaceU - worldSpaceM) / length(worldSpaceD - worldSpaceM);
				float rl = length(worldSpaceR - worldSpaceM) / length(worldSpaceL - worldSpaceM);
				if (ud < 1) ud = 1 / ud;
				if (rl < 1) rl = 1 / rl;
				weight.z = (ud + rl) * _SensitivityAndWidthArgs.z * 0.05;

				float3 avg = normalU + normalL + normalD + normalR;
				avg /= 4.0;
				weight.x = vectorAngle(normalM, avg) * 5.0 * _SensitivityAndWidthArgs.x;

				return weight;
			}

			float vectorAngle(float3 A, float3 B) {

				//assuming A and B are normalized
				//cos(theta) = dot(A, B);

				A = normalize(A);
				B = normalize(B);

				float dotProduct = dot(A, B);

				float angleInRads = acos(dotProduct);

				return abs(angleInRads);
			}

			float3 projectOnVector(float3 B, float3 A) {
				return dot(A, B) / length(A);
			}

			ENDCG
		}
	}
}
