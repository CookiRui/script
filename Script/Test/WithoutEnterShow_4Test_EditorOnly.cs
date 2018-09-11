using UnityEngine;
class WithoutEnterShow_4Test_EditorOnly : MonoBehaviour
{
    public string 加拿大打桩机 = "你有FreeStyle吗？";
    public static WithoutEnterShow_4Test_EditorOnly instance { get; private set; }

    private void Awake()
    {
#if UNITY_EDITOR
        instance = this;
#else
        DestroyImmediate(this);
#endif
    }
}
