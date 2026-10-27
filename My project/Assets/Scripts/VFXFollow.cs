using UnityEngine;

public class VFXFollow : MonoBehaviour
{
    private Transform target;
    private Vector3 offset;

    public void Initialize(Transform followTarget, Vector3 vfxOffset)
    {
        target = followTarget;
        offset = vfxOffset;

        if (target == null)
        {
            return;
        }

        // Ensure local position and rotation are cleanly set
        transform.localPosition = offset;
        transform.localRotation = Quaternion.identity;

        // Ensure all particle systems use Local simulation space so they follow cleanly
        ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in systems)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
        }
    }
}