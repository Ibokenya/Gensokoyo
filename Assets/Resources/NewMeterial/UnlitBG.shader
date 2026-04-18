Shader "Unlit/UnlitBG"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // 【新增1】纹理偏移量（用于背景滚动）
        _ScrollOffset ("Scroll Offset", Vector) = (0,0,0,0)
        // 【新增2】透明度参数（0=完全透明，1=完全不透明）
        _Alpha ("Alpha", Range(0,1)) = 1.0
    }
    SubShader
    {
        // 【修改1】调整Tags：适配透明渲染 + 背景层级
        Tags { 
            "RenderType"="Transparent"      // 标记为透明类型
            "Queue"="Transparent"           // 透明渲染队列（避免遮挡其他物体）
            "IgnoreProjector"="True"        // 忽略投影（背景不需要接收投影）
        }
        LOD 100

        // 【新增3】透明渲染状态：关闭深度写入 + 开启颜色混合
        ZWrite Off                       // 透明物体不占用深度缓冲区
        Blend SrcAlpha OneMinusSrcAlpha  // 标准透明混合模式

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            // 【新增4】声明Properties对应的变量（供着色器使用）
            float2 _ScrollOffset;
            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // 【修改2】UV叠加滚动偏移量（实现背景滚动）
                o.uv = TRANSFORM_TEX(v.uv, _MainTex) + _ScrollOffset;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // 【修改3】应用透明度（覆盖原纹理的Alpha通道）
                col.a = col.a * _Alpha;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
