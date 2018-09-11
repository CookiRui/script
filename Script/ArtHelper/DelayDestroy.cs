using System.Collections;
using UnityEngine;
class DelayDestroy : MonoBehaviour
{
    public float destroyTime = 5.0f;

    void Start()
    {
        StartCoroutine(delayDestroy());
    }

    IEnumerator delayDestroy()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(gameObject);
    }
}
