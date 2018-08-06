Shader "Custom/DoubleSide" {
    Properties {
            _Color ("Main Color", Color) = (1,1,1,1)
            _MainTex ("Base (RGB) Trans (A)", 2D) = "" {}
            _Cutoff ("Alpha Cutoff", Range(0,1)) = 0
    }

    SubShader {
            Tags { "Queue" = "Transparent" }
            Pass {
                    Cull Off
                    Blend SrcAlpha OneMinusSrcAlpha
                    Alphatest Greater [_Cutoff]
                    SetTexture [_MainTex] {
                        constantColor [_Color]
                        Combine texture * constant
                    }
            }
    }
}