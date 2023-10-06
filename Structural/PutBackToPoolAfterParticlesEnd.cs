using System.Collections;
using System.Linq;
using UnityEngine;

public class PutBackToPoolAfterParticlesEnd : MonoBehaviour
{

    private async void OnEnable()
    {

        var systems = GetComponentsInChildren<ParticleSystem>().ToList();
        systems.Add(GetComponent<ParticleSystem>());
        foreach (var system in systems)
        {
            system.Play();
        }

        StartCoroutine(wait());
        IEnumerator wait()
        {
            var delay = systems.Max(s => s.main.duration);
            yield return new WaitForSeconds(delay + 0.2f);

            Pooler.Put(gameObject);
        }

    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
