﻿using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SharedLibraryCore.Commands
{
    public class CommandProcessing
    {
        public static async Task<Command> ValidateCommand(GameEvent E)
        {
            var loc = Utilities.CurrentLocalization.LocalizationIndex;
            var Manager = E.Owner.Manager;

            string CommandString = E.Data.Substring(1, E.Data.Length - 1).Split(' ')[0];
            E.Message = E.Data;

            Command C = null;
            foreach (Command cmd in Manager.GetCommands())
            {
                if (cmd.Name == CommandString.ToLower() || cmd.Alias == CommandString.ToLower())
                {
                    C = cmd;
                }
            }

            if (C == null)
            {
                E.Origin.Tell(loc["COMMAND_UNKNOWN"]);
                throw new CommandException($"{E.Origin} entered unknown command \"{CommandString}\"");
            }

            E.Data = E.Data.RemoveWords(1);
            String[] Args = E.Data.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            // todo: the code below can be cleaned up 

            if (E.Origin.Level < C.Permission)
            {
                E.Origin.Tell(loc["COMMAND_NOACCESS"]);
                throw new CommandException($"{E.Origin} does not have access to \"{C.Name}\"");
            }

            if (Args.Length < (C.RequiredArgumentCount))
            {
                E.Origin.Tell(loc["COMMAND_MISSINGARGS"]);
                E.Origin.Tell(C.Syntax);
                throw new CommandException($"{E.Origin} did not supply enough arguments for \"{C.Name}\"");
            }

            if (C.RequiresTarget)
            {
                if (Args.Length > 0)
                {
                    if (!Int32.TryParse(Args[0], out int cNum))
                    {
                        cNum = -1;
                    }

                    if (Args[0][0] == '@') // user specifying target by database ID
                    {
                        int dbID = -1;
                        int.TryParse(Args[0].Substring(1, Args[0].Length - 1), out dbID);

                        var found = await Manager.GetClientService().Get(dbID);
                        if (found != null)
                        {
                            E.Target = found;
                            E.Target.CurrentServer = E.Owner;
                            E.Data = String.Join(" ", Args.Skip(1));
                        }
                    }

                    else if (Args[0].Length < 3 && cNum > -1 && cNum < E.Owner.MaxClients) // user specifying target by client num
                    {
                        if (E.Owner.Clients[cNum] != null)
                        {
                            E.Target = E.Owner.Clients[cNum];
                            E.Data = String.Join(" ", Args.Skip(1));
                        }
                    }
                }

                List<EFClient> matchingPlayers;

                if (E.Target == null && C.RequiresTarget) // Find active player including quotes (multiple words)
                {
                    matchingPlayers = E.Owner.GetClientByName(E.Data.Trim());
                    if (matchingPlayers.Count > 1)
                    {
                        E.Origin.Tell(loc["COMMAND_TARGET_MULTI"]);
                        throw new CommandException($"{E.Origin} had multiple players found for {C.Name}");
                    }
                    else if (matchingPlayers.Count == 1)
                    {
                        E.Target = matchingPlayers.First();

                        string escapedName = Regex.Escape(E.Target.Name);
                        var reg = new Regex($"(\"{escapedName}\")|({escapedName})", RegexOptions.IgnoreCase);
                        E.Data = reg.Replace(E.Data, "", 1).Trim();

                        if (E.Data.Length == 0 && C.RequiredArgumentCount > 1)
                        {
                            E.Origin.Tell(loc["COMMAND_MISSINGARGS"]);
                            E.Origin.Tell(C.Syntax);
                            throw new CommandException($"{E.Origin} did not supply enough arguments for \"{C.Name}\"");
                        }
                    }
                }

                if (E.Target == null && C.RequiresTarget && Args.Length > 0) // Find active player as single word
                {
                    matchingPlayers = E.Owner.GetClientByName(Args[0]);
                    if (matchingPlayers.Count > 1)
                    {
                        E.Origin.Tell(loc["COMMAND_TARGET_MULTI"]);
                        foreach (var p in matchingPlayers)
                        {
                            E.Origin.Tell($"[^3{p.ClientNumber}^7] {p.Name}");
                        }
                        throw new CommandException($"{E.Origin} had multiple players found for {C.Name}");
                    }

                    else if (matchingPlayers.Count == 1)
                    {
                        E.Target = matchingPlayers.First();

                        string escapedName = Regex.Escape(E.Target.Name);
                        string escapedArg = Regex.Escape(Args[0]);
                        var reg = new Regex($"({escapedName})|({escapedArg})", RegexOptions.IgnoreCase);
                        E.Data = reg.Replace(E.Data, "", 1).Trim();

                        if ((E.Data.Trim() == E.Target.Name.ToLower().Trim() ||
                            E.Data == String.Empty) &&
                            C.RequiresTarget)
                        {
                            E.Origin.Tell(loc["COMMAND_MISSINGARGS"]);
                            E.Origin.Tell(C.Syntax);
                            throw new CommandException($"{E.Origin} did not supply enough arguments for \"{C.Name}\"");
                        }
                    }
                }

                if (E.Target == null && C.RequiresTarget)
                {
                    E.Origin.Tell(loc["COMMAND_TARGET_NOTFOUND"]);
                    throw new CommandException($"{E.Origin} specified invalid player for \"{C.Name}\"");
                }
            }

            E.Data = E.Data.Trim();
            return C;
        }
    }
}
