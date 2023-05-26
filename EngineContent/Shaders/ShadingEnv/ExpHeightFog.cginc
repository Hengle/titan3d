#ifndef __EXP_HEIGHT_FOG_H__
#define __EXP_HEIGHT_FOG_H__

// ExponentialFogParameters: FogDensity * exp2(-FogHeightFalloff * (CameraWorldPosition.z - FogHeight)) in x, FogHeightFalloff in y, MaxWorldObserverHeight in z, StartDistance in w. 
// ExponentialFogParameters2: FogDensitySecond * exp2(-FogHeightFalloffSecond * (CameraWorldPosition.z - FogHeightSecond)) in x, FogHeightFalloffSecond in y, FogDensitySecond in z, FogHeightSecond in w 
// ExponentialFogParameters3: FogDensity in x, FogHeight in y, whether to use cubemap fog color in z, FogCutoffDistance in w. 
// FogInscatteringTextureParameters: mip distance scale in x, bias in y, num mips in z 
struct FFogStruct
{
    /// Uniform Begin
    float3 FogColor;
    float MinFogOpacity; 

    float FogDensity;
    float FogEnd;
    float FogHeightFalloff;
    float StartDistance;

    float3 InscatterColor;
    float InscatteringExponent;

    float3 LightPosition;
    float InscatterStartDistance;
    /// Uniform End

    float3 GetExpHeightFogColor(float3 worldPos, float3 color, float linearDepth, float3 ray, float2 uv)
    {
        float3 posInCamera = linearDepth * ray.xyz;
        //�ı����ϵ�  ��ǰ���  �������ֵ
        worldPos = CameraPosition + posInCamera;

        float fogDensity = FogDensity * exp2(-FogHeightFalloff * (CameraPosition.y - FogEnd));
        float falloff = FogHeightFalloff * (worldPos.y - CameraPosition.y);
        float fogFactor = (1 - exp2(-falloff)) / falloff;

        float fog = fogDensity * fogFactor;

        float PosInCameraLength = length(posInCamera);
        //�������Ӱ������,fragment��������ľ���
        fog *= max(PosInCameraLength - StartDistance, 0);
        fog = saturate(fog);

        //�������
        float sunAmount = pow(saturate(dot(normalize(posInCamera), normalize(LightPosition.xyz))), InscatteringExponent);

        float dirExponentialHeightLineIntegral = max(PosInCameraLength - InscatterStartDistance, 0.0f);
        float DirectionalInscatteringFogFactor = saturate(exp2(-dirExponentialHeightLineIntegral));
        sunAmount *= (1 - DirectionalInscatteringFogFactor);

        float3 fogColor = FogColor;
        //�����ɫ�͹�����ɫ
        fogColor.rgb = lerp(FogColor.rgb, InscatterColor.rgb, sunAmount);

        return lerp(color.rgb, fogColor.rgb, fog);
    }
};

#endif
//