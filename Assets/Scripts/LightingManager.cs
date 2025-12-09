using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script controls the day night lighting cycle
// it works in both play mode and edit mode for easy testing
[ExecuteInEditMode]
public class LightingManager : MonoBehaviour
{
    [SerializeField]
    private Light DirectionalLight;

    [SerializeField]
    private LightingPreset Preset;

    [SerializeField, Range(0, 24)]
    private float TimeOfDay;

    private void Update()
    {
        if(Preset == null)
            return;

        // in play mode automatically advance time
        if(Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime;
            TimeOfDay %= 24; 
            UpdateLighting(TimeOfDay / 24);
        }   
        else
        {
            // in edit mode just update based on slider value
            UpdateLighting(TimeOfDay / 24);
        }
    }

    // update all lighting based on time of day
    private void UpdateLighting(float TimePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(TimePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(TimePercent);

        if(DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(TimePercent);
            // rotate sun to simulate day night cycle
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((TimePercent * 360f) - 90f, 170f, 0));
        }
    }

    // auto find the directional light in the scene
    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        if (RenderSettings.sun != null)
        {
                DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach(Light light in lights)
            {
                if(light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }
}
