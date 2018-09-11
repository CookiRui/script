using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UIAtlas
{
    public string name { get; set; }

    public bool compressed { get; set; }

    public IEnumerable<Sprite> sprites { get; set; }

    public Material material { get; set; }

    public Sprite getSprite(string name)
    {
        if(name.isNullOrEmpty())
        {
            Debug.LogError("name is null");
            return null;
        }

        if(sprites.isNullOrEmpty())
        {
            return null;
        }

        return sprites.FirstOrDefault(a => a.name == name);
    }

    public static implicit operator bool (UIAtlas exists) { return !exists.isNull(); }
}
