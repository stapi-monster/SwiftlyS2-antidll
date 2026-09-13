namespace AntiDll;

public class AntiDllConfig
{
    // Тип наказания: 0 - Kick (кик), 1 - Пользовательская команда (punish_command)
    public int punish_type { get; set; } = 1;

    // Команда наказания в Admins (выполняется если punish_type = 1)
    // Формат Admins: sw_ban <steamid64|player> <time> <reason>
    // Доступные плейсхолдеры: {userid}, {steamid}, {name}
    public string punish_command { get; set; } = "sw_ban {steamid} 0 Cheating (AntiDLL)";

    // Причина кика (если punish_type = 0)
    public string kick_reason { get; set; } = "AntiDLL: Использование запрещенных читов / DLL";

    // Сообщение в чат всем игрокам при наказании читера (оставьте пустым "", чтобы не отправлять)
    public string chat_message { get; set; } = "[RED][AntiDLL][DEFAULT] Игрок [YELLOW]{name}[DEFAULT] был забанен за использование читов!";

    // Логировать ли блокировки в файл логов (true - да, false - нет)
    public bool logs { get; set; } = true;

    // Интервал проверки игроков на наличие DLL в секундах
    public float interval { get; set; } = 5.0f;
}