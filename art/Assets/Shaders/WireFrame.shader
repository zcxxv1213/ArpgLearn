Shader "Custom/WireFrame" 
{
	Properties 
	{
	    _MainTex ("MainTexRGB", 2D) = "white" {}
	    _Color ("Color", Color) = (1,1,1,1)
		_Factor("Factor",Range(1,5)) = 3
	}
	CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_ST;
		float4 _Color;
		float _Factor;
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
		v2f vert(appdata v) 
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.nDir = normalize( mul(unity_ObjectToWorld,float4(v.normal,0)));
			o.vDir = WorldSpaceViewDir(v.vertex);
			return o;
		}

		float4 frag(v2f i) : COLOR 
		{ 
			fixed4 col = tex2D(_MainTex, i.uv)*_Color;
			col.a = saturate(_Factor-abs(dot(i.vDir,i.nDir)));
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
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
