using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightFakeBounceLight : MonoBehaviour
{
    [SerializeField] private Light fakeBounceLight;
    
    [SerializeField] private float maxDistance;
    [SerializeField] private LayerMask layerMask;

    [SerializeField] private float maxIntensity;

    private bool _wasTargeting;
    private Quaternion _lastRot;
    private Vector3 _lastPos;

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out var hit, maxDistance, layerMask))
        {
            var fac =  (maxDistance - hit.distance) / maxDistance;
            var desiredIntensity = fac * fac * maxIntensity;
            
            fakeBounceLight.intensity = desiredIntensity;
            fakeBounceLight.transform.position = hit.point;
            fakeBounceLight.transform.rotation = Quaternion.LookRotation(hit.normal);
        }
        else
        {
            fakeBounceLight.intensity = 0;
        }
    }
}
