Shader "Custom/SugiyaShader" {
	Properties{
		_Color("Main Color", Color) = (.5,.5,.5,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_InkSize("Ink Size", Float) = 5.0
		_InkColor("Ink Color", Color) = (0,0,0,1)
		_ShadowSampler("Shadow Texture", 2D) = "gray" {}
		_Alpha("m Alpha", Range(0,1)) = 1
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 0  //声明外部控制开关
	}

		SubShader{
		Tags{
		"Queue" = "Transparent"
		"LightMode" = "ForwardBase"
	}
		Pass{
		Name "OutLine"
		Cull Off
		ZTest Less
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
		struct VS {
		half4 Pos: POSITION;
	};
		sampler2D _MainTex;
		fixed _InkSize;
		fixed4 _InkColor;
		float4 _Color;
		float _Alpha;
		VS vert(appdata_base i) {

			const fixed incDist = 1.1f;

			half4 normalUnit = normalize(half4 (i.normal, 0));

			half4 viewNormal = mul(UNITY_MATRIX_MV, normalUnit);
			half4 edgePos = mul(UNITY_MATRIX_MV, i.vertex);

			half4 edgeProj = mul(UNITY_MATRIX_P, edgePos);

			fixed baseSize = edgeProj.z;

			half distToCamera = clamp(length(edgePos) / incDist, 0.0, 1.0);
			half normalScale = lerp(0.1, 1.0, distToCamera) * _InkSize * 0.001;

			VS o;
			o.Pos = mul(UNITY_MATRIX_P, edgePos + normalScale * viewNormal * baseSize);
			return o;
		}
		fixed4 frag(VS i) : COLOR{
			return float4(_InkColor.rgb, _Alpha);
		}
		ENDCG
	}
		Pass{
			Name "BASE"
			Cull [_Cull]
			ZTest Less
			ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_ST;
		float4 _Color;
		float _Alpha;
		struct appdata {
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
		};

		struct v2f {
			float4 pos : POSITION;
			float2 texcoord : TEXCOORD0;
		};

		v2f vert(appdata v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
			return o;
		}

		float4 frag(v2f i) : COLOR
		{
			float4 col = _Color * tex2D(_MainTex, i.texcoord);
			return float4(col.r,col.g,col.b, _Alpha);
		}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
