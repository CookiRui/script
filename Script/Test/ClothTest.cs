using UnityEngine;
class ClothTest : MonoBehaviour
{
    public Cloth cloth;
    public Transform startPosition;
    public Vector3 force;
    public ForceMode forceMode;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GetComponent<Rigidbody>().Sleep();
            transform.position = startPosition.position;
            GetComponent<Rigidbody>().AddForce(force, forceMode);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            GetComponent<Rigidbody>().Sleep();
        }
    }
}
