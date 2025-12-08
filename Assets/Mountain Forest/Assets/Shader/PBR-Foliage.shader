// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:3,spmd:0,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:True,hqlp:False,rprd:True,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:2865,x:32719,y:32712,varname:node_2865,prsc:2|diff-6343-OUT,spec-1509-OUT,gloss-1813-OUT,normal-5964-RGB,amspl-167-OUT,clip-2252-OUT;n:type:ShaderForge.SFN_Multiply,id:6343,x:32523,y:32573,varname:node_6343,prsc:2|A-7736-RGB,B-7743-OUT;n:type:ShaderForge.SFN_Tex2d,id:7736,x:32329,y:32355,ptovrint:True,ptlb:Diffuse,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:1ae210fd011cfc445aa4c06d793a3c6a,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:5964,x:32435,y:32917,ptovrint:True,ptlb:Normal Map,ptin:_BumpMap,varname:_BumpMap,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:e9dce08c6cca8c148ad88cf6f232e92e,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Slider,id:1813,x:32356,y:32819,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:_Metallic_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.6158119,max:1;n:type:ShaderForge.SFN_Lerp,id:7743,x:32208,y:32621,varname:node_7743,prsc:2|A-8515-OUT,B-9738-OUT,T-9651-RGB;n:type:ShaderForge.SFN_VertexColor,id:9651,x:32045,y:32680,varname:node_9651,prsc:2;n:type:ShaderForge.SFN_Vector3,id:8515,x:32045,y:32526,varname:node_8515,prsc:2,v1:0.2,v2:0.8,v3:0.2;n:type:ShaderForge.SFN_Vector1,id:9738,x:32045,y:32621,varname:node_9738,prsc:2,v1:1;n:type:ShaderForge.SFN_Lerp,id:167,x:32435,y:33086,varname:node_167,prsc:2|A-3242-OUT,B-5416-OUT,T-5573-OUT;n:type:ShaderForge.SFN_Multiply,id:3242,x:32011,y:33313,varname:node_3242,prsc:2|A-2529-OUT,B-5416-OUT;n:type:ShaderForge.SFN_LightAttenuation,id:5573,x:30461,y:33145,varname:node_5573,prsc:2;n:type:ShaderForge.SFN_Vector3,id:2529,x:31826,y:33331,varname:node_2529,prsc:2,v1:0.6,v2:0.6,v3:0.4;n:type:ShaderForge.SFN_Lerp,id:5416,x:31826,y:33198,varname:node_5416,prsc:2|A-9559-OUT,B-4263-OUT,T-1048-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:1048,x:31658,y:33316,varname:node_1048,prsc:2,min:0,max:65|IN-1464-OUT;n:type:ShaderForge.SFN_Add,id:1464,x:31497,y:33316,varname:node_1464,prsc:2|A-5318-OUT,B-4844-OUT;n:type:ShaderForge.SFN_Lerp,id:1509,x:32147,y:32833,varname:node_1509,prsc:2|A-7145-OUT,B-8337-OUT,T-622-RGB;n:type:ShaderForge.SFN_Vector1,id:7145,x:31940,y:32819,varname:node_7145,prsc:2,v1:0;n:type:ShaderForge.SFN_Tex2d,id:622,x:31940,y:32915,ptovrint:False,ptlb:Specular,ptin:_Specular,varname:node_622,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:2f53b581c233ce9429c613b8ce8966f5,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Lerp,id:8337,x:31778,y:32857,varname:node_8337,prsc:2|A-596-RGB,B-9939-OUT,T-3338-OUT;n:type:ShaderForge.SFN_Lerp,id:9939,x:31569,y:32857,varname:node_9939,prsc:2|A-9998-OUT,B-1618-OUT,T-7811-OUT;n:type:ShaderForge.SFN_Vector3,id:8204,x:31758,y:32560,varname:node_8204,prsc:2,v1:0.5,v2:0.8,v3:0.2;n:type:ShaderForge.SFN_Clamp01,id:3338,x:32266,y:33445,varname:node_3338,prsc:2|IN-2591-OUT;n:type:ShaderForge.SFN_Add,id:2591,x:32266,y:33587,varname:node_2591,prsc:2|A-1537-OUT,B-1537-OUT;n:type:ShaderForge.SFN_Power,id:1537,x:32084,y:33623,varname:node_1537,prsc:2|VAL-9929-OUT,EXP-7075-OUT;n:type:ShaderForge.SFN_Power,id:7811,x:31906,y:33570,varname:node_7811,prsc:2|VAL-9929-OUT,EXP-4245-OUT;n:type:ShaderForge.SFN_Divide,id:7075,x:31906,y:33719,varname:node_7075,prsc:2|A-4245-OUT,B-9666-OUT;n:type:ShaderForge.SFN_Vector1,id:9666,x:31696,y:33784,varname:node_9666,prsc:2,v1:2.8;n:type:ShaderForge.SFN_Vector1,id:4245,x:31696,y:33719,varname:node_4245,prsc:2,v1:50;n:type:ShaderForge.SFN_Vector3,id:1618,x:31341,y:32835,varname:node_1618,prsc:2,v1:0.6,v2:0.6,v3:0.8;n:type:ShaderForge.SFN_Vector3,id:9998,x:31341,y:32748,varname:node_9998,prsc:2,v1:0.6,v2:0.6,v3:0.7;n:type:ShaderForge.SFN_HalfVector,id:8421,x:31555,y:33622,varname:node_8421,prsc:2;n:type:ShaderForge.SFN_NormalVector,id:7073,x:31555,y:33481,prsc:2,pt:False;n:type:ShaderForge.SFN_Dot,id:9929,x:31737,y:33481,varname:node_9929,prsc:2,dt:1|A-7073-OUT,B-8421-OUT;n:type:ShaderForge.SFN_Multiply,id:4263,x:31312,y:33246,varname:node_4263,prsc:2|A-2617-RGB,B-2617-A,C-3957-OUT;n:type:ShaderForge.SFN_Vector1,id:3957,x:31312,y:33374,varname:node_3957,prsc:2,v1:3;n:type:ShaderForge.SFN_Multiply,id:5318,x:31263,y:32956,varname:node_5318,prsc:2|A-5048-OUT,B-968-OUT;n:type:ShaderForge.SFN_Lerp,id:4844,x:31134,y:33482,varname:node_4844,prsc:2|A-3492-OUT,B-3867-OUT,T-5573-OUT;n:type:ShaderForge.SFN_Vector1,id:3867,x:30948,y:33537,varname:node_3867,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:3492,x:30948,y:33482,varname:node_3492,prsc:2,v1:0.2;n:type:ShaderForge.SFN_Cubemap,id:2617,x:31073,y:33261,ptovrint:False,ptlb:IBL_Spec,ptin:_IBL_Spec,varname:node_2617,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,pvfc:0|MIP-3400-OUT;n:type:ShaderForge.SFN_RemapRange,id:3400,x:30911,y:33261,varname:node_3400,prsc:2,frmn:0,frmx:1,tomn:0,tomx:6|IN-6615-OUT;n:type:ShaderForge.SFN_Vector1,id:6615,x:30911,y:33196,varname:node_6615,prsc:2,v1:1;n:type:ShaderForge.SFN_Add,id:5048,x:31057,y:32875,varname:node_5048,prsc:2|A-8711-OUT,B-8711-OUT;n:type:ShaderForge.SFN_Multiply,id:8711,x:30888,y:32876,varname:node_8711,prsc:2|A-386-OUT,B-386-OUT;n:type:ShaderForge.SFN_Fresnel,id:386,x:30723,y:32876,varname:node_386,prsc:2|EXP-3722-OUT;n:type:ShaderForge.SFN_Lerp,id:3722,x:30558,y:32761,varname:node_3722,prsc:2|A-8696-OUT,B-6575-OUT,T-5573-OUT;n:type:ShaderForge.SFN_Vector1,id:8696,x:30377,y:32740,varname:node_8696,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Vector1,id:6575,x:30377,y:32795,varname:node_6575,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Lerp,id:968,x:30841,y:33031,varname:node_968,prsc:2|A-3569-OUT,B-9413-OUT,T-5573-OUT;n:type:ShaderForge.SFN_Vector1,id:9413,x:30654,y:33065,varname:node_9413,prsc:2,v1:0.45;n:type:ShaderForge.SFN_Vector1,id:3569,x:30654,y:33016,varname:node_3569,prsc:2,v1:0.1;n:type:ShaderForge.SFN_Vector1,id:9559,x:31826,y:33140,varname:node_9559,prsc:2,v1:0;n:type:ShaderForge.SFN_VertexColor,id:596,x:31549,y:32462,varname:node_596,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2252,x:32846,y:32290,varname:node_2252,prsc:2|A-7736-A,B-2264-OUT;n:type:ShaderForge.SFN_Slider,id:2264,x:32523,y:32404,ptovrint:False,ptlb:Alpha Cutoff,ptin:_AlphaCutoff,varname:node_2264,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;proporder:5964-7736-1813-622-2617-2264;pass:END;sub:END;*/

Shader "Greenworks/PBR-Foliage" {
    Properties {
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _MainTex ("Diffuse", 2D) = "black" {}
        _Gloss ("Gloss", Range(0, 1)) = 0.6158119
        _Specular ("Specular", 2D) = "white" {}
        _IBL_Spec ("IBL_Spec", Cube) = "_Skybox" {}
        _AlphaCutoff ("Alpha Cutoff", Range(0, 1)) = 1
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _Gloss;
            uniform sampler2D _Specular; uniform float4 _Specular_ST;
            uniform samplerCUBE _IBL_Spec;
            uniform float _AlphaCutoff;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
                #if defined(LIGHTMAP_ON) || defined(UNITY_SHOULD_SAMPLE_SH)
                    float4 ambientOrLightmapUV : TEXCOORD10;
                #endif
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                #ifdef LIGHTMAP_ON
                    o.ambientOrLightmapUV.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
                    o.ambientOrLightmapUV.zw = 0;
                #elif UNITY_SHOULD_SAMPLE_SH
                #endif
                #ifdef DYNAMICLIGHTMAP_ON
                    o.ambientOrLightmapUV.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
                #endif
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap)));
                float3 normalLocal = _BumpMap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip((_MainTex_var.a*_AlphaCutoff) - 0.5);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _Gloss;
                float perceptualRoughness = 1.0 - _Gloss;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
/////// GI Data:
                UnityLight light;
                #ifdef LIGHTMAP_OFF
                    light.color = lightColor;
                    light.dir = lightDirection;
                    light.ndotl = LambertTerm (normalDirection, light.dir);
                #else
                    light.color = half3(0.f, 0.f, 0.f);
                    light.ndotl = 0.0f;
                    light.dir = half3(0.f, 0.f, 0.f);
                #endif
                UnityGIInput d;
                d.light = light;
                d.worldPos = i.posWorld.xyz;
                d.worldViewDir = viewDirection;
                d.atten = attenuation;
                #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
                    d.ambient = 0;
                    d.lightmapUV = i.ambientOrLightmapUV;
                #else
                    d.ambient = i.ambientOrLightmapUV;
                #endif
                #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMin[0] = unity_SpecCube0_BoxMin;
                    d.boxMin[1] = unity_SpecCube1_BoxMin;
                #endif
                #if UNITY_SPECCUBE_BOX_PROJECTION
                    d.boxMax[0] = unity_SpecCube0_BoxMax;
                    d.boxMax[1] = unity_SpecCube1_BoxMax;
                    d.probePosition[0] = unity_SpecCube0_ProbePosition;
                    d.probePosition[1] = unity_SpecCube1_ProbePosition;
                #endif
                d.probeHDR[0] = unity_SpecCube0_HDR;
                d.probeHDR[1] = unity_SpecCube1_HDR;
                Unity_GlossyEnvironmentData ugls_en_data;
                ugls_en_data.roughness = 1.0 - gloss;
                ugls_en_data.reflUVW = viewReflectDirection;
                UnityGI gi = UnityGlobalIllumination(d, 1, normalDirection, ugls_en_data );
                lightDirection = gi.light.dir;
                lightColor = gi.light.color;
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float node_9559 = 0.0;
                float4 _IBL_Spec_var = texCUBElod(_IBL_Spec,float4(viewReflectDirection,(1.0*6.0+0.0)));
                float node_386 = pow(1.0-max(0,dot(normalDirection, viewDirection)),lerp(0.5,0.5,attenuation));
                float node_8711 = (node_386*node_386);
                float3 node_5416 = lerp(float3(node_9559,node_9559,node_9559),(_IBL_Spec_var.rgb*_IBL_Spec_var.a*3.0),clamp((((node_8711+node_8711)*lerp(0.1,0.45,attenuation))+lerp(0.2,0.0,attenuation)),0,65));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float node_7145 = 0.0;
                float node_9929 = max(0,dot(i.normalDir,halfDirection));
                float node_4245 = 50.0;
                float node_1537 = pow(node_9929,(node_4245/2.8));
                float4 _Specular_var = tex2D(_Specular,TRANSFORM_TEX(i.uv0, _Specular));
                float3 specularColor = lerp(float3(node_7145,node_7145,node_7145),lerp(i.vertexColor.rgb,lerp(float3(0.6,0.6,0.7),float3(0.6,0.6,0.8),pow(node_9929,node_4245)),saturate((node_1537+node_1537))),_Specular_var.rgb);
                float specularMonochrome;
                float node_9738 = 1.0;
                float3 diffuseColor = (_MainTex_var.rgb*lerp(float3(0.2,0.8,0.2),float3(node_9738,node_9738,node_9738),i.vertexColor.rgb)); // Need this for specular when using metallic
                diffuseColor = EnergyConservationBetweenDiffuseAndSpecular(diffuseColor, specularColor, specularMonochrome);
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                half surfaceReduction;
                #ifdef UNITY_COLORSPACE_GAMMA
                    surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;
                #else
                    surfaceReduction = 1.0/(roughness*roughness + 1.0);
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                half grazingTerm = saturate( gloss + specularMonochrome );
                float3 indirectSpecular = (gi.indirect.specular + lerp((float3(0.6,0.6,0.4)*node_5416),node_5416,attenuation));
                indirectSpecular *= FresnelLerp (specularColor, grazingTerm, NdotV);
                indirectSpecular *= surfaceReduction;
                float3 specular = (directSpecular + indirectSpecular);
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += gi.indirect.diffuse;
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _BumpMap; uniform float4 _BumpMap_ST;
            uniform float _Gloss;
            uniform sampler2D _Specular; uniform float4 _Specular_ST;
            uniform float _AlphaCutoff;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float3 tangentDir : TEXCOORD5;
                float3 bitangentDir : TEXCOORD6;
                float4 vertexColor : COLOR;
                LIGHTING_COORDS(7,8)
                UNITY_FOG_COORDS(9)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _BumpMap_var = UnpackNormal(tex2D(_BumpMap,TRANSFORM_TEX(i.uv0, _BumpMap)));
                float3 normalLocal = _BumpMap_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip((_MainTex_var.a*_AlphaCutoff) - 0.5);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float Pi = 3.141592654;
                float InvPi = 0.31830988618;
///////// Gloss:
                float gloss = _Gloss;
                float perceptualRoughness = 1.0 - _Gloss;
                float roughness = perceptualRoughness * perceptualRoughness;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float LdotH = saturate(dot(lightDirection, halfDirection));
                float node_7145 = 0.0;
                float node_9929 = max(0,dot(i.normalDir,halfDirection));
                float node_4245 = 50.0;
                float node_1537 = pow(node_9929,(node_4245/2.8));
                float4 _Specular_var = tex2D(_Specular,TRANSFORM_TEX(i.uv0, _Specular));
                float3 specularColor = lerp(float3(node_7145,node_7145,node_7145),lerp(i.vertexColor.rgb,lerp(float3(0.6,0.6,0.7),float3(0.6,0.6,0.8),pow(node_9929,node_4245)),saturate((node_1537+node_1537))),_Specular_var.rgb);
                float specularMonochrome;
                float node_9738 = 1.0;
                float3 diffuseColor = (_MainTex_var.rgb*lerp(float3(0.2,0.8,0.2),float3(node_9738,node_9738,node_9738),i.vertexColor.rgb)); // Need this for specular when using metallic
                diffuseColor = EnergyConservationBetweenDiffuseAndSpecular(diffuseColor, specularColor, specularMonochrome);
                specularMonochrome = 1.0-specularMonochrome;
                float NdotV = abs(dot( normalDirection, viewDirection ));
                float NdotH = saturate(dot( normalDirection, halfDirection ));
                float VdotH = saturate(dot( viewDirection, halfDirection ));
                float visTerm = SmithJointGGXVisibilityTerm( NdotL, NdotV, roughness );
                float normTerm = GGXTerm(NdotH, roughness);
                float specularPBL = (visTerm*normTerm) * UNITY_PI;
                #ifdef UNITY_COLORSPACE_GAMMA
                    specularPBL = sqrt(max(1e-4h, specularPBL));
                #endif
                specularPBL = max(0, specularPBL * NdotL);
                #if defined(_SPECULARHIGHLIGHTS_OFF)
                    specularPBL = 0.0;
                #endif
                specularPBL *= any(specularColor) ? 1.0 : 0.0;
                float3 directSpecular = attenColor*specularPBL*FresnelTerm(specularColor, LdotH);
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                half fd90 = 0.5 + 2 * LdotH * LdotH * (1-gloss);
                float nlPow5 = Pow5(1-NdotL);
                float nvPow5 = Pow5(1-NdotV);
                float3 directDiffuse = ((1 +(fd90 - 1)*nlPow5) * (1 + (fd90 - 1)*nvPow5) * NdotL) * attenColor;
                diffuseColor *= 1-specularMonochrome;
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _AlphaCutoff;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
                float2 uv1 : TEXCOORD2;
                float2 uv2 : TEXCOORD3;
                float4 posWorld : TEXCOORD4;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip((_MainTex_var.a*_AlphaCutoff) - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
        Pass {
            Name "Meta"
            Tags {
                "LightMode"="Meta"
            }
            Cull Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_META 1
            #define SHOULD_SAMPLE_SH ( defined (LIGHTMAP_OFF) && defined(DYNAMICLIGHTMAP_OFF) )
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "UnityMetaPass.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            #pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
            #pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _Gloss;
            uniform sampler2D _Specular; uniform float4 _Specular_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float4 posWorld : TEXCOORD3;
                float3 normalDir : TEXCOORD4;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.uv2 = v.texcoord2;
                o.vertexColor = v.vertexColor;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : SV_Target {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 halfDirection = normalize(viewDirection+lightDirection);
                UnityMetaInput o;
                UNITY_INITIALIZE_OUTPUT( UnityMetaInput, o );
                
                o.Emission = 0;
                
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                float node_9738 = 1.0;
                float3 diffColor = (_MainTex_var.rgb*lerp(float3(0.2,0.8,0.2),float3(node_9738,node_9738,node_9738),i.vertexColor.rgb));
                float node_7145 = 0.0;
                float node_9929 = max(0,dot(i.normalDir,halfDirection));
                float node_4245 = 50.0;
                float node_1537 = pow(node_9929,(node_4245/2.8));
                float4 _Specular_var = tex2D(_Specular,TRANSFORM_TEX(i.uv0, _Specular));
                float3 specColor = lerp(float3(node_7145,node_7145,node_7145),lerp(i.vertexColor.rgb,lerp(float3(0.6,0.6,0.7),float3(0.6,0.6,0.8),pow(node_9929,node_4245)),saturate((node_1537+node_1537))),_Specular_var.rgb);
                float specularMonochrome = max(max(specColor.r, specColor.g),specColor.b);
                diffColor *= (1.0-specularMonochrome);
                float roughness = 1.0 - _Gloss;
                o.Albedo = diffColor + specColor * roughness * roughness * 0.5;
                
                return UnityMetaFragment( o );
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
