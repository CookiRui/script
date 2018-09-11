using System;
using System.Collections.Generic;

class BinMsgTable
{
    private static Dictionary<string, int> binaryMsg = new Dictionary<string, int>()
	{
		{"FirtMessageRet",30},{"FirtMessageReq",29},
	};

    public static string GetBinMsgName(int msgNum)
    {
        foreach (var it in binaryMsg)
        {
            if (it.Value == msgNum)
                return it.Key;
        }
        return "";
    }
}