/*
    author:jlx
*/

using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(Selectable))]
public class UISelectableGray : MonoBehaviour
{
    Selectable selectable;
    void Update()
    {
        if (!selectable)
        {
            selectable = GetComponent<Selectable>();
            if (selectable) return;
        }
        var colors = selectable.colors;
        colors.disabledColor = Color.white;
        selectable.colors = colors;

        if (selectable.interactable)
        {
            removeGrays();
        }
        else
        {
            var graphics = GetComponentsInChildren<Graphic>();
            graphics.forEach(a =>
            {
                if (!a.GetComponent<UIGray>())
                {
                    a.addComponent<UIGray>();
                }
            });
        }
    }

    void OnDestroy()
    {
        removeGrays();
    }

    void removeGrays()
    {
        var grays = GetComponentsInChildren<UIGray>();
        if (grays.isNullOrEmpty()) return;
        grays.forEach(a =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (a)
                {
                    DestroyImmediate(a);
                }
            };
#endif
        });
    }
}
