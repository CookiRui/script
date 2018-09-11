using UnityEngine;
using System.Collections.Generic;
using Cratos;

class InputEventTranslator : Singleton<InputEventTranslator>
{
    struct InputInfo
    {
        public int angle;
        public int[] btns;

        public InputInfo(int btnCount)
        {
            angle = short.MinValue;
            btns = new int[btnCount];
        }

        public bool equals(InputInfo info)
        {
            if (angle != info.angle) return false;
            if (btns.Length != info.btns.Length) return false;
            for (int i = 0; i < btns.Length; i++)
            {
                if (btns[i] != info.btns[i]) return false;
            }
            return true;
        }

        public void clear()
        {
            angle = short.MinValue;
            for (int i = 0; i < btns.Length; i++)
            {
                btns[i] = 0;
            }
        }
    }

    Queue<InputInfo> inputs = new Queue<InputInfo>();
    Queue<InputInfo> cacheInputs = new Queue<InputInfo>();
    InputInfo? lastInput;

    public List<ClientFrameMsg> translateInputToEvent()
    {
        if (inputs.isNullOrEmpty()) return null;

        var msgs = new List<ClientFrameMsg>();
        while (inputs.Count > 0)
        {
            var input = inputs.Dequeue();
            var msg = new ClientFrameMsg();
            msg.angle = (short)input.angle;
            msg.keys = new byte[] { (byte)input.btns[0], (byte)input.btns[1] };
            msgs.Add(msg);
            cacheInfo(input);
        }
        return msgs;
    }

    /// <summary>
    /// ��¼����
    /// </summary>
    public void record()
    {
#if UNITY_EDITOR
        var input = fromKeybord();
#else
        var input = fromEasytouch();
#endif
        if (lastInput.HasValue && lastInput.Value.equals(input))
        {
            cacheInfo(input);
            return;
        }
        inputs.Enqueue(input);
        lastInput = input;
    }

    public void clearLastInput()
    {
        lastInput = null;
    }

    public void clear()
    {
        lastInput = null;
        inputs.forEach(a => 
        {
            cacheInfo(a);
        });
        inputs.Clear();
    }

    /// <summary>
    /// ʹ�ü���
    /// </summary>
    /// <returns></returns>
    InputInfo fromKeybord()
    {
        var input = newInfo();
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");

        //jlx 2017.03.23-log:�޸����ɿ�ҡ�˺󻹻���Чһ��ʱ��
        //if (!(Input.GetKey(KeyCode.A)
        //    || Input.GetKey(KeyCode.D)
        //    || Input.GetKey(KeyCode.LeftArrow)
        //    || Input.GetKey(KeyCode.RightArrow)))
        //{
        //    x = 0;
        //}
        //if (!(Input.GetKey(KeyCode.W)
        //   || Input.GetKey(KeyCode.S)
        //   || Input.GetKey(KeyCode.UpArrow)
        //   || Input.GetKey(KeyCode.DownArrow)))
        //{
        //    y = 0;
        //}
        if (!(x == 0 && y == 0))
        {
            input.angle = (int)(Mathf.Atan2(y, x) * Mathf.Rad2Deg);
        }

        if (Input.GetButtonDown("Action2")
            || Input.GetButton("Action2"))
        {
            input.btns[0] = 1;
        }
        else if (Input.GetButtonUp("Action2"))
        {
            input.btns[0] = 2;
        }

        if (Input.GetButtonDown("Action1")
            || Input.GetButton("Action1"))
        {
            input.btns[1] = 1;
        }
        else if (Input.GetButtonUp("Action1"))
        {
            input.btns[1] = 2;
        }
        return input;
    }

    /// <summary>
    /// Easytouch
    /// </summary>
    /// <returns></returns>
    InputInfo fromEasytouch()
    {
        var input = newInfo();
#if !SCENEEDITOR_TOOL
        var x = ETCInput.GetAxis("Horizontal");
        var y = ETCInput.GetAxis("Vertical");
        if (!(x == 0 && y == 0))
        {
            input.angle = (int)(Mathf.Atan2(y, x) * Mathf.Rad2Deg);
        }

        if (ETCInput.GetButton("Button1"))
        {
            input.btns[0] = 1;
        }
        else if (ETCInput.GetButtonUp("Button1"))
        {
            input.btns[0] = 2;
        }

        if (ETCInput.GetButton("Button2"))
        {
            input.btns[1] = 1;
        }
        else if (ETCInput.GetButtonUp("Button2"))
        {
            input.btns[1] = 2;
        }
#endif
        return input;
    }

    InputInfo newInfo()
    {
        return cacheInputs.Count > 0 ? cacheInputs.Dequeue() : new InputInfo(2);
    }

    void cacheInfo(InputInfo inputInfo)
    {
        inputInfo.clear();
        cacheInputs.Enqueue(inputInfo);
    }

};
