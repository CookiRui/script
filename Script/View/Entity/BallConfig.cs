using System.Security;
using System.Collections.Generic;
using System;
using UnityEngine;

class BallConfig
{
    #region rotate animation
    public string[] commonAnimations;
    public Dictionary<uint, string[]> actorAnimations;

    #endregion

    #region const
    public float parabolaK;
    public float arclineK;
    public float chargeDelay;
    #endregion

    #region effect
    string[] energyEffects;

    public string passHitLandEffect;
    public string passHitWallEffect;
    public string passTrailEffect;

    public string normalTrailEffect;
    public string normalHitNetEffect;

    public string powerTrailEffect;

    string[] superTrailEffects;
    string[] superHitNetEffects;

    public string killerTrailEffect;

    public string chargeEffect;
    public float minLandHeightVelocity;

    #endregion

    public struct KillerSkillRotate
    {
        public Vector3 axis;
        public float angularVelocity;
    }

    public Dictionary<uint, KillerSkillRotate> killerSkillRotates;

    public BallConfig(SecurityElement se)
    {
        if (se == null)
        {
            Debuger.LogError("se is null");
            return;
        }
        var rotateSE = se.SearchForChildByTag("rotate");
        parabolaK = rotateSE.parseFloat("parabolak");
        arclineK = rotateSE.parseFloat("arclinek");

        var rotateAnimationSE = rotateSE.SearchForChildByTag("rotateanimation");
        var commonAnimationSE = rotateAnimationSE.SearchForChildByTag("common");
        var commonAnimationStr = commonAnimationSE.Attribute("name");
        commonAnimations = parseAnimations(commonAnimationStr);

        var actorsSE = rotateAnimationSE.SearchForChildByTag("actors");
        if (actorsSE.Children != null && actorsSE.Children.Count > 0)
        {
            actorAnimations = new Dictionary<uint, string[]>();
            foreach (SecurityElement childSE in actorsSE.Children)
            {
                var id = childSE.parseUint("id");
                var animationStr = childSE.Attribute("animation");
                var animations = parseAnimations(animationStr);
                if (animations != null)
                {
                    actorAnimations.Add(id, animations);
                }
            }
        }

        var killerShootSE = rotateSE.SearchForChildByTag("killershoot");
        if (killerShootSE.Children != null && killerShootSE.Children.Count > 0)
        {
            killerSkillRotates = new Dictionary<uint, KillerSkillRotate>();
            foreach (SecurityElement childSE in killerShootSE.Children)
            {
                killerSkillRotates.Add(childSE.parseUint("id"), new KillerSkillRotate
                {
                    axis = childSE.parseVector3(),
                    angularVelocity = childSE.parseFloat("angularvelocity"),
                });
            }
        }

        var constSE = se.SearchForChildByTag("const");
        chargeDelay = constSE.parseFloat("chargedelay");

        var effectsSE = se.SearchForChildByTag("effects");
        var energySE = effectsSE.SearchForChildByTag("energy");
        var energyEffectStr = energySE.Attribute("name");
        if (!string.IsNullOrEmpty(energyEffectStr))
        {
            energyEffects = energyEffectStr.Split(new char[] { ',' });
        }
        var trailSE = effectsSE.SearchForChildByTag("trail");
        passTrailEffect = trailSE.SearchForChildByTag("pass").Attribute("name");
        normalTrailEffect = trailSE.SearchForChildByTag("normal").Attribute("name");
        powerTrailEffect = trailSE.SearchForChildByTag("power").Attribute("name");
        var superTrialEffectStr = trailSE.SearchForChildByTag("super").Attribute("name");
        if (!string.IsNullOrEmpty(superTrialEffectStr))
        {
            superTrailEffects = superTrialEffectStr.Split(new char[] { ',' });
        }
        killerTrailEffect = trailSE.SearchForChildByTag("killer").Attribute("name");

        var hitSE = effectsSE.SearchForChildByTag("hit");
        var landSE = hitSE.SearchForChildByTag("land");
        passHitLandEffect = landSE.SearchForChildByTag("pass").Attribute("name");
        minLandHeightVelocity = landSE.parseFloat("minheightvelocity");

        var wallSE = hitSE.SearchForChildByTag("wall");
        passHitWallEffect = wallSE.SearchForChildByTag("pass").Attribute("name");

        var netSE = hitSE.SearchForChildByTag("net");
        normalHitNetEffect = netSE.SearchForChildByTag("normal").Attribute("name");
        var superHitNetEffectStr = netSE.SearchForChildByTag("super").Attribute("name");
        if (!string.IsNullOrEmpty(superHitNetEffectStr))
        {
            superHitNetEffects = superHitNetEffectStr.Split(new char[] { ',' });
        }
        var chargeSE = effectsSE.SearchForChildByTag("charge");
        chargeEffect = chargeSE.Attribute("name");
    }

    string[] parseAnimations(string str)
    {
        if (string.IsNullOrEmpty(str)) return null;
        var animations = str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < animations.Length; i++)
        {
            animations[i] = "Base Layer." + animations[i];
        }
        return animations;
    }

    public string getEnergyEffect(int idx)
    {
        if (idx < 0 || idx >= energyEffects.Length) return null;
        return energyEffects[idx];
    }

    public string getSuperTrailEffect(FiveElements element)
    {
        var idx = (int)element - 1;
        if (idx < 0 || idx >= superTrailEffects.Length) return null;
        return superTrailEffects[idx];
    }

    public string getHitNetEffect(FiveElements element)
    {
        var idx = (int)element - 1;
        if (idx < 0 || idx >= superHitNetEffects.Length) return null;
        return superHitNetEffects[idx];
    }

    public KillerSkillRotate getKillerSkillRotate(uint id)
    {
        KillerSkillRotate killerSkillRotate;
        if (killerSkillRotates.TryGetValue(id, out killerSkillRotate))
        {
            return killerSkillRotate;
        }
        return default(KillerSkillRotate);
    }
}
