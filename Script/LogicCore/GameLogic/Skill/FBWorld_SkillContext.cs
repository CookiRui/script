using FixMath.NET;
using BW31.SP2D;
using System;
using System.Collections.Generic;
using Skill;
using ML.SkillEdit.Runtime;

public partial class FBWorld
{

    LinkedList<IContext> skillContexts = new LinkedList<IContext>();
    List<IContext> updatingSkillList = new List<IContext>();

    Dictionary<int, NodeMapInfo> m_skills = new Dictionary<int, NodeMapInfo>();
    Dictionary<string, CustomNodePrefab> m_skillNodePrefabs = new Dictionary<string, CustomNodePrefab>();

    public bool doSkill(FBActor actor, int id)
    {
        if (actor != null) {
            
        }
        NodeMapInfo map;
        if (!m_skills.TryGetValue(id, out map)) {
            string[] skills = new string[] { "test" };
            //临时
            UnityEngine.TextAsset ta = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Skill/" + skills[id]);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(ta.bytes);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            map = (NodeMapInfo)formatter.Deserialize(ms);

            m_skills.Add(id, map);
        }

        SkillContext context = createNewSkillContext(actor, map);
        context.startup();
        if (context.activedObjectCount != 0) {
            skillContexts.AddLast(context.updater);
        }

        return true;
    }

    public CustomNodePrefab loadSkillNodePrefab(string path) {

        string name = path;
        int idx = name.LastIndexOf("Skill/common");
        if (idx != -1) {
            name = name.Substring(idx);
        }

        idx = name.LastIndexOf(".bytes");
        if (idx != -1) {
            name = name.Substring(0, idx);
        }

        CustomNodePrefab prefab;
        if (!m_skillNodePrefabs.TryGetValue(name, out prefab)) {
            UnityEngine.TextAsset ta = UnityEngine.Resources.Load<UnityEngine.TextAsset>(name);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(ta.bytes);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            prefab = formatter.Deserialize(ms) as CustomNodePrefab;
            m_skillNodePrefabs.Add(name, prefab);
        }
        return prefab;
    }

    SkillContext createNewSkillContext(FBActor actor, NodeMapInfo nodeMapInfo)
    {
        var context = new SkillContext(this, nodeMapInfo);
        context.globalData.add(actor != null ? actor.skillActor : null);       
        return context;
    }

    void updateSkillContext( Fix64 timeDelta )
    {
        var dt = new TFloat(timeDelta);
        updatingSkillList.AddRange(skillContexts);

        for (int i = 0; i < updatingSkillList.Count; ++i) {
            var ctx = updatingSkillList[i];
            ctx.update(dt);
            if (ctx.activedObjectCount == 0) {
                skillContexts.Remove(ctx.updater);
            }
        }

        updatingSkillList.Clear();
    }
}