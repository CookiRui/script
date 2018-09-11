using UnityEngine;
using UnityEngine.UI;
using LuaInterface;
using DG.Tweening;

public static class UIHelper
{
    public static void removeAllButtonClick(this Button button)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    public static void addButtonClick(this Button button, LuaFunction fun)
    {
        if (button != null && fun != null)
        {
            button.onClick.AddListener(fun.Call);
        }
    }

    public static void onStateChanged(this Toggle toggle, LuaFunction fun)
    {
        if (toggle != null && fun != null)
        {
            toggle.onValueChanged.AddListener((value) =>
            {
                fun.Call(value);
            });
        }
    }

    public static void removeAllInputValueChanged(this InputField input)
    {
        if (input != null) {
            input.onValueChanged.RemoveAllListeners();
        }
    }

    public static void onInputValueChanged(this InputField input, LuaFunction fun)
    {
        if (input != null && fun != null)
        {
            input.onValueChanged.AddListener((value) =>
            {
                fun.Call(value);
            });
        }
    }

    public static int[] getInputSelectedIndex(this InputField input)
    {
        if (input != null)
        {
            return InputFieldCaretPosition.getSelectedStringInfoFromReflection(input);
        }
        return null;
    }

    public static void setInputSelectedIndex(this InputField input,int args1,int args2)
    {
        if (input != null)
        {
            InputFieldCaretPosition.setSelectedStringInfoFromReflection(input, new int[] { args1, args2 });
        }
    }

    public static void removeAllInlineTextHrefClick(this InlineNormalText text)
    {
        if (text != null)
        {
            text.onHrefClick.RemoveAllListeners();
        }
    }

    public static void onInlineTextHrefClick(this InlineNormalText text, LuaFunction fun)
    {
        if (text != null && fun != null)
        {
            text.onHrefClick.AddListener((name) =>
            {
                fun.Call(name);
            });
        }
    }

    public static void switchToggleOn(this ToggleGroup toggleGroup, Toggle toggle)
    {
        if (toggle)
        {
            bool mAllowSwitchOff = toggleGroup.allowSwitchOff;
            toggleGroup.allowSwitchOff = true;
            toggle.isOn = true;
            toggleGroup.NotifyToggleOn(toggle);
            toggleGroup.allowSwitchOff = mAllowSwitchOff;
        }
    }

    public static void setWorld2ScreenPos(this Transform trans, Transform refer, float xOffset, float yOffset, float zOffset)
    {
        if (!trans)
        {
            Debuger.LogError("trans is null");
            return;
        }
        if (!refer)
        {
            Debuger.LogError("refer is null");
            return;
        }
        trans.position = Camera.main.WorldToScreenPoint(refer.position + new Vector3(xOffset, yOffset, zOffset));
    }

    public static Vector2 getRectSize(this RectTransform rectTrans)
    {
        if (rectTrans != null)
        {
            return new Vector2(rectTrans.rect.width, rectTrans.rect.height);
        }
        return Vector2.zero;
    }

    public static void fillEffectGraphicMesh(this RectTransform rectTrans, Vector3[] vertexPosition, Vector2[] vertexUV, Color[] vertexColors, int[] vertexIndices)
    {
        if (rectTrans != null) {
            if (vertexColors != null)
            {
                Color32[] colors = new Color32[vertexColors.Length];
                for (int index = 0, length = colors.Length; index < length; index++)
                {
                    colors[index] = vertexColors[index];
                }
                EffectGraphic.get(rectTrans).fillVertextData(vertexPosition, vertexUV, colors, vertexIndices);
            }
            else EffectGraphic.get(rectTrans).fillVertextData(vertexPosition, vertexUV, null, vertexIndices);
        }         
    }


    public static Component getComponentInChildren(this Transform trans, string name)
    {
        System.Type type = System.Type.GetType(name);
        if (type == null) return null;
        return trans.GetComponentInChildren(type);
    }

    public static void addClickEvent(this Transform trans, LuaFunction func)
    {
        if (func != null)
        {
            EventTriggerListener.get(trans).onClick += delegate
            {
                func.Call();
                //func.Call(trans);
            };
        }
        else {
            EventTriggerListener.get(trans).onClick = null;
        }
        
    }

    public static void addBeginDragEvent(this Transform trans, LuaFunction func)
    {
        if (func != null)
        {
            EventTriggerListener.get(trans).onBeginDrag = delegate
            {
                func.Call();
                //func.Call(trans);
            };
        }
        else {
            EventTriggerListener.get(trans).onBeginDrag = null;
        }

    }

    public static void addEndDragEvent(this Transform trans, LuaFunction func)
    {
        if (func != null)
        {
            EventTriggerListener.get(trans).onEndDrag = delegate
            {
                func.Call();
                //func.Call(trans);
            };
        }
        else {
            EventTriggerListener.get(trans).onEndDrag = null;
        }
    }

    public static void addDragEvent(this Transform trans, LuaFunction func)
    {
        if (func != null)
        {
            EventTriggerListener.get(trans).onDrag = delegate
            {
                func.Call();
                //func.Call(trans);
            };
        }
        else {
            EventTriggerListener.get(trans).onDrag = null;
        }
    }

    public static void addDeltaDragEvent(this Transform trans, LuaFunction func)
    {
        if (func != null)
        {
            EventTriggerListener.get(trans).onDeltaDrag = delegate(float x,float y)
            {
                func.Call(x,y);
                //func.Call(trans);
            };
        }
        else
        {
            EventTriggerListener.get(trans).onDrag = null;
        }
    }

    public static void addDownEvent(this Transform trans, LuaFunction func)
    {
        if (func != null)
        {
            EventTriggerListener.get(trans).onDown = delegate
            {
                func.Call();
                //func.Call(trans);
            };
        }
        else
        {
            EventTriggerListener.get(trans).onDown = null;
        }
    }


    public static void addUpEvent(this Transform trans, LuaFunction func)
    {
        if (func != null)
        {
            EventTriggerListener.get(trans).onUp = delegate
            {
                func.Call();
                //func.Call(trans);
            };
        }
        else
        {
            EventTriggerListener.get(trans).onUp = null;
        }
    }

    public static void addExitEvent(this Transform trans, LuaFunction func)
    {
        if (func != null)
        {
            EventTriggerListener.get(trans).onExit = delegate
            {
                func.Call();
                //func.Call(trans);
            };
        }
        else
        {
            EventTriggerListener.get(trans).onExit = null;
        }
    }

    public static void doColor(this Text text, Color color, float duration, LuaFunction func)
    {
        if (text != null)
        {
            text.DOColor(color,duration).OnComplete(delegate(){
                if (func != null) func.Call();
            });
        }
    }

    public static Material setImageMaterial(this Image image, string shaderName)
    {
        if (image != null)
        {
            if (shaderName == null)
            {
                image.material = null;
            }
            else{
                Shader shader = Shader.Find(shaderName);
                Material mat = new Material(shader);
                image.material = mat;
                return mat;
            }
        }
        return null;
    }
}
