using System;
using GameEntitySystem;
using Engine;

namespace Game
{
    public class SubsystemExperienceBlockBehavior : SubsystemBlockBehavior
    {
        public override void OnPickableGetNearToPlayer(Pickable pickable, ComponentBody target, Vector3 distanceToTarget)
        {
            float distance = distanceToTarget.Length();
            if (!pickable.ToRemove && distance < pickable.DistanceToPick)
            {
                ComponentLevel targetComponentLevel = target.Entity.FindComponent<ComponentLevel>();
                if (targetComponentLevel != null)
                {
                    targetComponentLevel.AddExperience(pickable.Count, playSound: true);
                    pickable.ToRemove = true;
                }
            }
            if (!pickable.StuckMatrix.HasValue)
            {
                pickable.FlyToPosition = target.Position + distanceToTarget + (0.1f * distance * target.Velocity);
                pickable.FlyToBody = target;
            }
        }
    }
}
