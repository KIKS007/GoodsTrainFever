// Upgrade NOTE: upgraded instancing buffer 'Water' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_Color("Color", Color) = (0,0,0,0)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 2.0
		#pragma multi_compile_instancing
		#pragma surface surf Lambert keepalpha addshadow fullforwardshadows novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			float3 worldPos;
		};

		UNITY_INSTANCING_BUFFER_START(Water)
			UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
#define _Color_arr Water
		UNITY_INSTANCING_BUFFER_END(Water)


		half3 HSVToRGB( half3 c )
		{
			half4 K = half4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			half3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, clamp( p - K.xxx, 0.0, 1.0 ), c.y );
		}


		half3 RGBToHSV(half3 c)
		{
			half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			half4 p = lerp( half4( c.bg, K.wz ), half4( c.gb, K.xy ), step( c.b, c.g ) );
			half4 q = lerp( half4( p.xyw, c.r ), half4( c.r, p.yzx ), step( p.x, c.r ) );
			half d = q.x - min( q.w, q.y );
			half e = 1.0e-10;
			return half3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			half3 hsvTorgb5 = RGBToHSV( UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color).rgb );
			float3 ase_worldPos = i.worldPos;
			half3 hsvTorgb8 = HSVToRGB( half3(hsvTorgb5.x,hsvTorgb5.y,( hsvTorgb5.z + clamp( (-0.1 + (ase_worldPos.y - -2.0) * (2.0 - -0.1) / (0.0 - -2.0)) , -0.1 , 2.0 ) )) );
			o.Albedo = hsvTorgb8;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=10001
727;92;809;771;880.4069;621.1021;1.6;True;False
Node;AmplifyShaderEditor.RangedFloatNode;17;-1304.104,119.8992;Half;False;Constant;_Float4;Float 4;1;0;0;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;15;-1306.104,204.8992;Half;False;Constant;_Float2;Float 2;1;0;-0.1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;16;-1298.104,347.8992;Half;False;Constant;_Float3;Float 3;1;0;2;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.WorldPosInputsNode;6;-1278.304,-168.5008;Float;False;0;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;18;-1309.104,37.89923;Half;False;Constant;_Float5;Float 5;1;0;-2;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.TFHCRemap;14;-1061.104,109.8992;Float;False;5;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;2;-1303.001,-394.8999;Fixed;False;InstancedProperty;_Color;Color;1;0;0,0,0,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;20;-827.8074,245.498;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RGBToHSVNode;5;-1029.001,-311.9;Half;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;10;-738.1036,-104.1008;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.HSVToRGBNode;8;-552.1036,-170.1008;Half;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Half;False;True;0;Half;ASEMaterialInspector;0;Lambert;Water;False;False;False;False;False;True;True;True;True;True;True;True;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;-1;-1;-1;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;14;0;6;2
WireConnection;14;1;18;0
WireConnection;14;2;17;0
WireConnection;14;3;15;0
WireConnection;14;4;16;0
WireConnection;20;0;14;0
WireConnection;20;1;15;0
WireConnection;20;2;16;0
WireConnection;5;0;2;0
WireConnection;10;0;5;3
WireConnection;10;1;20;0
WireConnection;8;0;5;1
WireConnection;8;1;5;2
WireConnection;8;2;10;0
WireConnection;0;0;8;0
ASEEND*/
//CHKSM=E466C08EAB5AB002FDAFA1236B4923BB08358E19