Shader "Custom/WaterShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex( "Noise Texture", 2D) = "bump" {}
        	_Mitigation("Mitigation", Range(1, 50)) = 20//用于控制映射图像的扭曲程度
		_SpeedX("Speed X", Range(0, 5)) = 1//控制纹理在X轴上的速度
		_SpeedY("Speed Y", Range(0, 5)) = 1//控制纹理在Y轴上的速度
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

		Pass
        {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			float _Mitigation;
			float _SpeedX;
			float _SpeedY;

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
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;
 
				float2 speed = float2(_SpeedX, _SpeedY);//纹理的平移速度
 
				fixed noise = tex2D(_NoiseTex, uv).r;//获取噪声纹理的值，由于噪声纹理是一张灰度图，所以使用哪个值都可以
 
				noise = noise / _Mitigation;//当噪声值除以一个数，该数值越大就越接近0，纹理扭曲的程度就越小
				
				uv += noise* sin(_Time.y * speed);//_Time是一个float4类型，xyzw分别表示t/20,t,t*2,t*3，当前我们选取正常速度也就y分量，通过周期函数sin()来控制其变化
				
				return tex2D(_MainTex, uv);
			}

            ENDCG
        }
    }
    FallBack "Diffuse"
}
