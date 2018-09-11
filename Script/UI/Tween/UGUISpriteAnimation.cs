/*
    author:jlx
*/

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UGUISpriteAnimation : MonoBehaviour
{
    public int fps = 5;
    public List<Sprite> sprites;
    public bool foward = true;
    public bool autoPlay = true;
    public bool loop ;

    bool isPlaying ;
    Image image;
    int curFrame = 0;
    float delta = 0;

    int frameCount
    {
        get
        {
            return sprites.Count;
        }
    }

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void Start()
    {
        if (autoPlay)
        {
            play();
        }
        else
        {
            isPlaying = false;
        }
    }

    private void setSprite(int idx)
    {
        image.sprite = sprites[idx];
        image.SetNativeSize();
    }

    public void play()
    {
        isPlaying = true;
        foward = true;
    }

    public void playReverse()
    {
        isPlaying = true;
        foward = false;
    }

    void Update()
    {
        if (!isPlaying || 0 == frameCount)
        {
            return;
        }

        delta += Time.deltaTime;
        if (delta > 1.0 / fps)
        {
            delta = 0;
            if (foward)
            {
                curFrame++;
            }
            else
            {
                curFrame--;
            }

            if (curFrame >= frameCount)
            {
                if (loop)
                {
                    curFrame = 0;
                }
                else
                {
                    isPlaying = false;
                    return;
                }
            }
            else if (curFrame < 0)
            {
                if (loop)
                {
                    curFrame = frameCount - 1;
                }
                else
                {
                    isPlaying = false;
                    return;
                }
            }

            setSprite(curFrame);
        }
    }

    public void pause()
    {
        isPlaying = false;
    }

    public void resume()
    {
        if (!isPlaying)
        {
            isPlaying = true;
        }
    }

    public void stop()
    {
        curFrame = 0;
        setSprite(curFrame);
        isPlaying = false;
    }

    public void rewind()
    {
        curFrame = 0;
        setSprite(curFrame);
        play();
    }
}