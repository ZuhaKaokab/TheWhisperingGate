Shader "WhisperingGate/HorrorSkybox"
{
    Properties
    {
        [Header(Sky Gradient)]
        _TopColor ("Top Color", Color) = (0.1, 0.02, 0.02, 1)
        _HorizonColor ("Horizon Color", Color) = (0.5, 0.1, 0.05, 1)
        _BottomColor ("Bottom Color", Color) = (0.05, 0.02, 0.02, 1)
        _HorizonSharpness ("Horizon Sharpness", Range(0.1, 2)) = 0.5

        [Header(Sun Moon)]
        _SunColor ("Sun/Moon Color", Color) = (1, 0.3, 0.1, 1)
        _SunDirection ("Sun Direction", Vector) = (0.5, 0.2, 0.5, 0)
        _SunSize ("Sun Size", Range(0.001, 0.5)) = 0.08
        _SunIntensity ("Sun Intensity", Range(0, 2)) = 1
        _SunGlow ("Sun Glow", Range(0, 2)) = 0.5

        [Header(Clouds)]
        _CloudColor ("Cloud Color", Color) = (0.2, 0.05, 0.02, 1)
        _CloudDensity ("Cloud Density", Range(0, 1)) = 0.5
        _CloudSpeed ("Cloud Speed", Range(0, 0.2)) = 0.02
        _CloudScale ("Cloud Scale", Range(0.5, 5)) = 2
        _CloudHeight ("Cloud Height", Range(0, 1)) = 0.3

        [Header(Stars)]
        _StarsIntensity ("Stars Intensity", Range(0, 1)) = 0
        _StarsSpeed ("Stars Twinkle Speed", Range(0, 0.5)) = 0.1
        _StarsDensity ("Stars Density", Range(10, 200)) = 80

        [Header(Distortion)]
        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.02
        _DistortionSpeed ("Distortion Speed", Range(0, 0.1)) = 0.01

        [Header(Animation)]
        _CustomTime ("Custom Time", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Properties
            fixed4 _TopColor;
            fixed4 _HorizonColor;
            fixed4 _BottomColor;
            float _HorizonSharpness;

            fixed4 _SunColor;
            float4 _SunDirection;
            float _SunSize;
            float _SunIntensity;
            float _SunGlow;

            fixed4 _CloudColor;
            float _CloudDensity;
            float _CloudSpeed;
            float _CloudScale;
            float _CloudHeight;

            float _StarsIntensity;
            float _StarsSpeed;
            float _StarsDensity;

            float _DistortionStrength;
            float _DistortionSpeed;
            float _CustomTime;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            // Noise functions
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float hash3(float3 p)
            {
                return frac(sin(dot(p, float3(127.1, 311.7, 74.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for (int i = 0; i < 5; i++)
                {
                    value += amplitude * noise(p * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                
                return value;
            }

            // Procedural stars
            float stars(float3 dir)
            {
                float3 p = dir * _StarsDensity;
                float3 i = floor(p);
                float3 f = frac(p);
                
                float brightness = 0;
                
                // Check neighboring cells
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            float3 cell = i + float3(x, y, z);
                            float3 cellCenter = cell + hash3(cell);
                            float dist = length(p - cellCenter);
                            
                            // Star twinkle
                            float twinkle = sin(_Time.y * _StarsSpeed * 10 + hash3(cell) * 100) * 0.5 + 0.5;
                            
                            // Only show stars above horizon
                            float starMask = saturate(dir.y * 5);
                            
                            brightness += starMask * twinkle * smoothstep(0.1, 0.0, dist) * hash3(cell);
                        }
                    }
                }
                
                return brightness;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.texcoord);
                float time = _CustomTime > 0 ? _CustomTime : _Time.y;
                
                // Add subtle distortion for horror effect
                float distortion = fbm(dir.xz * 3 + time * _DistortionSpeed) * _DistortionStrength;
                dir.y += distortion;
                dir = normalize(dir);
                
                // Calculate height gradient (0 at bottom, 1 at top)
                float heightGradient = dir.y * 0.5 + 0.5;
                
                // Create sky gradient with adjustable sharpness
                float horizonBlend = pow(abs(dir.y), _HorizonSharpness);
                
                fixed4 skyColor;
                if (dir.y >= 0)
                {
                    // Above horizon: blend from horizon to top
                    skyColor = lerp(_HorizonColor, _TopColor, horizonBlend);
                }
                else
                {
                    // Below horizon: blend from horizon to bottom
                    skyColor = lerp(_HorizonColor, _BottomColor, horizonBlend);
                }
                
                // Sun/Moon
                float3 sunDir = normalize(_SunDirection.xyz);
                float sunDist = distance(dir, sunDir);
                
                // Sun disc
                float sunDisc = smoothstep(_SunSize, _SunSize * 0.8, sunDist);
                
                // Sun glow (larger soft glow around sun)
                float sunGlowAmount = exp(-sunDist * (3.0 / max(_SunGlow, 0.01))) * _SunGlow;
                
                // Apply sun
                skyColor.rgb += _SunColor.rgb * sunDisc * _SunIntensity;
                skyColor.rgb += _SunColor.rgb * sunGlowAmount * _SunIntensity * 0.5;
                
                // Clouds
                if (_CloudDensity > 0.01)
                {
                    // Only render clouds above a certain height
                    float cloudMask = smoothstep(_CloudHeight - 0.1, _CloudHeight + 0.2, dir.y);
                    
                    // Scroll clouds
                    float2 cloudUV = dir.xz / (dir.y + 0.5) * _CloudScale;
                    cloudUV += time * _CloudSpeed;
                    
                    // Generate cloud pattern
                    float cloudNoise = fbm(cloudUV);
                    float clouds = smoothstep(1.0 - _CloudDensity, 1.0, cloudNoise);
                    
                    // Add darker cloud layers for depth
                    float cloudNoise2 = fbm(cloudUV * 1.5 + 100);
                    float cloudShadow = smoothstep(0.5, 1.0, cloudNoise2) * 0.3;
                    
                    // Apply clouds
                    skyColor.rgb = lerp(skyColor.rgb, _CloudColor.rgb, clouds * cloudMask);
                    skyColor.rgb = lerp(skyColor.rgb, _CloudColor.rgb * 0.5, cloudShadow * clouds * cloudMask);
                }
                
                // Stars (only visible at night / dark scenes)
                if (_StarsIntensity > 0.01)
                {
                    float starField = stars(dir);
                    skyColor.rgb += starField * _StarsIntensity;
                }
                
                // Vignette at horizon for more atmosphere
                float horizonVignette = 1.0 - smoothstep(0.0, 0.3, abs(dir.y));
                skyColor.rgb *= 1.0 - horizonVignette * 0.2;
                
                return skyColor;
            }
            ENDCG
        }
    }
}

