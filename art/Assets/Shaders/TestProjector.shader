// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TestProjector" {
	Properties{
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_AlphaTexLM("Trans (A)", 2D) = "white" {}
		_InvFade("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_Alpha("m Alpha", Range(0,1)) = 1
	}
		SubShader{
		
		Pass{
		Tags{ "RenderType" = "Transparent" }
		LOD 200
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		CGPROGRAM
#pragma vertex vert  
#pragma fragment frag  
#include "UnityCG.cginc"  

		sampler2D _MainTex;
		sampler2D _AlphaTexLM;
		fixed4 _TintColor;
		float _Alpha;
	float4x4 unity_Projector;

	struct appdata_t {
		float4 vertex : POSITION;
		fixed4 color : COLOR;
	};
	struct v2f
	{
		float4 pos:SV_POSITION;
		fixed4 color : COLOR;
		float4 texc:TEXCOORD0;
	};

	v2f vert(appdata_t v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		//将顶点变换到矩阵空间  
		o.color = v.color;
		o.texc = mul(unity_Projector,v.vertex);
		return o;
	}

	float4 frag(v2f o) :COLOR
	{
		//对光环图片进行投影采样  
		float4 c = tex2Dproj(_AlphaTexLM,o.texc);
		//限制投影方向  
		c = c*step(0,o.texc.w);
		c = 2.0f * o.color * _TintColor * tex2D(_MainTex, o.texc);
		c.a *= (tex2D(_AlphaTexLM, o.texc) * _Alpha);
		return c;
	}

		ENDCG
	}
		
	}
		FallBack "Diffuse"
}