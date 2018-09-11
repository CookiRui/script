using RAL;
using RenderingProcess;
using System.Collections.Generic;
using UnityEngine;

class PlayBackLogicFrameProcessor
{
    PlayBackLogicFrameList _playBackList = null;

    RenderActionGenerator _renderActionGenerator = null;

    public PlayBackLogicFrameProcessor()
    {
    }

    public void setRenderActionGenerator(RenderActionGenerator generator)
    {
        _renderActionGenerator = generator;
    }


    void reset()
    {
        m_time = 0;
        m_maxFrameId = -1;
                
        if (m_currentFrame != null)
        {
            m_currentFrame.release(_renderActionGenerator);
            m_currentFrame = null;
        }
        m_currentPhysicsIndex = -1;
    }

    public void replay( PlayBackLogicFrameList replayList, float startTime )
    {
        reset();
        m_time = startTime;
        _playBackList = replayList;
    }

    public void update()
    {
        float time = m_time + Time.unscaledDeltaTime;

        int maxFrameId = (int)(time * FrameSync.LOGIC_FPS);
        if (maxFrameId == m_maxFrameId)
        {
            m_time = time;
            _processCurrentFrame(m_time - m_maxFrameId / (float)FrameSync.LOGIC_FPS);
        }
        else
        {
            RAL.LogicFrame msg;
            while (_playBackList.pop(maxFrameId, out msg))
            {
                _processNewFrame(msg, time - msg.frameId / (float)FrameSync.LOGIC_FPS);
            }

            m_maxFrameId = maxFrameId;
            m_time = time;
       
        }
    }

    private void _processCurrentFrame(float cursor)
    {
        if (m_currentFrame == null)
        {
            return;
        }
        _process(ref m_currentFrame, ref m_currentPhysicsIndex, cursor);
    }

    private void _processNewFrame(RAL.LogicFrame frame, float cursor)
    {
        if (m_currentFrame != null)
        {
            for (int i = m_currentPhysicsIndex; i < m_currentFrame.physicsFrames.Length; ++i)
            {
                for (int j = 0; j < m_currentFrame.physicsFrames[i].actions.Length; ++j)
                {
                    _doRenderActionDone(m_currentFrame.physicsFrames[i].actions[j]);
                }
            }
            _doLogicFrameEnd(m_currentFrame);
            m_currentFrame = null;
            m_currentPhysicsIndex = -1;
        }
        if (frame != null)
        {
            m_currentFrame = frame;
            m_currentPhysicsIndex = 0;
            _doLogicFrameBegin(frame );
            _process(ref m_currentFrame, ref m_currentPhysicsIndex, cursor);
        }
    }

    private void _process(ref RAL.LogicFrame frame, ref int physicsIndex, float cursor)
    {
        int nowIndex = (int)(cursor * FrameSync.LOGIC_PHYSICS_FPS);
        if (nowIndex >= frame.physicsFrames.Length)
        {
            for (int i = physicsIndex; i < m_currentFrame.physicsFrames.Length; ++i)
            {
                for (int j = 0; j < frame.physicsFrames[i].actions.Length; ++j)
                {
                    _doRenderActionDone(frame.physicsFrames[i].actions[j]);
                }
            }
            physicsIndex = -1;            
            _doLogicFrameEnd(frame);
            frame = null;
        }
        else
        {
            for (int i = physicsIndex; i < nowIndex; ++i)
            {
                for (int j = 0; j < frame.physicsFrames[i].actions.Length; ++j)
                {
                    _doRenderActionDone(frame.physicsFrames[i].actions[j]);
                }
            }
            physicsIndex = nowIndex;
            var progress = cursor * FrameSync.LOGIC_PHYSICS_FPS - nowIndex;
            for (int i = 0; i < frame.physicsFrames[nowIndex].actions.Length; ++i)
            {
                _doRenderActionProgress(frame.physicsFrames[nowIndex].actions[i], progress);
            }
        }
    }

    private void _doRenderActionProgress(RAL.RenderAction ra, float progress)
    {
        RenderActionProcessorFactory.instance.doProgress(ra, progress);
    }

    private void _doRenderActionDone(RAL.RenderAction ra)
    {
        RenderActionProcessorFactory.instance.doDone(ra);
    }

    private void _doLogicFrameBegin(RAL.LogicFrame frame) 
    {
    }

    private void _doLogicFrameEnd(RAL.LogicFrame frame) 
    {
        frame.release(_renderActionGenerator);
    }


    float m_time = 0;
    int m_maxFrameId = -1;
    
    RAL.LogicFrame m_currentFrame = null;
    int m_currentPhysicsIndex = -1;

};



class PlayBackLogicFrameList
{
    public PlayBackLogicFrameList()
    {

    }

    public void setRenderActionGenerator(RenderActionGenerator generator)
    {
        _renderActionGenerator = generator;
    }

    RenderActionGenerator _renderActionGenerator = null;

    Queue<RAL.LogicFrame> replayList = new Queue<RAL.LogicFrame>();

    public virtual void clear()
    {
        var e = replayList.GetEnumerator();
        while (e.MoveNext())
        {
            e.Current.release(_renderActionGenerator);
        }
        replayList.Clear();
    }

    public void push(RAL.LogicFrame frame)
    {
        replayList.Enqueue(frame);
    }

     public bool pop(int maxFrameId, out LogicFrame msg) 
     {
        msg = null;
        if (replayList.Count == 0) 
        {
            return false;
        }
        var _msg = replayList.Peek();
        if (_msg.frameId <= maxFrameId) {
            msg = replayList.Dequeue();
            return true;
        }

        return false;
    }

    //¼ÇÂ¼²Ã¼ô
    public int cast( uint logicFrameID )
    {
        RAL.LogicFrame current = replayList.Peek();
        while( current != null && current.frameId < logicFrameID )
        {
            LogicFrame removedFrame = replayList.Dequeue();
            removedFrame.release(_renderActionGenerator);

            current = replayList.Peek();
        }
        if( current != null )
            return current.frameId;

        return -1;
    }

}
