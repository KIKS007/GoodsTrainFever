// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_Albedo("Albedo", Color) = (0,0.4206896,1,0)
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothnes("Smoothnes", Range( 0 , 1)) = 0
		_HeightFactor("HeightFactor", Range( 0 , 1)) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		uniform float4 _Albedo;
		uniform float _Metallic;
		uniform float _Smoothnes;
		uniform float _HeightFactor;


		float3 mod289( float3 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 mod289( float4 x ) { return x - floor( x / 289.0 ) * 289.0; }

		float4 permute( float4 x ) { return mod289( ( x * 34.0 + 1.0 ) * x ); }

		float4 taylorInvSqrt( float4 r ) { return 1.79284291400159 - r * 0.85373472095314; }

		float snoise( float3 v )
		{
			const float2 C = float2( 1.0 / 6.0, 1.0 / 3.0 );
			float3 i = floor( v + dot( v, C.yyy ) );
			float3 x0 = v - i + dot( i, C.xxx );
			float3 g = step( x0.yzx, x0.xyz );
			float3 l = 1.0 - g;
			float3 i1 = min( g.xyz, l.zxy );
			float3 i2 = max( g.xyz, l.zxy );
			float3 x1 = x0 - i1 + C.xxx;
			float3 x2 = x0 - i2 + C.yyy;
			float3 x3 = x0 - 0.5;
			i = mod289( i);
			float4 p = permute( permute( permute( i.z + float4( 0.0, i1.z, i2.z, 1.0 ) ) + i.y + float4( 0.0, i1.y, i2.y, 1.0 ) ) + i.x + float4( 0.0, i1.x, i2.x, 1.0 ) );
			float4 j = p - 49.0 * floor( p / 49.0 );  // mod(p,7*7)
			float4 x_ = floor( j / 7.0 );
			float4 y_ = floor( j - 7.0 * x_ );  // mod(j,N)
			float4 x = ( x_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 y = ( y_ * 2.0 + 0.5 ) / 7.0 - 1.0;
			float4 h = 1.0 - abs( x ) - abs( y );
			float4 b0 = float4( x.xy, y.xy );
			float4 b1 = float4( x.zw, y.zw );
			float4 s0 = floor( b0 ) * 2.0 + 1.0;
			float4 s1 = floor( b1 ) * 2.0 + 1.0;
			float4 sh = -step( h, 0.0 );
			float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
			float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
			float3 g0 = float3( a0.xy, h.x );
			float3 g1 = float3( a0.zw, h.y );
			float3 g2 = float3( a1.xy, h.z );
			float3 g3 = float3( a1.zw, h.w );
			float4 norm = taylorInvSqrt( float4( dot( g0, g0 ), dot( g1, g1 ), dot( g2, g2 ), dot( g3, g3 ) ) );
			g0 *= norm.x;
			g1 *= norm.y;
			g2 *= norm.z;
			g3 *= norm.w;
			float4 m = max( 0.6 - float4( dot( x0, x0 ), dot( x1, x1 ), dot( x2, x2 ), dot( x3, x3 ) ), 0.0 );
			m = m* m;
			m = m* m;
			float4 px = float4( dot( x0, g0 ), dot( x1, g1 ), dot( x2, g2 ), dot( x3, g3 ) );
			return 42.0 * dot( m, px);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul(unity_ObjectToWorld, v.vertex);
			float simplePerlin3D8 = snoise( ase_worldPos );
			float3 normalWorld = UnityObjectToWorldNormal( v.normal );
			v.vertex.xyz += ( ( ( simplePerlin3D8 * _HeightFactor ) * sin( ( ( ase_worldPos.x * ase_worldPos.z ) + _Time.y ) ) ) * normalWorld );
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3(ase_worldTangent, ase_worldBitangent, ase_worldNormal);
			float3 ase_worldPos = i.worldPos;
			o.Normal = mul( ase_worldToTangent , cross( normalize( ddy( ase_worldPos ) ) , normalize( ddx( ase_worldPos ) ) ) );
			o.Albedo = _Albedo.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothnes;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			# include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD6;
				float4 tSpace0 : TEXCOORD1;
				float4 tSpace1 : TEXCOORD2;
				float4 tSpace2 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=7003
0;92;1066;655;2439.05;-13.84858;1.6;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;7;-1949.075,231.6069;Float;True;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleTimeNode;47;-1475.847,777.0488;Float;False;0;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;-1431.05,557.8486;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;24;-1866.533,550.6948;Float;False;Property;_HeightFactor;HeightFactor;3;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;46;-1221.447,601.0486;Float;False;0;FLOAT;0.0,0,0,0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.NoiseGeneratorNode;8;-1594.875,351.9071;Float;False;Simplex3D;0;FLOAT3;0,0;False;FLOAT
Node;AmplifyShaderEditor.DdxOpNode;30;-1143.144,31.64905;Float;True;0;FLOAT3;0.0;False;FLOAT3
Node;AmplifyShaderEditor.DdyOpNode;32;-1135.644,259.0489;Float;True;0;FLOAT3;0.0;False;FLOAT3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-1400.076,438.0073;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.NormalizeNode;33;-961.2444,259.049;Float;False;0;FLOAT3;0.0,0,0,0;False;FLOAT3
Node;AmplifyShaderEditor.SinOpNode;48;-1069.448,580.2485;Float;False;0;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.NormalizeNode;31;-971.6446,32.34908;Float;False;0;FLOAT3;0.0,0,0,0;False;FLOAT3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-834.2468,473.0487;Float;False;0;FLOAT;0.0,0,0,0;False;1;FLOAT;0;False;FLOAT
Node;AmplifyShaderEditor.CrossProductOpNode;34;-782.0447,151.849;Float;True;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT3
Node;AmplifyShaderEditor.WorldToTangentMatrix;35;-735.6478,22.2488;Float;False;FLOAT3x3
Node;AmplifyShaderEditor.WorldNormalVector;18;-743.4761,603.4075;Float;False;0;FLOAT3;0,0,0;False;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-465.476,468.0073;Float;False;0;FLOAT;0.0;False;1;FLOAT3;0.0;False;FLOAT3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-474.8479,137.4488;Float;True;0;FLOAT3x3;0.0;False;1;FLOAT3;0.0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0;False;FLOAT3
Node;AmplifyShaderEditor.RangedFloatNode;3;-805.5,-333.5;Float;False;Property;_Metallic;Metallic;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;4;-803,-252.5;Float;False;Property;_Smoothnes;Smoothnes;2;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;2;-756.5,-512;Float;False;Property;_Albedo;Albedo;0;0;0,0.4206896,1,0;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;2;Float;ASEMaterialInspector;0;Standard;Water;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Spherical;Relative;0;;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;OBJECT;0.0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False
WireConnection;49;0;7;1
WireConnection;49;1;7;3
WireConnection;46;0;49;0
WireConnection;46;1;47;0
WireConnection;8;0;7;0
WireConnection;30;0;7;0
WireConnection;32;0;7;0
WireConnection;23;0;8;0
WireConnection;23;1;24;0
WireConnection;33;0;32;0
WireConnection;48;0;46;0
WireConnection;31;0;30;0
WireConnection;45;0;23;0
WireConnection;45;1;48;0
WireConnection;34;0;33;0
WireConnection;34;1;31;0
WireConnection;21;0;45;0
WireConnection;21;1;18;0
WireConnection;36;0;35;0
WireConnection;36;1;34;0
WireConnection;0;0;2;0
WireConnection;0;1;36;0
WireConnection;0;3;3;0
WireConnection;0;4;4;0
WireConnection;0;11;21;0
ASEEND*/
//CHKSM=EB101B0A64290CBEAF0DD9945B9EBE35DA5CFEE6