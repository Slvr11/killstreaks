using System;
using InfinityScript;
using System.Collections;
using static killstreaks.killstreaks;
using static InfinityScript.GSCFunctions;

public class nuke : BaseScript
{
    private static int[] effects = new int[3];
    private static int nukeTimer = 10;
    private static int cancelMode = 0;
    private static int nukeEmpTimeout = 60;
    private static bool nukeIncoming = false;
    private static Entity nukeInfo;
    public static int nukeEmpTimeRemaining;
    private static bool nukeChainsKills = false;
    private static bool destroyExplosives = false;
    private static bool explosivesDestroyed = false;
    private static bool nukeSloMotion = true;
    private static bool nukeEndsGame = false;
    //private Entity nukePlayer;
    //private string nukeTeam;
    private static readonly Entity level = Entity.GetEntity(2046);

    public static Action<Entity> nukeGiveFunc;
    public static Func<Entity, bool> nukeUseFunc;

    public nuke()
    {
        nukeGiveFunc = giveNuke;
        nukeUseFunc = tryUseNuke;

        effects[0] = LoadFX("explosions/player_death_nuke");
        effects[1] = LoadFX("explosions/player_death_nuke_flash");
        effects[2] = LoadFX("dust/nuke_aftermath_mp");
        nukeTimer = GetDvarInt("scr_nukeTimer");
        cancelMode = GetDvarInt("scr_nukeCancelMode");
        SetDvarIfUninitialized("scr_killstreaksChainToNuke", 0);
        SetDvarIfUninitialized("scr_nukeDestroysExplosives", 0);
        SetDvarIfUninitialized("scr_nukeSloMo", 1);
        SetDvarIfUninitialized("scr_nukeEndsGame", 0);
        nukeInfo = Spawn("script_model", Vector3.Zero);
        //level.SetField("nukeDetonated", 0);
        level.SetField("teamNukeEMPed_axis", false);
        level.SetField("teamNukeEMPed_allies", false);
        level.SetField("teamNukeEMPed_none", false);
        nukeChainsKills = GetDvarInt("scr_killstreaksChainToNuke") != 0;
        destroyExplosives = GetDvarInt("scr_nukeDestroysExplosives") != 0;
        nukeSloMotion = GetDvarInt("scr_nukeSloMo") != 0;
        nukeEndsGame = GetDvarInt("scr_nukeEndsGame") != 0;

        PlayerConnected += OnPlayerConnected;
    }

    private static void OnPlayerConnected(Entity player)
    {
        player.SpawnedPlayer += () => OnPlayerSpawned(player);
        //player.OnNotify("weapon_change", (p, weapon) => OnWeaponChange(p, (string)weapon));
        player.SetField("hasNuke", false);
        player.SetField("nuked", false);
        player.SetField("isEMPed", false);
        //Set vision from connect
        if (level.HasField("nukeDetonated"))
            player.VisionSetNakedForPlayer("aftermath", 0);
    }

    private static void OnPlayerSpawned(Entity player)
    {
        AfterDelay(50, () =>
            {
                if (level.GetField<bool>("teamNukeEMPed_" + player.SessionTeam))
                {
                    if (isTeamBased)
                        player.SetEMPJammed(true);
                    else
                    {
                        if (!nukeInfo.HasField("player") || (nukeInfo.HasField("player") && player != nukeInfo.GetField<Entity>("player") && nukeEmpTimeRemaining > 0))
                            player.SetEMPJammed(true);
                    }
                }
            });
        if (level.HasField("nukeDetonated"))
            player.VisionSetNakedForPlayer("aftermath", 0);
    }

    public static void giveNuke(Entity player)
    {
        player.SetField("hasNuke", true);
    }

    public static bool tryUseNuke(Entity player)
    {
        if (nukeIncoming)
        {
            player.IPrintLnBold("M.O.A.B. already inbound!");
            return false;
        }
        if (nukeEmpTimeRemaining > 0 && level.GetField<bool>("teamNukeEMPed_" + player.SessionTeam) && isTeamBased)
        {
            player.IPrintLnBold("M.O.A.B. fallout still active for " + nukeEmpTimeRemaining.ToString() + " seconds.");
            return false;
        }
        else if (!isTeamBased && nukeEmpTimeRemaining > 0 && nukeInfo.HasField("player") && nukeInfo.GetField<Entity>("player") != player)
        {
            player.IPrintLnBold("M.O.A.B. fallout still active for " + nukeEmpTimeRemaining.ToString() + " seconds.");
            return false;
        }
        if (!player.IsPlayer)
        {
            InfinityScript.Log.Write(LogLevel.Error, "Nuke attempted to call in from a non-player entity!");
            return false;
        }
        doNuke(player, false, false);

        player.SetField("hasNuke", false);

        player.Notify("used_nuke");
        return true;
    }

    private static void delaythread_nuke(int delay, Action func)
    {
        AfterDelay(delay, func);
    }

    private static void doNuke(Entity player, bool allowCancel, bool instant)
    {
        nukeInfo.SetField("player", player);
        nukeInfo.SetField("team", player.SessionTeam);
        nukeIncoming = true;
        SetDvar("ui_bomb_timer", 4);

        if (isTeamBased)
            teamPlayerCardSplash("used_nuke", player);
        else
            player.IPrintLnBold("Friendly M.O.A.B. inbound!");
        delaythread_nuke((nukeTimer * 1000) - 3300, new Action(nukeSoundIncoming));
        delaythread_nuke((nukeTimer * 1000), new Action(nukeSoundExplosion));
        if (nukeSloMotion) delaythread_nuke((nukeTimer * 1000), new Action(nukeSloMo));
        delaythread_nuke((nukeTimer * 1000), new Action(nukeEffects));
        delaythread_nuke((nukeTimer * 1000) + 250, new Action(nukeVision));
        delaythread_nuke((nukeTimer * 1000) + 1500, new Action(nukeDeath));
        if (destroyExplosives && !explosivesDestroyed) delaythread_nuke((nukeTimer * 1000) + 1600, destroyDestructables);
        //delaythread_nuke((nukeTimer * 1000) + 1500, new Action(nukeEarthquake));
        if (!level.HasField("nukeDetonated")) nukeAftermathEffect();
        update_ui_timers();

        if (cancelMode != 0 && allowCancel)
            cancelNukeOnDeath(player);

        int nukeTimer_loc = nukeTimer;
        OnInterval(1000, () =>
            {
                if (nukeTimer_loc > 0)
                {
                    level.PlaySound("ui_mp_nukebomb_timer");
                    nukeTimer_loc--;
                    return true;
                }
                return false;
            });
    }

    private static void cancelNukeOnDeath(Entity player)
    {
        OnInterval(50, () =>
            {
                if (!player.IsAlive || !player.IsPlayer)
                {
                    //if (Function.Call<int>(40, player) != 0 && cancelMode == 2)
                    //{ //Do EMP stuff here, can't be arsed to recode _emp!
                    //}

                    SetDvar("ui_bomb_timer", 0);
                    nukeIncoming = false;
                    Notify("nuke_cancelled");
                    return false;
                }
                if (nukeIncoming) return true;
                else return false;
            });
    }

    private static void nukeSoundIncoming()
    {
        foreach (Entity players in Players)
        {
            if (!players.IsPlayer) continue;
            players.PlayLocalSound("nuke_incoming");
        }
    }

    private static void nukeSoundExplosion()
    {
        foreach (Entity players in Players)
        {
            if (!players.IsPlayer) continue;
            players.PlayLocalSound("nuke_explosion");
            players.PlayLocalSound("nuke_wave");
        }
    }

    private static void nukeEffects()
    {
        SetDvar("ui_bomb_timer", 0);

        level.SetField("nukeDetonated", true);

        foreach (Entity player in Players)
        {
            if (!player.IsPlayer) continue;
            Vector3 playerForward = AnglesToForward(player.Angles);
            playerForward = new Vector3(playerForward.X, playerForward.Y, 0);
            playerForward.Normalize();

            int nukeDistance = 5000;

            Entity nukeEnt = Spawn("script_model", player.Origin + (playerForward * nukeDistance));
            nukeEnt.SetModel("tag_origin");
            nukeEnt.Angles = new Vector3(0, (player.Angles.Y + 180), 90);

            nukeEffect(nukeEnt, player);
        }
    }

    private static void nukeEffect(Entity nukeEnt, Entity player)
    {
        AfterDelay(50, () =>
            PlayFXOnTagForClients(effects[1], nukeEnt, "tag_origin", player));
    }

    private static void nukeAftermathEffect()
    {
        //OnNotify("spawning_intermission"
        Entity aftermathEnt = GetEnt("mp_global_intermission", "classname");
        Vector3 up = AnglesToUp(aftermathEnt.Angles);
        Vector3 right = AnglesToRight(aftermathEnt.Angles);

        PlayFX(effects[2], aftermathEnt.Origin, up, right);
    }

    private static void nukeSloMo()
    {
        SetSlowMotion(1f, .35f, .5f);
        AfterDelay(500, () =>
        {
            SetDvar("fixedtime", 1);
            foreach (Entity player in Players)
            {
                player.SetClientDvar("fixedtime", 2);
            }
        });
        OnInterval(50, () =>
        {
            SetSlowMotion(.25f, 1, 2f);
            AfterDelay(1500, () =>
            { 
                foreach (Entity player in Players)
                {
                    player.SetClientDvar("fixedtime", 0);
                }
                SetDvar("fixedtime", 0);
            });
            if (nukeIncoming) return true;
            return false;
        });
    }

    private static void nukeVision()
    {
        level.SetField("nukeVisionInProgress", true);
        VisionSetNaked("mpnuke", 1);

        OnInterval(1000, () =>
            {
                VisionSetNaked("aftermath", 5);
                VisionSetPain("aftermath");
                if (nukeIncoming) return true;
                return false;
            });
    }

    private static void nukeDeath()
    {
        Notify("nuke_death");

        AmbientStop(1);

        foreach (Entity player in Players)
        {
            if (!player.IsPlayer) continue;
            if (!nukeEndsGame)
            {
                if (isTeamBased)
                {
                    if (nukeInfo.HasField("team") && player.SessionTeam == nukeInfo.GetField<string>("team")) continue;
                }
                else
                {
                    if (nukeInfo.HasField("player") && player == nukeInfo.GetField<Entity>("player")) continue;
                }
            }

            player.SetField("nuked", true);
            if (player.IsAlive)
                player.FinishPlayerDamage(nukeInfo.GetField<Entity>("player"), nukeInfo.GetField<Entity>("player"), 999999, 0, "MOD_EXPLOSIVE", "nuke_mp", player.Origin, player.Origin, "none", 0);
        }


        if (!nukeEndsGame)
        {
            nuke_EMPJam();

            nukeIncoming = false;
        }
        else
        {
            SetWinningPlayer(nukeInfo.GetField<Entity>("player"));
            if (isTeamBased) SetWinningTeam(nukeInfo.GetField<Entity>("player").SessionTeam);
            nukeInfo.GetField<Entity>("player").Notify("menuresponse", "nuke", "endround");
        }
    }
    /*
    private nukeEarthquake()
    {
        OnNotify("nuke_death", () =>
            {

            });
    }
     */

    private static void nuke_EMPJam()
    {
        if (isTeamBased)
        {
            Notify("EMP_JamTeam_axis");
            Notify("EMP_JamTeam_allies");
        }
        else Notify("EMP_JamPlayers");

        Notify("nuke_EMPJam");

        if (isTeamBased)
        {
            level.SetField("teamNukeEMPed_" + otherTeam[nukeInfo.GetField<string>("team")], true);
        }
        else
        {
            level.SetField("teamNukeEMPed_" + nukeInfo.GetField<string>("team"), true);
            //level.SetField("teamNukeEMPed_" + otherTeam[nukeInfo.GetField<string>("team")], true);
        }

        Notify("nuke_emp_update");

        keepNukeEMPTimeRemaining();

        AfterDelay(nukeEmpTimeout * 1000, () =>
            {
                if (isTeamBased)
                {
                    level.SetField("teamNukeEMPed_" + otherTeam[nukeInfo.GetField<string>("team")], false);
                }
                else
                {
                    level.SetField("teamNukeEMPed_" + nukeInfo.GetField<string>("team"), false);
                    //level.SetField("teamNukeEMPed_" + otherTeam[nukeInfo.GetField<string>("team")], false);
                }

                foreach (Entity player in Players)
                {
                    if (isTeamBased && player.SessionTeam == nukeInfo.GetField<string>("team"))
                        continue;

                    player.SetField("nuked", false);
                    player.SetEMPJammed(false);
                }

                Notify("nuke_emp_ended");
            });
    }

    private static void keepNukeEMPTimeRemaining()
    {
        Notify("keepNukeEMPTimeRemaining");

        nukeEmpTimeRemaining = nukeEmpTimeout;
        OnInterval(1000, () =>
            {
                nukeEmpTimeRemaining--;
                if (nukeEmpTimeRemaining > 0) return true;
                else return false;
            });
    }

    private static void nuke_EMPTeamTracker()
    {
        foreach (Entity player in Players)
        {
            if (!player.IsPlayer) continue;
            if (player.SessionTeam == "spectator")
                continue;

            if (isTeamBased)
            {
                if (nukeInfo.HasField("team") && player.SessionTeam == nukeInfo.GetField<string>("team"))
                    continue;
            }
            else
            {
                if (nukeInfo.HasField("player") && player == nukeInfo.GetField<Entity>("player"))
                    continue;
            }

            bool jam = level.GetField<bool>("teamNukeEMPed_" + player.SessionTeam);
            player.SetEMPJammed(jam);
        }
    }

    private static void update_ui_timers()
    {
        int nukeEndMilliseconds = (nukeTimer * 1000) + GetTime();
        SetDvar("ui_nuke_end_milliseconds", nukeEndMilliseconds);


    }

    private static void destroyDestructables()
    {
        if (explosivesDestroyed) return;
        Entity attacker;
        if (nukeInfo.HasField("player")) attacker = nukeInfo.GetField<Entity>("player");
        else attacker = null;
        for (int i = 18; i < 2047; i++)
        {
            Entity ent = Entity.GetEntity(i);
            if (ent == null) continue;
            string entTarget = ent.TargetName;
            string model = ent.Model;
            if (entTarget == "destructable" || entTarget == "destructible" || entTarget == "explodable_barrel" || model == "vehicle_hummer_destructible")
            {
                if (attacker == null) attacker = ent;
                ent.Notify("damage", 999999, attacker, new Vector3(0, 0, 0), new Vector3(0, 0, 0), "MOD_EXPLOSIVE", "", "", "", 0, "frag_grenade_mp");
            }
        }
        explosivesDestroyed = true;
    }
}
