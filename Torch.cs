using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MonoBehaviour
{
    [SerializeField, Tooltip("Fire Particle System")]
    new ParticleSystem particleSystem;

    [SerializeField]
    bool isOn = true;
    public bool IsOn { get { return IsOn; } set { isOn = value; } }

    private void Awake()
    {
        particleSystem = particleSystem != null ? particleSystem : GetComponent<ParticleSystem>();
        if (particleSystem == null)
            Debug.LogError($"{name} is missing an particle system");
    }

    private void OnTriggerEnter(Collider other)
    {
        var disc = other.GetComponent<Disc>();
        if (disc != null)
            OnDiscTriggerEnter(disc);
    }

    private void LateUpdate()
    {
        if (!isOn)
            particleSystem.Stop();
        else if (!particleSystem.isPlaying)
            particleSystem.Play();
    }

    void OnDiscTriggerEnter(Disc disc)
    {
        if (disc.ElementalState == DiscElementalState.Fire)
            isOn = true;
        else if (isOn)
            disc.ElementalState = DiscElementalState.Fire;
    }
}
