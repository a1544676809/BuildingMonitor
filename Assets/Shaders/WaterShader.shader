Shader "Custom/WaterShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex( "Noise Texture", 2D) = "bump" {}
        	_Mitigation("Mitigation", Range(1, 50)) = 20//���ڿ���ӳ��ͼ���Ť���̶�
		_SpeedX("Speed X", Range(0, 5)) = 1//����������X���ϵ��ٶ�
		_SpeedY("Speed Y", Range(0, 5)) = 1//����������Y���ϵ��ٶ�
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
 
				float2 speed = float2(_SpeedX, _SpeedY);//�����ƽ���ٶ�
 
				fixed noise = tex2D(_NoiseTex, uv).r;//��ȡ���������ֵ����������������һ�ŻҶ�ͼ������ʹ���ĸ�ֵ������
 
				noise = noise / _Mitigation;//������ֵ����һ����������ֵԽ���Խ�ӽ�0������Ť���ĳ̶Ⱦ�ԽС
				
				uv += noise* sin(_Time.y * speed);//_Time��һ��float4���ͣ�xyzw�ֱ��ʾt/20,t,t*2,t*3����ǰ����ѡȡ�����ٶ�Ҳ��y������ͨ�����ں���sin()��������仯
				
				return tex2D(_MainTex, uv);
			}

            ENDCG
        }
    }
    FallBack "Diffuse"
}
