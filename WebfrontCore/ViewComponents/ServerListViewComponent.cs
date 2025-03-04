﻿using Microsoft.AspNetCore.Mvc;
using SharedLibraryCore;
using SharedLibraryCore.Dtos;
using System.Linq;
using System.Net;

namespace WebfrontCore.ViewComponents
{
    public class ServerListViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var servers = Program.Manager.GetServers();

            var serverInfo = servers.Select(s => new ServerInfo()
            {
                Name = s.Hostname,
                ID = s.EndPoint,
                Port = s.GetPort(),
                Map = s.CurrentMap.Alias,
                ClientCount = s.ClientNum,
                MaxClients = s.MaxClients,
                GameType = s.Gametype,
                PlayerHistory = s.ClientHistory.ToArray(),
                Players = s.GetClientsAsList()
                .Select(p => new PlayerInfo()
                {
                    Name = p.Name,
                    ClientId = p.ClientId,
                    Level = p.Level.ToLocalizedLevelName(),
                    LevelInt = (int)p.Level
                }).ToList(),
                ChatHistory = s.ChatHistory.ToList(),
                Online = !s.Throttled,
                IPAddress = $"{(IPAddress.Parse(s.IP).IsInternal() ? Program.Manager.ExternalIPAddress : s.IP)}:{s.GetPort()}",
                ConnectProtocolUrl = s.EventParser.URLProtocolFormat.FormatExt(IPAddress.Parse(s.IP).IsInternal() ? Program.Manager.ExternalIPAddress : s.IP, s.GetPort())
            }).ToList();
            return View("_List", serverInfo);
        }
    }
}
