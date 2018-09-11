using System.Collections.Generic;
using BW31.SP2D;
using FixMath.NET;
using System.Linq;
using Cratos;

public partial class FBGame
{
    private RAL.LogicFrameQueue m_logicFrameQueue = null;
    private List<RAL.RenderAction> m_logicBeforeRenderActions = new List<RAL.RenderAction>();
    private List<RAL.RenderAction> m_logicAfterRenderActions = new List<RAL.RenderAction>();
    private List<RAL.RenderAction>[] m_worldRenderActions = null;

    public enum RenderActionListType
    {
        kLogicBefore,
        kWorld_0,
        kWorld_1,
        kWorld_2,
        kWorld_3,
        kLogicAfter
    }

    public void setCurrentRenderActionList(RenderActionListType type)
    {
        switch (type)
        {
            case RenderActionListType.kLogicBefore:
                setCurrentRenderActionList(m_logicBeforeRenderActions);
                break;
            case RenderActionListType.kWorld_0:
                setCurrentRenderActionList(m_worldRenderActions[0]);
                break;
            case RenderActionListType.kWorld_1:
                setCurrentRenderActionList(m_worldRenderActions[1]);
                break;
            case RenderActionListType.kWorld_2:
                setCurrentRenderActionList(m_worldRenderActions[2]);
                break;
            case RenderActionListType.kWorld_3:
                setCurrentRenderActionList(m_worldRenderActions[3]);
                break;
            case RenderActionListType.kLogicAfter:
                setCurrentRenderActionList(m_logicAfterRenderActions);
                break;
        }
    }

    public void setCurrentRenderActionList(List<RAL.RenderAction> list)
    {
        m_currentRenderActionList = list;
    }

    private List<RAL.RenderAction> m_currentRenderActionList = null;

    public RAL.LogicFrameQueue logicFrameQueue { get { return m_logicFrameQueue; } }


    public void generateRenderAction<T>(params object[] parameters) where T : RAL.RenderAction
    {
        m_currentRenderActionList.Add(m_logicFrameQueue.renderActionGenerator.createRenderAction(typeof(T), parameters));
    }

    public void generateRenderActionToTargetList<T>(RenderActionListType target, params object[] parameters) where T : RAL.RenderAction
    {
        var old = m_currentRenderActionList;
        setCurrentRenderActionList(target);
        m_currentRenderActionList.Add(m_logicFrameQueue.renderActionGenerator.createRenderAction(typeof(T), parameters));
        m_currentRenderActionList = old;
    }

}