<div align="center">
  <img src="https://pan.samyyc.dev/s/VYmMXE" />
  <h2><strong>AntiDll</strong></h2>
</div>

Эффективный и легкий античит-плагин. 

Плагин сканирует активных клиентов на сервере на предмет несанкционированных подписок на скрытые или внутренние игровые события (`GameEvents`), которые используют популярные инжектируемые DLL-читы (Cheat DLLs / Internal Cheats). При обнаружении читера плагин автоматически применяет наказание (кик или команду бана через **Admins**).

> [!IMPORTANT]
> Для функции автоматического бана плагин разработан для работы в связке с официальной системой администраторов ([SwiftlyS2-Plugins/Admins](https://github.com/SwiftlyS2-Plugins/Admins)).

---

## Возможности

- **Детект DLL-читов**: Сканирование подписок игрока через нативный API `IsListeningToEvent`.
- **Интеграция с [Admins](https://github.com/SwiftlyS2-Plugins/Admins)**:
  - `0` — Kick (кик с сервера).
  - `1` — Команда бана `punish_command` через **Admins**: `sw_ban {steamid} 0 Cheating (AntiDLL)`.
- **Раздельная конфигурация**:
  - `config.jsonc` — параметры наказания, интервал сканирования, формат команд и оповещений.
  - `events.jsonc` — отдельный настраиваемый список отслеживаемых эвентов.
- **Оповещение сервера**: Гибкие чат-сообщения с поддержкой цветов `[...]` и подстановкой `{name}`, `{steamid}`, `{userid}`.

---

## Конфиг

### Основной конфиг (`config.jsonc`)
```jsonc
{
  "AntiDll": {
    // Тип наказания: 0 - Kick (кик), 1 - Пользовательская команда / Ban (punish_command)
    "punish_type": 1,

    // Команда бана для Admin (https://github.com/SwiftlyS2-Plugins/Admins)
    // Формат Admin: sw_ban <steamid64|player> <time> <reason>
    // Доступные плейсхолдеры: {userid}, {steamid}, {name}
    "punish_command": "sw_ban {steamid} 0 Cheating (AntiDLL)",

    // Причина кика (если punish_type = 0)
    "kick_reason": "AntiDLL: Использование запрещенных читов / DLL",

    // Сообщение в чат всем игрокам при наказании читера (пусто "" - не отправлять)
    "chat_message": "[RED][AntiDLL][DEFAULT] Игрок [YELLOW]{name}[DEFAULT] был забанен за использование читов!",

    // Логировать ли блокировки в консоль/файл логов (true - да, false - нет)
    "logs": true,

    // Интервал проверки игроков в секундах
    "interval": 5.0
  }
}
```

### Конфиг запрещенных эвентов (`events.jsonc`)
```jsonc
{
  "AntiDllEvents": {
    "events": [
      "bullet_impact",
      "bullet_damage",
      "bullet_flight_resolution",
      "weapon_fire_on_empty",
      "weapon_reload",
      "weapon_zoom",
      "player_footstep",
      "player_blind",
      "grenade_bounce",
      "molotov_detonate",
      "bomb_beginplant",
      "bomb_begindefuse",
      "item_purchase",
      "enter_buyzone",
      "buymenu_open",
      "item_pickup_slerp",
      "item_equip",
      "door_open",
      "show_deathpanel",
      "entity_visible"
    ]
  }
}
```