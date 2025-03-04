﻿using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharedLibraryCore;
using SharedLibraryCore.Configuration;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;

namespace IW4MAdmin.Plugins.ProfanityDeterment
{
    public class Plugin : IPlugin
    {
        public string Name => "ProfanityDeterment";

        public float Version => Assembly.GetExecutingAssembly().GetName().Version.Major + Assembly.GetExecutingAssembly().GetName().Version.Minor / 10.0f;

        public string Author => "RaidMax";

        BaseConfigurationHandler<Configuration> Settings;
        ConcurrentDictionary<int, Tracking> ProfanityCounts;
        IManager Manager;

        public Task OnEventAsync(GameEvent E, Server S)
        {
            if (!Settings.Configuration().EnableProfanityDeterment)
                return Task.CompletedTask;

            if (E.Type == GameEvent.EventType.Connect)
            {
                if (!ProfanityCounts.TryAdd(E.Origin.ClientId, new Tracking(E.Origin)))
                {
                    S.Logger.WriteWarning("Could not add client to profanity tracking");
                }

                var objectionalWords = Settings.Configuration().OffensiveWords;
                bool containsObjectionalWord = objectionalWords.FirstOrDefault(w => E.Origin.Name.ToLower().Contains(w)) != null;

                // we want to run regex against it just incase
                if (!containsObjectionalWord)
                {
                    foreach (string word in objectionalWords)
                    {
                        containsObjectionalWord |= Regex.IsMatch(E.Origin.Name.ToLower(), word, RegexOptions.IgnoreCase);
                    }
                }

                if (containsObjectionalWord)
                {
                    E.Origin.Kick(Settings.Configuration().ProfanityKickMessage, Utilities.IW4MAdminClient(E.Owner));
                };
            }

            if (E.Type == GameEvent.EventType.Disconnect)
            {
                if (!ProfanityCounts.TryRemove(E.Origin.ClientId, out Tracking old))
                {
                    S.Logger.WriteWarning("Could not remove client from profanity tracking");
                }
            }

            if (E.Type == GameEvent.EventType.Say)
            {
                var objectionalWords = Settings.Configuration().OffensiveWords;
                bool containsObjectionalWord = false;

                foreach (string word in objectionalWords)
                {
                    containsObjectionalWord |= Regex.IsMatch(E.Data.ToLower(), word, RegexOptions.IgnoreCase);

                    // break out early because there's at least one objectional word
                    if (containsObjectionalWord)
                    {
                        break;
                    }
                }

                if (containsObjectionalWord)
                {
                    var clientProfanity = ProfanityCounts[E.Origin.ClientId];
                    if (clientProfanity.Infringements >= Settings.Configuration().KickAfterInfringementCount)
                    {
                        clientProfanity.Client.Kick(Settings.Configuration().ProfanityKickMessage, Utilities.IW4MAdminClient(E.Owner));
                    }

                    else if (clientProfanity.Infringements < Settings.Configuration().KickAfterInfringementCount)
                    {
                        clientProfanity.Infringements++;

                        clientProfanity.Client.Warn(Settings.Configuration().ProfanityWarningMessage, Utilities.IW4MAdminClient(E.Owner));
                    }
                }
            }
            return Task.CompletedTask;
        }

        public async Task OnLoadAsync(IManager manager)
        {
            // load custom configuration
            Settings = new BaseConfigurationHandler<Configuration>("ProfanityDetermentSettings");
            if (Settings.Configuration() == null)
            {
                Settings.Set((Configuration)new Configuration().Generate());
                await Settings.Save();
            }

            ProfanityCounts = new ConcurrentDictionary<int, Tracking>();
            Manager = manager;
        }

        public Task OnTickAsync(Server S) => Task.CompletedTask;

        public Task OnUnloadAsync() => Task.CompletedTask;
    }
}
