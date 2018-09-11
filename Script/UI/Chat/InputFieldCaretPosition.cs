using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/InputFieldCaretPosition",100)]
public class InputFieldCaretPosition : MonoBehaviour
{
    public float caretPosition = -8f;

    private float mCaretPosition = 0f;
    private float originCaretPosition = 0f;
    private InputField input;
    private Text inputText;
    private RectTransform caret;
    private bool restoreCaretPos = false;

    private MethodInfo selectedStringMthod;
    private static PropertyInfo caretPositionInfo;
    private static PropertyInfo caretSelectPositionInfo;
  

    void Awake()
    {
        input = GetComponent<InputField>();

        if (input) inputText = input.textComponent;
    }

    void LateUpdate() {
        if (input == null)
        {
            return;
        }

        if (inputText && inputText.text.TrimEnd().Length > 0)
        {
            //默认输入时，可以不修改
            if (!restoreCaretPos && caret && caret.anchoredPosition.y != originCaretPosition)
            {
                restoreCaretPos = true;
                mCaretPosition = originCaretPosition;
                caret.anchoredPosition = new Vector2(caret.anchoredPosition.x, originCaretPosition);
            }
            InlineNormalText inlineText = inputText as InlineNormalText;
            if (inlineText != null)
            {
                if (inlineText.richTextParams[1] > 0 || inlineText.richTextParams[2] > 0)
                {
                    inlineText.alignByGeometry = true;
                }
                else
                {
                    inlineText.alignByGeometry = false;
                }
            }
            return;
        }

        if (caret == null)
        {
            restoreCaretPos = false;
            var temp = input.transform.Find(input.gameObject.name + " Input Caret");
            if (temp)
            {
                caret = temp as RectTransform;
                originCaretPosition = mCaretPosition = caret.anchoredPosition.y;
            }
        }
        if (caret && caretPosition != mCaretPosition)
        {
            inputText.alignByGeometry = true;
            restoreCaretPos = false;
            mCaretPosition = caretPosition;
            caret.anchoredPosition = new Vector2(caret.anchoredPosition.x, caretPosition);
        }
    }

    public int[] getSelectedStringInfo() {
        return getSelectedStringInfoFromReflection(input);
    }

    public static void setSelectedStringInfoFromReflection(InputField inputField,int[] infos)
    {
        caretPositionInfo.SetValue(inputField, infos[0],null);
        caretSelectPositionInfo.SetValue(inputField, infos[1], null);
        inputField.ForceLabelUpdate();
    }

    public static int[] getSelectedStringInfoFromReflection(InputField inputField) 
    {
        if (inputField == null) return null;
        if (caretPositionInfo == null)
        {
            caretPositionInfo = typeof(InputField).GetProperty("caretPositionInternal", BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        if (caretSelectPositionInfo == null)
        {
            caretSelectPositionInfo = typeof(InputField).GetProperty("caretSelectPositionInternal", BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        int caretPositionInternal = (int)caretPositionInfo.GetValue(inputField,null);
        int caretSelectPositionInternal = (int)caretSelectPositionInfo.GetValue(inputField, null);

        if (caretPositionInternal == caretSelectPositionInternal) return null;
        return new int[] { caretPositionInternal, caretSelectPositionInternal};
    }

}
