using System;
using System.Collections.Generic;
using System.Collections;
using InfinityScript;
using static InfinityScript.GSCFunctions;

namespace killstreaks
{
    public class killstreaks : BaseScript
    {
        public static Entity level = Entity.GetEntity(2046);
        public static bool gameEnded = false;

        public const string KILLSTREAK_STRING_TABLE = "mp/killstreakTable.csv";
        public const int STREAKCOUNT_MAX_COUNT = 3;
        public const int KILLSTREAK_NAME_COLUMN = 1;
        public const int KILLSTREAK_KILLS_COLUMN = 4;
        public const int KILLSTREAK_EARNED_HINT_COLUMN = 6;
        public const int KILLSTREAK_SOUND_COLUMN = 7;
        public const int KILLSTREAK_EARN_DIALOG_COLUMN = 8;
        public const int KILLSTREAK_ENEMY_USE_DIALOG_COLUMN = 11;
        public const int KILLSTREAK_WEAPON_COLUMN = 12;
        public const int KILLSTREAK_ICON_COLUMN = 14;
        public const int KILLSTREAK_OVERHEAD_ICON_COLUMN = 15;
        public const int KILLSTREAK_DPAD_ICON_COLUMN = 16;
        public const int NUM_KILLS_GIVE_ALL_PERKS = 8;
        public const int KILLSTREAK_GIMME_SLOT = 0;
        public const int KILLSTREAK_SLOT_1 = 1;
        public const int KILLSTREAK_SLOT_2 = 2;
        public const int KILLSTREAK_SLOT_3 = 3;
        public const int KILLSTREAK_ALL_PERKS_SLOT = 4;
        public const int KILLSTREAK_STACKING_START_SLOT = 5;

        public const int MAX_VEHICLES = 8;

        public static string thermal_vision = "thermal_mp";
        public static float armorPiercingMod = 1.5f;

        public static int fx_airstrike_afterburner;
        public static int fx_airstrike_contrail;
        public static Dictionary<string, Dictionary<string, int>> chopper_fx = new Dictionary<string, Dictionary<string, int>>();

        public static bool ac130InUse = false;
        public static Entity ac130 = null;
        public static Entity chopper = null;

        public static bool isTeamBased = true;
        public static Dictionary<string, bool> teamAirDenied = new Dictionary<string, bool>();
        public static Entity airDeniedPlayer = null;

        public static Dictionary<string, string> otherTeam = new Dictionary<string, string>();
        public static Dictionary<string, string> voice = new Dictionary<string, string>();

        public static Dictionary<string, Action<Entity>> killstreakFuncs = new Dictionary<string, Action<Entity>>();
        public static Dictionary<string, Func<Entity, bool>> killstreakUseFuncs = new Dictionary<string, Func<Entity, bool>>();

        public static List<Entity> littleBirds = new List<Entity>();
        public static int fauxVehicleCount = 0;

        private static bool[] _objIDList = new bool[32];
        private static Dictionary<Entity, byte> _objIDs = new Dictionary<Entity, byte>();

        public static List<Entity> helis = new List<Entity>();
        public static List<Entity> harriers = new List<Entity>();
        public static Dictionary<string, int> activeUAVs = new Dictionary<string, int>();
        public static Dictionary<string, int> activeCounterUAVs = new Dictionary<string, int>();
        public static Dictionary<string, List<Entity>> uavModels = new Dictionary<string, List<Entity>>();
        public static Dictionary<string, string> radarMode = new Dictionary<string, string>();

        public static int numIncomingVehicles = 0;
        public static int numDropCrates = 0;
        public static Dictionary<string, Dictionary<string, int>> crateTypes = new Dictionary<string, Dictionary<string, int>>();
        public static Dictionary<string, Dictionary<string, Func<Entity, string>>> crateFuncs = new Dictionary<string, Dictionary<string, Func<Entity, string>>>();
        public static Dictionary<string, int> crateMaxVal = new Dictionary<string, int>();
        public static Entity lowSpawn = null;

        public killstreaks()
        {
            //bool customStreaks = GetDvarInt("scr_game_hardpoints") == 0;//GetMatchRulesData("commonOption", "allowKillstreaks");
            //if (!customStreaks) return;

            string gametype = GetDvar("g_gametype");
            if (gametype == "dm" || gametype == "gun" || gametype == "oic" || gametype == "jugg")
                isTeamBased = false;

            if (GetDvar("mapname") == "mp_radar") thermal_vision = "thermal_snowlevel_mp";

            fx_airstrike_afterburner = LoadFX("fire/jet_afterburner");
            fx_airstrike_contrail = LoadFX("smoke/jet_contrail");

            //level.SetField("killstreakFuncs", new Parameter(new Dictionary<string, Action<Entity>>()));
            //level.SetField("killstreakSetupFuncs", new Parameter(new Dictionary<string, Delegate>()));
            //level.SetField("killstreakWeapons", new Parameter(new Dictionary<string, string>()));

            List<string> killstreakWieldWeapons = new List<string>();
            killstreakWieldWeapons.Add("cobra_player_minigun_mp");
            killstreakWieldWeapons.Add("artillery_mp");
            killstreakWieldWeapons.Add("stealth_bomb_mp");
            killstreakWieldWeapons.Add("pavelow_minigun_mp");
            killstreakWieldWeapons.Add("sentry_minigun_mp");
            killstreakWieldWeapons.Add("harrier_20mm_mp");
            killstreakWieldWeapons.Add("ac130_105mm_mp");
            killstreakWieldWeapons.Add("ac130_40mm_mp");
            killstreakWieldWeapons.Add("ac130_25mm_mp");
            killstreakWieldWeapons.Add("remotemissile_projectile_mp");
            killstreakWieldWeapons.Add("cobra_20mm_mp");
            killstreakWieldWeapons.Add("nuke_mp");
            killstreakWieldWeapons.Add("apache_minigun_mp");
            killstreakWieldWeapons.Add("littlebird_guard_minigun_mp");
            killstreakWieldWeapons.Add("uav_strike_marker_mp");
            killstreakWieldWeapons.Add("osprey_minigun_mp");
            killstreakWieldWeapons.Add("strike_marker_mp");
            killstreakWieldWeapons.Add("manned_minigun_turret_mp");
            killstreakWieldWeapons.Add("manned_gl_turret_mp");
            killstreakWieldWeapons.Add("airdrop_trap_explosive_mp");
            killstreakWieldWeapons.Add("uav_strike_projectile_mp");
            killstreakWieldWeapons.Add("remote_mortar_missile_mp");
            killstreakWieldWeapons.Add("manned_littlebird_sniper_mp");
            killstreakWieldWeapons.Add("iw5_m60jugg_mp");
            killstreakWieldWeapons.Add("iw5_mp412jugg_mp");
            killstreakWieldWeapons.Add("iw5_riotshieldjugg_mp");
            killstreakWieldWeapons.Add("iw5_usp45jugg_mp");
            killstreakWieldWeapons.Add("remote_turret_mp");
            killstreakWieldWeapons.Add("osprey_player_minigun_mp");
            killstreakWieldWeapons.Add("deployable_vest_marker_mp");
            killstreakWieldWeapons.Add("ugv_turret_mp");
            killstreakWieldWeapons.Add("ugv_gl_turret_mp");
            killstreakWieldWeapons.Add("remote_tank_projectile_mp");
            killstreakWieldWeapons.Add("uav_remote_mp");
            level.SetField("killstreakWeapons", new Parameter(killstreakWieldWeapons));

            Dictionary<string, string> killstreakChainingWeapons = new Dictionary<string, string>();
            killstreakChainingWeapons.Add("remotemissile_projectile_mp", "predator_missile");
            killstreakChainingWeapons.Add("ims_projectile_mp", "ims");
            killstreakChainingWeapons.Add("sentry_minigun_mp", "airdrop_sentry_minigun");
            killstreakChainingWeapons.Add("artillery_mp", "precision_airstrike");
            killstreakChainingWeapons.Add("cobra_20mm_mp", "helicopter");
            killstreakChainingWeapons.Add("apache_minigun_mp", "littlebird_flock");
            killstreakChainingWeapons.Add("littlebird_guard_minigun_mp", "littlebird_support");
            killstreakChainingWeapons.Add("remote_mortar_missile_mp", "remote_mortar");
            killstreakChainingWeapons.Add("ugv_turret_mp", "airdrop_remote_tank");
            killstreakChainingWeapons.Add("ugv_gl_turret_mp", "airdrop_remote_tank");
            killstreakChainingWeapons.Add("remote_tank_projectile_mp", "airdrop_remote_tank");
            killstreakChainingWeapons.Add("pavelow_minigun_mp", "helicopter_flares");
            killstreakChainingWeapons.Add("ac130_105mm_mp", "ac130");
            killstreakChainingWeapons.Add("ac130_40mm_mp", "ac130");
            killstreakChainingWeapons.Add("ac130_25mm_mp", "ac130");
            killstreakChainingWeapons.Add("iw5_m60jugg_mp", "airdrop_juggernaut");
            killstreakChainingWeapons.Add("iw5_mp412jugg_mp", "airdrop_juggernaut");
            killstreakChainingWeapons.Add("osprey_player_minigun_mp", "osprey_gunner");
            level.SetField("killstreakChainingWeapons", new Parameter(killstreakChainingWeapons));

            AfterDelay(50, () =>
            {
                killstreakFuncs.Add("nuke", nuke.nukeGiveFunc);
                killstreakFuncs.Add("predator_missile", remotemissile.missileGiveFunc);
                killstreakFuncs.Add("deployable_vest", deployablebox.deployableVestGiveFunc);
                killstreakFuncs.Add("uav", uav.uavGiveFunc);
                //killstreakFuncs.Add("uav_2", uav.uav2GiveFunc);
                killstreakFuncs.Add("uav_support", uav.uavSupportGiveFunc);
                killstreakFuncs.Add("double_uav", uav.uavDoubleGiveFunc);
                killstreakFuncs.Add("triple_uav", uav.uavTripleGiveFunc);
                killstreakFuncs.Add("counter_uav", uav.uavCounterGiveFunc);
                killstreakFuncs.Add("uav_strike", uav.uavStrikeGiveFunc);
                killstreakFuncs.Add("directional_uav", uav.uavDirectionalGiveFunc);

                killstreakFuncs.Add("aastrike", aastrike.aaStrikeGiveFunc);
                killstreakFuncs.Add("aamissile", aamissile.aaMissileGiveFunc);
                killstreakFuncs.Add("teamammorefill", teamammorefill.teamAmmoRefillGiveFunc);
                killstreakFuncs.Add("deployable_ammo", deployablebox.deployableAmmoGiveFunc);

                killstreakUseFuncs.Add("uav", uav.uavUseFunc);
                //killstreakUseFuncs.Add("uav_2", uav.uav2UseFunc);
                killstreakUseFuncs.Add("uav_support", uav.uavSupportUseFunc);
                killstreakUseFuncs.Add("double_uav", uav.uavDoubleUseFunc);
                killstreakUseFuncs.Add("triple_uav", uav.uavTripleUseFunc);
                killstreakUseFuncs.Add("counter_uav", uav.uavCounterUseFunc);
                killstreakUseFuncs.Add("uav_strike", uav.uavStrikeUseFunc);
                killstreakUseFuncs.Add("directional_uav", uav.uavDirectionalUseFunc);
                killstreakUseFuncs.Add("nuke", nuke.nukeUseFunc);
                killstreakUseFuncs.Add("predator_missile", remotemissile.missileUseFunc);
                killstreakUseFuncs.Add("deployable_vest", deployablebox.deployableVestUseFunc);

                killstreakUseFuncs.Add("aastrike", aastrike.aaStrikeUseFunc);
                killstreakUseFuncs.Add("aamissile", aamissile.aaMissileUseFunc);
                killstreakUseFuncs.Add("teamammorefill", teamammorefill.teamAmmoRefillUseFunc);
                killstreakUseFuncs.Add("deployable_ammo", deployablebox.deployableAmmoUseFunc);
            });

            //level.SetField("killstreakRoundDelay", GetDvar("scr_game_killstreakdelay"));

            //Setup dicts
            otherTeam["allies"] = "axis";
            otherTeam["axis"] = "allies";
            otherTeam["none"] = "none";

            teamAirDenied["allies"] = false;
            teamAirDenied["axis"] = false;
            teamAirDenied["none"] = false;

            //Hack to route notifies to level
            //Notified += notify_globalToLevel;

            voice["allies"] = getTeamVoicePrefix("allies");
            voice["axis"] = getTeamVoicePrefix("axis");

            PlayerConnected += onPlayerConnect;
        }

        public void onPlayerConnect(Entity player)
        {
            player.SetField("killstreak", 0);
            player.SetField("currentlySelectedClass", -1);
            player.SetField("lastStreakSlotUsed", 0);
            player.SetField("hasChangedClass", false);
            player.OnNotify("menuresponse", (p, menu, selection) => 
            {
                if ((string)menu == "changeclass")
                {
                    string classSelection = (string)selection;
                    if (classSelection.StartsWith("custom"))
                    {
                        classSelection = classSelection.Substring(6);
                        player.SetField("currentlySelectedClass", int.Parse(classSelection) - 1);
                        player.SetField("hasChangedClass", true);
                    }
                }
            });
            player.OnNotify("weapon_switch_started", (p, weapon) =>  killstreakUseWaiter(player, (string)weapon));
            for (int i = 1; i < 5; i++)
            {
                int slot = i;
                player.OnNotify("streakUsed" + slot, (p) =>
                {
                    player.SetField("lastStreakSlotUsed", slot-1);
                });
            }
            player.SpawnedPlayer += () => onPlayerSpawned(player);

            player.NotifyOnPlayerCommand("trigger_ks", "+activate");


            //Hit feedback for certain streaks
            HudElem hitFeedback = NewClientHudElem(player);
            hitFeedback.HorzAlign = HudElem.HorzAlignments.Center;
            hitFeedback.VertAlign = HudElem.VertAlignments.Middle;
            hitFeedback.X = -12;
            hitFeedback.Y = -12;
            hitFeedback.Archived = false;
            hitFeedback.HideWhenDead = false;
            hitFeedback.Sort = 0;
            hitFeedback.Alpha = 0;
            player.SetField("hud_hitFeedback", hitFeedback);
        }

        public void onPlayerSpawned(Entity player)
        {
            AfterDelay(50, () =>
            {
                player.SetField("lastDroppableWeapon", player.CurrentWeapon);
                if (player.GetField<int>("currentlySelectedClass") == -1) return;//Default classes are not supported
                StartAsync(waitForChangeTeam(player));

                if (!player.HasField("pers_killstreaks") || player.GetField<bool>("hasChangedClass"))
                    initPlayerKillstreaks(player);

                if (player.GetField<string>("streaktype") != "support")
                    player.SetField("killstreak", 0);

                setStreakCountToNext(player);
                updateStreakSlots(player);

                restoreOwnedKillstreakItems(player);
            });
        }
        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            if (attacker == player) return;

            attacker.SetField("killstreak", attacker.GetField<int>("killstreak") + 1);
            checkForKillstreaks(attacker);
            updateStreakSlots(attacker);
        }
        public override void OnSay(Entity player, string name, string message)
        {
            if (message == "kill")
            {
                player.SetField("killstreak", player.GetField<int>("killstreak") + 1);
                checkForKillstreaks(player);
                updateStreakSlots(player);
            }
            if (message.StartsWith("giveStreak "))
            {
                giveKillstreak(player, message.Split(' ')[1], KILLSTREAK_GIMME_SLOT);
            }
            if (message == "listGimme")
            {
                for (int i = 0; i < player.GetField<List<string>>("pers_killstreaks_gimmeSlot").Count; i++)
                {
                    Utilities.PrintToConsole("Slot " + i + " is " + player.GetField<List<string>>("pers_killstreaks_gimmeSlot")[i]);
                }
            }
        }

        private static void initPlayerKillstreaks(Entity player)
        {
            string[] killstreakList = new string[3];
            int selectedClass = player.GetField<int>("currentlySelectedClass");
            string selectedStreaktype = (string)player.GetPlayerData("customClasses", selectedClass, "perks", 5);

            string streaktype = "assault";

            switch (selectedStreaktype)
            {
                case "streaktype_support":
                    streaktype = "support";
                    break;
                case "streaktype_specialist":
                    streaktype = "custom";
                    break;
                default:
                    streaktype = "assault";
                    break;
            }
            player.SetField("streaktype", streaktype);

            if (streaktype != "custom")
            {
                for (int i = 1; i <= killstreakList.Length; i++)
                {
                    killstreakList[i-1] = (string)player.GetPlayerData("customClasses", selectedClass, streaktype + "Streaks", i-1);

                    player.SetPlayerData("killstreaksState", "icons", i, getKillstreakIndex(killstreakList[i-1]));
                }
            }
            else
            {
                string[] perkStreaks = new string[3];
                for (int i = 1; i <= killstreakList.Length; i++)
                {
                    perkStreaks[i-1] = (string)player.GetPlayerData("customClasses", selectedClass, "specialistStreaks", i-1);
                    killstreakList[i-1] = getCustomKillstreakName(perkStreaks[i-1]);
                    //Log.Debug("Replaced ks {0} with {1}", perkStreaks[i-1], killstreakList[i-1]);

                    player.SetPlayerData("killstreaksState", "icons", i, getKillstreakIndex(killstreakList[i-1]));
                }
            }
            //Hack to sort none streaks at the top so we dont have them before real streaks
            int streak1Cost = killstreakList[0] == "none" ? int.MaxValue : getKillstreakCost(killstreakList[0]);
            int streak2Cost = killstreakList[1] == "none" ? int.MaxValue : getKillstreakCost(killstreakList[1]);
            int streak3Cost = killstreakList[2] == "none" ? int.MaxValue : getKillstreakCost(killstreakList[2]);
            int[] streakCosts = new int[3] { streak1Cost, streak2Cost, streak3Cost };
            Array.Sort(streakCosts, killstreakList);

            InfinityScript.Log.Debug(string.Join(", ", killstreakList));

            player.SetField("pers_killstreaks", new Parameter(killstreakList));

            if (!player.HasField("pers_killstreaks_gimmeSlot"))
            {
                List<string> gimmeSlot = new List<string>();
                player.SetField("pers_killstreaks_gimmeSlot", new Parameter(gimmeSlot));
            }

            player.SetField("hasChangedClass", false);
        }

        private static void giveKillstreak(Entity player, string streak, int slot)
        {
            if (!killstreakFuncs.ContainsKey(streak)) return;

            //killstreakFuncs[streak]?.Invoke(player);
            if (slot == 0) addStreakToGimmeSlot(player, streak);

            doKillstreakSplash(player, getPlayerKillstreak(player, slot));
            string ksVoice = getKillstreakVoice(getPlayerKillstreak(player, slot));
            if (ksVoice != "null") player.PlayLocalSound(getTeamVoicePrefix(player.SessionTeam) + "1mc_" + ksVoice);

            player.SetPlayerData("killstreaksState", "hasStreak", slot, true);
            if (slot == 0) player.SetPlayerData("killstreaksState", "icons", slot, getKillstreakIndex(getPlayerKillstreak(player, 0)));
            else player.SetPlayerData("killstreaksState", "icons", slot, getKillstreakIndex(streak));

            giveOwnedKillstreakItem(player, slot);
        }
        private static void addStreakToGimmeSlot(Entity player, string streak)
        {
            List<string> gimmeSlot = player.GetField<List<string>>("pers_killstreaks_gimmeSlot");
            gimmeSlot.Add(streak);
            player.SetField("pers_killstreaks_gimmeSlot", new Parameter(gimmeSlot));
        }
        private static void usedStreakInGimmeSlot(Entity player, string streak)
        {
            List<string> gimmeSlot = player.GetField<List<string>>("pers_killstreaks_gimmeSlot");
            gimmeSlot.Remove(streak);
            player.SetField("pers_killstreaks_gimmeSlot", new Parameter(gimmeSlot));

            AfterDelay(100, () => updateStreakSlots(player));
        }
        private static void restoreOwnedKillstreakItems(Entity player)
        {
            //give whatever is in our gimme slot or any still aquired streaks
            for (int i = 0; i < KILLSTREAK_ALL_PERKS_SLOT; i++)
            {
                if (!playerHasKillstreak(player, i))
                    continue;

                string streak = getPlayerKillstreak(player, i);

                if (streak == "none" || streak == "")
                    continue;

                player.GiveWeapon(getKillstreakWieldWeapon(streak), 0, false);
                player.SetActionSlot(4 + i, "weapon", getKillstreakWieldWeapon(streak));
            }
        }
        private static void giveOwnedKillstreakItem(Entity player, int slot)
        {
            if (!playerHasKillstreak(player, slot))
                return;

            string streak = getPlayerKillstreak(player, slot);

            if (streak == "none" || streak == "")
                return;

            player.GiveWeapon(getKillstreakWieldWeapon(streak), 0, false);
            player.SetActionSlot(4 + slot, "weapon", getKillstreakWieldWeapon(streak));
        }

        private static void checkForKillstreaks(Entity player)
        {
            int killstreak = player.GetField<int>("killstreak");

            int firstCount = getKillstreakCost(getPlayerKillstreak(player, 1)); 

            if (killstreak == firstCount)
            {
                doKillstreakSplash(player, getPlayerKillstreak(player, 1));
                string ksVoice = getKillstreakVoice(getPlayerKillstreak(player, 1));
                if (ksVoice != "null") player.PlayLocalSound(getTeamVoicePrefix(player.SessionTeam) + "1mc_" + ksVoice);
                giveKillstreak(player, getPlayerKillstreak(player, 1), 1);
                return;
            }
            int secondCount = getKillstreakCost(getPlayerKillstreak(player, 2));
            if (killstreak == secondCount)
            {
                doKillstreakSplash(player, getPlayerKillstreak(player, 2));
                string ksVoice = getKillstreakVoice(getPlayerKillstreak(player, 2));
                if (ksVoice != "null") player.PlayLocalSound(getTeamVoicePrefix(player.SessionTeam) + "1mc_" + ksVoice);
                giveKillstreak(player, getPlayerKillstreak(player, 2), 2);
                return;
            }
            int thirdCount = getKillstreakCost(getPlayerKillstreak(player, 3));
            if (killstreak == thirdCount)
            {
                doKillstreakSplash(player, getPlayerKillstreak(player, 3));
                string ksVoice = getKillstreakVoice(getPlayerKillstreak(player, 3));
                if (ksVoice != "null") player.PlayLocalSound(getTeamVoicePrefix(player.SessionTeam) + "1mc_" + ksVoice);
                giveKillstreak(player, getPlayerKillstreak(player, 3), 3);
            }
        }

        private static void killstreakUseWaiter(Entity player, string weapon)
        {
            if (mayDropWeapon(weapon))
                player.SetField("lastDroppableWeapon", weapon);

            if (killstreakWeaponIsDelayed(player.CurrentWeapon) && weapon != player.CurrentWeapon)
            {
                player.SetField("cancelledKillstreak", true);
                player.Notify("cancelled_use_killstreak");
                return;
            }

            if (player.HasField("usingRemote") || !mayDropWeapon(player.CurrentWeapon)) return;

            int lastSlotUsed = player.GetField<int>("lastStreakSlotUsed");
            if (!(bool)player.GetPlayerData("killstreaksState", "hasStreak", lastSlotUsed)) return;
            string expectedWeapon = getKillstreakWieldWeapon(getPlayerKillstreak(player, lastSlotUsed));
            InfinityScript.Log.Debug("Last slot is {0} and weapon for it is {1}", lastSlotUsed, expectedWeapon);
            if (weapon != expectedWeapon) return;

            OnInterval(50, () =>
            {
                if (player.CurrentWeapon == expectedWeapon)
                {
                    if (!killstreakUseFuncs.ContainsKey(getPlayerKillstreak(player, lastSlotUsed)))
                    {
                        InfinityScript.Log.Write(LogLevel.Error, "Killstreak {0} does not exist in the use function table!", getPlayerKillstreak(player, lastSlotUsed));
                        player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                        return false;
                    }
                    if (!killstreakUseFuncs[getPlayerKillstreak(player, lastSlotUsed)].Invoke(player))
                    {
                        InfinityScript.Log.Write(LogLevel.Info, "Killstreak {1} failed to be called in for {0}!", player.Name, getPlayerKillstreak(player, lastSlotUsed));
                        player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                        return false;
                    }

                    if (!killstreakWeaponIsDelayed(expectedWeapon))
                    {
                        player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                        AfterDelay(1000, () => player.TakeWeapon(expectedWeapon));
                    }
                    else if (killstreakWeaponIsDelayed(expectedWeapon))
                    {
                        StartAsync(killstreakCallWaiter(player, expectedWeapon, lastSlotUsed));
                        return false;
                    }

                    player.SetPlayerData("killstreaksState", "hasStreak", lastSlotUsed, false);
                    if (lastSlotUsed == 0)
                        usedStreakInGimmeSlot(player, getPlayerKillstreak(player, 0));

                    return false;
                }
                if (!player.IsPlayer || !player.IsAlive) return false;
                return true;
            });

        }
        private static IEnumerator killstreakCallWaiter(Entity player, string weapon, int slot)
        {
            if (!weaponIsRideKillstreak(weapon))
            {
                string result = "";
                yield return player.WaitTill_any_return(new Action<string>((s) => result = s), "used_killstreak", "cancel_use_killstreak");

                if (result == "cancel_use_killstreak")
                    yield break;

                player.SetPlayerData("killstreaksState", "hasStreak", slot, false);
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                AfterDelay(1000, () => player.TakeWeapon(weapon));
                if (slot == 0)
                    usedStreakInGimmeSlot(player, getPlayerKillstreak(player, 0));
            }
            else
            {
                yield return player.WaitTill("stopped_using_remote");

                player.SetPlayerData("killstreaksState", "hasStreak", slot, false);
                player.SwitchToWeapon(player.GetField<string>("lastDroppableWeapon"));
                AfterDelay(1000, () => player.TakeWeapon(weapon));
                if (slot == 0)
                    usedStreakInGimmeSlot(player, getPlayerKillstreak(player, 0));
            }

            yield break;
        }

        private static void setStreakCountToNext(Entity player)
        {
            int killstreak = player.GetField<int>("killstreak");

            player.SetPlayerData("killstreaksState", "count", killstreak);

            int firstCount = getKillstreakCost(getPlayerKillstreak(player, 1));
            int secondCount = getKillstreakCost(getPlayerKillstreak(player, 2));
            int thirdCount = getKillstreakCost(getPlayerKillstreak(player, 3));

            if (killstreak < firstCount)
                player.SetPlayerData("killstreaksState", "countToNext", firstCount);
            else if (killstreak < secondCount)
                player.SetPlayerData("killstreaksState", "countToNext", secondCount);
            else if (killstreak < thirdCount)
                player.SetPlayerData("killstreaksState", "countToNext", thirdCount);
            else if (killstreak == thirdCount)
            {
                player.SetField("killstreak", 0);
                player.SetPlayerData("killstreaksState", "count", killstreak);
                player.SetPlayerData("killstreaksState", "countToNext", firstCount);
            }
        }
        private static void updateStreakSlots(Entity player)
        {
            int killstreak = player.GetField<int>("killstreak");
            int currentCount = (int)player.GetPlayerData("killstreaksState", "count");

            if (killstreak != currentCount)
            {
                player.SetPlayerData("killstreaksState", "count", killstreak);
                setStreakCountToNext(player);
            }

            //Update gimme slot
            List<string> gimmeSlot = player.GetField<List<string>>("pers_killstreaks_gimmeSlot");
            if (gimmeSlot.Count > 0)
            {
                player.SetPlayerData("killstreaksState", "hasStreak", 0, true);
                player.SetPlayerData("killstreaksState", "icons", 0, getKillstreakIndex(gimmeSlot[0]));
                giveOwnedKillstreakItem(player, 0);
            }
            else player.SetPlayerData("killstreaksState", "hasStreak", 0, false);
        }

        public static void giveJuggLoadout(Entity player, string team, string juggType)
        {
            player.TakeAllWeapons();
            player.ClearPerks();
        }

        private static IEnumerator waitForChangeTeam(Entity player)
        {
            yield return 0;
        }

        public string getPerkUpgrade(string perkName)
        {
            string perkUpgrade = TableLookup("mp/perktable.csv", 1, perkName, 8);

            if (perkName == "" || perkName == "specialty_null")
                return "specialty_null";

            return perkUpgrade;
        }

        private static int getKillstreakIndex(string streakName)
        {
            int ret = 0;

            if (isCustomStreak(streakName))
                return ret;

            ret = int.Parse(TableLookup(KILLSTREAK_STRING_TABLE, 1, streakName, 0));
            //ret = TableLookupRowNum(KILLSTREAK_STRING_TABLE, 1, streakName) - 1;

            return ret;
        }
        public static int? getKillstreakIndexForPlayer(Entity player, string streakname)
        {
            int? ret = null;
            string[] killstreaks = player.GetField<string[]>("pers_killstreaks");

            for (int i = 0; i < 3; i++)
            {
                if (killstreaks[i] == streakname)
                {
                    ret = i;
                    break;
                }
            }

            return ret;
        }
        public static string getPlayerKillstreak(Entity player, int slot)
        {
            if (slot == 0)
            {
                if (player.GetField<List<string>>("pers_killstreaks_gimmeSlot").Count == 0)
                    return "none";
                return player.GetField<List<string>>("pers_killstreaks_gimmeSlot")[0];
            }
            if (slot < 1 || slot > 3)
                return "none";
            return player.GetField<string[]>("pers_killstreaks")[slot-1];
        }
        public static bool playerHasKillstreak(Entity player, int slot)
        {
            return (bool)player.GetPlayerData("killstreaksState", "hasStreak", slot);
        }
        private static string getKillstreakIcon(string streakName)
        {
            string ret = "";
            ret = TableLookup(KILLSTREAK_STRING_TABLE, 1, streakName, 16);

            return ret;
        }
        public static string getKillstreakCrateIcon(string streakName)
        {
            if (streakName == "deployable_ammo")
                return "waypoint_ammo_friendly";

            string ret = "";
            ret = TableLookup(KILLSTREAK_STRING_TABLE, KILLSTREAK_NAME_COLUMN, streakName, KILLSTREAK_OVERHEAD_ICON_COLUMN);

            return ret;
        }
        private static int getKillstreakCost(string streakName)
        {
            int ret = 0;
            if (int.TryParse(TableLookup(KILLSTREAK_STRING_TABLE, 1, streakName, 4), out ret))
                return ret;
            else return 0;
        }
        private static string getKillstreakWieldWeapon(string streak)
        {
            string ret = "";
            if (streak == "none") return ret;

            if (isCustomStreak(streak))
            {
                if (streak == "aastrike")
                    return "killstreak_double_uav_mp";
                if (streak == "aamissile")
                    return "killstreak_predator_missile_mp";
                if (streak == "teamammorefill")
                    return "killstreak_uav_mp";
                if (streak == "double_uav")
                    return "killstreak_uav_mp";
                if (streak == "directional_uav")
                    return "killstreak_uav_mp";
                if (streak == "uav_strike")
                    return "uav_strike_marker_mp";
                if (streak == "deployable_ammo")
                    return "deployable_vest_marker_mp";
            }

            ret = TableLookup(KILLSTREAK_STRING_TABLE, 1, streak, 12);

            return ret;
        }
        private static bool isCustomStreak(string streak)
        {
            switch (streak)
            {
                case "aastrike":
                case "teamammorefill":
                case "uav_strike":
                case "aamissile":
                case "double_uav":
                case "directional_uav":
                case "deployable_ammo":
                    return true;
                default:
                    return false;
            }
        }
        private static string getCustomKillstreakName(string oldStreak)
        {
            string ret = "";

            switch (oldStreak)
            {
                case "specialty_longersprint_ks":
                    ret = "nuke";
                    break;
                default:
                    ret = "none";
                    break;
            }

            return ret;
        }

        public static void removeLightArmor(Entity player, int maxHealth = -1)
        {
            if (maxHealth != -1)
                player.MaxHealth = maxHealth;
            player.Health = player.MaxHealth;

            if (player.HasField("combatHighOverlay"))
                player.GetField<HudElem>("combatHighOverlay").Destroy();

            player.ClearField("hasLightArmor");

            player.Notify("remove_light_armor");
        }
        public static void giveLightArmor(Entity player)
        {
            player.MaxHealth = 150;
            player.Health = player.MaxHealth;

            if (!player.HasField("combatHighOverlay"))
                createCombatHighOverlay(player);

            player.SetField("hasLightArmor", true);

            watchLightArmor(player);

            player.Notify("equip_light_armor");
        }
        private static void watchLightArmor(Entity player)
        {
            OnInterval(50, () =>
            {
                if (player.Health < 100 || !player.IsAlive)
                {
                    removeLightArmor(player, 100);
                    return false;
                }
                return true;
            });
        }

        public static void refillAmmo(Entity player, bool refillEquipment)
        {
            List<string> weaponsList = new List<string>();
            bool foundBoth = false;
            //Populate this by ourselves
            weaponsList.Add(player.GetCurrentPrimaryWeapon());
            weaponsList.Add("alt_" + player.GetCurrentPrimaryWeapon());
            if (!weaponsList.Contains(player.CurrentWeapon))
            {
                weaponsList.Add(player.CurrentWeapon);
                weaponsList.Add("alt_" + player.CurrentWeapon);
                foundBoth = true;
            }
            if (!foundBoth)//Havent found both equipped guns, try to track it
            {
                StartAsync(refillSecondaryAmmo(player));
            }
            //Add all equipment manually
            if (refillEquipment)
            {
                weaponsList.Add("frag_grenade_mp");
                weaponsList.Add("semtex_mp");
                weaponsList.Add("throwingknife_mp");
                weaponsList.Add("c4_mp");
                weaponsList.Add("claymore_mp");
                weaponsList.Add("bouncingbetty_mp");
                weaponsList.Add(player.GetCurrentOffhand());
                weaponsList.Add("flash_grenade_mp");
                weaponsList.Add("concussion_grenade_mp");
                weaponsList.Add("smoke_grenade_mp");
                weaponsList.Add("emp_grenade_mp");
                weaponsList.Add("trophy_mp");
            }

            if (refillEquipment)
            {
                if (player.HasPerk("specialty_tacticalinsertion") && player.GetAmmoCount("flare_mp") < 1)
                {
                    player.SetPerk("specialty_tacticalinsertion", false);
                    player.GiveMaxAmmo("flare_mp");
                }
                if (player.HasPerk("specialty_scrambler") && player.GetAmmoCount("scrambler_mp") < 1)
                {
                    player.SetPerk("specialty_scrambler", false);
                    player.GiveMaxAmmo("scrambler_mp");
                }
                if (player.HasPerk("specialty_portable_radar") && player.GetAmmoCount("portable_radar_mp") < 1)
                {
                    player.SetPerk("specialty_portable_radar", false);
                    player.GiveMaxAmmo("portable_radar_mp");
                }
            }

            foreach (string weaponName in weaponsList)
            {
                if (IsSubStr(weaponName, "grenade") || (GetSubStr(weaponName, 0, 3) == "alt"))
                {
                    if (!refillEquipment || player.GetAmmoCount(weaponName) >= 1)
                        continue;
                }

                player.GiveMaxAmmo(weaponName);
            }
        }
        private static IEnumerator refillSecondaryAmmo(Entity player)
        {
            yield return player.WaitTill_any("weapon_change", "joined_team", "joined_spectators", "disconnect", "death");

            if (!player.IsAlive || player.Classname != "player") yield break;

            player.GiveMaxAmmo(player.CurrentWeapon);
            player.GiveMaxAmmo("alt_" + player.CurrentWeapon);
        }

        public static void updateKillstreaks(Entity player, bool updateAll)
        {

        }

        public static void clearKillstreaks(Entity player)
        {

        }

        public static void teamPlayerCardSplash(string splash, Entity owner, bool friendlyOnly = false)
        {
            foreach (Entity players in Players)
            {
                if (!players.IsPlayer) continue;
                players.SetCardDisplaySlot(owner, 5);
                players.ShowHudSplash(splash, 1);
            }
        }
        public static void leaderDialog(string sound, string team = "", List<Entity> excludeList = null)
        {
            foreach (Entity player in Players)
            {
                if (player.Classname != "player")
                    continue;

                if (team != "" && player.SessionTeam != team)
                    continue;

                if (excludeList != null)
                {
                    if (!excludeList.Contains(player))
                        player.PlayLocalSound(sound);
                }
                else
                    player.PlayLocalSound(sound);
            }
        }
        public static string getTeamVoicePrefix(string team)
        {
            string ret;
            if (isTeamBased && team == "allies")
            {
                string allies = GetMapCustom("allieschar");
                ret = TableLookup("mp/factiontable.csv", 0, allies, 7);
            }
            else if (isTeamBased && team == "axis")
            {
                string axis = GetMapCustom("axischar");
                ret = TableLookup("mp/factiontable.csv", 0, axis, 7);
            }
            else ret = "US_";
            return ret;
        }
        public static string getKillstreakVoice(string streak)
        {
            string ret = "null";
            if (streak == "none") return ret;

            ret = TableLookup(KILLSTREAK_STRING_TABLE, 1, streak, 8);

            return ret;
        }
        public static void doKillstreakSplash(Entity player, string splash)
        {
            player.ShowHudSplash(splash, 0, player.GetField<int>("killstreak"));
        }
        public static bool mayDropWeapon(string weapon)
        {
            if (weapon == "none")
                return false;

            if (weapon.Contains("ac130"))
                return false;

            if (weapon.Contains("killstreak"))
                return false;

            if (weapon.Contains("marker"))
                return false;

            string invType = WeaponInventoryType(weapon);
            if (invType != "primary")
                return false;

            return true;
        }
        public static bool weaponIsRideKillstreak(string weapon)
        {
            switch (weapon)
            {
                case "killstreak_ac130_mp":
                case "killstreak_predator_missile_mp":
                case "killstreak_helicopter_minigun_mp":
                case "killstreak_remote_tank_laptop_mp":
                case "killstreak_remote_turret_laptop_mp":
                case "killstreak_remote_mortar_mp":
                    return true;
                default: return false;
            }
        }
        public static bool killstreakWeaponIsDelayed(string weapon)
        {
            if (weapon.Contains("marker")) return true;

            switch (weapon)
            {
                case "killstreak_ac130_mp":
                case "killstreak_predator_missile_mp":
                case "killstreak_helicopter_minigun_mp":
                case "killstreak_precision_airstrike_mp":
                case "killstreak_sentry_mp":
                case "killstreak_stealth_airstrike_mp":
                case "killstreak_ims_mp":
                case "killstreak_remote_tank_mp":
                case "killstreak_remote_tank_laptop_mp":
                case "killstreak_remote_turret_mp":
                case "killstreak_remote_turret_laptop_mp":
                case "killstreak_remote_mortar_mp":
                case "killstreak_remote_uav_mp":
                    return true;
                default: return false;
            }
        }

        public static void updateMoveSpeedScale(Entity player)
        {

        }
        public static IEnumerator initRideKillstreak(Entity player, string streak = "", Action<string> result = null)
        {
            string laptopWait = "";
            if (streak != "" && (streak == "osprey_gunner" || streak == "remote_uav" || streak == "remote_tank"))
                laptopWait = "timeout";
            else yield return player.WaitTill_notify_or_timeout("weapon_switch_started", 1, new Action<string>((s) => laptopWait = s));

            if (laptopWait == "weapon_switch_started")
            {
                result?.Invoke("fail");
                yield return null;
            }

            if (!player.IsAlive || player.Classname != "player")
            {
                result?.Invoke("fail");
                yield return null;
            }

            if (player.GetField<bool>("nuked") || player.GetField<bool>("isEMPed") || isAirDenied(player))
            {
                result?.Invoke("fail");
                yield return null;
            }

            player.VisionSetNakedForPlayer("black_bw", 0.75f);
            string blackoutWait = "timeout";
            yield return player.WaitTill_notify_or_timeout("death", 1, new Action<string>((s) => blackoutWait = s));

            //Utilities.PrintToConsole(blackoutWait);

            if (blackoutWait != "death")
            {
                StartAsync(clearRideIntro(player, 1.0f));

                if (player.SessionTeam == "spectator")
                {
                    result?.Invoke("fail");
                    yield return null;
                }

                if (player.Classname != "player")
                {
                    result?.Invoke("disconnect");
                    yield return null;
                }
            }

            if (player.IsOnLadder())
            {
                result?.Invoke("fail");
                yield return null;
            }

            if (!player.IsAlive)
            {
                result?.Invoke("fail");
                yield return null;
            }

            if (player.GetField<bool>("nuked") || player.GetField<bool>("isEMPed") || isAirDenied(player))
            {
                result?.Invoke("fail");
                yield return null;
            }

            if (player.Classname != "player")
            {
                result?.Invoke("disconnect");
                yield return null;
            }

            if (blackoutWait != "death")
                    result?.Invoke("success");
        }
        private static IEnumerator clearRideIntro(Entity player, float delay = 0f)
        {
            if (delay != 0f) yield return Wait(delay);

            if (level.HasField("nukeDetonated"))
                player.VisionSetNakedForPlayer("aftermath", 0);
            else
                player.VisionSetNakedForPlayer("", 0);
        }
        public static bool isAirDenied(Entity player)
        {
            //Check airspace and return false if needed
            return false;
        }
        public static bool isJuggernaut(Entity player)
        {
            if (player.HasField("isJuggernaut") || player.HasField("isJuggernautRecon"))
                return true;

            return false;
        }
        public static void setUsingRemote(Entity player, string remote)
        {
            player.DisableOffhandWeapons();
            player.SetField("usingRemote", remote);
            player.Notify("using_remote", remote);
        }
        public static void clearUsingRemote(Entity player, string remote)
        {
            player.ClearField("usingRemote");
            player.FreezeControls(false);
            player.EnableOffhandWeapons();
            player.Notify("stopped_using_remote", remote);
        }
        public static bool isUsingRemote(Entity player)
        {
            return player.HasField("usingRemote");
        }
        public static bool validateUseStreak(Entity player)
        {
            if (player.HasField("lastStand") && !player.HasPerk("specialty_finalstand"))
            {
                player.IPrintLnBold("Killstreak cannot be used in Last Stand.");
                return false;
            }
            else if (level.GetField<bool>("civilianJetFlyBy"))
            {
                player.IPrintLnBold("Civilian air traffic in area.");
                return false;
            }
            else if (isUsingRemote(player))
            {
                return false;
            }
            /*
            else if (isEMPed(player))
            {
                return false;
            }
            */
            else
                return true;
        }
        public static List<Entity> getLevelSpawnpoints(string type)
        {
            List<Entity> ret = new List<Entity>();
            for (int i = 0; i < 1000; i++)
            {
                Entity e = Entity.GetEntity(i);
                if (e == null) continue;
                if (e.Classname == "mp_" + type + "_spawn")
                {
                    ret.Add(e);
                }
                else continue;
            }
            return ret;
        }
        public static HudElem createPrimaryProgressBar(Entity player, int xOffset, int yOffset, string text = "")
        {
            HudElem progressBar = HudElem.CreateIcon(player, "progress_bar_fill", 0, 9);//NewClientHudElem(player);
            progressBar.SetField("frac", 0);
            progressBar.Color = new Vector3(1, 1, 1);
            progressBar.Sort = -2;
            progressBar.Shader = "progress_bar_fill";
            progressBar.SetShader("progress_bar_fill", 1, 9);
            progressBar.Alpha = 1;
            progressBar.SetPoint("center", "", 0, -61);
            progressBar.AlignX = HudElem.XAlignments.Left;
            progressBar.X = -60;

            HudElem progressBarBG = HudElem.CreateIcon(player, "progress_bar_bg", 124, 13);//NewClientHudElem(player);
            progressBarBG.SetPoint("center", "", 0, -61);
            progressBarBG.SetField("bar", progressBar);
            progressBarBG.Sort = -3;
            progressBarBG.Color = new Vector3(0, 0, 0);
            progressBarBG.Alpha = .5f;

            if (text != "")
            {
                HudElem progressBarText = HudElem.CreateFontString(player, HudElem.Fonts.HudBig, .6f);//NewClientHudElem(player);
                progressBarText.Parent = progressBarBG;
                progressBarText.SetPoint("center", "center", 0, -10);
                progressBarText.Sort = -1;
                progressBarText.SetText(text);
            }

            return progressBarBG;
        }

        public static void updateBar(HudElem barBG, int barFrac, float rateOfChange)
        {
            //int barWidth = (int)(barBG.Width * barFrac + .5f);

            //if (barWidth == null)
            //barWidth = 1;

            HudElem bar = (HudElem)barBG.GetField("bar");
            bar.SetField("frac", barFrac);
            //bar.SetShader("progress_bar_fill", barWidth, barBG.Height);

            if (rateOfChange > 0)
                bar.ScaleOverTime(rateOfChange, barFrac, bar.Height);
            else if (rateOfChange < 0)
                bar.ScaleOverTime(-1 * rateOfChange, barFrac, bar.Height);

            //bar.SetField("rateOfChange", rateOfChange);
            //int time = GetTime();
            //bar.SetField("lastUpdateTime", time);
        }
        public static void destroyPrimaryProgressBar(HudElem barBG)
        {
            HudElem bar = (HudElem)barBG.GetField("bar");
            HudElem text = null;
            if (barBG.Children.Count > 0)
            {
                 text = barBG.Children[0];
            }

            bar.Destroy();
            if (text != null) text.Destroy();
            barBG.Destroy();
        }
        public static HudElem createCombatHighOverlay(Entity player)
        {
            HudElem icon = NewClientHudElem(player);
            icon.X = 0;
            icon.Y = 0;
            icon.AlignX = HudElem.XAlignments.Left;
            icon.AlignY = HudElem.YAlignments.Top;
            icon.HorzAlign = HudElem.HorzAlignments.Fullscreen;
            icon.VertAlign = HudElem.VertAlignments.Fullscreen;
            icon.SetShader("combathigh_overlay", 640, 480);
            icon.Sort = -10;
            //icon.Archived = true;
            icon.HideWhenInMenu = false;
            icon.HideIn3rdPerson = true;
            icon.Foreground = false;
            icon.Alpha = 1;
            player.SetField("combatHighOverlay", icon);
            return icon;
        }

        public static void updateDamageFeedback(Entity player, string type)
        {
            HudElem hitFeedback = player.GetField<HudElem>("hud_hitFeedback");

            string shader = "damage_feedback";
            if (type == "deployable_vest")
                shader = "damage_feedback_lightarmor";
            else if (type == "juggernaut")
                shader = "damage_feedback_juggernaut";

            hitFeedback.SetShader(shader, 24, 48);
            hitFeedback.Alpha = 1;
            //player.SetField("hud_damageFeedback", hitFeedback);
            player.PlayLocalSound("hit_feedback");

            hitFeedback.FadeOverTime(1);
            hitFeedback.Alpha = 0;
            //AfterDelay(1000, () => hitFeedback.Destroy());
        }

        public static void setHeadIcon(Entity ent, Entity showTo, string icon, Vector3 offset, int width = 10, int height = 10, bool archived = true, float delay = 0.066f, bool constantSize = true, bool pinToScreenEdge = true, bool fadeOutPinnedIcon = false, bool is3D = true)
        {
            Dictionary<string, HudElem> entityHeadIcons = new Dictionary<string, HudElem>();

            if (!ent.HasField("entityHeadIcons"))
                ent.SetField("entityHeadIcons", new Parameter(entityHeadIcons));
            else entityHeadIcons = ent.GetField<Dictionary<string, HudElem>>("entityHeadIcons");

            if (showTo != null && showTo.Classname != "player")
            {
                foreach (string key in entityHeadIcons.Keys)
                {
                    if (IsDefined(entityHeadIcons[key]))
                        entityHeadIcons[key].Destroy();

                    entityHeadIcons.Remove(key);
                    ent.SetField("entityHeadIcons", new Parameter(entityHeadIcons));
                    return;
                }
            }

            HudElem headIcon = null;

            if (showTo != null && showTo.Classname == "player")
            {
                if (entityHeadIcons.ContainsKey(showTo.GUID.ToString()))
                {
                    entityHeadIcons[showTo.GUID.ToString()].Destroy();
                    entityHeadIcons.Remove(showTo.GUID.ToString());
                    ent.SetField("entityHeadIcons", new Parameter(entityHeadIcons));
                }

                if (icon == "")
                    return;

                if (entityHeadIcons.ContainsKey(showTo.SessionTeam))
                {
                    entityHeadIcons[showTo.SessionTeam].Destroy();
                    entityHeadIcons.Remove(showTo.SessionTeam);
                    ent.SetField("entityHeadIcons", new Parameter(entityHeadIcons));
                }

                headIcon = NewClientHudElem(showTo);
                entityHeadIcons.Add(showTo.GUID.ToString(), headIcon);
                ent.SetField("entityHeadIcons", new Parameter(entityHeadIcons));
            }
            else
            {
                //Team based code
            }

            if (headIcon == null) return;

            headIcon.Archived = archived;
            headIcon.X = ent.Origin.X + offset.X;
            headIcon.Y = ent.Origin.Y + offset.Y;
            headIcon.Z = ent.Origin.Z + offset.Z;
            headIcon.Alpha = 0.85f;
            headIcon.SetShader(icon, width, height);
            headIcon.SetWaypoint(constantSize, pinToScreenEdge, fadeOutPinnedIcon, is3D);

            StartAsync(destroyIconsOnDeath(ent));
            if (showTo != null && showTo.Classname == "player")
                StartAsync(destroyOnOwnerDisconnect(headIcon, showTo));
            if (ent.Classname == "player")
                StartAsync(destroyOnOwnerDisconnect(headIcon, ent));
        }
        private static IEnumerator destroyIconsOnDeath(Entity ent)
        {
            yield return ent.WaitTill("death");

            Dictionary<string, HudElem> entityHeadIcons = ent.GetField<Dictionary<string, HudElem>>("entityHeadIcons");
            foreach (string key in entityHeadIcons.Keys)
            {
                entityHeadIcons[key].Destroy();
                entityHeadIcons.Remove(key);
                ent.SetField("entityHeadIcons", new Parameter(entityHeadIcons));
            }
        }
        private static IEnumerator destroyOnOwnerDisconnect(HudElem headIcon, Entity owner)
        {
            yield return owner.WaitTill("disconnect");

            headIcon.Destroy();
        }

        public static int getNextObjID()
        {
            for (int i = 0; i < _objIDList.Length; i++)
            {
                if (!_objIDList[31 - i]) return i;
            }
            return 0;
        }
        public static void _objective_delete(Entity ent)
        {
            if (_objIDs.ContainsKey(ent))
            {
                int icon = _objIDs[ent];
                Objective_Delete(icon);
                _objIDList[icon] = false;
                _objIDs.Remove(ent);
            }
        }
        public static void addObjID(Entity ent, int id)
        {
            if (!_objIDs.ContainsKey(ent)) _objIDs.Add(ent, (byte)id);
            else
            {
                Utilities.PrintToConsole("An entity tried to apply a currently used objID. No objID will be set.");
                return;

            }

            _objIDList[id] = true;
        }
        public static bool coinToss()
        {
            int coin = RandomInt(100);

            if (coin > 50)
                return true;
            return false;
        }
        public static int maxVehiclesAllowed()
        {
            return MAX_VEHICLES;
        }
        public static void incrementFauxVehicleCount()
        {
            fauxVehicleCount++;
        }
        public static Entity[] _getEntArray(string name, string key)
        {
            List<Entity> ret = new List<Entity>();

            for (int i = 0; i < 2046; i++)
            {
                Entity ent = Entity.GetEntity(i);
                if (key == "classname")
                {
                    if (ent.Classname == name)
                        ret.Add(ent);

                    continue;
                }
                else if (key == "code_classname")
                {
                    if (ent.Code_Classname == name)
                        ret.Add(ent);

                    continue;
                }
                else if (key == "targetname")
                {
                    if (ent.TargetName == name)
                        ret.Add(ent);

                    continue;
                }
                else if (key == "target")
                {
                    if (ent.Target == name)
                        ret.Add(ent);

                    continue;
                }
            }

            return ret.ToArray();
        }
    }
}
