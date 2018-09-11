using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace FBCamera
{
    public class CameraPosition
    {
        public int id = 0;
        public Vector3 position = Vector3.zero;
        public int weight = 0;
        public List<CameraActionBase> actions = new List<CameraActionBase>();
        public List<CameraActionBase> shootActions = new List<CameraActionBase>();
        public List<CameraActionBase> goalActions = new List<CameraActionBase>();

        public IEnumerable<CameraActionBase> getActions()
        {
            return actions;
        }

        public IEnumerable<CameraActionBase> getShootActions(float randomValue)
        {
            return getRandomActions(shootActions, randomValue);
        }

        public IEnumerable<CameraActionBase> getGoalActions(float randomValue)
        {
            return getRandomActions(goalActions, randomValue);
        }

        IEnumerable<CameraActionBase> getRandomActions(IEnumerable<CameraActionBase> actions, float randomValue)
        {
            if (actions.isNullOrEmpty()) return null;
            if (randomValue <= 0) return null;

            var tmpActions = actions.Where(a => a.weight == null).ToList();
            var randomActions = actions.Where(a => a.weight.HasValue && a.weight.Value > 0);

            if (!randomActions.isNullOrEmpty())
            {
                var sum = randomActions.Sum(a => a.weight.Value);
                var min = 0f;
                foreach (var item in randomActions)
                {
                    var max = min + (float)item.weight.Value / sum;
                    if (min < randomValue && randomValue <= max)
                    {
                        tmpActions.Add(item);
                        break;
                    }
                    min = max;
                }
            }
            return tmpActions;
        }

        public Vector3 getPosition(bool inverse = false)
        {
            return inverse ? new Vector3 { x = -position.x, y = position.y, z = position.z } : position;
        }
    }
}