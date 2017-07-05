// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Two Color"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_AlbedoAO("Albedo AO", 2D) = "white" {}
		_Albedo2("Albedo 2", Color) = (0,0,0,0)
		_Albedo1("Albedo 1", Color) = (1,1,1,0)
		_AllebdoPower("Allebdo Power", Range( 0 , 1)) = 0
		_AOPower("AO Power", Range( 0 , 1)) = 0
		_Texture0("Texture 0", 2D) = "bump" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Texture0;
		uniform float4 _Texture0_ST;
		uniform float4 _Albedo1;
		uniform sampler2D _AlbedoAO;
		uniform float4 _AlbedoAO_ST;
		uniform float _AllebdoPower;
		uniform float4 _Albedo2;
		uniform float _AOPower;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Texture0 = i.uv_texcoord * _Texture0_ST.xy + _Texture0_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _Texture0, uv_Texture0 ) ,0.0 );
			float2 uv_AlbedoAO = i.uv_texcoord * _AlbedoAO_ST.xy + _AlbedoAO_ST.zw;
			float4 tex2DNode3 = tex2D( _AlbedoAO, uv_AlbedoAO );
			float3 appendResult4 = float3( tex2DNode3.r , tex2DNode3.g , tex2DNode3.b );
			float3 temp_output_13_0 = lerp( float3( 1,1,1 ) , appendResult4 , _AllebdoPower );
			o.Albedo = lerp( ( _Albedo1 * float4( temp_output_13_0 , 0.0 ) ) , ( _Albedo2 * float4( temp_output_13_0 , 0.0 ) ) , tex2D( _Texture0, uv_Texture0 ).a ).rgb;
			o.Metallic = 0.0;
			o.Smoothness = 0.0;
			o.Occlusion = lerp( 1.0 , tex2DNode3.a , _AOPower );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=10001
2567;238;2546;1014;1269.11;662.1978;1;True;False
Node;AmplifyShaderEditor.SamplerNode;3;-763.8,-467.9997;Float;True;Property;_AlbedoAO;Albedo AO;0;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.AppendNode;4;-375.8001,-475.9997;Float;False;FLOAT3;0;0;0;0;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.RangedFloatNode;10;-586.9013,-589.0995;Float;False;Property;_AllebdoPower;Allebdo Power;3;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;13;-104.4048,-454.0983;Float;False;3;0;FLOAT3;1,1,1;False;1;FLOAT3;0.0;False;2;FLOAT;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.TexturePropertyNode;21;-961.11,-123.1978;Float;True;Property;_Texture0;Texture 0;5;0;None;False;bump;Auto;0;1;SAMPLER2D
Node;AmplifyShaderEditor.ColorNode;17;-494.8049,-810.4977;Float;False;Property;_Albedo2;Albedo 2;1;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;7;-472.9016,-1020.399;Float;False;Property;_Albedo1;Albedo 1;2;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;23;-649.11,64.80219;Float;True;Property;_TextureSample0;Texture Sample 0;6;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;191.1951,-474.4977;Float;False;2;0;COLOR;0.0;False;1;FLOAT3;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;195.6003,-573.6996;Float;False;2;0;COLOR;0,0,0;False;1;FLOAT3;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;14;-507.4048,-238.1986;Float;False;Property;_AOPower;AO Power;4;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;5;397.3989,-226.2003;Float;False;Constant;_Metallic;Metallic;2;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;16;-46.50499,-297.3986;Float;False;3;0;FLOAT;1.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;20;146.0951,-824.2977;Float;False;Constant;_Color1;Color 1;6;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.LerpOp;19;407.3952,-482.3979;Float;False;3;0;COLOR;0.0;False;1;COLOR;0.0,0,0,0;False;2;FLOAT;0.0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;6;386.6994,-135.5001;Float;False;Constant;_Smoothness;Smoothness;2;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;1;-671,-129;Float;True;Property;_Normal;Normal;0;0;None;True;0;True;white;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;642,-285;Float;False;True;2;Float;ASEMaterialInspector;0;Standard;Two Color;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;-1;-1;-1;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;3;1
WireConnection;4;1;3;2
WireConnection;4;2;3;3
WireConnection;13;1;4;0
WireConnection;13;2;10;0
WireConnection;23;0;21;0
WireConnection;18;0;17;0
WireConnection;18;1;13;0
WireConnection;8;0;7;0
WireConnection;8;1;13;0
WireConnection;16;1;3;4
WireConnection;16;2;14;0
WireConnection;19;0;8;0
WireConnection;19;1;18;0
WireConnection;19;2;23;4
WireConnection;1;0;21;0
WireConnection;0;0;19;0
WireConnection;0;1;1;0
WireConnection;0;3;5;0
WireConnection;0;4;6;0
WireConnection;0;5;16;0
ASEEND*/
//CHKSM=16F8A821BA6991CDFDB68FC274C9E242602B8A6B