Shader "Transparent/DiffuseDetachAlpha" {
Properties {
	_MainTex ("LigthMap Base (RGB) A", 2D) = "white" {}
	_MainTexLM ("Base (RGB)", 2D) = "white" {}
	_AlphaTexLM ("Trans (A)", 2D) = "white" {}
	_Alpha("m Alpha", Range(0,1)) = 1
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alpha

sampler2D _MainTexLM;
sampler2D _AlphaTexLM;
float _Alpha;

struct Input {
	float2 uv_MainTexLM;
	float2 uv_AlphaTexLM;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTexLM, IN.uv_MainTexLM);
	fixed4 a = tex2D(_AlphaTexLM, IN.uv_AlphaTexLM);
	o.Albedo = c.rgb;
	o.Alpha = a * _Alpha;
}
ENDCG
}

Fallback "Transparent/VertexLit"
}
