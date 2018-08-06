Shader "Role-Transparent"
{
	Properties
	{
		_MainTex ("MainTexRGB", 2D) = "white" {}
		_MainTexA ("MainTexA", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_CutOff("CutOff",Range(0,1)) = 1
	}

	CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _MainTexA;
		float4 _MainTex_ST;
		float4 _Color;
		float _CutOff;

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
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			return o;
		}
			
		fixed4 frag_1 (v2f i) : SV_Target
		{
			fixed a = tex2D(_MainTexA, i.uv);
			if(a<_CutOff)discard;//剔除半透明部分
			fixed4 col = tex2D(_MainTex, i.uv);
			col.a = a;
			col *= _Color;
			return col;
		}
		fixed4 frag_2 (v2f i) : SV_Target
		{
			fixed a = tex2D(_MainTexA, i.uv);
			if(a>=_CutOff)discard;//剔除半透明部分
			fixed4 col = tex2D(_MainTex, i.uv);
			col.a = a;
			col *= _Color;
			return col;
		}
	ENDCG

	SubShader
	{
	    //Transparent
	    //Opaque
		//Geometry
	    Tags { "Queue" = "Transparent"} 
		//渲染不透明的部分  
		Pass
		{
		    Cull Off
			ZTest On
			ZWrite On
            Lighting Off  
			//AlphaTest GEqual 1 
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_1
			ENDCG
		}
		//渲染半透明的部分
		Pass
		{
		    Cull Off
		    ZWrite Off
			ZTest On
			Lighting Off
			//AlphaTest Less 1 
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_2
			ENDCG
		}
	}
}
