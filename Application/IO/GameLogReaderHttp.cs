﻿using IW4MAdmin.Application.API.GameLogServer;
using RestEase;
using SharedLibraryCore;
using SharedLibraryCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using static SharedLibraryCore.Utilities;

namespace IW4MAdmin.Application.IO
{
    /// <summary>
    /// provides capibility of reading log files over HTTP
    /// </summary>
    class GameLogReaderHttp : IGameLogReader
    {
        readonly IEventParser Parser;
        readonly IGameLogServer Api;
        readonly string logPath;

        public GameLogReaderHttp(Uri gameLogServerUri, string logPath, IEventParser parser)
        {
            this.logPath = logPath.ToBase64UrlSafeString(); ;
            Parser = parser;
            Api = RestClient.For<IGameLogServer>(gameLogServerUri);
        }

        public long Length => -1;

        public int UpdateInterval => 350;

        public async Task<ICollection<GameEvent>> ReadEventsFromLog(Server server, long fileSizeDiff, long startPosition)
        {
#if DEBUG == true
            server.Logger.WriteDebug($"Begin reading from http log");
#endif
            var events = new List<GameEvent>();
            string b64Path = logPath;
            var response = await Api.Log(b64Path);

            if (!response.Success)
            {
                server.Logger.WriteError($"Could not get log server info of {logPath}/{b64Path} ({server.LogPath})");
                return events;
            }

            // parse each line
            foreach (string eventLine in response.Data.Split(Environment.NewLine))
            {
                if (eventLine.Length > 0)
                {
                    try
                    {
                        var e = Parser.GetEvent(server, eventLine);
#if DEBUG == true
                        server.Logger.WriteDebug($"Parsed event with id {e.Id}  from http");
#endif
                        events.Add(e);
                    }

                    catch (Exception e)
                    {
                        server.Logger.WriteWarning("Could not properly parse event line");
                        server.Logger.WriteDebug(e.Message);
                        server.Logger.WriteDebug(eventLine);
                    }
                }
            }

            return events;
        }
    }
}
