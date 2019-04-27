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
    public class uav : BaseScript
    {
        public static int fx_ac130_engineeffect;
        public static int radarViewTime = 30;
        public static int uavBlockTime = 30;
        public static int uav_fx_explode;
        public static int uav_fx_trail;
        public static int fx_laserTarget;
        public static int fx_stingerFXid;

        public static Action<Entity> uavGiveFunc;
        public static Func<Entity, bool> uavUseFunc;
        public static Action<Entity> uavSupportGiveFunc;
        public static Func<Entity, bool> uavSupportUseFunc;
        public static Action<Entity> uav2GiveFunc;
        public static Func<Entity, bool> uav2UseFunc;
        public static Action<Entity> uavDoubleGiveFunc;
        public static Func<Entity, bool> uavDoubleUseFunc;
        public static Action<Entity> uavTripleGiveFunc;
        public static Func<Entity, bool> uavTripleUseFunc;
        public static Action<Entity> uavCounterGiveFunc;
        public static Func<Entity, bool> uavCounterUseFunc;
        public static Action<Entity> uavStrikeGiveFunc;
        public static Func<Entity, bool> uavStrikeUseFunc;
        public static Action<Entity> uavDirectionalGiveFunc;
        public static Func<Entity, bool> uavDirectionalUseFunc;

        public static Entity UAVRig;

        public uav()
        {
            fx_ac130_engineeffect = LoadFX("fire/jet_engine_ac130");
            uav_fx_explode = LoadFX("explosions/uav_advanced_death");
            uav_fx_trail = LoadFX("smoke/advanced_uav_contrail");
            fx_laserTarget = LoadFX("misc/laser_glow");

            uavGiveFunc = null;
            uavSupportGiveFunc = null;
            uav2GiveFunc = null;
            uavDoubleGiveFunc = null;
            uavTripleGiveFunc = null;
            uavCounterGiveFunc = null;
            uavStrikeGiveFunc = UAVStrikeSetup;
            uavDirectionalGiveFunc = null;

            uavUseFunc = tryUseUAV;
            uavSupportUseFunc = tryUseUAVSupport;
            uav2UseFunc = tryUseUAV;
            uavDoubleUseFunc = tryUseDoubleUAV;
            uavTripleUseFunc = tryUseTripleUAV;
            uavCounterUseFunc = tryUseCounterUAV;
            uavStrikeUseFunc = tryUseUAVStrike;
            uavDirectionalUseFunc = tryUseDirectionalUAV;

            //UAVRig spawned already
            AfterDelay(100, () => UAVRig = GetEnt("uavrig_script_model", "targetname"));

            if (isTeamBased)
            {
                radarMode.Add("allies", "normal_radar");
                radarMode.Add("axis", "normal_radar");
                activeUAVs.Add("allies", 0);
                activeUAVs.Add("axis", 0);
                activeCounterUAVs.Add("allies", 0);
                activeCounterUAVs.Add("axis", 0);

                uavModels.Add("allies", new List<Entity>());
                uavModels.Add("axis", new List<Entity>());
            }
            else
            {
                //Setup radars by guid on connect
                PlayerConnected += onPlayerConnect;
            }

            AfterDelay(100, () => UAVRig.OnNotify("uav_update", (u) => UAVTracker()));
        }

        private static void onPlayerConnect(Entity player)
        {
            activeUAVs.Add(player.GUID.ToString(), 0);
            activeUAVs.Add(player.GUID.ToString() + "_radarStrength", 0);
            activeCounterUAVs.Add(player.GUID.ToString(), 0);
            uavModels.Add(player.GUID.ToString(), new List<Entity>());

            radarMode.Add(player.GUID.ToString(), "normal_radar");
        }

        private static IEnumerator launchUAV(Entity owner, string team, int duration, string uavType)
        {
            bool isCounter;
            if (uavType == "counter_uav")
                isCounter = true;
            else
                isCounter = false;

            Entity UAVModel = Spawn("script_model", UAVRig.GetTagOrigin("tag_origin"));

            UAVModel.SetField("value", 1);
            if (uavType == "double_uav")
                UAVModel.SetField("value", 2);
            else if (uavType == "triple_uav")
                UAVModel.SetField("value", 3);

            if (UAVModel.GetField<int>("value") != 3)
            {
                UAVModel.SetModel("vehicle_uav_static_mp");

                UAVModel.SetCanDamage(true);

                UAVModel.Health = 999999;
                UAVModel.SetField("maxHealth", 1000);
                UAVModel.SetField("damageTaken", 0);

                StartAsync(damageTracker(UAVModel, isCounter, false));
            }
            else
            {
                UAVModel.SetModel("vehicle_phantom_ray");

                UAVModel.SetCanDamage(true);

                UAVModel.Health = 999999;
                UAVModel.SetField("maxHealth", 2000);
                UAVModel.SetField("damageTaken", 0);

                spawnFxDelay(UAVModel, uav_fx_trail, "tag_jet_trail");
                StartAsync(damageTracker(UAVModel, isCounter, true));
                
            }

            UAVModel.SetField("team", team);
            UAVModel.SetField("owner", owner);
            UAVModel.SetField("timeToAdd", 0);

            StartAsync(handleIncomingStinger(UAVModel));

            addUAVModel(UAVModel);

            float zOffset = RandomIntRange(3000, 5000);

            // we need to make sure the uav doesn't go higher than 8100 units because bullets die at 8192
            List<Entity> spawns = getLevelSpawnpoints("tdm");

            Entity lowestSpawn = spawns[0];
            foreach (Entity spawn in spawns)
            {
                if (spawn.Origin.Z < lowestSpawn.Origin.Z)
                    lowestSpawn = spawn;
            }
            float lowestZ = lowestSpawn.Origin.Z;
            float UAVRigZ = UAVRig.Origin.Z;
            if (lowestZ < 0)
            {
                UAVRigZ += lowestZ * -1;
                lowestZ = 0;
            }
            float diffZ = UAVRigZ - lowestZ;
            if (diffZ + zOffset > 8100.0f)
            {
                zOffset -= ((diffZ + zOffset) - 8100.0f);
            }

            int angle = RandomInt(360);
            int radiusOffset = RandomInt(2000) + 5000;

            float xOffset = Cos(angle) * radiusOffset;
            float yOffset = Sin(angle) * radiusOffset;

            Vector3 angleVector = VectorNormalize(new Vector3(xOffset, yOffset, zOffset));
            angleVector = angleVector * RandomIntRange(6000, 7000);

            UAVModel.LinkTo(UAVRig, "tag_origin", angleVector, new Vector3(0, angle - 90, 0));

            StartAsync(updateUAVModelVisibility(UAVModel));

            if (isCounter)
            {
                UAVModel.SetField("uavType", "counter");
                addActiveCounterUAV(UAVModel);
            }
            else
            {
                addActiveUAV(UAVModel);
                UAVModel.SetField("uavType", "standard");
            }

            //this adds 5 seconds of time to all active UAV's of the same type.
            if (activeUAVs.ContainsKey(team))
            {
                foreach (Entity uav in uavModels[team])
                {
                    if (uav == UAVModel)
                        continue;

                    if (uav.GetField<string>("uavType") == "counter" && isCounter)
                        uav.SetField("timeToAdd", uav.GetField<string>("timeToAdd") + 5);
                    if (uav.GetField<string>("uavType") == "standard" && !isCounter)
                        uav.SetField("timeToAdd", uav.GetField<string>("timeToAdd") + 5);
                }
            }

            UAVRig.Notify("uav_update");

            switch (uavType)
            {
                case "uav_strike":
                    duration = 2;
                    break;
                default:
                    //duration = duration - 7;
                    break;
            }
            yield return Wait(duration);

            if (!Utilities.isEntDefined(UAVModel))
                yield break;

            if (UAVModel.GetField<int>("damageTaken") < UAVModel.GetField<int>("maxHealth"))
            {
                UAVModel.Unlink();

                Vector3 destPoint = UAVModel.Origin + (AnglesToForward(UAVModel.Angles) * 20000);
                UAVModel.MoveTo(destPoint, 60);
                PlayFXOnTag(fx_ac130_engineeffect, UAVModel, "tag_origin");

                yield return UAVModel.WaitTill_notify_or_timeout("death", 3);

                if (UAVModel.GetField<int>("damageTaken") < UAVModel.GetField<int>("maxHealth"))
                {
                    UAVModel.Notify("leaving");
                    UAVModel.SetField("isLeaving", true);
                    UAVModel.MoveTo(destPoint, 4, 4, 0.0f);
                }

                if (UAVModel.HasField("isDestroyed")) UAVModel.ClearField("isDestroyed");

                yield return UAVModel.WaitTill_notify_or_timeout("uav_death", 4 + UAVModel.GetField<int>("timeToAdd"));
            }

            if (isCounter)
                removeActiveCounterUAV(UAVModel);
            else
                removeActiveUAV(UAVModel);

            //UAVModel.Notify("uav_death");
            UAVModel.Delete();
            removeUAVModel(UAVModel);

            if (uavType == "directional_uav")
            {
                owner.RadarShowEnemyDirection = false;
                if (isTeamBased)
                {
                    foreach (Entity player in Players)
                    {
                        if (player.SessionTeam == team)
                        {
                            player.RadarShowEnemyDirection = false;
                        }
                    }
                }
            }

            UAVRig.Notify("uav_update");
        }

        private static void spawnFxDelay(Entity entity, int fxID, string tag)
        {
            AfterDelay(500, () => PlayFXOnTag(fxID, entity, tag));
        }

        private static IEnumerator monitorUAVStrike(Entity player)
        {
            yield return player.WaitTill_any("death", "uav_strike_cancel", "uav_strike_successful");

            if (!player.IsAlive)
                yield break;

            if (!player.HasField("uavStrikeSuccessful"))
                yield break;

            else
                yield break;
        }

        private static IEnumerator showLazeMessage(Entity player)
        {
            HudElem msg = HudElem.CreateFontString(player, HudElem.Fonts.BigFixed, 0.75f);
            msg.SetPoint("CENTER", "CENTER", 0, 150);
            msg.SetText("Lase target for Predator Strike.");

            AfterDelay(4000, () => player.Notify("uav_strike_destroy_message"));

            yield return player.WaitTill_any("death", "uav_strike_successful", "uav_strike_cancel", "uav_strike_destroy_message");

            msg.Destroy();
        }

        private static IEnumerator waitForLazeDiscard(Entity player)
        {
            yield return player.WaitTill("weapon_change");

            if (player.CurrentWeapon != "uav_strike_marker_mp")
            {
                player.Notify("uav_strike_cancel");
                yield break;
            }
            else
            {
                yield return WaitForFrame();
                StartAsync(waitForLazeDiscard(player));
                yield break;
            }
        }

        private static IEnumerator waitForLazedTarget(Entity player)
        {
            StartAsync(showLazeMessage(player));
            StartAsync(waitForLazeDiscard(player));

            string weapon = player.GetField<string>("lastDroppableWeapon");
            player.GiveWeapon("uav_strike_marker_mp");
            player.SwitchToWeapon("uav_strike_marker_mp");

            Vector3 targetPosition;

            string msg = "";
            yield return player.WaitTill_any_return(new Action<string>((s) => msg = s), "weapon_fired", "uav_strike_cancel");

            if (msg == "uav_strike_cancel")
                yield break;

            bool timedOut = false;
            yield return player.WaitTill_notify_or_timeout("uav_strike_lazed", 5, new Action<string>((s) => timedOut = s == "timeout"));

            if (timedOut && !player.HasField("uavStrikeTracePos"))
            {
                Vector3 origin = player.GetEye();
                Vector3 forward = AnglesToForward(player.GetPlayerAngles());
                Vector3 endPoint = PhysicsTrace(origin, origin + forward * 1000);

                targetPosition = endPoint;
            }
            else
                targetPosition = player.GetField<Vector3>("uavStriketracePos");

            player.Notify("uav_strike_used");

            Entity fxEnt = SpawnFX(fx_laserTarget, targetPosition);
            TriggerFX(fxEnt);
            waitFxEntDie(fxEnt);

            MagicBullet("uav_strike_projectile_mp", targetPosition + new Vector3(0, 0, 4000), targetPosition, player);

            player.TakeWeapon("uav_strike_marker_mp");
            if (msg != "uav_strike_cancel")
            {
                player.SwitchToWeapon(weapon);
                player.Notify("used_killstreak");
            }

            player.Notify("uav_strike_successful");
        }


        private static void waitFxEntDie(Entity fx)
        {
            AfterDelay(2000, () => fx.Delete());
        }

        private static IEnumerator updateUAVModelVisibility(Entity UAVModel)
        {
            yield return level.WaitTill_any("joined_team", "uav_update");

            UAVModel.Hide();
            foreach (Entity player in Players)
            {
                if (isTeamBased)
                {
                    if (player.SessionTeam != UAVModel.GetField<string>("team"))
                        UAVModel.ShowToPlayer(player);
                }
                else
                {
                    if (UAVModel.HasField("owner") && player == UAVModel.GetField<Entity>("owner"))
                        continue;

                    UAVModel.ShowToPlayer(player);
                }
            }

            if (Utilities.isEntDefined(UAVModel) && UAVModel.HasField("owner"))
                StartAsync(updateUAVModelVisibility(UAVModel));//Loop
        }

        private static IEnumerator damageTracker(Entity UAVModel, bool isCounter, bool isAdvanced)
        {
            Parameter[] param = null;

            yield return UAVModel.WaitTill_return("damage", new Action<Parameter[]>((p) => param = p));

            if (param == null)
                yield break;

            int damage = (int)param[0];
            Entity attacker = (Entity)param[1];
            string meansOfDeath = (string)param[4];
            string weapon = (string)param[9];

            if (attacker.Classname != "player")
            {
                StartAsync(damageTracker(UAVModel, isCounter, isAdvanced));
                yield break;
            }

            UAVModel.SetField("wasDamaged", true);

            int modifiedDamage = damage;

            if (attacker.Classname == "player")
            {
                updateDamageFeedback(attacker, "");

                if (meansOfDeath == "MOD_RIFLE_BULLET" || meansOfDeath == "MOD_PISTOL_BULLET")
                {
                    if (attacker.HasPerk("specialty_armorpiercing"))
                        modifiedDamage += damage * (int)armorPiercingMod;
                }
            }

            if (weapon != "")
            {
                switch (weapon)
                {
                    case "stinger_mp":
                    case "javelin_mp":
                        UAVModel.SetField("largeProjectileDamage", true);
                        modifiedDamage = UAVModel.GetField<int>("maxHealth") + 1;
                        break;
                    case "sam_projectile_mp":
                        UAVModel.SetField("largeProjectileDamage", true);
                        float mult = 0.25f;
                        if (isAdvanced)
                            mult = 0.15f;
                        modifiedDamage = (int)(UAVModel.GetField<int>("maxHealth") * mult);
                        break;
                }
            }

            UAVModel.SetField("damageTaken", UAVModel.GetField<int>("damageTaken") + modifiedDamage);

            if (UAVModel.GetField<int>("damageTaken") >= UAVModel.GetField<int>("maxHealth"))
            {
                if (attacker.Classname == "player" && (!UAVModel.HasField("owner") || attacker != UAVModel.GetField<Entity>("owner")))
                {
                    UAVModel.SetField("isDestroyed", true);
                    UAVModel.Hide();
                    Vector3 forward = (AnglesToRight(UAVModel.Angles) * 200);
                    PlayFX(uav_fx_explode, UAVModel.Origin, forward);

                    if (UAVModel.HasField("uavType") && UAVModel.GetField<string>("uavType") == "remote_mortar")
                        teamPlayerCardSplash("callout_destroyed_remote_mortar", attacker);
                    else if (isCounter)
                        teamPlayerCardSplash("callout_destroyed_counter_uav", attacker);
                    else
                        teamPlayerCardSplash("callout_destroyed_uav", attacker);

                    //Vehicle killed
                    //Give XP
                    attacker.Notify("destroyed_killstreak");

                    //if (UAVModel.HasField("UAVRemoteMarkedBy") && UAVModel.GetField<Entity>("UAVRemoteMarkedBy") != attacker)
                    //process assist
                }

                UAVModel.Notify("uav_death");
                yield break;
            }

            StartAsync(damageTracker(UAVModel, isCounter, isAdvanced));
        }

        private static bool tryUseUAV(Entity player)
        {
            return useUAV(player, "uav");
        }
        private static bool tryUseUAVSupport(Entity player)
        {
            return useUAV(player, "uav_support");
        }
        private static bool tryUseDoubleUAV(Entity player)
        {
            return useUAV(player, "double_uav");
        }
        private static bool tryUseTripleUAV(Entity player)
        {
            return useUAV(player, "triple_uav");
        }
        private static bool tryUseCounterUAV(Entity player)
        {
            return useUAV(player, "counter_uav");
        }
        private static void UAVStrikeSetup(Entity player)
        {
            player.SetField("usedStrikeUAV", 0);
            player.OnNotify("missile_fire", (ent, missile, weapon) =>
            {
                missile.As<Entity>().OnNotify("explode", (mis, pos) =>
                {
                    player.SetField("uavStrikeTracePos", pos);
                    player.Notify("uav_strike_lazed");
                });
            });
        }
        private static bool tryUseUAVStrike(Entity player)
        {
            /*
            if (player.GetField<int>("usedStrikeUAV") == 0)
            {
                player.SetField("usedStrikeUAV", 1);
                
            }
            */

            StartAsync(waitForLazedTarget(player));
            StartAsync(monitorUAVStrike(player));
            return true;
        }
        private static bool tryUseDirectionalUAV(Entity player)
        {
            return useUAV(player, "directional_uav");
        }

        private static bool useUAV(Entity player, string uavType)
        {
            string team = player.SessionTeam;
            int useTime = radarViewTime;

            StartAsync(launchUAV(player, team, useTime, uavType));

            switch (uavType)
            {
                case "counter_uav":
                    player.Notify("used_counter_uav");
                    break;
                case "double_uav":
                    player.Notify("used_double_uav");
                    break;
                case "triple_uav":
                    teamPlayerCardSplash("used_triple_uav", player, true);
                    player.Notify("used_triple_uav");
                    break;
                case "directional_uav":
                    player.RadarShowEnemyDirection = true;
                    if (isTeamBased)
                    {
                        foreach (Entity players in Players)
                        {
                            if (players.SessionTeam == team)
                            {
                                players.RadarShowEnemyDirection = true;
                            }
                        }
                    }
                    teamPlayerCardSplash("used_directional_uav", player, true);
                    player.Notify("used_directional_uav");
                    break;
                default:
                    player.Notify("used_uav");
                    break;
            }

            return true;
        }

        private static void UAVTracker()
        {
            if (isTeamBased)
            {
                updateTeamUAVStatus("allies");
                updateTeamUAVStatus("axis");
            }
            else
            {
                updatePlayersUAVStatus();
            }
        }

        private static int _getRadarStrength(string team)
        {
            int activeUAVs = 0;
            int activeCounterUAVs = 0;

            foreach (Entity uav in uavModels[team])
            {
                if (uav.GetField<string>("uavType") == "counter")
                    continue;

                if (uav.GetField<string>("uavType") == "remote_mortar")
                    continue;

                activeUAVs += uav.GetField<int>("value");
            }

            foreach (Entity uav in uavModels[otherTeam[team]])
            {
                if (uav.GetField<string>("uavType") != "counter")
                    continue;

                activeCounterUAVs += uav.GetField<int>("value");
            }

            int radarStrength;
            if (activeCounterUAVs > 0)
                radarStrength = -3;
            else
                radarStrength = activeUAVs;

            int strengthMin = GetUAVStrengthMin();
            int strengthMax = GetUAVStrengthMax();

            //clamp between min/max
            if (radarStrength <= strengthMin)
            {
                radarStrength = strengthMin;
            }
            else if (radarStrength >= strengthMax)
            {
                radarStrength = strengthMax;
            }

            return radarStrength;
        }

        private static void updateTeamUAVStatus(string team)
        {
            int radarStrength = _getRadarStrength(team);

            SetTeamRadarStrength(team, radarStrength);

            if (radarStrength >= GetUAVStrengthLevelNeutral())
                UnBlockTeamRadar(team);
            else
                BlockTeamRadar(team);

            if (radarStrength <= GetUAVStrengthLevelNeutral())
            {
                setTeamRadarWrapper(team, 0);
                updateTeamUAVType(team);
                return;
            }

            if (radarStrength >= GetUAVStrengthLevelShowEnemyFastSweep())
                radarMode[team] = "fast_radar";
            else
                radarMode[team] = "normal_radar";

            updateTeamUAVType(team);
            setTeamRadarWrapper(team, 1);
        }

        private static void updatePlayersUAVStatus()
        {
            int strengthMin = GetUAVStrengthMin();
            int strengthMax = GetUAVStrengthMax();
            int strengthDirectional = GetUAVStrengthLevelShowEnemyDirectional();

            foreach (Entity player in Players)
            {
                int radarStrength = activeUAVs[player.GUID.ToString() + "_radarStrength"];

                // if there are any counters up that aren't this player's then they are blocked
                foreach (Entity enemyPlayer in Players)
                {
                    if (enemyPlayer == player)
                        continue;

                    if (!activeCounterUAVs.ContainsKey(enemyPlayer.GUID.ToString()))
                        continue;

                    int _activeCounterUAVs = activeCounterUAVs[enemyPlayer.GUID.ToString()];
                    if (_activeCounterUAVs > 0)
                    {
                        radarStrength = -3;
                        break;
                    }
                }

                //clamp between min/max
                if (radarStrength <= strengthMin)
                {
                    radarStrength = strengthMin;
                }
                else if (radarStrength >= strengthMax)
                {
                    radarStrength = strengthMax;
                }

                player.RadarStrength = radarStrength;

                if (radarStrength >= GetUAVStrengthLevelNeutral())
                    player.IsRadarBlocked = false;
                else
                    player.IsRadarBlocked = true;

                if (radarStrength <= GetUAVStrengthLevelNeutral())
                {
                    player.HasRadar = false;
                    player.RadarShowEnemyDirection = false;
                    continue;
                }

                if (radarStrength >= GetUAVStrengthLevelShowEnemyFastSweep())
                    player.RadarMode = "fast_radar";
                else
                    player.RadarMode = "normal_radar";

                player.RadarShowEnemyDirection = radarStrength >= strengthDirectional;

                player.HasRadar = true;
            }
        }

        private static void blockPlayerUAV(Entity player)
        {
            player.Notify("blockPlayerUAV");

            player.IsRadarBlocked = true;

            AfterDelay(uavBlockTime * 1000, () => player.IsRadarBlocked = false);
        }

        private static void updateTeamUAVType(string team)
        {
            bool shouldBeDirectional = _getRadarStrength(team) >= GetUAVStrengthLevelShowEnemyDirectional();

            foreach (Entity player in Players)
            {
                if (player.SessionTeam == "spectator")
                    continue;

                player.RadarMode = radarMode[player.SessionTeam];

                if (player.SessionTeam == team)
                {
                    player.RadarShowEnemyDirection = shouldBeDirectional;
                }
            }
        }

        private static void usePlayerUAV(Entity player, bool doubleUAV, int useTime)
        {
            player.Notify("usePlayerUAV");

            if (doubleUAV)
                player.RadarMode = "fast_radar";
            else
                player.RadarMode = "normal_radar";

            player.HasRadar = true;

            AfterDelay(useTime * 1000, () => player.HasRadar = false);
        }

        private static void setTeamRadarWrapper(string team, int value)
        {
            SetTeamRadar(team, value == 1);
            UAVRig.Notify("radar_status_change", team);
        }

        private static IEnumerator handleIncomingStinger(Entity UAVModel)
        {
            Parameter[] param = null;

            yield return level.WaitTill_return("stinger_fired", new Action<Parameter[]>((p) => param = p));

            if (param == null)
                yield break;

            Entity player = (Entity)param[0];
            Entity missile = (Entity)param[1];
            Entity lockTarget = (Entity)param[2];

            if (!Utilities.isEntDefined(UAVModel))
                yield break;

            if (!Utilities.isEntDefined(lockTarget) || (lockTarget != UAVModel))
            {
                StartAsync(handleIncomingStinger(UAVModel));
                yield break;
            }

            stingerProximityDetonate(missile, lockTarget, player);
            StartAsync(handleIncomingStinger(UAVModel));
        }

        private static void stingerProximityDetonate(Entity missile, Entity targetEnt, Entity player)
        {
            float minDist = Distance(missile.Origin, targetEnt.GetPointInBounds(Vector3.Zero));
            Vector3 lastCenter = targetEnt.GetPointInBounds(Vector3.Zero);

            OnInterval(50, () =>
            {
                Vector3 center;
                if (targetEnt.Classname == "")
                    center = lastCenter;
                else
                    center = targetEnt.GetPointInBounds(Vector3.Zero);

                lastCenter = center;

                float curDist = Distance(missile.Origin, center);

                if (curDist < minDist)
                    minDist = curDist;

                if (curDist > minDist)
                {
                    if (curDist > 1536)
                        return true;

                    RadiusDamage(missile.Origin, 1536, 600, 600, player, "MOD_EXPLOSIVE", "stinger_mp");
                    PlayFX(fx_stingerFXid, missile.Origin);

                    missile.Hide();

                    missile.Notify("deleted");
                    AfterDelay(50, () =>
                    {
                        missile.Delete();
                        player.Notify("killstreak_destroyed");
                    });
                    return false;
                }
                if (missile.Classname != "") return true;
                return false;
            });
        }

        private static void addUAVModel(Entity UAVModel)
        {
            if (isTeamBased)
                uavModels[UAVModel.GetField<string>("team")].Add(UAVModel);
            else
                uavModels[UAVModel.GetField<Entity>("owner").GUID.ToString()].Add(UAVModel);
        }

        public static void removeUAVModel(Entity UAVModel)
        {
            if (isTeamBased)
            {
                List<Entity> UAVModels = new List<Entity>();

                string team = UAVModel.GetField<string>("team");

                foreach (Entity uavModel in uavModels[team])
                {
                    if (uavModel.HasField("isDestroyed"))
                        continue;

                    UAVModels.Add(uavModel);
                }

                uavModels[team] = UAVModels;
            }
            else
            {
                Dictionary<string, List<Entity>> UAVModels = new Dictionary<string, List<Entity>>();
                foreach (string keys in uavModels.Keys)
                {
                    List<Entity> uavs = new List<Entity>();
                    foreach (Entity uavModel in uavModels[keys])
                    {
                        if (uavModel.HasField("isDestroyed"))
                            continue;

                        uavs.Add(uavModel);
                    }

                    UAVModels.Add(keys, uavs);
                }

                uavModels = UAVModels;
            }
        }

        private static void addActiveUAV(Entity UAVModel)
        {
            if (isTeamBased)
            {
                activeUAVs[UAVModel.GetField<string>("team")]++;
            }
            else
            {
                activeUAVs[UAVModel.GetField<Entity>("owner").GUID.ToString()]++;
                activeUAVs[UAVModel.GetField<Entity>("owner").GUID.ToString() + "_radarStrength"] += UAVModel.GetField<int>("value");
            }
        }
        private static void addActiveCounterUAV(Entity UAVModel)
        {
            if (isTeamBased)
                activeCounterUAVs[UAVModel.GetField<string>("team")]++;
            else
                activeCounterUAVs[UAVModel.GetField<Entity>("owner").GUID.ToString()]++;
        }

        private static void removeActiveUAV(Entity UAVModel)
        {
            if (isTeamBased)
            {
                activeUAVs[UAVModel.GetField<string>("team")]--;
            }
            else
            {
                activeUAVs[UAVModel.GetField<Entity>("owner").GUID.ToString()]--;
                activeUAVs[UAVModel.GetField<Entity>("owner").GUID.ToString() + "_radarStrength"] -= UAVModel.GetField<int>("value");
            }
        }
        private static void removeActiveCounterUAV(Entity UAVModel)
        {
            if (isTeamBased)
                activeCounterUAVs[UAVModel.GetField<string>("team")]--;
            else
                activeCounterUAVs[UAVModel.GetField<Entity>("owner").GUID.ToString()]--;
        }
    }
}
