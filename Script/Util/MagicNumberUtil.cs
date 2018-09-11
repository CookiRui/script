using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicNumberUtil
{
    static uint generate(char c0, char c1, char c2, char c3)
    {
        return c0 | ((uint)c1) << 8 | ((uint)c2) << 16 | ((uint)c3) << 24;
    }

    public static uint generateSceneConfig()
    {
        return generate('X', 'M', 'P', 'S');
    } 

    public static uint generateUICommonConfig()
    {
        return generate('U', 'I', 'C', 'M');
    }
}
