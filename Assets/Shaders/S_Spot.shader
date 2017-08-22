// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Spot"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_Emission("Emission", Color) = (0,0,0,0)
		_MainColor("MainColor", Color) = (0,0,0,0)
		_Opacity("Opacity", Float) = 0.2
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 2.0
		#pragma surface surf Standard alpha:fade keepalpha nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			fixed filler;
		};

		uniform fixed4 _MainColor;
		uniform fixed4 _Emission;
		uniform fixed _Opacity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _MainColor.rgb;
			o.Emission = _Emission.rgb;
			o.Alpha = _Opacity;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=10001
1040;92;1042;771;926.9043;467.1482;1.3;True;False
Node;AmplifyShaderEditor.ColorNode;3;-282.4003,-137.7999;Fixed;False;Property;_Emission;Emission;0;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;2;-265.3,176.7999;Fixed;False;Property;_Opacity;Opacity;1;0;0.2;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;1;-385.3,-393.2001;Fixed;False;Property;_MainColor;MainColor;0;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;88,-157;Half;False;True;0;Half;ASEMaterialInspector;0;Standard;Spot;False;False;False;False;False;False;True;True;True;True;True;True;Back;0;0;False;0;0;Transparent;0.5;True;False;0;False;Transparent;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;-1;-1;-1;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;1;0
WireConnection;0;2;3;0
WireConnection;0;9;2;0
ASEEND*/
//CHKSM=FFED62D8B0E623FED1B2A6968AEE4898931F7A4C