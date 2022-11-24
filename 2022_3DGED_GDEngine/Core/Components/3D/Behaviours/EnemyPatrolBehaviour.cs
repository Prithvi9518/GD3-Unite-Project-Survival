using GD.Engine.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace GD.Engine
{
    /// <summary>
    /// Component that moves enemies throughout the scene using a List of waypoints.
    /// </summary>
    public class EnemyPatrolBehaviour : Component
    {
        private List<Vector3> waypoints;
        private float enemyMovementSpeed;
        private int currentWaypointIndex = 0;
        private bool isMoving;

        private float margin = 0.05f;

        public EnemyPatrolBehaviour(List<Vector3> waypoints, float enemyMovementSpeed) :
            this(waypoints, enemyMovementSpeed, true)
        {
        }

        public EnemyPatrolBehaviour(List<Vector3> waypoints, float enemyMovementSpeed, bool isMoving)
        {
            this.waypoints = waypoints;
            this.enemyMovementSpeed = enemyMovementSpeed;
            this.isMoving = isMoving;

            EventDispatcher.Subscribe(EventCategoryType.NonPlayer, TriggerEnemyMovement);
        }

        /// <summary>
        /// Updates the object's translation towards the next waypoint by calculating the direction vector.
        /// Once the object is within a certain margin of the waypoint, it updates the next waypoint to travel to.
        /// </summary>
        /// <param name="gameTime"></param>
        private void MoveToNextWaypoint(GameTime gameTime)
        {
            float distance = Vector3.Distance(transform.Translation, waypoints[currentWaypointIndex]);

            if (distance < margin)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            }
            else
            {
                Vector3 currentWaypoint = waypoints[currentWaypointIndex];

                Vector3 direction = currentWaypoint - transform.Translation;
                direction.Normalize();

                float targetRotation = GetTargetRotation(currentWaypoint);

                transform.SetRotation(transform.Rotation.X, MathHelper.ToDegrees(targetRotation), transform.Rotation.Z);

                Vector3 translation = direction * enemyMovementSpeed * gameTime.ElapsedGameTime.Milliseconds;
                transform.Translate(translation);
            }
        }

        /// <summary>
        /// Gets the target rotation (rotates the enemy towards the direction of the next waypoint)
        /// </summary>
        /// <param name="currentWaypoint"></param>
        /// <returns></returns>
        private float GetTargetRotation(Vector3 currentWaypoint)
        {
            // Reference for finding target rotation:
            // https://subscription.packtpub.com/book/game-development/9781849692403/4/ch04lvl1sec107/time-for-action-enemy-update-and-draw

            float targetRotation = Convert.ToSingle(
                Math.Atan2(
                    -(transform.Translation.Z - currentWaypoint.Z),
                    transform.Translation.X - currentWaypoint.X
                )
            );

            return targetRotation;
        }

        private void TriggerEnemyMovement(EventData eventData)
        {
            if (eventData.EventActionType == EventActionType.OnEnemyAlert)
            {
                string param = eventData.Parameters[0] as string;
                if (param == "true")
                    isMoving = true;
                else if (param == "false")
                    isMoving = false;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (isMoving)
                MoveToNextWaypoint(gameTime);
            //base.Update(gameTime);
        }
    }
}