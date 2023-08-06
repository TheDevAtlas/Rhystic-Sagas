using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Blur")]
public class BlurSettings : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Standard Deviation (spread) of the blur. Grid size is approx. 3x larger.")]
    public ClampedFloatParameter strength = new ClampedFloatParameter(0.0f, 0.0f, 15.0f);
    [Tooltip("Standard Deviation (spread) of the blur. Grid size is approx. 3x larger.")]
    public ClampedFloatParameter second = new ClampedFloatParameter(0.0f, 0.0f, 15.0f);
    [Tooltip("Your tooltip description here.")]
    public TextureParameter hatch = new TextureParameter(null);


    public bool IsActive()
    {
        return (strength.value > 0.0f) && active;   
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
