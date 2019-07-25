using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoK
{
    public class AttackFirstPersonSync : MonoBehaviour
    {
        public Collider[] serverOnlyColliders;
        public MeshRenderer[] clientOnlyRenderers;
        public ParticleSystem[] clientOnlyParticleSystems;
        public TrailRenderer[] clientOnlyTrailRenderers;
    }
}
