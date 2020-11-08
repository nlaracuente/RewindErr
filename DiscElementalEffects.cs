using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Disc), typeof(TrailRenderer))]
public class DiscElementalEffects : MonoBehaviour
{
    [System.Serializable]
    struct TrailColor
    {
        public Color start;
        public Color end;
    }

    [System.Serializable]
    struct ElementalTrailColor
    {
        public DiscElementalState element;
        public TrailColor color;
    }

    [SerializeField]
    ParticleSystem fireParticles;

    [SerializeField]
    ElementalTrailColor[] elementalTrailColors;
    Dictionary<DiscElementalState, TrailColor> trailColors;


    TrailRenderer trailRenderer;
    Disc disc;

    private void Awake()
    {
        disc = GetComponent<Disc>();
        trailRenderer = GetComponent<TrailRenderer>();

        trailColors = new Dictionary<DiscElementalState, TrailColor>();
        if (elementalTrailColors != null)
            foreach (var e in elementalTrailColors)
                if (!trailColors.ContainsKey(e.element))
                    trailColors.Add(e.element, e.color);
    }

    private void LateUpdate()
    {
        switch(disc.ElementalState)
        {
            case DiscElementalState.Fire:
                if(disc.State == DiscState.Flying)
                    fireParticles.Play();
                break;
            default:
                fireParticles.Stop();
                break;
        }

        if (trailColors.ContainsKey(disc.ElementalState))
        {
            var color = trailColors[disc.ElementalState];
            trailRenderer.startColor = color.start;
            trailRenderer.startColor = color.end;
        }   
    }
}
