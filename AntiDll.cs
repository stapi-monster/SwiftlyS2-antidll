using System.Collections.Concurrent;
using SwiftlyS2.Shared.Plugins;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.GameEventDefinitions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AntiDll;

[PluginMetadata(
    Id = "AntiDll",
    Version = "1.0.0",
    Name = "AntiDll",
    Author = "stapi",
    Description = "Anti-cheat plugin detecting unauthorized game event listeners and banning cheaters automatically via AdminsSystem."
)]
public class AntiDll : BasePlugin
{
    private AntiDllConfig Config = new();
    private AntiDllEventsConfig EventsConfig = new();

    // Кэш наказанных игроков для исключения повторных проверок
    private readonly ConcurrentDictionary<ulong, byte> punishedPlayers = new();

    private CancellationTokenSource? scanTimerCts;

    public AntiDll(ISwiftlyCore core) : base(core)
    {
    }

    public override void Load(bool hotReload)
    {
        Core.Configuration
            .InitializeWithTemplate("config.jsonc", "config.template.jsonc")
            .Configure(builder => builder.AddJsonFile("config.jsonc", optional: false, reloadOnChange: true));

        Core.Configuration
            .InitializeWithTemplate("events.jsonc", "events.template.jsonc")
            .Configure(builder => builder.AddJsonFile("events.jsonc", optional: false, reloadOnChange: true));

        ServiceCollection services = new();
        services.AddSwiftly(Core);
        services.AddOptionsWithValidateOnStart<AntiDllConfig>().BindConfiguration("AntiDll");
        services.AddOptionsWithValidateOnStart<AntiDllEventsConfig>().BindConfiguration("AntiDllEvents");

        var provider = services.BuildServiceProvider();
        var mainOptionsMonitor = provider.GetService<IOptionsMonitor<AntiDllConfig>>();
        var eventsOptionsMonitor = provider.GetService<IOptionsMonitor<AntiDllEventsConfig>>();

        if (mainOptionsMonitor != null)
        {
            Config = mainOptionsMonitor.CurrentValue;
            mainOptionsMonitor.OnChange(newConfig => Config = newConfig);
        }

        if (eventsOptionsMonitor != null)
        {
            EventsConfig = eventsOptionsMonitor.CurrentValue;
            eventsOptionsMonitor.OnChange(newEvents => EventsConfig = newEvents);
        }

        Core.Event.OnMapLoad += _ => punishedPlayers.Clear();

        Core.GameEvent.HookPre<EventPlayerDisconnect>((@event) =>
        {
            var player = Core.PlayerManager.GetPlayer(@event.PlayerID);
            if (player != null)
            {
                punishedPlayers.TryRemove(player.SteamID, out _);
            }
            return HookResult.Continue;
        });

        StartScanner();

        Core.Logger.LogInformation("[AntiDLL] Плагин успешно загружен. HotReload: {HotReload}", hotReload);
    }

    public override void Unload()
    {
        StopScanner();
        punishedPlayers.Clear();
        Core.Logger.LogInformation("[AntiDLL] Плагин выгружен.");
    }

    private void StartScanner()
    {
        StopScanner();

        scanTimerCts = new CancellationTokenSource();
        Core.Scheduler.StopOnMapChange(scanTimerCts);
        var token = scanTimerCts.Token;

        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    int delayMs = (int)Math.Max(1000, Config.interval * 1000);
                    await Task.Delay(delayMs, token);

                    if (token.IsCancellationRequested) break;

                    Core.Scheduler.NextTick(ScanPlayers);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Core.Logger.LogError(ex, "[AntiDLL] Ошибка сканирования.");
                }
            }
        }, token);
    }

    private void StopScanner()
    {
        if (scanTimerCts != null)
        {
            scanTimerCts.Cancel();
            scanTimerCts.Dispose();
            scanTimerCts = null;
        }
    }

    // Оптимизированное сканирование игроков на несанкционированные подписки
    private void ScanPlayers()
    {
        var eventsList = EventsConfig.events;
        if (eventsList == null || eventsList.Count == 0) return;

        var allPlayers = Core.PlayerManager.GetAllPlayers();

        foreach (var player in allPlayers)
        {
            if (player == null || !player.IsValid || player.IsFakeClient) continue;

            ulong steamId = player.SteamID;
            if (steamId == 0 || punishedPlayers.ContainsKey(steamId)) continue;

            int playerId = player.PlayerID;

            foreach (string eventName in eventsList)
            {
                if (string.IsNullOrWhiteSpace(eventName)) continue;

                if (Core.GameEvent.IsListeningToEvent(playerId, eventName.Trim()))
                {
                    punishedPlayers[steamId] = 1;

                    string playerName = player.Controller?.PlayerName ?? "";
                    if (string.IsNullOrWhiteSpace(playerName)) playerName = steamId.ToString();

                    string steamIdStr = steamId.ToString();
                    string userIdStr = playerId.ToString();

                    if (Config.logs)
                    {
                        Core.Logger.LogWarning("[AntiDLL] ЧИТЕР ОБНАРУЖЕН! Игрок '{Name}' ({SteamID}) использует эвент '{Event}'!",
                            playerName, steamIdStr, eventName);
                    }

                    if (!string.IsNullOrWhiteSpace(Config.chat_message))
                    {
                        BroadcastChatMessage(FormatPlaceholders(Config.chat_message, playerName, steamIdStr, userIdStr));
                    }

                    PunishPlayer(player, playerName, steamIdStr, userIdStr);
                    break;
                }
            }
        }
    }

    private void PunishPlayer(IPlayer player, string playerName, string steamId, string userId)
    {
        if (Config.punish_type == 0)
        {
            player.ExecuteCommand($"disconnect \"{Config.kick_reason}\"");
        }
        else
        {
            string cmd = string.IsNullOrWhiteSpace(Config.punish_command)
                ? $"disconnect \"{Config.kick_reason}\""
                : FormatPlaceholders(Config.punish_command, playerName, steamId, userId);

            player.ExecuteCommand(cmd);
        }
    }

    private string FormatPlaceholders(string template, string name, string steamId, string userId)
    {
        if (string.IsNullOrEmpty(template)) return "";

        return template
            .Replace("{name}", name)
            .Replace("{steamid}", steamId)
            .Replace("{userid}", userId);
    }

    private void BroadcastChatMessage(string message)
    {
        foreach (var p in Core.PlayerManager.GetAllPlayers())
        {
            if (p != null && p.IsValid && !p.IsFakeClient)
            {
                p.SendChat(message);
            }
        }
    }
}