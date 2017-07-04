// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MainShader"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_Normal("Normal", 2D) = "bump" {}
		_AlbedoAO("Albedo AO", 2D) = "white" {}
		_Albedo("Albedo", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
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
		uniform float4 _Albedo;
		uniform sampler2D _AlbedoAO;
		uniform float4 _AlbedoAO_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _Normal, uv_Normal ) ,0.0 );
			float2 uv_AlbedoAO = i.uv_texcoord * _AlbedoAO_ST.xy + _AlbedoAO_ST.zw;
			float4 tex2DNode3 = tex2D( _AlbedoAO, uv_AlbedoAO );
			float3 appendResult4 = float3( tex2DNode3.r , tex2DNode3.g , tex2DNode3.b );
			o.Albedo = ( _Albedo * float4( appendResult4 , 0.0 ) ).rgb;
			float temp_output_5_0 = 0.0;
			float3 temp_cast_2 = (temp_output_5_0).xxx;
			o.Emission = temp_cast_2;
			o.Metallic = temp_output_5_0;
			o.Smoothness = 0.0;
			o.Occlusion = tex2DNode3.a;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=10001
2567;238;2546;1014;1305;897;1;True;False
Node;AmplifyShaderEditor.SamplerNode;3;-769,-377;Float;True;Property;_AlbedoAO;Albedo AO;1;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;7;-668,-724;Float;False;Property;_Albedo;Albedo;2;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.AppendNode;4;-381,-385;Float;False;FLOAT3;0;0;0;0;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT3
Node;AmplifyShaderEditor.RangedFloatNode;5;-718,-122;Float;False;Constant;_Metallic;Metallic;2;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;1;-805,107;Float;True;Property;_Normal;Normal;0;0;None;True;0;True;bump;LockedToTexture2D;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-220,-404;Float;False;2;0;COLOR;0,0,0;False;1;FLOAT3;0.0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;6;-712,-8;Float;False;Constant;_Smoothness;Smoothness;2;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;24,-293;Float;False;True;2;Float;ASEMaterialInspector;0;Standard;MainShader;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;-1;-1;-1;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;3;1
WireConnection;4;1;3;2
WireConnection;4;2;3;3
WireConnection;8;0;7;0
WireConnection;8;1;4;0
WireConnection;0;0;8;0
WireConnection;0;1;1;0
WireConnection;0;2;5;0
WireConnection;0;3;5;0
WireConnection;0;4;6;0
WireConnection;0;5;3;4
ASEEND*/
//CHKSM=63134A8BC02F233497D3372DF0F68A27CACA280F