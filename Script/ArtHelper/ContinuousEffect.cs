using System.Collections;
using UnityEngine;
class ContinuousEffect : MonoBehaviour
{
    public void show()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        enableParticle(true);
    }

    public void hide(bool destroy, float delay = 2)
    {
        if (!gameObject.activeSelf) return;
        StopAllCoroutines();
        enableParticle(false);
        StartCoroutine(delayHide(destroy, delay));
    }

    IEnumerator delayHide(bool destroy, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (destroy)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    void enableParticle(bool enable)
    {
        var particleSystems = GetComponentsInChildren<ParticleSystem>();
        if (particleSystems != null)
        {
            particleSystems.forEach(a =>
            {
                var emission = a.emission;
                emission.enabled = enable;
            });
        }

        var trailRenderers = GetComponentsInChildren<TrailRenderer>();
        if (trailRenderers != null)
        {
            trailRenderers.forEach(a =>
            {
                a.enabled = enable;
            });
        }
    }
}
