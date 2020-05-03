Shader "OutlineToolkit/EdgeCombine"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0, 0, 0, 1)
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

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
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			int _EdgedetectDebugMode;

			float4 _Color;
			
			sampler2D _MainTex;
			//float2 _MainTex_TexelSize;
			sampler2D _EdgeTex;
			//float2  _EdgeTex_TexelSize;

			/*float3 HSVtoRGB(float3 HSV);
			float3 RGBtoHSV(float3 RGB);
			float2 rand_2_10(in float2 uv);

			float2 rand_2_10(in float2 uv) {
				float noiseX = (frac(sin(dot(uv, float2(12.9898, 78.233) * 2.0)) * 43758.5453));
				float noiseY = sqrt(1 - noiseX * noiseX);
				return float2(noiseX, noiseY);
			}

			float3 HSVtoRGB(float3 HSV)
			{
				float3 RGB = 0;
				float C = HSV.z * HSV.y;
				float H = HSV.x * 6;
				float X = C * (1 - abs(fmod(H, 2) - 1));
				if (HSV.y != 0)
				{
					float I = floor(H);
					if (I == 0) { RGB = float3(C, X, 0); }
					else if (I == 1) { RGB = float3(X, C, 0); }
					else if (I == 2) { RGB = float3(0, C, X); }
					else if (I == 3) { RGB = float3(0, X, C); }
					else if (I == 4) { RGB = float3(X, 0, C); }
					else { RGB = float3(C, 0, X); }
				}
				float M = HSV.z - C;
				return RGB + M;
			}

			float3 RGBtoHSV(float3 RGB)
			{
				float3 HSV = 0;
				float M = min(RGB.r, min(RGB.g, RGB.b));
				HSV.z = max(RGB.r, max(RGB.g, RGB.b));
				float C = HSV.z - M;
				if (C != 0)
				{
					HSV.y = C / HSV.z;
					float3 D = (((HSV.z - RGB) / 6) + (C / 2)) / C;
					if (RGB.r == HSV.z)
						HSV.x = D.b - D.g;
					else if (RGB.g == HSV.z)
						HSV.x = (1.0 / 3.0) + D.r - D.b;
					else if (RGB.b == HSV.z)
						HSV.x = (2.0 / 3.0) + D.g - D.r;
					if (HSV.x < 0.0) { HSV.x += 1.0; }
					if (HSV.x > 1.0) { HSV.x -= 1.0; }
				}
				return HSV;
			}*/

			fixed4 frag(v2f i) : SV_Target
			{
				i.uv.y = 1 - i.uv.y;

				if (_EdgedetectDebugMode != 0) {
					return tex2D(_EdgeTex, i.uv);
				}

				float3 weights = tex2D(_EdgeTex, i.uv);

				float weight = weights.x + weights.y + weights.z;

				weight -= 0.25;
				if (weight < 0)
					weight = 0;
				weight /= 0.75;
				weight = clamp(weight, 0, 1);

				i.uv.y = 1 - i.uv.y;
				weight *= _Color.a;
				return tex2D(_MainTex, i.uv) * (1-weight) + weight * _Color;

				/*
				float2 originalUv = i.uv;

				float2 a = 1 / _EdgeTex_TexelSize;
				int pixelSize = a.y / 128;
				//int pixelSize = a.y / 256;
				//int pixelSize = a.y / 32;

				i.uv.x = round(i.uv.x * a.x / pixelSize) / (a.x / pixelSize);
				i.uv.y = round(i.uv.y * a.y / pixelSize) / (a.y / pixelSize);

				float4 color = 0;
				float black = 0;
				float4 m = 0;

				//[unroll(pixelSize * pixelSize)]
				for (int x = 0; x < pixelSize; x++) {
					for (int y = 0; y < pixelSize; y++) {
						color += tex2D(_MainTex, i.uv + float2(x * _MainTex_TexelSize.x, y * _MainTex_TexelSize.y));
						if (tex2D(_EdgeTex, i.uv + float2(x * _MainTex_TexelSize.x, y * _MainTex_TexelSize.y)).r < 0.5)
							black++;
					}
				}
				color = color / (pixelSize * pixelSize);


				color.xy += rand_2_10(i.uv) / 16;
				color.z += rand_2_10(i.uv + float2(128, 128)).x / 16;
				color = round(color * 16) / 16;

				//float2 rand = rand_2_10(i.uv);
				//rand -= 0.5;

				//float3 colorHSV = RGBtoHSV(color);
				//colorHSV.y += rand.x * 0.08333333333;
				//colorHSV.z += rand.y * 0.08333333333;
				//colorHSV.yz = round(colorHSV.yz * 6) / 6;
				//float3 newColor = HSVtoRGB(colorHSV);

				float modX = fmod(originalUv.x / _EdgeTex_TexelSize.x, 2);
				float modY = fmod(originalUv.y / _EdgeTex_TexelSize.y, 2);
				if (modX < 1 && modY < 1)
					return color;
				else if (modX >= 1 && modY >= 1)
					return color;

				if (black > 0) {
					//if (_Time.x < 4)
					//	return 1;
					return 0;
				}
				else {
					return color;
					//return tex2D(_MainTex, originalUv);
					//return float4(newColor.x, newColor.y, newColor.z, 1);
				}*/
			}
			ENDCG
		}
	}
}
