using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using InfinityScript;
using static InfinityScript.GSCFunctions;
using static killstreaks.killstreaks;

namespace killstreaks
{
    public class aamissile : BaseScript
    {
        private static int AAMissileLaunchVert = 14000;
        private static int AAMissileLaunchHorz = 30000;
        private static int AAMissileLaunchTargetDist = 1500;

        private static List<Entity> rockets = new List<Entity>();

        public static Action<Entity> aaMissileGiveFunc;
        public static Func<Entity, bool> aaMissileUseFunc;

        public aamissile()
        {
            PreCacheItem("aamissile_projectile_mp");
            PreCacheShader("ac130_overlay_grain");

            aaMissileGiveFunc = giveAAMissile;
            aaMissileUseFunc = tryUseAAMissile;
        }

        private static void giveAAMissile(Entity player)
        {
        }
        private static bool tryUseAAMissile(Entity player)
        {
            if (level.GetField<bool>("civilianJetFlyBy"))
            {
                player.IPrintLnBold("Civilian air traffic in area.");
                return false;
            }

            StartAsync(initUseAAMissile(player));
            return true;
        }
        private static IEnumerator initUseAAMissile(Entity player)
        {
            string result = "";
            yield return initRideKillstreak(player, "", new Action<string>((s) => result = s));

            if (result != "success")
            {
                if (result == "disconnect")
                    clearUsingRemote(player, "aamissile");

                yield return null;
            }
            setUsingRemote(player, "aamissile");

            aa_missile_fire(player);

            player.Notify("used_killstreak", "aamissile");

            yield break;
        }

        private static Entity getTargets(Entity player)
        {
            List<Entity> lbTargets = new List<Entity>();
            List<Entity> heliTargets = new List<Entity>();

            if (littleBirds.Count > 0)
            {
                foreach (Entity lb in littleBirds)
                {
                    if (lb.HasField("team") && lb.GetField<string>("team") != player.SessionTeam)
                        lbTargets.Add(lb);
                }
            }

            if (helis.Count > 0)
            {
                foreach (Entity heli in helis)
                {
                    if (heli.HasField("team") && heli.GetField<string>("team") != player.SessionTeam)
                        heliTargets.Add(heli);
                }
            }

            if (ac130InUse && killstreaks.ac130.HasField("owner") && killstreaks.ac130.GetField<Entity>("owner").SessionTeam != player.SessionTeam)
                return killstreaks.ac130.GetField<Entity>("planemodel");
            if (heliTargets.Count > 0)
                return heliTargets[0];
            else if (lbTargets.Count > 0)
                return lbTargets[0];

            return null;
        }

        private static void aa_missile_fire(Entity player)
        {
            //Entity aaMissileSpawn = null;

            Vector3 upVector = new Vector3(0, 0, AAMissileLaunchVert);
            int backDist = AAMissileLaunchHorz;
            int targetDist = AAMissileLaunchTargetDist;

            Entity bestTarget = getTargets(player);
            Vector3 targetPos;

            if (bestTarget == null)
                targetPos = Vector3.Zero;
            else
            {
                targetPos = bestTarget.Origin;
                upVector = new Vector3(0, 0, targetPos.Z) + new Vector3(0, 0, 1000);
            }

            Vector3 forward = AnglesToForward(player.Angles);
            Vector3 startPos = player.Origin + upVector + forward * backDist * -1;
            
            Entity rocket = MagicBullet("aamissile_projectile_mp", startPos, targetPos, player);

            //spawn f16 model and make it do somthing cool

            if (rocket == null)
            {
                clearUsingRemote(player, "aamissile");
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

            clearUsingRemote(player, "aamissile");

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

                clearUsingRemote(player, "aamissile");
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
    }
}
