using UnityEngine;
class PrintKeyTime : MonoBehaviour
{
    float timer;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            timer = 0;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            timer += Time.deltaTime;
        }
        else if (Input.GetKeyUp(KeyCode.K))
        {
            Debug.Log("按住K键：" + timer + "  秒" );
            timer = 0;
        }
    }
}
