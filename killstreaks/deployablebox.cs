using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace killstreaks
{
    /*
	Deployable box killstreaks: the player will be able to place a box in the world and teammates can grab items from it
		this will be used on multiple killstreaks where you can place a box in the world with something in it
    */
    public class deployablebox : BaseScript
    {
        private static readonly Vector3 HEADICON_OFFSET = new Vector3(0, 0, 20);
        public static Action<Entity> deployableVestGiveFunc;
        public static Func<Entity, bool> deployableVestUseFunc;
        public static Action<Entity> deployableAmmoGiveFunc;
        public static Func<Entity, bool> deployableAmmoUseFunc;
        private static Dictionary<string, Dictionary<string, string>> boxSettings = new Dictionary<string, Dictionary<string, string>>();
        private static int box_explode_fx;
        public deployablebox()
        {
            deployableVestGiveFunc = giveDeployableVest;
            deployableVestUseFunc = tryUseDeployableVest;
            deployableAmmoGiveFunc = giveDeployableAmmo;
            deployableAmmoUseFunc = tryUseDeployableAmmo;
            Dictionary<string, string> deployable_vest = new Dictionary<string, string>();
            deployable_vest.Add("weaponInfo", "deployable_vest_marker_mp");
            deployable_vest.Add("modelBase", "com_deploy_ballistic_vest_friend_world");
            deployable_vest.Add("hintString", "Press and hold ^3[{+activate}]^7 for Ballistic Vest");
            deployable_vest.Add("capturingString", "Getting Ballistic Vest...");
            deployable_vest.Add("eventString", "Vest Taken");
            deployable_vest.Add("streakName", "deployable_vest");
            deployable_vest.Add("splashName", "used_deployable_vest");
            deployable_vest.Add("shaderName", "compass_objpoint_deploy_friendly");
            deployable_vest.Add("lifeSpan", "60");
            deployable_vest.Add("xp", "50");
            deployable_vest.Add("voDestroyed", "ballistic_vest_destroyed");
            boxSettings.Add("deployable_vest", deployable_vest);
            Dictionary<string, string> deployable_ammo = new Dictionary<string, string>();
            deployable_ammo.Add("weaponInfo", "deployable_vest_marker_mp");
            deployable_ammo.Add("modelBase", "com_deploy_ballistic_vest_friend_viewmodel");
            deployable_ammo.Add("hintString", "Press and hold ^3[{+activate}]^7 for a Weapon.");
            deployable_ammo.Add("capturingString", "Getting a Weapon...");
            deployable_ammo.Add("eventString", "Weapon Taken");
            deployable_ammo.Add("streakName", "deployable_ammo");
            deployable_ammo.Add("splashName", "used_deployable_ammo");
            deployable_ammo.Add("shaderName", "airdrop_icon");
            deployable_ammo.Add("lifeSpan", "60");
            deployable_ammo.Add("xp", "50");
            deployable_ammo.Add("voDestroyed", "");
            boxSettings.Add("deployable_ammo", deployable_ammo);

            box_explode_fx = LoadFX("fire/ballistic_vest_death");

            PreCacheShader("waypoint_ammo_friendly");
            PreCacheShader("airdrop_icon");

            PlayerConnected += box_onPlayerConnect;
        }

        private static void box_onPlayerConnect(Entity player)
        {
            player.SpawnedPlayer += () => box_onPlayerSpawned(player);
        }
        private static void box_onPlayerSpawned(Entity player)
        {
            killstreaks.level.SetField("box_playerSpawned", player);
            killstreaks.level.Notify("box_spawned_player");
        }

        public static void giveDeployableVest(Entity player)
        {

        }
        public static void giveDeployableAmmo(Entity player)
        {

        }
        public static bool tryUseDeployableVest(Entity player)
        {
            bool result = checkForDeployableMarkerPermission(player, "deployable_vest");

            if (!result)
                return false;

            //StartAsync(beginDeployableViaMarker(player, "deployable_vest"));
            StartAsync(watchMarkerUsage(player, "deployable_vest"));

            return true;
        }
        public static bool tryUseDeployableAmmo(Entity player)
        {
            bool result = checkForDeployableMarkerPermission(player, "deployable_ammo");

            if (!result)
                return false;

            StartAsync(watchMarkerUsage(player, "deployable_ammo"));

            return true;
        }
        private static bool checkForDeployableMarkerPermission(Entity player, string boxType)
        {
            if (player.GetAmmoCount(boxSettings[boxType]["weaponInfo"]) <= 0)
                return false;

            return true;
        }

        /*
        private static IEnumerator beginDeployableViaMarker(Entity player, string boxType)
        {
            player.SetField("marker", player);

            StartAsync(watchMarkerUsage(player, boxType));

            yield return player.WaitTill("weapon_change");

            string markerWeapon;
            string currentWeapon = player.CurrentWeapon;

            if (isMarker(currentWeapon))
                markerWeapon = currentWeapon;
            else
                markerWeapon = "";


        }
        */
        private static IEnumerator watchMarkerUsage(Entity player, string boxType)
        {
            player.Notify("watchMarkerUsage");

            StartAsync(watchMarker(player, boxType));

            yield return player.WaitTill_any("grenade_pullback", "disconnect");

            if (player.Classname != "player")//if disconnect
                yield break;

            if (!isMarker(player.CurrentWeapon))
                yield break;

            player.DisableUsability();

            StartAsync(beginMarkerTracking(player));
        }
        private static IEnumerator watchMarker(Entity player, string boxType)
        {
            player.Notify("watchMarker");

            Parameter[] param = null;

            yield return player.WaitTill_return("grenade_fire", new Action<Parameter[]>((p) => param = p));

            if (param == null) yield break;
            Entity marker = param[0].As<Entity>();
            string weapName = (string)param[1];

            if (!isMarker(weapName))
                yield break;

            if (!player.IsAlive)
            {
                marker.Delete();
                yield break;
            }

            marker.SetField("owner", player);
            marker.SetField("weaponName", weapName);

            player.SetField("marker", marker);

            StartAsync(takeWeaponOnStuck(player, marker, weapName));

            StartAsync(markerActivate(marker, boxType, box_setActive));
        }

        private static IEnumerator takeWeaponOnStuck(Entity player, Entity weap, string weapName)
        {
            yield return weap.WaitTill_notify_or_timeout("missile_stuck", 5);

            if (killstreaks.isTeamBased) weap.PlaySoundToTeam("mp_vest_deployed_ui", player.SessionTeam);
            else weap.PlaySound("mp_vest_deployed_ui");

            if (player.HasWeapon(weapName))
            {
                player.TakeWeapon(weapName);
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
            }
        }

        private static IEnumerator beginMarkerTracking(Entity player)
        {
            yield return player.WaitTill_any("grenade_fire", "waepon_change");
            player.EnableUsability();
        }
        private static IEnumerator markerActivate(Entity marker, string boxType, Action<Entity> usedCallback)
        {
            yield return marker.WaitTill("missile_stuck");

            if (!Utilities.isEntDefined(marker))
                yield break;

            if (!marker.HasField("owner")) yield break;

            Entity owner = marker.GetField<Entity>("owner");
            Vector3 position = marker.Origin;

            owner.Notify("used_killstreak", boxType);

            Entity box = createBoxForPlayer(boxType, position, owner);

            yield return Wait(0.05f);

            usedCallback.Invoke(box);

            marker.Delete();
        }

        private static bool isMarker(string weapon)
        {
            switch (weapon)
            {
                case "deployable_vest_marker_mp":
                    return true;
                default:
                    return false;
            }
        }

        ///////////////////////////////////////////////////
        // BOX HANDLER FUNCTIONS
        //////////////////////////////////////////////////

        private static Entity createBoxForPlayer(string boxType, Vector3 position, Entity owner)
        {
            Entity box = Spawn("script_model", position);
            box.SetModel(boxSettings[boxType]["modelBase"]);
            box.Health = 1000;
            box.Angles = owner.Angles;
            box.SetField("boxType", boxType);
            box.SetField("owner", owner);
            box.SetField("team", owner.SessionTeam);

            box_setInactive(box);
            StartAsync(box_handleOwnerDisconnect(box));

            return box;
        }

        private static void box_setActive(Entity box)
        {
            box.SetCursorHint("HINT_NOICON");
            box.SetHintString(boxSettings[box.GetField<string>("boxType")]["hintString"]);

            box.SetField("inUse", false);

            if (killstreaks.isTeamBased)
            {
                int curObjID = killstreaks.getNextObjID();
                Objective_Add(curObjID, "invisible", Vector3.Zero);
                Objective_Position(curObjID, box.Origin);
                Objective_State(curObjID, "active");
                Objective_Icon(curObjID, boxSettings[box.GetField<string>("boxType")]["shaderName"]);
                Objective_Team(curObjID, box.GetField<string>("team"));
                killstreaks.addObjID(box, curObjID);
                box.SetField("objIdFriendly", curObjID);

                foreach (Entity player in Players)
                {
                    if (box.GetField<string>("team") == player.SessionTeam && !killstreaks.isJuggernaut(player))
                        killstreaks.setHeadIcon(box, player, killstreaks.getKillstreakCrateIcon(boxSettings[box.GetField<string>("boxType")]["streakName"]), HEADICON_OFFSET, 14, 14, true, 0.066f, true, true, false, false);
                }
            }
            else
            {
                int curObjID = killstreaks.getNextObjID();
                Objective_Add(curObjID, "invisible", Vector3.Zero);
                Objective_Position(curObjID, box.Origin);
                Objective_State(curObjID, "active");
                Objective_Icon(curObjID, boxSettings[box.GetField<string>("boxType")]["shaderName"]);
                Objective_Player(curObjID, box.GetField<Entity>("owner").GetEntityNumber());
                killstreaks.addObjID(box, curObjID);
                box.SetField("objIdFriendly", curObjID);

                if (!killstreaks.isJuggernaut(box.GetField<Entity>("owner")))
                    killstreaks.setHeadIcon(box, box.GetField<Entity>("owner"), killstreaks.getKillstreakCrateIcon(boxSettings[box.GetField<string>("boxType")]["streakName"]), HEADICON_OFFSET, 14, 14, true, 0.066f, true, true, false, false);
            }

            box.MakeUsable();
            box.SetField("isUsable", true);
            box.SetCanDamage(true);
            box.Health = 999999;
            box.SetField("maxHealth", 300);
            box.SetField("damageTaken", 0);
            StartAsync(box_handleDamage(box));
            StartAsync(box_handleDeath(box));
            StartAsync(box_timeOut(box));
            //disableWhenJuggernaut(box);

            foreach (Entity player in Players)
            {
                if (player.Classname != "player") continue;

                if (killstreaks.isTeamBased)
                {
                    if (box.GetField<string>("team") == player.SessionTeam)
                    {
                        if (killstreaks.isJuggernaut(player))
                        {
                            box.DisablePlayerUse(player);
                            StartAsync(doubleDip(box, player));
                        }
                        else
                        {
                            box.EnablePlayerUse(player);
                        }
                        StartAsync(boxThink(box, player));
                    }
                    else
                    {
                        box.DisablePlayerUse(player);
                    }
                    StartAsync(box_playerJoinedTeam(box, player));
                }
                else
                {
                    if (box.HasField("owner") && box.GetField<Entity>("owner") == player)
                    {
                        if (killstreaks.isJuggernaut(player))
                        {
                            box.DisablePlayerUse(player);
                            StartAsync(doubleDip(box, player));
                        }
                        else
                        {
                            box.EnablePlayerUse(player);
                        }
                        StartAsync(boxThink(box, player));
                    }
                    else
                    {
                        box.DisablePlayerUse(player);
                    }
                }
            }

            killstreaks.teamPlayerCardSplash(boxSettings[box.GetField<string>("boxType")]["splashName"], box.GetField<Entity>("owner"), true);

            //StartAsync(box_playerConnected(box));
        }
        private static IEnumerator box_playerConnected(Entity box)
        {
            yield return killstreaks.level.WaitTill("box_spawned_player");
            Entity player = killstreaks.level.GetField<Entity>("box_playerSpawned");

            if (killstreaks.isTeamBased)
            {
                if (box.GetField<string>("team") == player.SessionTeam)
                {
                    box.EnablePlayerUse(player);
                    StartAsync(boxThink(box, player));
                    killstreaks.setHeadIcon(box, player, killstreaks.getKillstreakCrateIcon(boxSettings[box.GetField<string>("boxType")]["streakName"]), HEADICON_OFFSET, 14, 14, true, 0.066f, true, true, false, false);
                }
                else
                {
                    box.DisablePlayerUse(player);
                    killstreaks.setHeadIcon(box, player, "", Vector3.Zero);
                }
            }

            if (Utilities.isEntDefined(box))
                StartAsync(box_playerConnected(box));
        }

        private static IEnumerator box_playerJoinedTeam(Entity box, Entity player)
        {
            yield return player.WaitTill("joined_team");

            if (killstreaks.isTeamBased)
            {
                if (box.GetField<string>("team") == player.SessionTeam)
                {
                    box.EnablePlayerUse(player);
                    StartAsync(boxThink(box, player));
                    killstreaks.setHeadIcon(box, player, killstreaks.getKillstreakCrateIcon(boxSettings[box.GetField<string>("boxType")]["streakName"]), HEADICON_OFFSET, 14, 14, true, 0.066f, true, true, false, false);
                }
                else
                {
                    box.DisablePlayerUse(player);
                    killstreaks.setHeadIcon(box, player, "", Vector3.Zero);
                }
            }
        }

        private static void box_setInactive(Entity box)
        {
            box.MakeUnUsable();
            box.SetField("isUsable", false);
            killstreaks.setHeadIcon(box, null, "", Vector3.Zero);
            if (box.HasField("objIdFriendly"))
                killstreaks._objective_delete(box);
        }

        private static IEnumerator box_handleDamage(Entity box)
        {
            Parameter[] param = null;

            yield return box.WaitTill_return("damage", new Action<Parameter[]>((p) => param = p));

            //Utilities.PrintToConsole("Damaged");

            if (param == null)
                yield break;
            int damage = (int)param[0];
            Entity attacker = (Entity)param[1];
            string meansOfDeath = (string)param[4];
            string weapon = (string)param[9];

            if (!box.HasField("boxType")) yield break;

            if (killstreaks.isTeamBased && attacker != box.GetField<Entity>("owner") && attacker.SessionTeam == box.GetField<Entity>("owner").SessionTeam)
            {
                StartAsync(box_handleDamage(box));
                yield break;
            }

            switch (weapon)
            {
                case "concussion_grenade_mp":
                case "flash_grenade_mp":
                case "smoke_grenade_mp":
                    StartAsync(box_handleDamage(box));
                    yield break;
            }

            if (meansOfDeath == "MOD_MELEE")
                box.SetField("damageTaken", box.GetField<int>("maxHealth"));

            box.SetField("wasDamaged", true);

            int modifiedDamage = damage;

            if (attacker.Classname == "player")
            {
                killstreaks.updateDamageFeedback(attacker, "deployable_bag");

                if (meansOfDeath == "MOD_RIFLE_BULLET" || meansOfDeath == "MOD_PISTOL_BULLET")
                {
                    if (attacker.HasPerk("specialty_armorpiercing"))
                        modifiedDamage += (int)(damage * killstreaks.armorPiercingMod);
                }
            }

            if (attacker.HasField("owner") && attacker.GetField<Entity>("owner").Classname == "player")
            {
                killstreaks.updateDamageFeedback(attacker, "deployable_bag");
            }

            switch (weapon)
            {
                case "ac130_105mm_mp":
                case "ac130_40mm_mp":
                case "stinger_mp":
                case "javelin_mp":
                case "remote_mortar_missile_mp":
                case "remotemissile_projectile_mp":
                    box.SetField("largeProjectileDamage", true);
                    modifiedDamage = box.GetField<int>("maxHealth") + 1;
                    break;
                case "artillery_mp":
                case "stealth_bomb_mp":
                    box.SetField("largeProjectileDamage", false);
                    modifiedDamage += (damage * 4);
                    break;
                case "bomb_site_mp":
                    box.SetField("largeProjectileDamage", false);
                    modifiedDamage = box.GetField<int>("maxHealth") + 1;
                    break;
            }

            box.SetField("damageTaken", box.GetField<int>("damageTaken") + modifiedDamage);

            if (box.GetField<int>("damageTaken") >= box.GetField<int>("maxHealth"))
            {
                if (attacker.Classname == "player" && attacker != box.GetField<Entity>("owner"))
                {
                    //Give 100 xp
                    attacker.Notify("destroyed_killstreak");
                }

                if (box.HasField("owner"))
                    box.GetField<Entity>("owner").PlayLocalSound(boxSettings[box.GetField<string>("boxType")]["voDestroyed"]);

                box.Notify("box_death");
                yield break;
            }

            StartAsync(box_handleDamage(box));
        }

        private static IEnumerator box_handleDeath(Entity box)
        {
            yield return box.WaitTill("box_death");

            box_setInactive(box);

            PlayFX(box_explode_fx, box.Origin);

            AfterDelay(500, () =>
            {
                box.Notify("deleting");

                box.Delete();
            });
        }

        private static IEnumerator box_handleOwnerDisconnect(Entity box)
        {
            yield return box.GetField<Entity>("owner").WaitTill_any("disconnect", "joined_team", "joined_spectators");

            if (Utilities.isEntDefined(box))
                box.Notify("box_death");
        }

        private static IEnumerator boxThink(Entity box, Entity player)
        {
            StartAsync(playerUseTriggerLoop(box, player));

            StartAsync(boxCaptureThink(box, player));

            yield return box.GetField<Entity>("owner").WaitTill("captured");

            //Utilities.PrintToConsole("Box was captured");

            if (box.GetField<Entity>("capturer") != player)
            {
                StartAsync(boxThink(box, player));
                yield break;
            }

            switch (box.GetField<string>("boxType"))
            {
                case "deployable_vest":
                    player.PlayLocalSound("ammo_crate_use");
                    killstreaks.giveLightArmor(player);
                    break;
                case "deployable_ammo":
                    player.PlayLocalSound("ammo_crate_use");
                    giveRandomWeapon(player);
                    break;
            }

            if (player != box.GetField<Entity>("owner"))
            {
                player.IPrintLnBold("^3+100");//Give xp
            }

            killstreaks.setHeadIcon(box, player, "", Vector3.Zero);
            box.DisablePlayerUse(player);
            StartAsync(doubleDip(box, player));
        }

        private static IEnumerator playerUseTriggerLoop(Entity box, Entity player)
        {
            yield return player.WaitTill("trigger_ks");

            if (!Utilities.isEntDefined(box))
                yield break;

            if (player.Origin.DistanceTo(box.Origin) < 100)
            {
                box.SetField("triggerer", player);
                box.GetField<Entity>("owner").Notify("trigger_box");
            }
            else
                StartAsync(playerUseTriggerLoop(box, player));
        }

        private static IEnumerator doubleDip(Entity box, Entity player)
        {
            string result = "";
            yield return player.WaitTill_any_return(new Action<string>((s) => result = s), "death", "disconnect");

            if (result == "disconnect")
                yield break;

            if (!Utilities.isEntDefined(box))
                yield break;

            if (killstreaks.isTeamBased)
            {
                if (box.GetField<string>("team") == player.SessionTeam)
                {
                    killstreaks.setHeadIcon(box, player, killstreaks.getKillstreakCrateIcon(boxSettings[box.GetField<string>("boxType")]["streakName"]), HEADICON_OFFSET, 14, 14, true, 0.066f, true, true, false, false);
                    box.EnablePlayerUse(player);
                    StartAsync(boxCaptureThink(box, player));
                    StartAsync(playerUseTriggerLoop(box, player));
                    StartAsync(doubleDip(box, player));
                }
            }
            else
            {
                if (box.GetField<Entity>("owner") == player)
                {
                    killstreaks.setHeadIcon(box, player, killstreaks.getKillstreakCrateIcon(boxSettings[box.GetField<string>("boxType")]["streakName"]), HEADICON_OFFSET, 14, 14, true, 0.066f, true, true, false, false);
                    box.EnablePlayerUse(player);
                    StartAsync(boxCaptureThink(box, player));
                    StartAsync(playerUseTriggerLoop(box, player));
                    StartAsync(doubleDip(box, player));
                }
            }
        }
        private static IEnumerator boxCaptureThink(Entity box, Entity player)
        {
            yield return box.GetField<Entity>("owner").WaitTill("trigger_box");

            if (!box.HasField("triggerer"))
                yield break;

            Entity triggerer = box.GetField<Entity>("triggerer");

            if (triggerer != player)
                yield break;

            //Utilities.PrintToConsole("starting useHoldThink");

            StartAsync(useHoldThink(box, player, 2000));

            string result = "useHoldThink_cancelled";
            yield return player.WaitTill_any_return(new Action<string>((s) => result = s), "useHoldThink_captured", "useHoldThink_cancelled");

            //Utilities.PrintToConsole("useHoldThink loop done as " + result);

            if (result != "useHoldThink_cancelled")
            {
                box.SetField("capturer", player);
                box.GetField<Entity>("owner").Notify("captured", player);
            }
            else
            {
                StartAsync(boxCaptureThink(box, player));
                StartAsync(playerUseTriggerLoop(box, player));
            }
        }

        private static bool isFriendlyToBox(Entity player, Entity box)
        {
            if (killstreaks.isTeamBased && player.SessionTeam == box.GetField<string>("team"))
                return true;

            return false;
        }

        private static IEnumerator box_timeOut(Entity box)
        {
            int lifeSpan = int.Parse(boxSettings[box.GetField<string>("boxType")]["lifeSpan"]);

            yield return box.WaitTill_notify_or_timeout("box_death", lifeSpan);

            box.Notify("box_death");
        }

        private static IEnumerator deleteOnOwnerDeath(Entity boxModel, Entity box)
        {
            yield return Wait(0.25f);
            boxModel.LinkTo(box, "tag_origin", Vector3.Zero, Vector3.Zero);

            yield return box.WaitTill("box_death");

            boxModel.Delete();
        }

        private static IEnumerator useHoldThink(Entity box, Entity player, int useTime)
        {
            player.LinkTo(box);
            player.PlayerLinkedOffsetEnable();

            player.DisableWeapons();

            player.SetField("boxParams_useTime", useTime);

            HudElem bar = killstreaks.createPrimaryProgressBar(player, 0, 0, boxSettings[box.GetField<string>("boxType")]["capturingString"]);

            bool result = false;
            useHoldThinkLoop(player, (r) => result = r);
            killstreaks.updateBar(bar, 120, 2);

            yield return player.WaitTill("useHoldThinkLoop_done");

            killstreaks.destroyPrimaryProgressBar(bar);

            if (player.IsAlive)
            {
                player.EnableWeapons();
                player.Unlink();
            }

            if (result) player.Notify("useHoldThink_captured");
            else player.Notify("useHoldThink_cancelled");
        }
        private static void useHoldThinkLoop(Entity player, Action<bool> returnResult)
        {
            int currentProgress = 0;
            OnInterval(50, () =>
                {
                    if (GetDvarInt("scr_gameended") == 0 && player.IsAlive && player.UseButtonPressed() && currentProgress < player.GetField<int>("boxParams_useTime"))
                    {
                        currentProgress += 50;

                        if (currentProgress >= player.GetField<int>("boxParams_useTime"))
                        {
                            returnResult.Invoke(player.IsAlive);
                            player.Notify("useHoldThinkLoop_done");
                            return player.IsAlive;
                        }
                        return true;
                    }

                    returnResult.Invoke(false);
                    player.Notify("useHoldThinkLoop_done");
                    return false;
                });
        }
        private static void giveRandomWeapon(Entity player)
        {
            string currentWeapon = player.CurrentWeapon;
            string newWeapon = getRandomWeapon(true);

            player.TakeWeapon(currentWeapon);
            player.GiveWeapon(newWeapon);
            AfterDelay(500, () => player.SwitchToWeapon(newWeapon));
        }
        private static string getRandomWeapon(bool addAttachments = true)
        {
            int random = RandomInt(53);
            string weapon;

            switch (random)
            {
                case 0:
                    weapon = "iw5_spas12_mp";
                    break;
                case 1:
                    weapon = "iw5_striker_mp";
                    break;
                case 2:
                    weapon =  "iw5_1887_mp";
                    break;
                case 3:
                    weapon = "iw5_ksg_mp";
                    break;
                case 4:
                    weapon = "iw5_mp5_mp";
                    break;
                case 5:
                    weapon = "iw5_m9_mp";
                    break;
                case 6:
                    weapon = "iw5_p90_mp";
                    break;
                case 7:
                    weapon = "iw5_pp90m1_mp";
                    break;
                case 8:
                    weapon = "iw5_ump45_mp";
                    break;
                case 9:
                    weapon = "iw5_mp7_mp";
                    break;
                case 10:
                    weapon = "iw5_acr_mp";
                    break;
                case 11:
                    weapon = "iw5_type95_mp";
                    break;
                case 12:
                    weapon = "iw5_m4_mp";
                    break;
                case 13:
                    weapon = "iw5_ak47_mp";
                    break;
                case 14:
                    weapon = "iw5_m16_mp";
                    break;
                case 15:
                    weapon = "iw5_mk14_mp";
                    break;
                case 16:
                    weapon = "iw5_g36c_mp";
                    break;
                case 17:
                    weapon = "iw5_scar_mp";
                    break;
                case 18:
                    weapon = "iw5_fad_mp";
                    break;
                case 19:
                    weapon = "iw5_cm901_mp";
                    break;
                case 20:
                    weapon = "iw5_m60_mp";
                    break;
                case 21:
                    weapon = "iw5_mk46_mp";
                    break;
                case 22:
                    weapon = "iw5_pecheneg_mp";
                    break;
                case 23:
                    weapon = "iw5_sa80_mp";
                    break;
                case 24:
                    weapon = "iw5_mg36_mp";
                    break;
                case 25:
                    weapon = "iw5_barrett_mp_barrettscope";
                    break;
                case 26:
                    weapon = "iw5_msr_mp_msrscope";
                    break;
                case 27:
                    weapon = "iw5_rsass_mp_rsassscope";
                    break;
                case 28:
                    weapon = "iw5_dragunov_mp_dragunovscope";
                    break;
                case 29:
                    weapon = "iw5_as50_mp_as50scope";
                    break;
                case 30:
                    weapon = "iw5_l96a1_mp_l96a1scope";
                    break;
                case 31:
                    weapon = "iw5_barrett_mp_acog";
                    break;
                case 32:
                    weapon = "iw5_msr_mp_acog";
                    break;
                case 33:
                    weapon = "iw5_rsass_mp_acog";
                    break;
                case 34:
                    weapon = "iw5_dragunov_mp_acog";
                    break;
                case 35:
                    weapon = "iw5_as50_mp_acog";
                    break;
                case 36:
                    weapon = "iw5_l96a1_mp_acog";
                    break;
                case 37:
                    weapon = "riotshield_mp";
                    break;
                case 38:
                    weapon = "rpg_mp";
                    break;
                case 39:
                    weapon = "javelin_mp";
                    break;
                case 40:
                    weapon = "iw5_smaw_mp";
                    break;
                case 41:
                    weapon = "m320_mp";
                    break;
                case 42:
                    weapon = "xm25_mp";
                    break;
                case 43:
                    weapon = "at4_mp";
                    break;
                case 44:
                    weapon = "iw5_usp45_mp";
                    break;
                case 45:
                    weapon = "iw5_p99_mp";
                    break;
                case 46:
                    weapon = "iw5_44magnum_mp";
                    break;
                case 47:
                    weapon = "iw5_mp412_mp";
                    break;
                case 48:
                    weapon = "iw5_deserteagle_mp";
                    break;
                case 49:
                    weapon = "iw5_fmg9_mp";
                    break;
                case 50:
                    weapon = "iw5_skorpion_mp";
                    break;
                case 51:
                    weapon = "iw5_g18_mp";
                    break;
                case 52:
                    weapon = "iw5_mp9_mp";
                    break;
                default:
                    weapon = "iw5_m4_mp";
                    break;
            }

            string[] attachments = null;
            if (addAttachments) attachments = getRandomAttachmentsForWeapon(weapon);

            Utilities.PrintToConsole(string.Join(", ", attachments));

            if (addAttachments)
                weapon = Utilities.BuildWeaponName(weapon, attachments[0], attachments[1], 0, 0);
            else
                weapon = Utilities.BuildWeaponName(weapon, "", "", 0, 0);

            return weapon;
        }
        private static string[] getRandomAttachmentsForWeapon(string weapon)
        {
            weapon = weapon.Split('_')[0] + "_" + weapon.Split('_')[1];
            string[] attachments = new string[2];
            //string weaponClass = TableLookup("mp/statsTable.csv", 4, weapon, 2);

            Utilities.PrintToConsole(weapon);

            int randomCol = RandomIntRange(11, 20);

            if (killstreaks.coinToss())
                attachments[0] = TableLookup("mp/statsTable.csv", 4, weapon, randomCol);

            //Roll for a second
            randomCol = RandomIntRange(11, 20);
            if (killstreaks.coinToss())
                attachments[1] = TableLookup("mp/statsTable.csv", 4, weapon, randomCol);

            return attachments;
        }
    }
}
