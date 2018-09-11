using UnityEngine;

namespace ArtHelper
{

    public class InFrontOfCamera : MonoBehaviour {

        public float distance = 1;

        private void LateUpdate() {
            if (transform.parent != null && Camera.main!=null) {
                transform.localPosition = Quaternion.Inverse(transform.parent.rotation) * (Camera.main.transform.position - transform.parent.position).normalized * distance;
            }
        }
    }

}
