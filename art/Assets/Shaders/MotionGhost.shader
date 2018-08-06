Shader "Custom/MotionGhost" 
{
	Properties 
	{
	    _MainTex ("MainTexRGB", 2D) = "white" {}
	    _Color ("Color", Color) = (1,1,1,1)
		_Factor("Factor",Range(1,10)) = 3
		_Offset0 ("Offset 0", vector) = (0, 0, 0, 0)
		_Offset1 ("Offset 1", vector) = (0, 0, 0, 0)
		_Offset2 ("Offset 2", vector) = (0, 0, 0, 0)
		_Offset3 ("Offset 3", vector) = (0, 0, 0, 0)
	}
	CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_ST;
		float4 _Color;
		float _Factor;
		float4 _Offset0;
		float4 _Offset1;
		float4 _Offset2;
		float4 _Offset3;
		
		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal :NORMAL;
			float2 uv : TEXCOORD0;
		};
		struct v2f 
		{
			float4 pos : POSITION;
			float2 uv:TEXCOORD0;
			float3 nDir:TEXCOORD1;
			float3 vDir:TEXCOORD2;
		};
		v2f vert_offset_1(appdata v) 
		{
			v2f o;
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex) + _Offset0;
			o.pos = mul(UNITY_MATRIX_VP, worldPos);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.nDir = normalize( mul(unity_ObjectToWorld,float4(v.normal,0)));
			o.vDir = WorldSpaceViewDir(mul(unity_WorldToObject, worldPos));
			return o;
		}
		v2f vert_offset_2(appdata v) 
		{
			v2f o;
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex) + _Offset1;
			o.pos = mul(UNITY_MATRIX_VP, worldPos);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.nDir = normalize( mul(unity_ObjectToWorld,float4(v.normal,0)));
			o.vDir = WorldSpaceViewDir(mul(unity_WorldToObject, worldPos));
			return o;
		}
		v2f vert_offset_3(appdata v) 
		{
			v2f o;
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex) + _Offset2;
			o.pos = mul(UNITY_MATRIX_VP, worldPos);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.nDir = normalize( mul(unity_ObjectToWorld,float4(v.normal,0)));
			o.vDir = WorldSpaceViewDir(mul(unity_WorldToObject, worldPos));
			return o;
		}
		v2f vert_offset_4(appdata v) 
		{
			v2f o;
			float4 worldPos = mul(unity_ObjectToWorld, v.vertex) + _Offset3;
			o.pos = mul(UNITY_MATRIX_VP, worldPos);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.nDir = normalize( mul(unity_ObjectToWorld,float4(v.normal,0)));
			o.vDir = WorldSpaceViewDir(mul(unity_WorldToObject, worldPos));
			return o;
		}

		float4 frag_1(v2f i) : COLOR 
		{ 
			fixed4 col = tex2D(_MainTex, i.uv)*_Color;
			col.a = saturate(_Factor-abs(dot(i.vDir,i.nDir)));
			return col;
		}
		float4 frag_2(v2f i) : COLOR 
		{ 
			fixed4 col = tex2D(_MainTex, i.uv)*_Color;
			col.a = saturate(_Factor-abs(dot(i.vDir,i.nDir))) * 0.8;
			return col;
		}
		float4 frag_3(v2f i) : COLOR 
		{ 
			fixed4 col = tex2D(_MainTex, i.uv)*_Color;
			col.a = saturate(_Factor-abs(dot(i.vDir,i.nDir))) * 0.6;
			return col;
		}
		float4 frag_4(v2f i) : COLOR 
		{ 
			fixed4 col = tex2D(_MainTex, i.uv)*_Color;
			col.a = saturate(_Factor-abs(dot(i.vDir,i.nDir))) * 0.4;
			return col;
		}
	ENDCG


	SubShader 
	{ 
	    Tags { "Queue" = "Transparent"} 
		Pass 
		{ 
		    Blend SrcAlpha One
			//Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			CGPROGRAM
			#pragma vertex vert_offset_4
			#pragma fragment frag_4
			ENDCG
		}
		Pass 
		{
		    Blend SrcAlpha One
			//Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			CGPROGRAM
			#pragma vertex vert_offset_3
			#pragma fragment frag_3
			ENDCG
		}
		Pass 
		{
		    Blend SrcAlpha One
			//Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			CGPROGRAM
			#pragma vertex vert_offset_2
			#pragma fragment frag_2
			ENDCG
		}
		Pass 
		{
		    Blend SrcAlpha One
			//Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			CGPROGRAM
			#pragma vertex vert_offset_1
			#pragma fragment frag_1
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
