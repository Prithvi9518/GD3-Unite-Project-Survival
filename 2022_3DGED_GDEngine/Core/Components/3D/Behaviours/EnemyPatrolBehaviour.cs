using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GD.Engine
{
    public class EnemyPatrolBehaviour : Component
    {
        private List<Vector3> waypoints;
        private float enemyMovementSpeed;
        private int currentWaypointIndex = 0;
        private bool isMoving;

        public EnemyPatrolBehaviour(List<Vector3> waypoints, float enemyMovementSpeed) :
            this(waypoints, enemyMovementSpeed, true)
        {
        }

        public EnemyPatrolBehaviour(List<Vector3> waypoints, float enemyMovementSpeed, bool isMoving)
        {
            this.waypoints = waypoints;
            this.enemyMovementSpeed = enemyMovementSpeed;
            this.isMoving = isMoving;
        }

        private void MoveToNextWaypoint(GameTime gameTime)
        {
            float distance = Vector3.Distance(transform.translation, waypoints[currentWaypointIndex]);

            if (distance < 0.05f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            }
            else
            {
                Vector3 currentWaypoint = waypoints[currentWaypointIndex];
                Vector3 direction = currentWaypoint - transform.translation;
                direction.Normalize();

                // Reference for finding target rotation:
                // https://subscription.packtpub.com/book/game-development/9781849692403/4/ch04lvl1sec107/time-for-action-enemy-update-and-draw

                float targetRotation = Convert.ToSingle(Math.Atan2(transform.translation.Z - currentWaypoint.Z,
                    transform.translation.X - currentWaypoint.X));

                transform.SetRotation(transform.rotation.X, targetRotation, transform.rotation.Z);

                Vector3 translation = direction * enemyMovementSpeed * gameTime.ElapsedGameTime.Milliseconds;
                transform.Translate(translation);
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