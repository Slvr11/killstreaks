using System;
using System.Collections.Generic;
using System.Collections;
using InfinityScript;

namespace killstreaks
{
    public class killstreaks : BaseScript
    {
        public static readonly Entity level = Entity.GetEntity(-1);

        public static string KILLSTREAK_STRING_TABLE = "mp/killstreakTable.csv";
        public static int STREAKCOUNT_MAX_COUNT = 3;
        public static int KILLSTREAK_NAME_COLUMN = 1;
        public static int KILLSTREAK_KILLS_COLUMN = 4;
        public static int KILLSTREAK_EARNED_HINT_COLUMN = 6;
        public static int KILLSTREAK_SOUND_COLUMN = 7;
        public static int KILLSTREAK_EARN_DIALOG_COLUMN = 8;
        public static int KILLSTREAK_ENEMY_USE_DIALOG_COLUMN = 11;
        public static int KILLSTREAK_WEAPON_COLUMN = 12;
        public static int KILLSTREAK_ICON_COLUMN = 14;
        public static int KILLSTREAK_OVERHEAD_ICON_COLUMN = 15;
        public static int KILLSTREAK_DPAD_ICON_COLUMN = 16;
        public static int NUM_KILLS_GIVE_ALL_PERKS = 8;
        public static int KILLSTREAK_GIMME_SLOT = 0;
        public static int KILLSTREAK_SLOT_1 = 1;
        public static int KILLSTREAK_SLOT_2 = 2;
        public static int KILLSTREAK_SLOT_3 = 3;
        public static int KILLSTREAK_ALL_PERKS_SLOT = 4;
        public static int KILLSTREAK_STACKING_START_SLOT = 5;
        public killstreaks()
        {
            //initKillstreakData();

            level.SetField("killstreakFuncs", new Parameter(new Dictionary<string, Delegate>()));
            level.SetField("killstreakSetupFuncs", new Parameter(new Dictionary<string, Delegate>()));
            level.SetField("killstreakWeapons", new Parameter(new Dictionary<string, string>()));

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
            killstreakWieldWeapons.Add("a10_30mm_mp");
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
            level.SetField("killstreakWieldWeapons", new Parameter(killstreakWieldWeapons));

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

            level.SetField("killstreakRoundDelay", GSCFunctions.GetDvar("scr_game_killstreakdelay"));

            PlayerConnected += onPlayerConnect;
        }

        private void initKillstreakData()
        {
            //IW has this handled
            return;
            for (int i = 1; true; i++)
            {
                string retVal = GSCFunctions.TableLookup(KILLSTREAK_STRING_TABLE, 0, i, 1);
            }
        }

        public void onPlayerConnect(Entity player)
        {
            player.SpawnedPlayer += () => onPlayerSpawned(player);
        }

        public void onPlayerSpawned(Entity player)
        {
            killstreakUseWaiter(player);
            ISWait.Start(waitForChangeTeam(player));

            if (player.GetField("streakType") != "specialist")
            {
                streakSelectUpTracker(player);
                streakSelectDownTracker(player);
                streakNotifyTracker(player);
            }

            if (!player.HasField("pers_killstreaks"))
                initPlayerKillstreaks(player);
            if (!player.HasField("earnedStreakLevel"))
                player.SetField("earnedStreakLevel", 0);
            if (!player.HasField("adrenaline"))
                player.SetField("adrenaline", player.GetPlayerData("killstreaksState", "count"));
            //if (player.GetField<int>("adrenaline") == player.Call("getPlayerData", "killstreaksState", "countToNext"))
            {
                setStreakCountToNext(player);
                updateStreakSlots(player);
            }

            if (player.GetField("streakType") == "specialist")
                updateSpecialistKillstreaks(player);
            else
                giveOwnedKillstreakItem(player);
        }

        private void initPlayerKillstreaks(Entity player)
        {
            if (!player.HasField("streakType"))
                return;

            if (player.GetField("streakType") == "specialist")
                player.SetPlayerData("killstreaksState", "isSpecialist", true);
            else
                player.SetPlayerData("killstreaksState", "isSpecialist", false);

            Entity[] pers_killstreaks = new Entity[5];
            pers_killstreaks[KILLSTREAK_GIMME_SLOT] = GSCFunctions.SpawnStruct();
            pers_killstreaks[KILLSTREAK_GIMME_SLOT].SetField("available", false);
            pers_killstreaks[KILLSTREAK_GIMME_SLOT].SetField("streakName", "");
            pers_killstreaks[KILLSTREAK_GIMME_SLOT].SetField("earned", false);
            pers_killstreaks[KILLSTREAK_GIMME_SLOT].SetField("awardxp", "");
            pers_killstreaks[KILLSTREAK_GIMME_SLOT].SetField("owner", "");
            pers_killstreaks[KILLSTREAK_GIMME_SLOT].SetField("kID", "");
            pers_killstreaks[KILLSTREAK_GIMME_SLOT].SetField("lifeId", "");
            pers_killstreaks[KILLSTREAK_GIMME_SLOT].SetField("isGimme", true);
            pers_killstreaks[KILLSTREAK_GIMME_SLOT].SetField("isSpecialist", false);
            pers_killstreaks[KILLSTREAK_GIMME_SLOT].SetField("nextSlot", "");

            for (int i = 1; i < KILLSTREAK_ALL_PERKS_SLOT; i++)
            {
                pers_killstreaks[i] = GSCFunctions.SpawnStruct();
                pers_killstreaks[i].SetField("available", false);
                pers_killstreaks[i].SetField("streakName", "");
                pers_killstreaks[i].SetField("earned", true);
                pers_killstreaks[i].SetField("awardxp", 1);
                pers_killstreaks[i].SetField("owner", "");
                pers_killstreaks[i].SetField("kID", "");
                pers_killstreaks[i].SetField("lifeId", -1);
                pers_killstreaks[i].SetField("isGimme", false);
                pers_killstreaks[i].SetField("isSpecialist", false);
            }

            pers_killstreaks[KILLSTREAK_ALL_PERKS_SLOT] = GSCFunctions.SpawnStruct();
            pers_killstreaks[KILLSTREAK_ALL_PERKS_SLOT].SetField("available", false);
            pers_killstreaks[KILLSTREAK_ALL_PERKS_SLOT].SetField("streakName", "all_perks_bonus");
            pers_killstreaks[KILLSTREAK_ALL_PERKS_SLOT].SetField("earned", true);
            pers_killstreaks[KILLSTREAK_ALL_PERKS_SLOT].SetField("awardxp", 0);
            pers_killstreaks[KILLSTREAK_ALL_PERKS_SLOT].SetField("owner", "");
            pers_killstreaks[KILLSTREAK_ALL_PERKS_SLOT].SetField("kID", "");
            pers_killstreaks[KILLSTREAK_ALL_PERKS_SLOT].SetField("lifeId", -1);
            pers_killstreaks[KILLSTREAK_ALL_PERKS_SLOT].SetField("isGimme", false);
            pers_killstreaks[KILLSTREAK_ALL_PERKS_SLOT].SetField("isSpecialist", true);

            player.SetField("pers_killstreaks", new Parameter(pers_killstreaks));

            for (int i = 0; i < 4; i++)
                player.SetPlayerData("killstreaksState", "icons", i, 0);
            player.SetPlayerData("killstreaksState", "hasStreak", KILLSTREAK_GIMME_SLOT, false);

            int index = 1;
            foreach (string streakName in player.GetField("killstreaks") as string[])
            {
                player.GetField<Entity[]>("pers_killstreaks")[index].SetField("streakName", streakName);
                player.GetField<Entity[]>("pers_killstreaks")[index].SetField("isSpecialist", (player.GetField("streakType") == "specialist"));

                string killstreakIndexName = player.GetField<Entity[]>("pers_killstreaks")[index].GetField<string>("streakName");
                if (player.GetField<string>("streakType") == "specialist")
                {
                    string[] perkTokens = Utilities.Tokenize(player.GetField<Entity[]>("pers_killstreaks")[index].GetField<string>("streakName"));
                    if (perkTokens[perkTokens.Length - 1] == "ks")
                    {
                        string perkName = string.Empty;
                        foreach (string tokens in perkTokens)
                        {
                            if (tokens != "ks")
                            {
                                if (perkName != string.Empty)
                                    perkName = tokens;
                                else
                                    perkName += ("_" + tokens);
                            }
                        }

                        if (perkName != string.Empty && getPerkUpgrade(perkName) != "specialty_null")
                            killstreakIndexName = player.GetField<Entity[]>("pers_killstreaks")[index].GetField("streakName") + "_pro";
                    }
                }

                player.SetPlayerData("killstreaksState", "icons", index, getKillstreakIndex(killstreakIndexName));
                player.SetPlayerData("killstreaksState", "hasStreak", index, false);

                index++;
            }

            player.SetPlayerData("killstreaksState", "nextIndex", 1);
            player.SetPlayerData("killstreaksState", "selectedIndex", -1);
            player.SetPlayerData("killstreaksState", "numAvailable", 0);

            player.SetPlayerData("killstreaksState", "hasStreak", KILLSTREAK_ALL_PERKS_SLOT, false);
        }

        public void updateStreakCount(Entity player)
        {
            if (!player.HasField("pers_killstreaks"))
                return;
            if (player.GetField("adrenaline") == player.GetField("previousAdrenaline"))
                return;

            int curCount = (int)player.GetField("adrenaline");

            player.SetPlayerData("killstreaksState", "count", (int)player.GetField("adrenaline"));

            if ((int)player.GetField("adrenaline") >= (int)player.GetPlayerData("killstreaksState", "countToNext"))
                setStreakCountToNext(player);
        }

        public void resetStreakCount(Entity player)
        {
            player.SetPlayerData("killstreakState", "count", 0);
            setStreakCountToNext(player);
        }

        public void setStreakCountToNext(Entity player)
        {
            if (!player.HasField("streakType"))
            {
                player.SetPlayerData("killstreaksState", "countToNext", 0);
                return;
            }

            if (getMaxStreakCost(player) == 0)
            {
                player.SetPlayerData("killstreaksState", "countToNext", 0);
                return;
            }

            if (player.GetField("streakType") == "specialist")
            {
                if ((int)player.GetField("adrenaline") >= getMaxStreakCost(player))
                    return;
            }

            string nextStreakName = getNextStreakName(player);
            if (nextStreakName == null)
                return;
            int nextStreakCost = getStreakCost(nextStreakName);
            player.SetPlayerData("killstreaksState", "countToNext", nextStreakCost);
        }

        public string getNextStreakName(Entity player)
        {
            int adrenaline;
            if ((int)player.GetField("adrenaline") >= getMaxStreakCost(player) && player.GetField("streakType") != "specialist")
            {
                adrenaline = 0;
            }
            else adrenaline = (int)player.GetField("adrenaline");

            foreach (string streakName in player.GetField<string[]>("killstreaks"))
            {
                int streakVal = getStreakCost(player);

                if (streakVal > adrenaline)
                    return streakName;
            }
            return string.Empty;
        }

        public int getMaxStreakCost(Entity player)
        {
            int maxCost = 0;
            foreach (string streakName in player.GetField<string[]>("killstreaks"))
            {
                int streakVal = getStreakCost(player);

                if (streakVal > maxCost)
                    maxCost = streakVal;
            }
            return maxCost;
        }

        public void updateStreakSlots(Entity player)
        {
            if (!player.HasField("streakType"))
                return;

            int numStreaks = 0;
            for (int i = 0; i < KILLSTREAK_SLOT_3 + 1; i++)
            {
                if (player.GetField<Entity[]>("pers_killstreaks")[i] != null && player.GetField<Entity[]>("pers_killstreaks")[i].GetField("streakName") != string.Empty)
                {
                    player.SetPlayerData("killstreaksState", "hasStreak", i, player.GetField<Entity[]>("pers_killstreaks")[i].GetField<bool>("available"));
                    if (player.GetField<Entity[]>("pers_killstreaks")[i].GetField<bool>("available"))
                        numStreaks++;
                }
            }
            if (player.GetField("streakType") != "specialist")
                player.SetPlayerData("killstreaksState", "numAvailable", numStreaks);

            int minLevel = (int)player.GetField("earnedStreakLevel");
            int maxLevel = getMaxStreakCost(player);
            if ((int)player.GetField("earnedStreakLevel") == maxLevel && player.GetField("streakType") != "specialist")
                minLevel = 0;

            int nextIndex = 1;

            foreach (string streakName in player.GetField<string[]>("killstreaks"))
            {
                int streakVal = getStreakCost(streakName);

                if (streakVal > minLevel)
                {
                    string nextStreak = streakName;
                    break;
                }

                if (player.GetField("streakType") == "specialist")
                {
                    if ((int)player.GetField("earnedStreakLevel") == maxLevel)
                        break;
                }

                nextIndex++;
            }

            player.SetPlayerData("killstreaksState", "nextIndex", nextIndex);

            if ((int)player.GetField("killstreakIndexWeapon") != -1 && player.GetField("streakType") != "specialist")
            {
                player.SetPlayerData("killstreaksState", "selectedIndex", (int)player.GetField("killstreakIndexWeapon"));
            }
            else
            {
                if (player.GetField("streakType") == "specialist" && player.GetField<Entity[]>("pers_killstreaks")[KILLSTREAK_GIMME_SLOT].GetField<bool>("available"))
                    player.SetPlayerData("killstreakState", "selectedIndex", 0);
                else
                    player.SetPlayerData("killstreakState", "selectedIndex", -1);
            }
        }

        public IEnumerator waitForChangeTeam(Entity player)
        {
            player.Notify("waitForChangeTeam");

            yield return player.WaitTill("joined_team");
            clearKillstreaks(player);
        }

        public bool isRideKillstreak(string streakName)
        {
            switch (streakName)
            {
                case "helicopter_minigun":
                case "helicopter_mk19":
                case "ac130":
                case "predator_missile":
                case "osprey_gunner":
                case "remote_mortar":
                case "remote_uav":
                case "remote_tank":
                    return true;

                default:
                    return false;
            }
        }

        public bool isCarryKillstreak(string streakName)
        {
            switch (streakName)
            {
                case "sentry":
                case "sentry_gl":
                case "minigun_turret":
                case "gl_turret":
                case "deployable_vest":
                case "deployable_exp_ammo":
                case "ims":
                    return true;

                default:
                    return false;
            }
        }

        public bool deadlyKillstreak(string streakName)
        {
            switch (streakName)
            {
                case "predator_missile":
                case "precision_airstrike":
                case "harrier_airstrike":
                case "helicopter":
                case "helicopter_flares":
                case "stealth_airstrike":
                case "helicopter_minigun":
                case "littlebird_support":
                case "littlebird_flock":
                case "remote_mortar":
                case "osprey_gunner":
                case "ac130":
                case "remote_tank":
                    return true;
            }

            return false;
        }

        public bool killstreakUsePressed(Entity player)
        {
            string streakName = (string)player.GetField<Entity[]>("pers_killstreaks")[player.GetField<int>("killstreakIndexWeapon")].GetField("streakName");
            int lifeId = (int)player.GetField<Entity[]>("pers_killstreaks")[player.GetField<int>("killstreakIndexWeapon")].GetField("lifeId");
            bool isEarned = (bool)player.GetField<Entity[]>("pers_killstreaks")[player.GetField<int>("killstreakIndexWeapon")].GetField("earned");
            int awardXp = (int)player.GetField<Entity[]>("pers_killstreaks")[player.GetField<int>("killstreakIndexWeapon")].GetField("awardXp");
            int kID = (int)player.GetField<Entity[]>("pers_killstreaks")[player.GetField<int>("killstreakIndexWeapon")].GetField("kID");
            bool isGimme = (bool)player.GetField<Entity[]>("pers_killstreaks")[player.GetField<int>("killstreakIndexWeapon")].GetField("isGimme");

            if (!player.IsOnGround() && (isRideKillstreak(streakName) || isCarryKillstreak(streakName)))
                return false;

            if (player.IsUsingTurret())//isUsingRemote
                return true;

            //if ((bool)player.GetField("selectingLocation"))
                //return false;

            if (deadlyKillstreak(streakName) && (bool)level.GetField("killstreakRoundDelay") && getGametypeNumLives())
            {
                if ((int)level.GetField("gracePeriod") - (int)level.GetField("inGracePeriod") < (int)level.GetField("killstreakRoundDelay"))
                {
                    player.IPrintLnBold("Unavailable for " + ((int)level.GetField("killstreakRoundDelay") - ((int)level.GetField("gracePeriod") - (int)level.GetField("inGracePeriod"))) + " seconds.");
                    return false;
                }
            }

            if (((bool)level.GetField("isTeamBased") && (bool)level.GetField("teamEMPed_" + player.GetField("sessionteam"))) || (!(bool)level.GetField("isTeamBased") && level.GetField("empPlayer") != Entity.Level && level.GetField("empPlayer") != player))
            {
                if (streakName != "deployable_vest")
                {
                    player.IPrintLnBold("Unavailable for " + level.GetField("empTimeRemaining") + " seconds.");
                    return false;
                }

                if (player.HasField("nuked") && (bool)player.GetField("nuked"))
                {
                    if (streakName != "deployable_vest")
                    {
                        player.IPrintLnBold("Unavailable for " + nuke.nukeEmpTimeRemaining + " seconds.");
                        return false;
                    }
                }
            }

            if (player.IsUsingTurret() && (isRideKillstreak(streakName) || isCarryKillstreak(streakName)))
            {
                player.IPrintLnBold("Unavailable while using a turret.");
                return false;
            }

            if (player.HasField("lastStand") && isRideKillstreak(streakName))
            {
                player.IPrintLnBold("Unavailable in last stand.");
                return false;
            }

            bool removeExplosiveAmmo = false;
            if (player.HasPerk("specialty_explosivebullets") && GSCFunctions.IsSubStr(streakName, "explosive_amoo"))
                removeExplosiveAmmo = true;

            if (GSCFunctions.IsSubStr(streakName, "airdrop") || streakName == "littlebird_flock")
            {
                if ((bool)level.GetField<Dictionary<string, Delegate>>("killstreakFuncs")[streakName].DynamicInvoke(player, lifeId, kID))
                    return false;
            }
            else
            {
                if ((bool)level.GetField<Dictionary<string, Delegate>>("killstreakFuncs")[streakName].DynamicInvoke(player, lifeId))
                    return false;
            }

            if (removeExplosiveAmmo)
                player.UnSetPerk("specialty_explosivebullets");

            updateKillstreaks(player);
            usedKillstreak(player, streakName, awardXp);

            return true;
        }

        public void usedKillstreak(Entity player, string streakName, int awardXp)
        {
            player.PlayLocalstring("weap_c4detpack_trigger_plr");

            if (awardXp > 0)
            {
                onXPEvent(player, "killstreak_" + streakName);
                useHardpoint(player, streakName);
            }

            string awardref = getKillstreakAwardRef(streakName);
            if (awardref != String.Empty)
                incPlayerStat(awardref, 1);

            if (isAssaultKillstreak(streakName))
                incPlayerStat("assaultkillstreakused", 1);

            if (isSupportKillstreak(streakName))
                incPlayerStat("supportkillstreakused", 1);

            if (isSpecialistKillstreak(streakName))
            {
                incPlayerStat("specialistkillstreakused", 1);
                return;
            }

            string team = (string)player.GetField("sessionteam");
            if ((bool)level.GetField("teamBased"))
            {
                leaderDialog(team + "_friendly_" + streakName + "_inbound", (string)level.GetField("otherTeam_" + team));

                if (getKillstreakInformEnemy(streakName))
                    leaderDialog(team + "_enemy_" + streakName + "_inbound", (string)level.GetField("otherTeam_" + team));
            }
            else
            {
                leaderDialogOnPlayer(team + "_friendly_" + streakName + "_inbound");

                if (getKillstreakInformEnemy(streakName))
                {
                    Entity[] excludeList = new Entity[1]{player};
                    leaderDialog(team + "_enemy_" + streakName + "_inbound", string.Empty, string.Empty, excludeList);
                }
            }
        }

        public void updateKillstreaks(Entity player, bool keepCurrent = false)
        {
            if (!keepCurrent)
            {
                player.GetField<Entity[]>("pers_killstreaks")[player.GetField<int>("killstreakIndexWeapon")].SetField("available", false);

                if (player.GetField<int>("killstreakIndexWeapon") == KILLSTREAK_GIMME_SLOT)
                {
                    //player.GetField<Entity[]>("pers_killstreaks")[player.GetField<Entity[]>("pers_killstreaks")[KILLSTREAK_GIMME_SLOT].GetField<int>("nextSlot")]
                }
            }
        }

        public static void clearKillstreaks(Entity player)
        {
            for (int i = player.GetField<Entity[]>("pers_killstreaks").Length - 1; i > -1; i--)
            {
               
            }
        }

        public string getPerkUpgrade(string perkName)
        {
            string perkUpgrade = GSCFunctions.TableLookup("mp/perktable.csv", 1, perkName, 8);

            if (perkName == "" || perkName == "specialty_null")
                return "specialty_null";

            return perkUpgrade;
        }

        private int getKillstreakIndex(string streakName)
        {
            int ret = 0;
            ret = GSCFunctions.TableLookupRowNum("mp/killstreakTable.csv", 1, streakName) - 1;

            return ret;
        }
    }
}
