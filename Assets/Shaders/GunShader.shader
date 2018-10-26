Shader "vnc/Gun " {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_EmissionMap("Emission Map", 2D) = "black" {}
		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
			
			Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
			LOD 200
			Cull Back		
			
			Pass{
				ZWrite On
				ZTest Always

				CGPROGRAM
				#pragma fragment frag
				#pragma vertex vert
				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
				};

				struct v2f
				{
					float4 pos : SV_POSITION;
				};
				
				v2f vert(appdata v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);					
					return o;
				}

				half4 frag(v2f i) : SV_Target
				{
					return float4(0.0, 0.0, 0.0, 0.0);
				}

				ENDCG
			}

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _EmissionMap;
			float4 _EmissionColor;

			struct Input {
				float2 uv_MainTex;
				float2 uv_BumpMap;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o) {
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

				half4 emission = tex2D(_EmissionMap, IN.uv_MainTex) * _EmissionColor;

				o.Albedo = c.rgb;
				o.Emission = emission.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
				o.Alpha = c.a;
			}
			ENDCG
		}
			
		FallBack "Diffuse"
}
