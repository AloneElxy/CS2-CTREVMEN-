using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Memory;

namespace CTSRespawnPlugin
{
    public class CTSRespawn : BasePlugin
    {
        public override string ModuleName => "CT Menu Plugin";
        public override string ModuleVersion => "1.0";
        public override string ModuleAuthor => "Alone Elxy";
        public override string ModuleDescription => "CT takımı için otomatik canlandırma sistemi";

        private int ctRespawnCredits = 3;
        private const int MaxRespawnCredits = 3;
        private const string PluginTag = " \x02[Alone Elxy]\x08";
        private const string AdminPermission = "@css/generic";

        public override void Load(bool hotReload)
        {
            RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            RegisterEventHandler<EventRoundStart>(OnRoundStart);

            AddCommand("css_haksifirla", "CT canlanma haklarını sıfırlar", ResetRespawnCredits);
            AddCommand("css_hakkontrol", "Kalan canlanma haklarını gösterir", CheckRespawnCredits);

            Console.WriteLine($"{ModuleName} v{ModuleVersion} yüklendi!");
        }

        private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
        {
            var victim = @event.Userid;
            if (victim == null || victim.TeamNum != (int)CsTeam.CounterTerrorist || !victim.IsValid)
                return HookResult.Continue;

            if (ctRespawnCredits > 0)
            {
                ctRespawnCredits--;
                Server.NextFrame(() => RespawnPlayer(victim));
                Server.PrintToChatAll($"{PluginTag} {victim.PlayerName} canlandı! Kalan hak: {ctRespawnCredits}");
            }
            else
            {
                Server.PrintToChatAll($"{PluginTag} CT takımının canlanma hakkı bitti!");
            }

            return HookResult.Continue;
        }

        private void RespawnPlayer(CCSPlayerController player)
        {
            if (player == null || !player.IsValid)
                return;

            player.Respawn();
        }

        private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
        {
            ctRespawnCredits = MaxRespawnCredits;
            Server.PrintToChatAll($"{PluginTag} Round bitti, canlanma hakları yenilendi!");
            return HookResult.Continue;
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            Server.PrintToChatAll($"{PluginTag} Yeni round başladı! CT takımının {MaxRespawnCredits} canlanma hakkı var!");
            return HookResult.Continue;
        }

        private void ResetRespawnCredits(CCSPlayerController player, CommandInfo command)
        {
            if (player == null || !AdminManager.PlayerHasPermissions(player, AdminPermission))
            {
                Server.PrintToChatAll($"{PluginTag} {player.PlayerName} yetkisiz komut kullanmaya çalıştı!");
                return;
            }

            ctRespawnCredits = MaxRespawnCredits;
            Server.PrintToChatAll($"{PluginTag} Canlanma hakları {player.PlayerName} tarafından sıfırlandı!");
        }

        private void CheckRespawnCredits(CCSPlayerController player, CommandInfo command)
        {
            if (player == null) return;

            Server.PrintToChatAll($"{PluginTag} CT takımının kalan canlanma hakkı: {ctRespawnCredits}");
        }

        private bool HasPermissionToReset(CCSPlayerController player)
        {
            return AdminManager.PlayerHasPermissions(player, AdminPermission);
        }
    }
}
