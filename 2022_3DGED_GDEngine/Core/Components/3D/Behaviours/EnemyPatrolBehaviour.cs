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

        public EnemyPatrolBehaviour(List<Vector3> waypoints, float enemyMovementSpeed)
        {
            this.waypoints = waypoints;

            this.enemyMovementSpeed = enemyMovementSpeed;
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
                Vector3 direction = waypoints[currentWaypointIndex] - transform.translation;
                direction.Normalize();

                Vector3 translation = direction * enemyMovementSpeed * gameTime.ElapsedGameTime.Milliseconds;
                transform.Translate(translation);
            }
        }

        public override void Update(GameTime gameTime)
        {
            MoveToNextWaypoint(gameTime);
            //base.Update(gameTime);
        }
    }
}