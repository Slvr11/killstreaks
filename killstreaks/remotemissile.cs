using System;
using System.Collections;
using System.Linq;
using System.Text;
using InfinityScript;
using static killstreaks.killstreaks;
using static InfinityScript.GSCFunctions;

namespace killstreaks
{
    public class remotemissile : BaseScript
    {
        public static Action<Entity> missileGiveFunc;
        public static Func<Entity, bool> missileUseFunc;

        //private static int missileRemoteLaunchVert = 14000;
        //private static int missileRemoteLaunchHorz = 7000;
        //private static int missileRemoteLaunchTargetDist = 1500;
        public remotemissile()
        {
            missileGiveFunc = givePredatorMissile;
            missileUseFunc = tryUsePredatorMissile;
            //string mapname = GSCFunctions.GetDvar("mapname");
            level.SetField("civilianJetFlyBy", false);

            //GSCFunctions.PreCacheItem("remotemissile_projectile_mp");
            PreCacheShader("ac130_overlay_grain");

            //Entity[] rockets;
            //level.SetField("rockets", new Parameter(rockets));

            level.SetField("remotemissile_fx", "explosions/aerial_explosion");

            OnNotify("game_over", () => level.Notify("game_ended"));
        }

        public static void givePredatorMissile(Entity player)
        {
            player.SetField("hasRemoteMissile", true);
        }

        public static bool tryUsePredatorMissile(Entity player)
        {
            if (level.GetField<bool>("civilianJetFlyBy"))
            {
                player.IPrintLnBold("Civilian air traffic in area.");
                return false;
            }

            StartAsync(initUsePredatorMissile(player));
            return true;
        }
        private static IEnumerator initUsePredatorMissile(Entity player)
        {
            string result = "";
            yield return initRideKillstreak(player, "", new Action<string>((s) => result = s));

            if (result != "success")
            {
                if (result == "disconnect")
                    clearUsingRemote(player, "remote_missile");

                yield return null;
            }
            setUsingRemote(player, "remotemissile");

            _fire(player);

            player.Notify("used_killstreak", "predator_missile");

            yield break;
        }

        private static void _fire(Entity player)
        {
            Entity remoteMissileSpawn = getRandomMissileSpawn();//GetEnt("remoteMissileSpawn", "targetname");

            if (remoteMissileSpawn == null || remoteMissileSpawn.Target == "")
            {
                Utilities.PrintToConsole("Remote missile spawn doesn't exist in the map!");
                return;
            }

            Entity target = GetEnt(remoteMissileSpawn.Target, "targetname");

            if (target == null)
            {
                Utilities.PrintToConsole("Remote missile spawn doesn't have a spawn point!");
                return;
            }

            Vector3 startPos = remoteMissileSpawn.Origin;
            Vector3 targetPos = target.Origin;

            Vector3 vector = VectorNormalize(startPos - targetPos);
            startPos = (vector * 14000) + targetPos;
            /*
            Vector3 upVector = new Vector3(0, 0, missileRemoteLaunchVert);
            int backDist = missileRemoteLaunchHorz;
            int targetDist = missileRemoteLaunchTargetDist;

            Vector3 forward = AnglesToForward(player.Angles);
            Vector3 startPos = player.Origin + upVector + forward * backDist * -1;
            Vector3 targetPos = player.Origin + forward * targetDist;
            */

            Entity rocket = MagicBullet("remotemissile_projectile_mp", startPos, targetPos, player);

            if (rocket == null)
            {
                clearUsingRemote(player, "remotemissile");
                return;
            }
            rocket.SetCanDamage(true);

            StartAsync(missileEyes(player, rocket));
        }

        private static IEnumerator missileEyes(Entity player, Entity rocket)
        {
            StartAsync(player_cleanupOnGameEnded(player, rocket));
            StartAsync(player_cleanupOnTeamChange(player, rocket));

            player.VisionSetMissileCamForPlayer("black_bw", 0);

            if (player.Classname != "player") yield return null;

            player.VisionSetMissileCamForPlayer(thermal_vision, 1);
            StartAsync(delayedFOFOverlay(player));

            player.CameraLinkTo(rocket, "tag_origin");
            player.ControlsLinkTo(rocket);

            yield return rocket.WaitTill("death");

            player.ControlsUnlink();
            player.FreezeControls(true);

            if (GetDvarInt("scr_gameended") == 0)
                StartAsync(staticEffect(player, 0.5f));

            yield return Wait(0.5f);

            player.ThermalVisionFOFOverlayOff();

            player.CameraUnlink();

            clearUsingRemote(player, "remotemissile");

            yield break;
        }
        private static IEnumerator delayedFOFOverlay(Entity player)
        {
            if (!player.IsAlive || player.Classname != "player") yield return null;

            yield return Wait(0.15f);

            player.ThermalVisionFOFOverlayOn();

            yield break;
        }
        private static IEnumerator staticEffect(Entity player, float duration)
        {
            if (player.Classname != "player") yield return null;

            HudElem staticBG = NewClientHudElem(player);
            staticBG.HorzAlign = HudElem.HorzAlignments.Fullscreen;
            staticBG.VertAlign = HudElem.VertAlignments.Fullscreen;
            staticBG.SetShader("white", 640, 480);
            staticBG.Archived = true;
            staticBG.Sort = 10;

            HudElem staticFG = NewClientHudElem(player);
            staticFG.HorzAlign = HudElem.HorzAlignments.Fullscreen;
            staticFG.VertAlign = HudElem.VertAlignments.Fullscreen;
            staticFG.SetShader("ac130_overlay_grain", 640, 480);
            staticFG.Archived = true;
            staticFG.Sort = 20;

            yield return Wait(duration);

            staticFG.Destroy();
            staticBG.Destroy();

            yield break;
        }
        private static IEnumerator player_cleanupOnTeamChange(Entity player, Entity rocket)
        {
            string result = "";
            yield return player.WaitTill_any_return(new Action<string>((s) => result = s), "joined_team", "joined_spectators", "disconnect", "stopped_using_remote");//player.WaitTill_notify_or_timeout("joined_team", 10, new Action<string>((s) => result = s));

            if (result != "disconnect" || result != "stopped_using_remote")
            {
                if (player.SessionTeam != "spectator")
                {
                    player.ThermalVisionFOFOverlayOff();
                    player.ControlsUnlink();
                    player.CameraUnlink();
                }

                clearUsingRemote(player, "remotemissile");
            }

            yield break;
        }
        private static IEnumerator player_cleanupOnGameEnded(Entity player, Entity rocket)
        {
            string result = "";
            yield return level.WaitTill_notify_or_timeout("game_ended", 10, new Action<string>((s) => result = s));

            if (result != "notify") yield return null;

            if (rocket.Classname == "rocket")
            {
                player.ThermalVisionFOFOverlayOff();
                player.ControlsUnlink();
                player.CameraUnlink();
            }

            yield break;
        }

        private static Entity getRandomMissileSpawn()
        {
            Entity ret = null;
            for (int i = 0; i < 700; i++)
            {
                Entity e = Entity.GetEntity(i);
                if (e == null) continue;
                if (e.TargetName == "remoteMissileSpawn")
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
