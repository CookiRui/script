using UnityEngine;

/*负责lua和c#互相通讯*/
namespace LuaInterface
{
    public partial class LuaBridge
    {
        public Sprite loadSkillIcon(string iconName)
        {
            if(iconName.isNullOrEmpty())
            {
                Debugger.LogError("load skill icon name is null");
                return null;
            }

            Sprite sprite = UIResourceLoader.inst.getSkillIcon(iconName);
            return sprite;
        }

        public Sprite loadItemIcon(string iconName)
        {
            if(iconName.isNullOrEmpty())
            {
                Debugger.LogError("load item icon name is null");
                return null;
            }

            Sprite sprite = UIResourceLoader.inst.getItemIcon(iconName);
            return sprite;
        }

        public Sprite loadEmotionIcon(string iconName)
        {
            if(iconName.isNullOrEmpty())
            {
                Debuger.LogError("load emotion icon name is null");
                return null;
            }

            Sprite sprite = UIResourceLoader.inst.getEmotionIcon(iconName);
            return sprite;
        }

        public Sprite loadHeadIcon(string iconName)
        {
            if (iconName.isNullOrEmpty())
            {
                Debuger.LogError("load head icon name is null");
                return null;
            }

            Sprite sprite = UIResourceLoader.inst.getHeadIcon(iconName);
            return sprite;
        }

        public GameObject loadModelAvatar(string avatorName,string meshIndex,string meshname)
        {
            if (avatorName.isNullOrEmpty())
            {
                Debuger.LogError("load model avatar name is null");
                return null;
            }
            System.Collections.Generic.Dictionary<string,string> avatar = new System.Collections.Generic.Dictionary<string,string>();
            avatar[meshIndex] = meshname;
            return ModelResourceLoader.inst.createAvatar(avatorName, avatar, null);
        }
    }
}

