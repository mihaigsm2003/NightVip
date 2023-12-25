﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Listeners;
using System;
using System.Data;
using System.Numerics;
using System.Text.Json.Serialization;
using static CounterStrikeSharp.API.Core.Listeners;
using CounterStrikeSharp.API.Modules.Cvars;
using static System.Formats.Asn1.AsnWriter;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace NightVip;

public class NightVipConfig : BasePluginConfig
{
    [JsonPropertyName("PluginStartTime")] public string PluginStartTime { get; set; } = "20:00:00";
    [JsonPropertyName("PluginEndTime")] public string PluginEndTime { get; set; } = "06:00:00";
    [JsonPropertyName("AutoGiveVip")] public bool AutoGiveVip { get; set; } = true;

    //[JsonPropertyName("EnableAutoBhop")] public bool EnableAutoBhop { get; set; } = true;

    [JsonPropertyName("GiveHealthShot")] public bool GiveHealthShot { get; set; } = true;
    [JsonPropertyName("GivePlayerWeapons")] public bool GivePlayerWeapons { get; set; } = true;
    [JsonPropertyName("UseScoreBoardTag")] public bool UseScoreBoardTag { get; set; } = true;
    [JsonPropertyName("EnableJumpCount")] public bool EnableJumpCount { get; set; } = true;
    [JsonPropertyName("ScoreBoardTag")] public string ScoreBoardTag { get; set; } = "[NightVip]";
    [JsonPropertyName("DisableVipRounds")] public string DisableVipRounds { get; set; } = "1,13";
    [JsonPropertyName("JumpCount")] public int JumpCount { get; set; } = 2;
    [JsonPropertyName("Health")] public int Health { get; set; } = 100;
    [JsonPropertyName("Armor")] public int Armor { get; set; } = 100;
    [JsonPropertyName("Money")] public int Money { get; set; } = 16000;
    [JsonPropertyName("Gravity")] public float Gravity { get; set; } = 1.0f;
    [JsonPropertyName("WeaponsList")] public List<string?> WeaponsList { get; set; } = new List<string?> { "weapon_ak47", "weapon_deagle" };
}

[MinimumApiVersion(126)]
public class NightVip : BasePlugin, IPluginConfig<NightVipConfig>
{
    public override string ModuleName => "NightVip";
    public override string ModuleVersion => "v1.4.0";
    public override string ModuleAuthor => "jockii";

    public static List<int?> _vips = new List<int?>();

    public static List<int> _noVipsRounds = new List<int>();

    public static Dictionary<CCSPlayerController, int> _jumps = new Dictionary<CCSPlayerController, int>();

    private static Dictionary<string, int> _weaponslot = new Dictionary<string, int>();

    public int _round = 0;

    public NightVipConfig Config { get; set; } = new NightVipConfig();

    public void OnConfigParsed(NightVipConfig config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        if (Config.AutoGiveVip)
            RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);

        Console.WriteLine($"##########################################\n" +
            $"Plugin: {ModuleName} {ModuleVersion}\nAuthor: {ModuleAuthor}\nInfo: https://github.com/jockii\nSupport me: https://www.buymeacoffee.com/jockii\n" +
            $"##########################################");

        RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            var pl = @event.Userid;

            if (_vips.Contains(pl.UserId))
                _vips.Remove(pl.UserId);

            if (_jumps.ContainsKey(pl))
                _jumps.Remove(pl);

            return HookResult.Continue;
        });

        //RegisterEventHandler<EventRoundStart>((@event, info) =>
        //{
        //    var players = Utilities.GetPlayers();

        //    return HookResult.Continue;
        //});

        RegisterEventHandler<EventRoundEnd>((@event, info) =>
        {
            _round++;

            return HookResult.Continue;
        });


        // for no vip rounds
        string[] _ = Config.DisableVipRounds.Split(',');

        foreach (var item in _)
        {
            int itemInteger = Convert.ToInt32(item);
            _noVipsRounds.Add(itemInteger);
        }

        AddTimer(2.0f, () => { CreateDictWeaponsNameSlot(); });
    }

    private Dictionary<string, int> CreateDictWeaponsNameSlot()
    {
        _weaponslot = new Dictionary<string, int>()
        {
            { "weapon_glock",               1 },
            { "weapon_hkp2000",             1 },
            { "weapon_usp_silencer",        1 },
            { "weapon_usp_silencer",        1 },
            { "weapon_p250",                1 },
            { "weapon_tec9",                1 },
            { "weapon_cz75a",               1 },
            { "weapon_fiveseven",           1 },
            { "weapon_deagle",              1 },
            { "weapon_revolver",            1 },
            { "weapon_mac10",               0 },
            { "weapon_mp9",                 0 },
            { "weapon_mp7",                 0 },
            { "weapon_mp5sd",               0 },
            { "weapon_ump45",               0 },
            { "weapon_p90",                 0 },
            { "weapon_bizon",               0 },
            { "weapon_nova",                0 },
            { "weapon_xm1014",              0 },
            { "weapon_sawedoff",            0 },
            { "weapon_mag7",                0 },
            { "weapon_m249",                0 },
            { "weapon_negev",               0 },
            { "weapon_galil",               0 },
            { "weapon_famas",               0 },
            { "weapon_ak47",                0 },
            { "weapon_m4a1",                0 },
            { "weapon_m4a1_silencer",       0 },
            { "weapon_ssg08",               0 },
            { "weapon_sg556",               0 },
            { "weapon_aug",                 0 },
            { "weapon_awp",                 0 },
            { "weapon_g3sg1",               0 },
            { "weapon_scar20",              0 },
            { "weapon_taser",               2 },
            { "weapon_molotov",             3 },
            { "weapon_incgrenade",          3 },
            { "weapon_decoy",               3 },
            { "weapon_flashbang",           3 },
            { "weapon_hegrenade",           3 },
            { "weapon_smokegrenade",        3 }
        };

        return _weaponslot;
    }

    private void GiveWeaponItem(CCSPlayerController pl, string item)
    {
        if (item == null || item == "")
            return;

        int itemSlot = _weaponslot[item];

        var weapons = pl.PlayerPawn.Value?.WeaponServices;

        foreach (var weapon in weapons!.MyWeapons)
        {
            if (weapon == null) continue;

            if (itemSlot > 3) continue;

            CCSWeaponBaseVData? _weapon = weapon?.Value?.As<CCSWeaponBase>().VData;

            if (_weapon == null) continue;

            if (_weapon.GearSlot == gear_slot_t.GEAR_SLOT_RIFLE && _weapon.GearSlot == (gear_slot_t)itemSlot)
            {

            }

            if (_weapon.GearSlot == gear_slot_t.GEAR_SLOT_PISTOL && _weapon.GearSlot == (gear_slot_t)itemSlot)
            {

            }

            if (_weapon.GearSlot == gear_slot_t.GEAR_SLOT_KNIFE && _weapon.GearSlot == (gear_slot_t)itemSlot)
            {

            }

            if (_weapon.GearSlot == gear_slot_t.GEAR_SLOT_GRENADES && _weapon.GearSlot == (gear_slot_t)itemSlot)
            {

            }
        }
        //CCSWeaponBaseVData? _weapon = weapon?.Value?.As<CCSWeaponBase>().VData;
    }

    //private void Bhop(CCSPlayerController pl) // try EventPlayerJump
    //{
    //    if (!pl.PawnIsAlive) return;

    //    if (!_vips.Contains(pl.UserId))
    //        return;

    //    if (_vips.Contains(pl.UserId))
    //    {
    //        var pawn = pl.Pawn.Value;
    //        var flags = (PlayerFlags)pawn!.Flags;
    //        var client = pl.Index;
    //        var buttons = pl.Buttons;

    //        if ((flags & PlayerFlags.FL_ONGROUND) != 0 && (buttons & PlayerButtons.Jump) != 0)
    //            pawn!.AbsVelocity.Z = 280;
    //    }
    //}

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV || !player.PlayerPawn.IsValid)
            return HookResult.Continue;
        else
            _vips.Add(player.UserId);

        return HookResult.Continue;
    }

    [ConsoleCommand("nightvip", "Add players in vips list")]
    [ConsoleCommand("nvip", "Add players in vips list")]
    [ConsoleCommand("nv", "Add players in vips list")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnCommandNightVip(CCSPlayerController? controller, CommandInfo info)
    {
        if (controller == null || !controller.IsValid || !controller.Pawn.IsValid || controller.IsBot || controller.IsHLTV) return;

        TimeSpan.TryParse(Config.PluginStartTime, out TimeSpan start_time);
        TimeSpan.TryParse(Config.PluginEndTime, out TimeSpan end_time);
        TimeSpan.TryParse(DateTime.Now.ToString("HH:mm:ss"), out TimeSpan current_time);

        if (start_time <= current_time || current_time < end_time)
        {
            if (_vips.Contains(controller.UserId))
                controller.PrintToChat($" {ChatColors.LightPurple}{Config.ScoreBoardTag} {ChatColors.LightRed}You already have status NightVip");

            if (!_vips.Contains(controller.UserId))
            {
                _vips.Add(controller.UserId);

                Server.NextFrame(() =>
                {
                    AddTimer(1.0f, () =>
                    {
                        controller.Pawn.Value!.Health = Config.Health;
                        controller.PawnArmor = Config.Armor;
                        controller.PawnHasHelmet = true;
                        controller.InGameMoneyServices!.Account = Config.Money;
                        controller.Pawn.Value.GravityScale = Config.Gravity;

                        if (Config.GiveHealthShot)
                            controller.GiveNamedItem("weapon_healthshot");

                        if (Config.UseScoreBoardTag)
                            controller.Clan = Config.ScoreBoardTag;
                    });
                });
                controller.PrintToChat($" {ChatColors.LightPurple}{Config.ScoreBoardTag} {ChatColors.Olive}Perfectly. Now you have privileges!");
            }
        }
        else
            controller.PrintToChat($" {ChatColors.LightPurple}{Config.ScoreBoardTag} {ChatColors.LightRed}In general, it is not yet night for a night vip");
    }

    [GameEventHandler(mode: HookMode.Post)]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var pl = @event.Userid;

        TimeSpan.TryParse(Config.PluginStartTime, out TimeSpan start_time);
        TimeSpan.TryParse(Config.PluginEndTime, out TimeSpan end_time);
        TimeSpan.TryParse(DateTime.Now.ToString("HH:mm:ss"), out TimeSpan current_time);

        if (start_time <= current_time || current_time < end_time)
        {
            if (pl == null || !pl.IsValid || !pl.Pawn.IsValid || pl.IsBot || pl.IsHLTV) return HookResult.Continue;

            if (_vips.Contains(pl.UserId))
            {
                if (_noVipsRounds.Contains(_round)) return HookResult.Continue;

                AddTimer(1.0f, () =>
                {
                    if (Config.GivePlayerWeapons)
                    {
                        for (int i = 0; i < Config.WeaponsList.Count; i++)
                        {
                            if (i > Config.WeaponsList.Count)
                                break;

                            GiveWeaponItem(pl, Config.WeaponsList[i]!);
                        }
                    }

                    pl.Pawn.Value!.Health = Config.Health;
                    pl.PlayerPawn.Value!.ArmorValue = Config.Armor;
                    pl.PawnHasHelmet = true;
                    pl.InGameMoneyServices!.Account = Config.Money;
                    pl.Pawn.Value.GravityScale = Config.Gravity;

                    if (Config.GiveHealthShot)
                        pl.GiveNamedItem("weapon_healthshot");

                    if (Config.UseScoreBoardTag)
                        pl.Clan = Config.ScoreBoardTag;
                });
            }

            if (!_vips.Contains(pl.UserId)) return HookResult.Continue;
        }
        else
        {
            if (_vips.Count > 0)
                _vips.Clear();
            else
                return HookResult.Continue;
        }

        return HookResult.Continue;
    }
}