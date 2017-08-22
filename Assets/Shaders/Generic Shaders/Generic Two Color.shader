// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Generic Two Color"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_Normal("Normal", 2D) = "bump" {}
		_AlbedoAO("Albedo AO", 2D) = "white" {}
		_Albedo1("Albedo 1", Color) = (1,1,1,0)
		_Albedo2("Albedo 2", Color) = (0,0,0,0)
		_AllebdoPower("Allebdo Power", Range( 0 , 1)) = 0
		_AOPower("AO Power", Range( 0 , 1)) = 0
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

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform fixed4 _Albedo1;
		uniform sampler2D _AlbedoAO;
		uniform float4 _AlbedoAO_ST;
		uniform half _AllebdoPower;
		uniform fixed4 _Albedo2;
		uniform half _AOPower;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _Normal, uv_Normal ) ,0.0 );
			float2 uv_AlbedoAO = i.uv_texcoord * _AlbedoAO_ST.xy + _AlbedoAO_ST.zw;
			half4 tex2DNode3 = tex2D( _AlbedoAO, uv_AlbedoAO );
			float temp_output_13_0 = lerp( 1 , tex2DNode3.r , _AllebdoPower );
			o.Albedo = lerp( ( _Albedo1 * temp_output_13_0 ) , ( _Albedo2 * temp_output_13_0 ) , tex2DNode3.b ).rgb;
			o.Metallic = 0.0;
			o.Smoothness = 0.0;
			o.Occlusion = lerp( 0.0 , tex2DNode3.g , _AOPower );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=10001
1032;92;1000;771;1424.856;947.3994;1.714002;True;False
Node;AmplifyShaderEditor.RangedFloatNode;10;-586.9013,-589.0995;Half;False;Property;_AllebdoPower;Allebdo Power;4;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;3;-763.8,-467.9997;Float;True;Property;_AlbedoAO;Albedo AO;1;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;7;-472.9016,-1020.399;Fixed;False;Property;_Albedo1;Albedo 1;2;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;17;-494.8049,-810.4977;Fixed;False;Property;_Albedo2;Albedo 2;3;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.LerpOp;13;-104.4048,-454.0983;Float;False;3;0;FLOAT;1;False;1;FLOAT;0,0,0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;14;-507.4048,-238.1986;Half;False;Property;_AOPower;AO Power;5;0;0;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;195.6003,-573.6996;Float;False;2;0;COLOR;0,0,0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;191.1951,-474.4977;Float;False;2;0;COLOR;0.0;False;1;FLOAT;0.0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;5;397.3989,-226.2003;Half;False;Constant;_Metallic;Metallic;2;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;6;386.6994,-135.5001;Half;False;Constant;_Smoothness;Smoothness;2;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;1;-671,-129;Float;True;Property;_Normal;Normal;0;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.LerpOp;19;407.3952,-482.3979;Float;False;3;0;COLOR;0.0;False;1;COLOR;0.0,0,0,0;False;2;FLOAT;0.0;False;1;COLOR
Node;AmplifyShaderEditor.LerpOp;16;-46.50499,-297.3986;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;642,-285;Half;False;True;2;Half;ASEMaterialInspector;0;Standard;Generic Two Color;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;-1;-1;-1;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;13;1;3;1
WireConnection;13;2;10;0
WireConnection;8;0;7;0
WireConnection;8;1;13;0
WireConnection;18;0;17;0
WireConnection;18;1;13;0
WireConnection;19;0;8;0
WireConnection;19;1;18;0
WireConnection;19;2;3;3
WireConnection;16;1;3;2
WireConnection;16;2;14;0
WireConnection;0;0;19;0
WireConnection;0;1;1;0
WireConnection;0;3;5;0
WireConnection;0;4;6;0
WireConnection;0;5;16;0
ASEEND*/
//CHKSM=B04D9FBF86A55BA777A10789F6960CA84DA82EDD