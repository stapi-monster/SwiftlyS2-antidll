namespace AntiDll;

public class AntiDllEventsConfig
{
    // Актуальный список игровых событий CS2, подписка клиента на которые указывает на работу инжектированного чит-модуля (DLL)
    public List<string> events { get; set; } = new()
    {
        // 1. События стрельбы, попаданий и отдачи (Aimbot / Triggerbot / Hitmarkers / Recoil)
        "bullet_impact",
        "bullet_damage",
        "bullet_flight_resolution",
        "weapon_fire_on_empty",
        "weapon_reload",
        "weapon_zoom",
        "weapon_zoom_rifle",
        "inspect_weapon",
        "silencer_detach",

        // 2. События передвижения и звука (Sound ESP / Radar / Wallhack)
        "player_footstep",
        "player_jump",
        "player_falldamage",
        "player_blind",

        // 3. Эвенты гранат и траекторий (Grenade Helper / Prediction)
        "grenade_bounce",
        "molotov_detonate",
        "tagrenade_detonate",
        "inferno_extinguish",
        "decoy_firing",

        // 4. Эвенты бомбы C4 (Bomb ESP / C4 Timer / AutoDefuse)
        "bomb_beginplant",
        "bomb_abortplant",
        "bomb_begindefuse",
        "bomb_abortdefuse",
        "bomb_beep",

        // 5. Покупки и эвенты зоны магазина (AutoBuy / Enemy Economy Tracker)
        "item_purchase",
        "enter_buyzone",
        "exit_buyzone",
        "buymenu_open",
        "buymenu_close",
        "buytime_ended",

        // 6. Подбор и отслеживание инвентаря (Item ESP / Skinchanger)
        "item_pickup_slerp",
        "item_pickup_failed",
        "item_equip",
        "item_remove",
        "inventory_updated",
        "client_loadout_changed",

        // 7. Интерактивные объекты карты (Door ESP / AutoDoor)
        "door_open",
        "door_closed",
        "door_break",
        "door_moving",
        "break_prop",
        "break_breakable",

        // 8. Состояние игрока и отображения (Deathpanel Bypass / Target ESP)
        "show_deathpanel",
        "hide_deathpanel",
        "entity_visible",
        "local_player_pawn_changed",
        "local_player_controller_team",
        "player_decal"
    };
}