using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using InfinityScript;
using static killstreaks.killstreaks;

namespace killstreaks
{
    class juggernaut : BaseScript
    {
        public juggernaut()
        {
            Dictionary<string, Entity> juggSettings = new Dictionary<string, Entity>();

            juggSettings["juggernaut"] = GSCFunctions.SpawnStruct();
            juggSettings["juggernaut"].SetField("splashUsedName", "used_juggernaut");
            juggSettings["juggernaut"].SetField("overlay", "goggles_overlay");

            juggSettings["juggernaut_recon"] = GSCFunctions.SpawnStruct();
            juggSettings["juggernaut_recon"].SetField("splashUsedName", "used_juggernaut");
            juggSettings["juggernaut_recon"].SetField("overlay", "goggles_overlay");

            juggSettings["tjugg_juggernaut"] = GSCFunctions.SpawnStruct();
            juggSettings["tjugg_juggernaut"].SetField("splashUsedName", "callout_new_juggernaut");
            juggSettings["tjugg_juggernaut"].SetField("overlay", "goggles_overlay");

            juggSettings["jugg_juggernaut"] = GSCFunctions.SpawnStruct();
            juggSettings["jugg_juggernaut"].SetField("splashUsedName", "callout_new_juggernaut");
            juggSettings["jugg_juggernaut"].SetField("overlay", "goggles_overlay");

            level.SetField("juggSettings", new Parameter(juggSettings));

            foreach (string jugg in juggSettings.Keys)
            {
                //GSCFunctions.PreCacheShader(juggSettings[jugg].GetField<string>("overlay"));
                GSCFunctions.PreCacheShader("overlay_goggles");
            }
        }

        public static IEnumerator giveJuggernaut(Entity player, string juggType)
        {
            yield return WaitForFrame();

            //remove light armor
            if (player.HasPerk("specialty_lightarmor"))
                removeLightArmor(player, player.GetField<int>("previousMaxHealth"));

            //remove explosive bullets
            if (player.HasPerk("specialty_explosivebullets"))
                player.UnSetPerk("specialty_explosivebullets");

            bool createObjectiveIcon = true;

            switch (juggType)
            {
                case "juggernaut":
                    player.SetField("isJuggernaut", true);
                    giveJuggLoadout(player, player.SessionTeam, juggType);
                    player.SetMoveSpeedScale(.65f);
                    break;
                case "juggernaut_recon":
                    player.SetField("isJuggernautRecon", true);
                    giveJuggLoadout(player, player.SessionTeam, juggType);
                    player.SetMoveSpeedScale(.75f);

                    Entity portable_radar = GSCFunctions.Spawn("script_model", player.Origin);
                    portable_radar.SetField("team", player.SessionTeam);

                    portable_radar.MakePortableRadar(player);
                    player.SetField("personalRadar", portable_radar);

                    radarMover(player, portable_radar);
                    break;
                case "tjugg_juggernaut":
                    player.SetField("isJuggernaut", true);
                    giveJuggLoadout(player, player.SessionTeam, "gamemode");
                    player.SetMoveSpeedScale(.7f);

                    Entity portable_radar1 = GSCFunctions.Spawn("script_model", player.Origin);
                    portable_radar1.SetField("team", player.SessionTeam);

                    portable_radar1.MakePortableRadar(player);
                    player.SetField("personalRadar", portable_radar1);

                    radarMover(player, portable_radar1);
                    break;
                case "jugg_juggernaut":
                    player.SetField("isJuggernaut", true);
                    giveJuggLoadout(player, player.SessionTeam, "gamemode");
                    player.SetMoveSpeedScale(.7f);
                    if (GSCFunctions.GetMatchRulesData("juggData", "showJuggRadarIcon") == "0")
                        createObjectiveIcon = false;
                    break;
            }

            updateMoveSpeedScale(player);

            player.DisableWeaponPickup();

            if (GSCFunctions.GetDvarInt("camera_thirdPerson") == 0)
            {
                HudElem juggernautOverlay = GSCFunctions.NewClientHudElem(player);
                juggernautOverlay.X = 0;
                juggernautOverlay.Y = 0;
                juggernautOverlay.AlignX = HudElem.XAlignments.Left;
                juggernautOverlay.AlignY = HudElem.YAlignments.Top;
                juggernautOverlay.HorzAlign = HudElem.HorzAlignments.Fullscreen;
                juggernautOverlay.VertAlign = HudElem.VertAlignments.Fullscreen;
                juggernautOverlay.SetShader(level.GetField<Dictionary<string, Entity>>("juggSettings")[juggType].GetField<string>("overlay"), 640, 480);
                juggernautOverlay.Sort = -10;
                juggernautOverlay.Archived = true;
                juggernautOverlay.HideIn3rdPerson = true;
            }

            juggernautSounds(player);

            player.SetPerk("specialty_radarjuggernaut", true, false);

            teamPlayerCardSplash(level.GetField<Dictionary<string, Entity>>("juggSettings")[juggType].GetField<string>("splashUsedName"), player);
            player.PlaySoundToTeam(voice[player.SessionTeam] + "use_juggernaut", player.SessionTeam, player);
            player.PlaySoundToTeam(voice[otherTeam[player.SessionTeam]] + "enemy_juggernaut", otherTeam[player.SessionTeam]);

            updateKillstreaks(player, true);

            StartAsync(juggRemover(player));

            //reapply flag
            if (player.HasField("carryFlag"))
            {
                yield return WaitForFrame();
                player.Attach(player.GetField<string>("carryFlag"), "J_spine4", true);
            }

            Notify("juggernaut_equipped", player);

            //Log data?
        }

        private static void juggernautSounds(Entity player)
        {
            OnInterval(3000, () =>
            {
                player.PlaySound("juggernaut_breathing_sound");
                if (player.HasField("isJuggernaut") || player.HasField("isJuggernautDef") || player.HasField("isJuggernautGL") || player.HasField("isJuggernautRecon")) return true;
                else return false;
            });
        }

        private static void radarMover(Entity player, Entity portableRadar)
        {
            OnInterval(50, () =>
            {
                portableRadar.MoveTo(player.Origin, .05f);
                if (player.HasField("isJuggernaut") || player.HasField("isJuggernautDef") || player.HasField("isJuggernautGL") || player.HasField("isJuggernautRecon")) return true;
                else return false;
            });
        }

        private static IEnumerator juggRemover(Entity player)
        {
            StartAsync(juggRemoveOnGameEnded(player));
            yield return player.WaitTill_any("death", "joined_team", "joined_spectators", "lost_juggernaut");

            player.EnableWeaponPickup();
            player.ClearField("isJuggernaut");
            player.ClearField("isJuggernautDef");
            player.ClearField("isJuggernautGL");
            player.ClearField("isJuggernautRecon");
            if (player.HasField("juggernautOverlay"))
            {
                player.GetField<HudElem>("juggernautOverlay").Destroy();
                player.ClearField("juggernautOverlay");
            }

            player.UnSetPerk("specialty_radarJuggernaut", true);

            if (player.HasField("personalRadar"))
            {
                player.Notify("jugdar_removed");
                deletePortableRadar(player.GetField<Entity>("personalRadar"));
                player.ClearField("personalRadar");
            }

            player.Notify("jugg_removed");
        }

        private static IEnumerator juggRemoveOnGameEnded(Entity player)
        {
            yield return level.WaitTill("game_ended");

            if (player.HasField("juggernautOverlay"))
            {
                player.GetField<HudElem>("juggernautOverlay").Destroy();
                player.ClearField("juggernautOverlay");
            }
        }

        private static void deletePortableRadar(Entity radar)
        {
            radar.Notify("delete");
            radar.Delete();
        }
    }
}
