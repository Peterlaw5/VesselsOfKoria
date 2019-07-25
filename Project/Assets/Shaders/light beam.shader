// Shader created with Shader Forge v1.38 
// Shader Forge (c) Freya Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:0,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:True,fgod:False,fgor:False,fgmd:0,fgcr:0,fgcg:0,fgcb:0,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:True,fnfb:True,fsmp:False;n:type:ShaderForge.SFN_Final,id:4795,x:32833,y:32607,varname:node_4795,prsc:2|emission-2393-OUT;n:type:ShaderForge.SFN_Tex2d,id:6074,x:32239,y:32618,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:575f95a3f94383f49bd37672459f08c3,ntxv:2,isnm:False|UVIN-9158-OUT;n:type:ShaderForge.SFN_Multiply,id:2393,x:32655,y:32648,varname:node_2393,prsc:2|A-9682-OUT,B-2053-RGB,C-797-RGB,D-9248-OUT,E-9503-OUT;n:type:ShaderForge.SFN_VertexColor,id:2053,x:32235,y:32780,varname:node_2053,prsc:2;n:type:ShaderForge.SFN_Color,id:797,x:32235,y:32930,ptovrint:True,ptlb:Color,ptin:_TintColor,varname:_TintColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Vector1,id:9248,x:32235,y:33081,varname:node_9248,prsc:2,v1:2;n:type:ShaderForge.SFN_Multiply,id:9503,x:32482,y:32727,varname:node_9503,prsc:2|A-6074-A,B-797-A;n:type:ShaderForge.SFN_ValueProperty,id:3006,x:31268,y:32596,ptovrint:False,ptlb:xSpeed,ptin:_xSpeed,varname:node_3006,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.3;n:type:ShaderForge.SFN_ValueProperty,id:8759,x:31268,y:32682,ptovrint:False,ptlb:ySpeed,ptin:_ySpeed,varname:node_8759,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Append,id:7653,x:31484,y:32568,varname:node_7653,prsc:2|A-3006-OUT,B-8759-OUT;n:type:ShaderForge.SFN_Multiply,id:8707,x:31730,y:32535,varname:node_8707,prsc:2|A-1874-T,B-7653-OUT;n:type:ShaderForge.SFN_Time,id:1874,x:31268,y:32417,varname:node_1874,prsc:2;n:type:ShaderForge.SFN_TexCoord,id:5555,x:31268,y:32204,varname:node_5555,prsc:2,uv:0,uaff:False;n:type:ShaderForge.SFN_Add,id:9158,x:31917,y:32478,varname:node_9158,prsc:2|A-5555-UVOUT,B-8707-OUT,C-6644-OUT;n:type:ShaderForge.SFN_Tex2d,id:3680,x:32245,y:32422,ptovrint:False,ptlb:mask frame,ptin:_maskframe,varname:node_3680,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:3a5a96df060a5cf4a9cc0c59e13486b7,ntxv:0,isnm:False|UVIN-9158-OUT;n:type:ShaderForge.SFN_Multiply,id:9682,x:32450,y:32509,varname:node_9682,prsc:2|A-3680-RGB,B-6074-RGB;n:type:ShaderForge.SFN_Tex2d,id:7915,x:31469,y:32055,ptovrint:False,ptlb:Distrortion,ptin:_Distrortion,varname:node_7915,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:28c7aad1372ff114b90d330f8a2dd938,ntxv:2,isnm:False|UVIN-8047-OUT;n:type:ShaderForge.SFN_ComponentMask,id:8273,x:31669,y:32055,varname:node_8273,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-7915-RGB;n:type:ShaderForge.SFN_Slider,id:1145,x:31469,y:32255,ptovrint:False,ptlb:dsitort power,ptin:_dsitortpower,varname:node_1145,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.1,max:1;n:type:ShaderForge.SFN_Vector1,id:5740,x:31741,y:32313,varname:node_5740,prsc:2,v1:2;n:type:ShaderForge.SFN_Multiply,id:1795,x:31956,y:32098,varname:node_1795,prsc:2|A-8273-OUT,B-5740-OUT;n:type:ShaderForge.SFN_Subtract,id:2882,x:32209,y:32086,varname:node_2882,prsc:2|A-1795-OUT,B-7202-OUT;n:type:ShaderForge.SFN_Vector1,id:7202,x:31940,y:32269,varname:node_7202,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:6644,x:32473,y:32184,varname:node_6644,prsc:2|A-2882-OUT,B-1145-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1897,x:31268,y:32055,ptovrint:False,ptlb:xNoiseSpeed,ptin:_xNoiseSpeed,varname:_xSpeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.3;n:type:ShaderForge.SFN_ValueProperty,id:1879,x:31268,y:32144,ptovrint:False,ptlb:yNoiseSpeed,ptin:_yNoiseSpeed,varname:_ySpeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0;n:type:ShaderForge.SFN_Append,id:5523,x:31462,y:31915,varname:node_5523,prsc:2|A-1897-OUT,B-1879-OUT;n:type:ShaderForge.SFN_Multiply,id:815,x:31708,y:31882,varname:node_815,prsc:2|A-1874-T,B-5523-OUT;n:type:ShaderForge.SFN_Add,id:8047,x:31897,y:31909,varname:node_8047,prsc:2|A-815-OUT,B-5555-UVOUT;proporder:6074-797-3006-8759-3680-7915-1145-1897-1879;pass:END;sub:END;*/

Shader "Vok/laserBeam" {
    Properties {
        _MainTex ("MainTex", 2D) = "black" {}
        [HDR]_TintColor ("Color", Color) = (0.5,0.5,0.5,1)
        _xSpeed ("xSpeed", Float ) = 0.3
        _ySpeed ("ySpeed", Float ) = 0
        _maskframe ("mask frame", 2D) = "white" {}
        _Distrortion ("Distrortion", 2D) = "black" {}
        _dsitortpower ("dsitort power", Range(0, 1)) = 0.1
        _xNoiseSpeed ("xNoiseSpeed", Float ) = 0.3
        _yNoiseSpeed ("yNoiseSpeed", Float ) = 0
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float4 _TintColor;
            uniform float _xSpeed;
            uniform float _ySpeed;
            uniform sampler2D _maskframe; uniform float4 _maskframe_ST;
            uniform sampler2D _Distrortion; uniform float4 _Distrortion_ST;
            uniform float _dsitortpower;
            uniform float _xNoiseSpeed;
            uniform float _yNoiseSpeed;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_1874 = _Time;
                float2 node_8047 = ((node_1874.g*float2(_xNoiseSpeed,_yNoiseSpeed))+i.uv0);
                float4 _Distrortion_var = tex2D(_Distrortion,TRANSFORM_TEX(node_8047, _Distrortion));
                float2 node_8273 = _Distrortion_var.rgb.rg;
                float2 node_6644 = (((node_8273*2.0)-1.0)*_dsitortpower);
                float4 _maskframe_var = tex2D(_maskframe,TRANSFORM_TEX((i.uv0+(node_1874.g*float2(_xSpeed,_ySpeed))+node_6644), _maskframe));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX((i.uv0+(node_1874.g*float2(_xSpeed,_ySpeed))+node_6644), _MainTex));
                float3 emissive = ((_maskframe_var.rgb*_MainTex_var.rgb)*i.vertexColor.rgb*_TintColor.rgb*2.0*(_MainTex_var.a*_TintColor.a));
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }
    CustomEditor "ShaderForgeMaterialInspector"
}
