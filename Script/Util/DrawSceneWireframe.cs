using UnityEngine;
class DrawSceneWireframe : MonoBehaviour
{
    public Vector3 size;
    public Vector3 doorSize;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        size.y = 0;
        Gizmos.DrawWireCube(Vector3.zero, size);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(new Vector3 { x = -(size.x + doorSize.x) * 0.5f, y = doorSize.y * 0.5f }, doorSize);
        Gizmos.DrawWireCube(new Vector3 { x = (size.x + doorSize.x) * 0.5f, y = doorSize.y * 0.5f }, doorSize);
    }

    public void set(Vector3 size, Vector3 doorSize)
    {
        this.size = size;
        this.doorSize = doorSize;
    }
}
