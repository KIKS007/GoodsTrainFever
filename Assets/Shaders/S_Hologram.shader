// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:9058,x:34286,y:32707,varname:node_9058,prsc:2|emission-8753-OUT,alpha-4547-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:963,x:32168,y:32398,varname:node_963,prsc:2;n:type:ShaderForge.SFN_Vector1,id:6253,x:32168,y:32546,varname:node_6253,prsc:2,v1:1;n:type:ShaderForge.SFN_ValueProperty,id:1315,x:32168,y:32638,ptovrint:False,ptlb:HologramDensity,ptin:_HologramDensity,varname:node_1315,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:10;n:type:ShaderForge.SFN_Append,id:4843,x:32367,y:32572,varname:node_4843,prsc:2|A-6253-OUT,B-1315-OUT;n:type:ShaderForge.SFN_Append,id:4532,x:32367,y:32398,varname:node_4532,prsc:2|A-963-X,B-963-Z;n:type:ShaderForge.SFN_ValueProperty,id:4913,x:32165,y:32728,ptovrint:False,ptlb:HologramSpeed1,ptin:_HologramSpeed1,varname:node_4913,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:8306,x:32168,y:32808,ptovrint:False,ptlb:HologramSpeed2,ptin:_HologramSpeed2,varname:node_8306,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:10;n:type:ShaderForge.SFN_Append,id:7924,x:32367,y:32728,varname:node_7924,prsc:2|A-4913-OUT,B-8306-OUT;n:type:ShaderForge.SFN_Time,id:818,x:32367,y:32887,varname:node_818,prsc:2;n:type:ShaderForge.SFN_Multiply,id:7116,x:32545,y:32821,varname:node_7116,prsc:2|A-7924-OUT,B-818-TSL;n:type:ShaderForge.SFN_Multiply,id:9760,x:32547,y:32490,varname:node_9760,prsc:2|A-4532-OUT,B-4843-OUT;n:type:ShaderForge.SFN_Add,id:4460,x:32739,y:32631,varname:node_4460,prsc:2|A-9760-OUT,B-7116-OUT;n:type:ShaderForge.SFN_OneMinus,id:3229,x:32909,y:32631,varname:node_3229,prsc:2|IN-4460-OUT;n:type:ShaderForge.SFN_ComponentMask,id:2327,x:33073,y:32631,varname:node_2327,prsc:2,cc1:1,cc2:-1,cc3:-1,cc4:-1|IN-3229-OUT;n:type:ShaderForge.SFN_Frac,id:9442,x:33250,y:32632,varname:node_9442,prsc:2|IN-2327-OUT;n:type:ShaderForge.SFN_ValueProperty,id:579,x:33248,y:32817,ptovrint:False,ptlb:HologramExposure,ptin:_HologramExposure,varname:node_579,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Power,id:5795,x:33511,y:32716,varname:node_5795,prsc:2|VAL-9442-OUT,EXP-579-OUT;n:type:ShaderForge.SFN_Multiply,id:3553,x:33771,y:32788,varname:node_3553,prsc:2|A-5795-OUT,B-4862-OUT;n:type:ShaderForge.SFN_Slider,id:4862,x:33362,y:32882,ptovrint:False,ptlb:HologramOpacity,ptin:_HologramOpacity,varname:node_4862,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Add,id:2960,x:33946,y:32882,varname:node_2960,prsc:2|A-3553-OUT,B-5201-OUT;n:type:ShaderForge.SFN_Slider,id:5201,x:33623,y:32968,ptovrint:False,ptlb:Opacity,ptin:_Opacity,varname:node_5201,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.4247056,max:1;n:type:ShaderForge.SFN_Color,id:6824,x:33940,y:32713,ptovrint:False,ptlb:node_6824,ptin:_node_6824,varname:node_6824,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5620134,c2:0.8500926,c3:0.9926471,c4:1;n:type:ShaderForge.SFN_Multiply,id:8753,x:34110,y:32806,varname:node_8753,prsc:2|A-6824-RGB,B-2960-OUT;n:type:ShaderForge.SFN_Multiply,id:4547,x:34109,y:32959,varname:node_4547,prsc:2|A-2960-OUT,B-1242-OUT;n:type:ShaderForge.SFN_Slider,id:1242,x:33794,y:33081,ptovrint:False,ptlb:node_1242,ptin:_node_1242,varname:node_1242,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.561393,max:1;proporder:1315-4913-8306-579-4862-5201-6824-1242;pass:END;sub:END;*/

Shader "Custom/S_Hologram" {
    Properties {
        _HologramDensity ("HologramDensity", Float ) = 10
        _HologramSpeed1 ("HologramSpeed1", Float ) = 0
        _HologramSpeed2 ("HologramSpeed2", Float ) = 10
        _HologramExposure ("HologramExposure", Float ) = 1
        _HologramOpacity ("HologramOpacity", Range(0, 1)) = 1
        _Opacity ("Opacity", Range(0, 1)) = 0.4247056
        _node_6824 ("node_6824", Color) = (0.5620134,0.8500926,0.9926471,1)
        _node_1242 ("node_1242", Range(0, 1)) = 0.561393
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform float _HologramDensity;
            uniform float _HologramSpeed1;
            uniform float _HologramSpeed2;
            uniform float _HologramExposure;
            uniform float _HologramOpacity;
            uniform float _Opacity;
            uniform float4 _node_6824;
            uniform float _node_1242;
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_818 = _Time + _TimeEditor;
                float node_2960 = ((pow(frac((1.0 - ((float2(i.posWorld.r,i.posWorld.b)*float2(1.0,_HologramDensity))+(float2(_HologramSpeed1,_HologramSpeed2)*node_818.r))).g),_HologramExposure)*_HologramOpacity)+_Opacity);
                float3 emissive = (_node_6824.rgb*node_2960);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,(node_2960*_node_1242));
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
