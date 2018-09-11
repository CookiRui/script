using UnityEngine;
class DrawRangeTest : MonoBehaviour
{
    public float radius = 2;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + new Vector3{y = 0.2f}, radius);
    }
}
