using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace killstreaks
{
    public class aastrike : BaseScript
    {
        public static Action<Entity> aaStrikeGiveFunc;
        public static Func<Entity, bool> aaStrikeUseFunc;
        public aastrike()
        {
            PreCacheItem("aamissile_projectile_mp");
            PreCacheItem("killstreak_double_uav_mp");
            PreCacheMiniMapIcon("hud_minimap_harrier_green");
            PreCacheMiniMapIcon("hud_minimap_harrier_red");
            PreCacheShader("hud_minimap_harrier_green");
            PreCacheShader("hud_minimap_harrier_red");

            killstreaks.level.SetField("teamAirDenied_axis", false);
            killstreaks.level.SetField("teamAirDenied_allies", false);

            aaStrikeGiveFunc = giveAAStrike;
            aaStrikeUseFunc = tryUseAAStrike;
        }

        private static void giveAAStrike(Entity player)
        {

        }

        private static bool tryUseAAStrike(Entity player)
        {
            if (killstreaks.level.GetField<bool>("civilianJetFlyBy"))
            {
                player.IPrintLnBold("Civilian air traffic in area.");
                return false;
            }

            if (player.HasField("usingRemote"))
                return false;

            if (killstreaks.isAirDenied(player))
                return false;

            //if (killstreaks.isEMPed(player))
            //return false;

            finishAAStrike(player);
            killstreaks.teamPlayerCardSplash("used_aastrike", player, true);

            return true;
        }

        private static void cycleTargets(Entity player)
        {
            if (player.HasField("usingRemote") && killstreaks.airDeniedPlayer != player)
                return;

            AfterDelay(50, () =>
            {
                findTargets(player);
                AfterDelay(RandomIntRange(4, 5) * 1000, () => cycleTargets(player));
            });
        }

        private static void findTargets(Entity player)
        {
            List<Entity> lbTargets = new List<Entity>();
            List<Entity> heliTargets = new List<Entity>();
            List<Entity> uavTargets = new List<Entity>();

            if (killstreaks.littleBirds.Count > 0)
            {
                foreach (Entity lb in killstreaks.littleBirds)
                {
                    if (lb.HasField("team") && lb.GetField<string>("team") != player.SessionTeam)
                        lbTargets.Add(lb);
                }
            }

            if (killstreaks.helis.Count > 0)
            {
                foreach (Entity heli in killstreaks.helis)
                {
                    if (heli.HasField("team") && heli.GetField<string>("team") != player.SessionTeam)
                        heliTargets.Add(heli);
                }
            }

            string otherTeam = killstreaks.otherTeam[player.SessionTeam];

            if (killstreaks.isTeamBased && killstreaks.activeUAVs[otherTeam] > 0)
            {
                foreach (Entity uav in killstreaks.uavModels[otherTeam])
                {
                    uavTargets.Add(uav);
                }
            }

            int targetCount = 0;
            foreach (Entity lb in lbTargets)
            {
                AfterDelay(3000, () =>
                {
                    if (targetCount % 2 == 0)
                        fireAtTarget(player, lb, player.SessionTeam, true);
                    else
                        fireAtTarget(player, lb, player.SessionTeam, false);
                });
            }

            foreach (Entity heli in heliTargets)
            {
                AfterDelay(3000, () => fireAtTarget(player, heli, player.SessionTeam, true));
            }

            foreach (Entity uav in uavTargets)
            {
                AfterDelay(500, () => fireAtTarget(player, uav, player.SessionTeam, false));
            }

            if (killstreaks.ac130InUse && killstreaks.ac130.HasField("owner") && killstreaks.ac130.GetField<Entity>("owner").SessionTeam != player.SessionTeam)
            {
                Entity ac130Target = killstreaks.level.GetField<Entity>("ac130").GetField<Entity>("planemodel");
                AfterDelay(6000, () => fireAtTarget(player, ac130Target, player.SessionTeam, true));
            }
        }

        private static IEnumerator earlyAbortWatcher(Entity player)
        {
            string team = player.SessionTeam;

            yield return player.WaitTill_any("disconnect", "joined_team", "joined_spectators", "game_ended");

            killstreaks.teamAirDenied[killstreaks.otherTeam[team]] = false;
            killstreaks.airDeniedPlayer = null;
        }

        private static void finishAAStrike(Entity player)
        {
            killstreaks.teamAirDenied[killstreaks.otherTeam[player.SessionTeam]] = true;
            killstreaks.airDeniedPlayer = player;
            StartAsync(earlyAbortWatcher(player));

            cycleTargets(player);

            for (int i = 0; i < 4; i++)
            {
                int delay = 6000 * i;
                int oldI = i;
                AfterDelay(delay, () =>
                {
                    if (oldI == 1 || oldI == 3)
                        doFlyBy(player, true);
                    else
                        doFlyBy(player, false);
                });
            }

            AfterDelay(21000, () =>
            {
                player.Notify("stopFindingTargets");
                killstreaks.teamAirDenied[killstreaks.otherTeam[player.SessionTeam]] = false;
                killstreaks.airDeniedPlayer = null;
            });
        }

        private static void fireAtTarget(Entity player, Entity curTarget, string team, bool showIcon)
        {
            Vector3 upVector = new Vector3(0, 0, 14000);
            Vector3 miniUpVector = new Vector3(0, 0, 1500);
            int backDist = 15000;
            int forwardDist = 20000;

            Vector3 targetPos = curTarget.Origin;
            upVector = new Vector3(0, 0, targetPos.Z) + new Vector3(0, 0, 1000);

            Vector3 curTargetYaw = new Vector3(0, curTarget.Angles.Y, 0);

            Vector3 forward = AnglesToForward(curTargetYaw);
            Vector3 startpos = curTarget.Origin + miniUpVector + forward * backDist * -1;
            Vector3 endpos = curTarget.Origin + miniUpVector + forward * forwardDist;

            Entity rocket1 = MagicBullet("aamissile_projectile_mp", startpos + new Vector3(0, 0, -75), curTarget.Origin, player);
            rocket1.SetTargetEnt(curTarget);
            rocket1.SetFlightModeDirect();

            Entity rocket2 = MagicBullet("aamissile_projectile_mp", startpos + new Vector3(RandomInt(500), RandomInt(500), -75), curTarget.Origin, player);
            rocket2.SetTargetEnt(curTarget);
            rocket2.SetFlightModeDirect();

            Entity plane;

            if (showIcon)
                plane = SpawnPlane(player, "script_model", startpos, "hud_minimap_harrier_green", "hud_minimap_harrier_red");
            else
                plane = SpawnPlane(player, "script_model", startpos, "", "");

            if (player.SessionTeam == "allies")
                plane.SetModel("vehicle_av8b_harrier_jet_mp");
            else
                plane.SetModel("vehicle_av8b_harrier_jet_opfor_mp");

            float length = Distance(startpos, endpos);

            plane.Angles = VectorToAngles(endpos - startpos);

            AASoundManager(plane, length);
            playPlaneFx(plane);

            length = Distance(startpos, endpos);
            plane.MoveTo(endpos * 2, length / 2000, 0, 0);

            AfterDelay((int)(length / 3000) * 1000, () => plane.Delete());
        }

        private static void AASoundManager(Entity plane, float length)
        {
            plane.PlayLoopSound("veh_aastrike_flyover_loop");

            AfterDelay((int)((length / 2) / 2000) * 1000, () =>
              {
                  plane.StopLoopSound();
                  plane.PlayLoopSound("veh_aastrike_flyover_outgoing_loop");
              });
        }

        private static void doFlyBy(Entity player, bool showIcon)
        {
            Vector3 randSpawn = getRandomSpawnpoint().Origin;
            Vector3 targetPos = new Vector3(randSpawn.X, randSpawn.Y, 0);

            int backDist = 20000;
            int forwardDist = 20000;
            Entity heightEnt = GetEnt("airstrikeheight", "targetname");

            Vector3 upVector = new Vector3(0, 0, heightEnt.Origin.Z + RandomIntRange(-100, 600));

            Vector3 forward = AnglesToForward(new Vector3(0, RandomInt(45), 0));

            Vector3 startpos = targetPos + upVector + forward * backDist * -1;
            Vector3 endPos = targetPos + upVector + forward * forwardDist;

            Vector3 plane2StartPos = startpos + new Vector3(RandomIntRange(400, 500), RandomIntRange(400, 500), RandomIntRange(200, 300));
            Vector3 plane2EndPos = endPos + new Vector3(RandomIntRange(400, 500), RandomIntRange(400, 500), RandomIntRange(200, 300));

            Entity plane;

            if (showIcon)
                plane = SpawnPlane(player, "script_model", startpos, "hud_minimap_harrier_green", "hud_minimap_harrier_red");
            else
                plane = SpawnPlane(player, "script_model", startpos, "", "");

            Entity plane2 = SpawnPlane(player, "script_model", plane2StartPos, "", "");

            if (player.SessionTeam == "allies")
            {
                plane.SetModel("vehicle_av8b_harrier_jet_mp");
                plane2.SetModel("vehicle_av8b_harrier_jet_mp");
            }
            else
            {
                plane.SetModel("vehicle_av8b_harrier_jet_opfor_mp");
                plane2.SetModel("vehicle_av8b_harrier_jet_opfor_mp");
            }

            plane.Angles = VectorToAngles(endPos - startpos);
            plane.PlayLoopSound("veh_aastrike_flyover_loop");
            playPlaneFx(plane);

            plane2.Angles = VectorToAngles(endPos - plane2StartPos);
            playPlaneFx(plane2);

            float length = Distance(startpos, endPos);
            plane.MoveTo(endPos * 2, length / 1800, 0, 0);
            AfterDelay(RandomIntRange(250, 500), () =>
            {
                plane2.MoveTo(plane2EndPos * 2, length / 1800, 0, 0);

                AfterDelay((int)(length / 1600) * 1000, () =>
                  {
                      plane.Delete();
                      plane2.Delete();
                  });
            });
        }

        private static IEnumerator playPlaneFx(Entity plane)
        {
            yield return Wait(0.5f);
            PlayFXOnTag(killstreaks.fx_airstrike_afterburner, plane, "tag_engine_right");
            yield return Wait(0.5f);
            PlayFXOnTag(killstreaks.fx_airstrike_afterburner, plane, "tag_engine_left");
            yield return Wait(0.5f);
            PlayFXOnTag(killstreaks.fx_airstrike_contrail, plane, "tag_right_wingtip");
            yield return Wait(0.5f);
            PlayFXOnTag(killstreaks.fx_airstrike_contrail, plane, "tag_left_wingtip");
        }

        private static Entity getRandomSpawnpoint()
        {
            Entity ret = null;
            for (int i = 0; i < 700; i++)
            {
                Entity e = Entity.GetEntity(i);
                if (e == null) continue;
                if (e.Classname == "mp_tdm_spawn")
                {
                    ret = e;
                    if (RandomInt(100) > 50) break;
                }
                else continue;
            }
            return ret;
        }
    }
}
