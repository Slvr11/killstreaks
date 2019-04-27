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
    class airdrop : BaseScript
    {
        Dictionary<string, string> strings = new Dictionary<string, string>();
        private static Entity airDropCrateCollision;
        public static Action<Entity> airdrop_assaultGiveFunc;
        public static Func<Entity, bool> airdrop_assaultUseFunc;
        public static Action<Entity> airdrop_supportGiveFunc;
        public static Func<Entity, bool> airdrop_supportUseFunc;
        public static Action<Entity> airdrop_megaGiveFunc;
        public static Func<Entity, bool> airdrop_megaUseFunc;
        public static Action<Entity> airdrop_predator_missileGiveFunc;
        public static Func<Entity, bool> airdrop_predator_missileUseFunc;
        public static Action<Entity> airdrop_juggernautGiveFunc;
        public static Func<Entity, bool> airdrop_juggernautUseFunc;
        public static Action<Entity> airdrop_juggernaut_defGiveFunc;
        public static Func<Entity, bool> airdrop_juggernaut_defUseFunc;
        public static Action<Entity> airdrop_juggernaut_glGiveFunc;
        public static Func<Entity, bool> airdrop_juggernaut_glUseFunc;
        public static Action<Entity> airdrop_juggernaut_reconGiveFunc;
        public static Func<Entity, bool> airdrop_juggernaut_reconUseFunc;
        public static Action<Entity> airdrop_trophyGiveFunc;
        public static Func<Entity, bool> airdrop_trophyUseFunc;
        public static Action<Entity> airdrop_trapGiveFunc;
        public static Func<Entity, bool> airdrop_trapUseFunc;
        public static Action<Entity> airdrop_remote_tankGiveFunc;
        public static Func<Entity, bool> airdrop_remote_tankUseFunc;
        public static Action<Entity> ammoGiveFunc;
        public static Func<Entity, bool> ammoUseFunc;
        public static Action<Entity> explosive_ammoGiveFunc;
        public static Func<Entity, bool> explosive_ammoUseFunc;
        public static Action<Entity> explosive_ammo2GiveFunc;
        public static Func<Entity, bool> explosive_ammo2UseFunc;
        public static Action<Entity> light_armorGiveFunc;
        public static Func<Entity, bool> light_armorUseFunc;

        public airdrop()
        {
            strings.Add("ammo_hint", "Press and hold ^3[{+activate}]^7 for Ammo Pickup");
            strings.Add("explosive_ammo_hint", "Press and hold ^3[{+activate}]^7 for Explosive Ammo Pickup");
            strings.Add("uav_hint", "Press and hold ^3[{+activate}]^7 for a UAV");
            strings.Add("counter_uav_hint", "Press and hold ^3[{+activate}]^7 for a Counter-UAV");
            strings.Add("sentry_hint", "Press and hold ^3[{+activate}]^7 for a Sentry Gun");
            strings.Add("juggernaut_hint", "Press and hold ^3[{+activate}]^7 for Juggernaut");
            strings.Add("airdrop_juggernaut_hint", "Press and hold ^3[{+activate}]^7 for Juggernaut");
            strings.Add("airdrop_juggernaut_def_hint", "Press and hold ^3[{+activate}]^7 for Juggernaut");
            strings.Add("airdrop_juggernaut_gl_hint", "Press and hold ^3[{+activate}]^7 for Juggernaut");
            strings.Add("airdrop_juggernaut_recon_hint", "Press and hold ^3[{+activate}]^7 for Juggernaut");
            strings.Add("trophy_hint", "Press and hold ^3[{+activate}]^7 to pick up Trophy");
            strings.Add("predator_missile_hint", "Press and hold ^3[{+activate}]^7 for a Predator Missile");
            strings.Add("airstrike_hint", "Press and hold ^3[{+activate}]^7 for an Airstrike");
            strings.Add("precision_airstrike_hint", "Press and hold ^3[{+activate}]^7 for a Precision Airstrike");
            strings.Add("harrier_airstrike_hint", "Press and hold ^3[{+activate}]^7 for a Harrier Airstrike");
            strings.Add("helicopter_hint", "Press and hold ^3[{+activate}]^7 for an Attack Helicopter");
            strings.Add("helicopter_flares_hint", "Press and hold ^3[{+activate}]^7 for a Pave Low");
            strings.Add("stealth_airstrike_hint", "Press and hold ^3[{+activate}]^7 for a Stealth Bomber");
            strings.Add("helicopter_minigun_hint", "Press and hold ^3[{+activate}]^7 for a Chopper Gunner");
            strings.Add("ac130_hint", "Press and hold ^3[{+activate}]^7 for an AC-130");
            strings.Add("emp_hint", "Press and hold ^3[{+activate}]^7 for an EMP");
            strings.Add("littlebird_support_hint", "Press and hold ^3[{+activate}]^7 for AH-6 Overwatch");
            strings.Add("littlebird_flock_hint", "Press and hold ^3[{+activate}]^7 for Strafe Run");
            strings.Add("uav_strike_hint", "Press and hold ^3[{+activate}]^7 for UAV Strike");
            strings.Add("light_armor_hint", "Press and hold ^3[{+activate}]^7 for Ballistic Vest");
            strings.Add("minigun_turret_hint", "Press and hold ^3[{+activate}]^7 for Minigun Turret");
            strings.Add("team_ammo_refill_hint", "Press and hold ^3[{+activate}]^7 for Team Ammo Refill");
            strings.Add("deployable_vest_hint", "Press and hold ^3[{+activate}]^7 for Ballistic Vest Duffel Bag");
            strings.Add("deployable_exp_ammo_hint", "Press and hold ^3[{+activate}]^7 for Explosive Ammo Box");
            strings.Add("gl_turret_hint", "Press and hold ^3[{+activate}]^7 for Grenade Launcher Turret");
            strings.Add("directional_uav_hint", "Press and hold ^3[{+activate}]^7 for Directional UAV");
            strings.Add("ims_hint", "Press and hold ^3[{+activate}]^7 for I.M.S.");
            strings.Add("heli_sniper_hint", "Press and hold ^3[{+activate}]^7 for Heli Sniper");
            strings.Add("heli_minigunner_hint", "Press and hold ^3[{+activate}]^7 for Heli Minigunner");
            strings.Add("remote_mortar_hint", "Press and hold ^3[{+activate}]^7 for Reaper");
            strings.Add("remote_uav_hint", "Press and hold ^3[{+activate}]^7 for Recon Drone");
            strings.Add("osprey_gunner_hint", "Press and hold ^3[{+activate}]^7 for Osprey Gunner");
            strings.Add("remote_tank_hint", "Press and hold ^3[{+activate}]^7 for Assault Drone");
            strings.Add("triple_uav_hint", "Press and hold ^3[{+activate}]^7 for Advanced UAV");
            strings.Add("remote_mg_turret_hint", "Press and hold ^3[{+activate}]^7 for Remote Sentry");
            strings.Add("sam_turret_hint", "Press and hold ^3[{+activate}]^7 for SAM Turret");
            strings.Add("escort_airdrop_hint", "Press and hold ^3[{+activate}]^7 for Escort Airdrop");

            airDropCrateCollision = GetEnt("care_package", "targetname");
            airDropCrateCollision = GetEnt(airDropCrateCollision.Target, "targetname");

            //ASSAULT
            addCrateType("airdrop_assault", "uav", 10, killstreakCrateThink);
            addCrateType("airdrop_assault", "ims", 20, killstreakCrateThink);
            addCrateType("airdrop_assault", "predator_missile", 20, killstreakCrateThink);
            addCrateType("airdrop_assault", "sentry", 20, killstreakCrateThink);
            addCrateType("airdrop_assault", "precision_airstrike", 6, killstreakCrateThink);
            addCrateType("airdrop_assault", "helicopter", 4, killstreakCrateThink);
            addCrateType("airdrop_assault", "littlebird_support", 4, killstreakCrateThink);
            addCrateType("airdrop_assault", "littlebird_flock", 4, killstreakCrateThink);
            addCrateType("airdrop_assault", "remote_mortar", 3, killstreakCrateThink);
            addCrateType("airdrop_assault", "remote_tank", 3, killstreakCrateThink);
            addCrateType("airdrop_assault", "helicopter_flares", 2, killstreakCrateThink);
            addCrateType("airdrop_assault", "ac130", 2, killstreakCrateThink);
            addCrateType("airdrop_assault", "airdrop_juggernaut", 1, juggernautCrateThink);
            addCrateType("airdrop_assault", "osprey_gunner", 1, killstreakCrateThink);

            //OSPREY GUNNER
            addCrateType("airdrop_osprey_gunner", "uav", 10, killstreakCrateThink);
            addCrateType("airdrop_osprey_gunner", "ims", 20, killstreakCrateThink);
            addCrateType("airdrop_osprey_gunner", "predator_missile", 20, killstreakCrateThink);
            addCrateType("airdrop_osprey_gunner", "sentry", 20, killstreakCrateThink);
            addCrateType("airdrop_osprey_gunner", "precision_airstrike", 6, killstreakCrateThink);
            addCrateType("airdrop_osprey_gunner", "littlebird_flock", 4, killstreakCrateThink);
            addCrateType("airdrop_osprey_gunner", "remote_mortar", 3, killstreakCrateThink);
            addCrateType("airdrop_osprey_gunner", "remote_tank", 3, killstreakCrateThink);
            addCrateType("airdrop_osprey_gunner", "helicopter_flares", 2, killstreakCrateThink);
            addCrateType("airdrop_osprey_gunner", "ac130", 2, killstreakCrateThink);
            addCrateType("airdrop_osprey_gunner", "airdrop_juggernaut", 1, juggernautCrateThink);

            //SUPPORT
            addCrateType("airdrop_support", "uav", 9, killstreakCrateThink);
            addCrateType("airdrop_support", "counter_uav", 9, killstreakCrateThink);
            addCrateType("airdrop_support", "deployable_vest", 8, killstreakCrateThink);
            addCrateType("airdrop_support", "sam_turret", 6, killstreakCrateThink);
            addCrateType("airdrop_support", "remote_uav", 5, killstreakCrateThink);
            addCrateType("airdrop_support", "remote_mg_turret", 5, killstreakCrateThink);
            addCrateType("airdrop_support", "stealth_airstrike", 4, killstreakCrateThink);
            addCrateType("airdrop_support", "triple_uav", 3, killstreakCrateThink);
            addCrateType("airdrop_support", "airdrop_juggernaut_recon", 2, juggernautCrateThink);
            addCrateType("airdrop_support", "escort_airdrop", 1, killstreakCrateThink);
            addCrateType("airdrop_support", "emp", 1, killstreakCrateThink);

            //ESCORT AIRDROP
            addCrateType("airdrop_escort", "airdrop_trap", 10, trapCrateThink);
            addCrateType("airdrop_escort", "uav", 8, killstreakCrateThink);
            addCrateType("airdrop_escort", "counter_uav", 8, killstreakCrateThink);
            addCrateType("airdrop_escort", "deployable_vest", 7, killstreakCrateThink);
            addCrateType("airdrop_escort", "sentry", 7, killstreakCrateThink);
            addCrateType("airdrop_escort", "ims", 7, killstreakCrateThink);
            addCrateType("airdrop_escort", "sam_turret", 6, killstreakCrateThink);
            addCrateType("airdrop_escort", "stealth_airstrike", 5, killstreakCrateThink);
            addCrateType("airdrop_escort", "airdrop_juggernaut_recon", 5, juggernautCrateThink);
            addCrateType("airdrop_escort", "remote_uav", 5, killstreakCrateThink);
            addCrateType("airdrop_escort", "triple_uav", 3, killstreakCrateThink);
            addCrateType("airdrop_escort", "remote_mg_turret", 3, killstreakCrateThink);
            addCrateType("airdrop_escort", "emp", 1, killstreakCrateThink);

            //TRAP CONTENTS
            addCrateType("airdrop_trapcontents", "ims", 6, trapNullFunc);
            addCrateType("airdrop_trapcontents", "predator_missile", 7, trapNullFunc);
            addCrateType("airdrop_trapcontents", "sentry", 7, trapNullFunc);
            addCrateType("airdrop_trapcontents", "precision_airstrike", 7, trapNullFunc);
            addCrateType("airdrop_trapcontents", "helicopter", 8, trapNullFunc);
            addCrateType("airdrop_trapcontents", "littlebird_support", 8, trapNullFunc);
            addCrateType("airdrop_trapcontents", "littlebird_flock", 8, trapNullFunc);
            addCrateType("airdrop_trapcontents", "remote_mortar", 9, trapNullFunc);
            addCrateType("airdrop_trapcontents", "remote_tank", 9, trapNullFunc);
            addCrateType("airdrop_trapcontents", "helicopter_flares", 10, trapNullFunc);
            addCrateType("airdrop_trapcontents", "ac130", 10, trapNullFunc);
            addCrateType("airdrop_trapcontents", "airdrop_juggernaut", 10, trapNullFunc);
            addCrateType("airdrop_trapcontents", "osprey_gunner", 10, trapNullFunc);

            //GRINDER DROP
            //Removed because we don't do grinder drops

            addCrateType("airdrop_sentry_minigun", "sentry", 100, killstreakCrateThink);
            addCrateType("airdrop_juggernaut", "airdrop_juggernaut", 100, juggernautCrateThink);
            addCrateType("airdrop_juggernaut_recon", "airdrop_juggernaut_recon", 100, juggernautCrateThink);
            addCrateType("airdrop_trophy", "airdrop_trophy", 100, trophyCrateThink);
            addCrateType("airdrop_trap", "airdrop_trap", 100, trapCrateThink);
            addCrateType("littlebird_support", "littlebird_support", 100, killstreakCrateThink);
            addCrateType("airdrop_remote_tank", "remote_tank", 100, killstreakCrateThink);

            // generate the max weighted value
            foreach (string dropType in crateTypes.Keys)
            {
                if (!crateMaxVal.ContainsKey(dropType))
                    crateMaxVal.Add(dropType, 0);
                else crateMaxVal[dropType] = 0;

                foreach (string crateType in crateTypes[dropType].Keys)
                {
                    crateMaxVal[dropType] += crateTypes[dropType][crateType];
                    crateTypes[dropType][crateType] = crateMaxVal[dropType];
                }
            }

            Entity[] tdmSpawns = _getEntArray("mp_tdm_spawn", "classname");
            Entity _lowSpawn = null;

            foreach (Entity lspawn in tdmSpawns)
            {
                if (_lowSpawn == null || lspawn.Origin.Z < _lowSpawn.Origin.Z)
                {
                    _lowSpawn = lspawn;
                }
            }
            lowSpawn = _lowSpawn;
        }

        private static void addCrateType(string dropType, string crateType, int crateWeight, Func<Entity, string> crateFunc)
        {
            if (!crateTypes.ContainsKey(dropType))
                crateTypes.Add(dropType, new Dictionary<string, int>());

            if (!crateTypes[dropType].ContainsKey(crateType))
                crateTypes[dropType].Add(crateType, crateWeight);
            else
                crateTypes[dropType][crateType] = crateWeight;

            if (!crateFuncs.ContainsKey(dropType))
                crateFuncs.Add(dropType, new Dictionary<string, Func<Entity, string>>());

            if (!crateFuncs[dropType].ContainsKey(crateType))
                crateFuncs[dropType].Add(crateType, crateFunc);
            else
                crateFuncs[dropType][crateType] = crateFunc;
        }

        private static string getRandomCrateType(Entity crate, string dropType)
        {
            int value = RandomInt(crateMaxVal[dropType]);
            bool charmed;

            if (crate != null && crate.HasField("owner") && crate.GetField<Entity>("owner").HasPerk("specialty_luckycharm"))
                charmed = true;
            else
                charmed = false;

            string selectedCrateType = "";
            foreach (string crateType in crateTypes[dropType].Keys)
            {
                selectedCrateType = crateType;

                if (crateTypes[dropType][crateType] > value)
                {
                    if (charmed)
                    {
                        charmed = false;
                        continue;
                    }
                    break;
                }
            }

            return selectedCrateType;
        }

        private static string getCrateTypeForDropType(string dropType)
        {
            switch (dropType)
            {
                case "airdrop_sentry_minigun":
                    return "sentry";
                case "airdrop_predator_missile":
                    return "predator_missile";
                case "airdrop_juggernaut":
                    return "airdrop_juggernaut";
                case "airdrop_juggernaut_def":
                    return "airdrop_juggernaut_def";
                case "airdrop_juggernaut_gl":
                    return "airdrop_juggernaut_gl";
                case "airdrop_juggernaut_recon":
                    return "airdrop_juggernaut_recon";
                case "airdrop_trap":
                    return "airdrop_trap";
                case "airdrop_trophy":
                    return "airdrop_trophy";
                case "airdrop_remote_tank":
                    return "remote_tank";
                case "airdrop_assault":
                case "airdrop_support":
                case "airdrop_escort":
                case "airdrop_mega":
                case "airdrop_grnd":
                case "airdrop_grnd_mega":
                default:
                    return getRandomCrateType(null, dropType);
            }
        }

        /**********************************************************
        *		 Usage functions
        ***********************************************************/

        public static bool tryUseAssaultAirdrop(Entity player)
        {
            return tryUseAirdrop(player, "airdrop_assault");
        }
        public static bool tryUseSupportAirdrop(Entity player)
        {
            return tryUseAirdrop(player, "airdrop_support");
        }
        public static bool tryUseAirdropPredatorMissile(Entity player)
        {
            return tryUseAirdrop(player, "airdrop_predator_missile");
        }
        public static bool tryUseAirdropSentryMinigun(Entity player)
        {
            return tryUseAirdrop(player, "airdrop_sentry_minigun");
        }
        public static bool tryUseJuggernautAirdrop(Entity player)
        {
            return tryUseAirdrop(player, "airdrop_juggernaut");
        }
        public static bool tryUseJuggernautGLAirdrop(Entity player)
        {
            return tryUseAirdrop(player, "airdrop_juggernaut_gl");
        }
        public static bool tryUseJuggernautReconAirdrop(Entity player)
        {
            return tryUseAirdrop(player, "airdrop_juggernaut_recon");
        }
        public static bool tryUseJuggernautDefAirdrop(Entity player)
        {
            return tryUseAirdrop(player, "airdrop_juggernaut_def");
        }
        public static bool tryUseMegaAirdrop(Entity player)
        {
            return tryUseAirdrop(player, "airdrop_mega");
        }
        public static bool tryUseAirdropTrap(Entity player)
        {
            if (tryUseAirdrop(player, "airdrop_trap"))
            {
                if (isTeamBased)
                {
                    leaderDialog(otherTeam[player.SessionTeam] + "_enemy_airdrop_assault_inbound", otherTeam[player.SessionTeam]);
                }
                else
                {
                    List<Entity> excludeList = new List<Entity>() { player };
                    leaderDialog(otherTeam[player.SessionTeam] + "_enemy_airdrop_assault_inbound", "", excludeList);
                }
                return true;
            }
            else
                return false;
        }
        public static bool tryUseAirdropRemoteTank(Entity player)
        {
            return tryUseAirdrop(player, "airdrop_remote_tank");
        }
        public static bool tryUseAmmo(Entity player)
        {
            if (isJuggernaut(player))
                return false;
            else
            {
                refillAmmo(player, true);
                return true;
            }
        }
        public static bool tryUseExplosiveAmmo(Entity player)
        {
            if (isJuggernaut(player))
                return false;
            else
            {
                refillAmmo(player, false);
                player.SetPerk("specialty_explosivebullets", true, false);
                return true;
            }
        }

        public static bool tryUseLightArmor(Entity player)
        {
            if (isJuggernaut(player))
                return false;
            else
            {
                giveLightArmor(player);
                return true;
            }
        }

        public static bool tryUseAirdrop(Entity player, string dropType)
        {
            bool result;

            numIncomingVehicles = 1;
            if ((littleBirds.Count >= 4 || fauxVehicleCount >= 4) && dropType != "airdrop_mega" && !IsSubStr(ToLower(dropType), "juggernaut"))
            {
                player.IPrintLnBold("Air space too crowded.");
                return false;
            }
            else if (/*currentActiveVehicleCount() >= maxVehiclesAllowed() ||*/ fauxVehicleCount + numIncomingVehicles >= maxVehiclesAllowed())
            {
                player.IPrintLnBold("Too many vehicles already in the area.");
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
            else if (!validateUseStreak(player))
            {
                return false;
            }

            if (dropType != "airdrop_mega" && !IsSubStr(ToLower(dropType), "juggernaut"))
            {
                //StartAsync(watchDisconnect(player));
            }

            incrementFauxVehicleCount();

            result = true;
            StartAsync(beginAirdropViaMarker(player, dropType));

            return result;
        }

        /**********************************************************
        *		 Marker functions
        ***********************************************************/

        private static IEnumerator beginAirdropViaMarker(Entity player, string dropType)
        {
            player.Notify("beginAirdropViaMarker");

            player.SetField("threwAirDropMarker", false);
            StartAsync(watchAirDropWeaponChange(player, dropType));
            StartAsync(watchAirDropMarkerUsage(player, dropType));
            StartAsync(watchAirDropMarker(player, dropType));

            string result = "";
            yield return player.WaitTill_any_return(new Action<string>(s => result = s), "notAirDropWeapon", "markerDetermined", "death");
            if (result != "" && result == "markerDetermined")
            {
                if (dropType == "airdrop_mega")
                    teamPlayerCardSplash("used_airdrop_mega", player);
            }
            else if ((result != "" && result == "death") && player.GetField<bool>("threwAirDropMarker"))
            { }

            //return false;
        }
        private static IEnumerator watchAirDropWeaponChange(Entity player, string dropType)
        {
            /*
            while (player.IsSwitchingWeapon())
            {
                yield return WaitForFrame();
            }
            */

            yield return player.WaitTill("weapon_change");

            string currentWeapon = player.GetCurrentWeapon();
            string airdropMarkerWeapon;

            if (isAirdropMarker(currentWeapon))
                airdropMarkerWeapon = currentWeapon;
            else
                airdropMarkerWeapon = "";

            while (isAirdropMarker(currentWeapon))
            {
                yield return player.WaitTill("weapon_change");

                currentWeapon = player.GetCurrentWeapon();

                if (isAirdropMarker(currentWeapon))
                    airdropMarkerWeapon = currentWeapon;
            }

            if (player.HasField("threwAirDropMarker") && player.GetField<bool>("threwAirDropMarker"))
            {
                //take weapon

                player.Notify("markerDetermined");
            }
            else
                player.Notify("notAirDropWeapon");
        }
        private static IEnumerator watchAirDropMarkerUsage(Entity player, string dropType)
        {
            while (true)
            {
                Parameter[] param = null;
                yield return player.WaitTill_return("grenade_pullback", new Action<Parameter[]>(p => param = p));

                if (param == null)
                    yield break;

                string weaponName = (string)param[0];

                if (!isAirdropMarker(weaponName))
                    continue;

                player.DisableUsability();

                StartAsync(beginAirDropMarkerTracking(player));
            }
        }
        private static IEnumerator watchAirDropMarker(Entity player, string dropType)
        {
            while (true)
            {
                Parameter[] param = null;
                yield return player.WaitTill_return("grenade_fire", new Action<Parameter[]>(p => param = p));

                if (param == null)
                    yield break;

                Entity airDropWeapon = (Entity)param[0];
                string weapname = (string)param[1];

                if (!isAirdropMarker(weapname))
                    continue;

                player.SetField("threwAirDropMarker", true);
                StartAsync(airdropDetonateOnStuck(airDropWeapon));

                airDropWeapon.SetField("owner", player);
                airDropWeapon.SetField("weaponName", weapname);

                StartAsync(airDropMarkerActivate(airDropWeapon, dropType));
            }
        }
        private static IEnumerator beginAirDropMarkerTracking(Entity player)
        {
            yield return player.WaitTill_any("grenade_fire", "weapon_change");
            player.EnableUsability();
        }
        private static IEnumerator airDropMarkerActivate(Entity marker, string dropType)
        {
            Parameter[] param = null;
            yield return marker.WaitTill_return("explode", new Action<Parameter[]>(p => param = p));

            if (param == null)
                yield break;

            Vector3 position = param[0].As<Vector3>();

            Entity owner = marker.GetField<Entity>("owner");

            //if (isEMPed(owner))
            //  yield break;

            if (isAirDenied(owner))
                yield break;

            if (IsSubStr(ToLower(dropType), "escort_airdrop") && chopper != null)
                yield break;

            if (IsSubStr(ToLower(dropType), "escort_airdrop") && chopper_fx.ContainsKey("smoke") && chopper_fx["smoke"].ContainsKey("signal_smoke_30sec"))
            {
                PlayFX(chopper_fx["smoke"]["signal_smoke_30sec"], position, new Vector3(0, 0, -1));
            }

            yield return WaitForFrame();

            if (IsSubStr(ToLower(dropType), "juggernaut"))
                doC130FlyBy(owner, position, RandomFloat(360), dropType);
            //else if (IsSubStr(ToLower(dropType), "escort_airdrop"))
            //escortairdrop.finishSupportEscortUsage(owner, position, RandomFloat(360), "escort_airdrop");
            else
                doFlyBy(owner, position, RandomFloat(360), dropType);
        }

        /**********************************************************
        *		 crate functions
        ***********************************************************/

        private static void initAirDropCrate(Entity crate)
        {
            crate.SetField("inUse", false);
            crate.Hide();

            if (!string.IsNullOrEmpty(crate.Target))
            {
                crate.SetField("collision", GetEnt(crate.Target, "targetname"));
                crate.GetField<Entity>("collision").NotSolid();
            }
            else
            {
                crate.ClearField("collision");
            }
        }

        private static IEnumerator deleteOnOwnerDeath(Entity crate, Entity owner)
        {
            yield return Wait(0.25f);
            crate.LinkTo(owner, "tag_origin", Vector3.Zero, Vector3.Zero);

            yield return owner.WaitTill("death");

            crate.Delete();
        }
        private static IEnumerator crateTeamModelUpdater(Entity crate)
        {

        }
        private static IEnumerator crateModelTeamUpdater(Entity crate, string showForTeam)
        {
            crate.Hide();

            foreach (Entity player in Players)
            {
                if (player.SessionTeam == showForTeam)
                    crate.ShowToPlayer(player);
            }
        }
        private static IEnumerator crateModelPlayerUpdater(Entity crate, Entity owner, bool friendly)
        {
            crate.Hide();

            foreach (Entity player in Players)
            {
                if (friendly && player != owner)
                    continue;
                if (!friendly && player == owner)
                    continue;

                crate.ShowToPlayer(player);
            }
        }



        private static bool isAirdropMarker(string weaponName)
        {
            switch (weaponName)
            {
                case "airdrop_marker_mp":
                case "airdrop_mega_marker_mp":
                case "airdrop_sentry_marker_mp":
                case "airdrop_juggernaut_mp":
                case "airdrop_juggernaut_def_mp":
                    return true;
                default:
                    return false;
            }
        }
    }
}
