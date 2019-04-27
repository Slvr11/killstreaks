using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace killstreaks
{
    class harrier : BaseScript
    {
        private static Entity beginHarrier(Entity player, Vector3 startPoint, Vector3 pos)
        {
            Entity heightEnt = GetEnt("airstrikeheight", "targetname");
            float trueHeight;

            if (heightEnt != null)
                trueHeight = heightEnt.Origin.Z;
            else
                trueHeight = 850;

            Vector3 pathGoal = new Vector3(pos.X, pos.Y, 0) + new Vector3(0, 0, trueHeight);

            Entity harrier = spawnDefensiveHarrier(player, startPoint, pathGoal);
            harrier.SetField("pathGoal", pathGoal);

            return harrier;
        }
        /*
        private static Vector3 getCorrectHeight(float x, float y, float rand)
        {
            float offGroundHeight = 1200;
            float groundHeight = self traceGroundPoint(x, y);
            float trueHeight = groundHeight + offGroundHeight;

            if (isDefined(level.airstrikeHeightScale) && trueHeight < (850 * level.airstrikeHeightScale))
                trueHeight = (950 * level.airstrikeHeightScale);

            trueHeight += RandomInt(rand);

            return trueHeight;
        }
        */
        private static Entity spawnDefensiveHarrier(Entity owner, Vector3 pathStart, Vector3 pathGoal)
        {
            Vector3 forward = VectorToAngles(pathGoal - pathStart);

            Entity harrier;
            if (owner.SessionTeam == "allies")
                harrier = SpawnHelicopter(owner, pathStart, forward, "harrier_mp", "vehicle_av8b_harrier_jet_mp");
            else
                harrier = SpawnHelicopter(owner, pathStart, forward, "harrier_mp", "vehicle_av8b_harrier_jet _opfor_mp");

            if (harrier == null)
                return null;

            addToHeliList(harrier);
            StartAsync(removeFromHeliListOnDeath(harrier));

            harrier.SetField("speed", 250);//Field 6
            harrier.SetField("accel", 175);
            harrier.Health = 3000;
            harrier.SetField("maxHealth", harrier.Health);
            harrier.SetField("team", owner.SessionTeam);
            harrier.SetField("owner", owner);
            harrier.SetCanDamage(true);
            StartAsync(harrierDestroyed(harrier));
            harrier.SetMaxPitchRoll(0, 90);
            harrier.SetSpeed(harrier.GetField<int>("speed"), harrier.GetField<int>("accel"));
            StartAsync(playHarrierFx(harrier));
            harrier.SetDamageState(3);
            harrier.SetField("missiles", 6);
            harrier.SetHoverParams(50, 100, 50);
            harrier.SetTurningAbility(0.05f);
            harrier.SetYawSpeed(45, 25, 25, .5f);
            harrier.SetField("defendLoc", pathGoal);

            killstreaks.harriers.Add(harrier);

            return harrier;
        }

        private static IEnumerator defendLocation(Entity harrier)
        {
            StartAsync(harrierTimer(harrier));

            harrier.SetVehGoalPos(harrier.GetField<Vector3>("pathGoal"), true);
            StartAsync(closeToGoalCheck(harrier, harrier.GetField<Vector3>("pathGoal")));

            yield return harrier.WaitTill("goal");
            stopHarrierWingFx(harrier);
            engageGround(harrier);
        }

        private static IEnumerator closeToGoalCheck(Entity harrier, Vector3 pathGoal)
        {
            for (;;)
            {
                if (Distance2D(harrier.Origin, pathGoal) < 768)
                {
                    harrier.SetMaxPitchRoll(42, 25);
                    break;
                }

                yield return WaitForFrame();
            }
        }

        private static void engageGround(Entity harrier)
        {
            harrier.Notify("engageGround");

            StartAsync(harrierGetTargets(harrier));
            StartAsync(randomHarrierMovement(harrier));

            Vector3 pathGoal = harrier.GetField<Vector3>("defendLoc");

            harrier.SetSpeed(15, 5);
            harrier.SetVehGoalPos(pathGoal, true);
        }

        private static IEnumerator harrierLeave(Entity harrier)
        {
            harrier.SetMaxPitchRoll(0, 0);
            harrier.Notify("leaving");
            breakTarget(harrier, true);
            harrier.Notify("stopRand");

            for (;;)
            {
                harrier.SetSpeed(35, 25);
                Vector3 pathGoal = harrier.Origin + ((AnglesToForward(new Vector3(0, RandomInt(360), 0)) * 500));
                pathGoal += new Vector3(0, 0, 900);

                bool leaveTrace = BulletTracePassed(harrier.Origin, harrier.Origin + new Vector3(0, 0, 900), false, harrier);
                if (leaveTrace)
                    break;

                yield return Wait(0.10f);
            }

            harrier.SetVehGoalPos(pathGoal, true);
            StartAsync(startHarrierWingFx(harrier));
            yield return harrier.WaitTill("goal");
            harrier.PlaySound("harrier_fly_away");
            Vector3 pathEnd = getPathEnd(harrier);
            harrier.SetSpeed(250, 75);
            yield return harrier.WaitTill("goal");

            //killstreaks.airPlane.Remove(harrier);

            harrier.Notify("harrier_gone");
            harrierDelete(harrier);
        }

        private static void harrierDelete(Entity harrier)
        {
            harrier.Delete();
        }

        private static IEnumerator harrierTimer(Entity harrier)
        {
            yield return harrier.WaitTill_notify_or_timeout("death", 45);
            StartAsync(harrierLeave(harrier));
        }

        private static IEnumerator randomHarrierMovement(Entity harrier)
        {
            harrier.Notify("randomHarrierMovement");

            Vector3 pos = harrier.GetField<Vector3>("defendLoc");

            for (;;)
            {
                Vector3 newPos = getNewPoint(harrier, harrier.Origin);//Supposed to be a loop
                harrier.SetVehGoalPos(newPos, true);
                yield return harrier.WaitTill("goal");
                yield return Wait(RandomIntRange(3, 6));
                harrier.Notify("randMove");
            }
        }

        private static Vector3 getNewPoint(Entity harrier, Vector3 pos, Entity targ = null)
        {
            float pointX;
            float pointY;
            float newHeight;

            if (targ == null)
            {
                List<int> enemyPointsX = new List<int>();
                List<int> enemyPointsY = new List<int>();
                List<int> enemyPointsZ = new List<int>();

                foreach (Entity player in Players)
                {
                    if (player.Classname != "player")
                        continue;

                    if (!killstreaks.isTeamBased || player.SessionTeam != harrier.GetField<string>("team"))
                    {
                        enemyPointsX.Add((int)player.Origin.X);
                        enemyPointsY.Add((int)player.Origin.Y);
                        enemyPointsZ.Add((int)player.Origin.Z);
                    }
                }

                if (enemyPoints.Count > 0)
                {
                    Vector3 gotoPoint = new Vector3((float)enemyPointsX.Average(), (float)enemyPointsY.Average(), (float)enemyPointsZ.Average());

                    pointX = gotoPoint.X;
                    pointY = gotoPoint.Y;
                }
                else
                {
                    Vector3 center = killstreaks.mapCenter;
                    float movementDist = (killstreaks.mapSize / 6) - 200;

                    pointX = RandomFloatRange(center.X - movementDist, center.X + movementDist);
                    pointY = RandomFloatRange(center.Y - movementDist, center.Y + movementDist);
                }

                newHeight = getCorrectionHeight(harrier, pointX, pointY, 20);
            }
            else
            {
                if (killstreaks.coinToss())
                {

                }
            }
        }
    }
}
