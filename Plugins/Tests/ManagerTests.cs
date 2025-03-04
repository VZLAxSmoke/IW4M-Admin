﻿using IW4MAdmin.Application;
using SharedLibraryCore;
using SharedLibraryCore.Database.Models;
using SharedLibraryCore.Interfaces;
using SharedLibraryCore.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace Tests
{
    [Collection("ManagerCollection")]
    public class ManagerTests
    {
        readonly ApplicationManager Manager;

        public ManagerTests(ManagerFixture fixture)
        {
            Manager = fixture.Manager;
        }

        [Fact]
        public void AreCommandNamesUnique()
        {
            bool test = Manager.GetCommands().Count == Manager.GetCommands().Select(c => c.Name).Distinct().Count();
            Assert.True(test, "command names are not unique");
        }

        [Fact]
        public void AreCommandAliasesUnique()
        {
            var mgr = Program.ServerManager;
            bool test = mgr.GetCommands().Count == mgr.GetCommands().Select(c => c.Alias).Distinct().Count();

            Assert.True(test, "command aliases are not unique");
        }

        [Fact]
        public void AddAndRemoveClientsViaJoinShouldSucceed()
        {
            var server = Manager.GetServers().First();
            var waiters = new Queue<GameEvent>();

            int clientStartIndex = 4;
            int clientNum = 10;

            for (int i = clientStartIndex; i < clientStartIndex + clientNum; i++)
            {
                var e = new GameEvent()
                {
                    Type = GameEvent.EventType.Join,
                    Origin = new EFClient()
                    {
                        Name = $"Player{i}",
                        NetworkId = i,
                        ClientNumber = i - 1
                    },
                    Owner = server
                };

                server.Manager.GetEventHandler().AddEvent(e);
                waiters.Enqueue(e);
            }

            while (waiters.Count > 0)
            {
                waiters.Dequeue().OnProcessed.Wait();
            }

            Assert.True(server.ClientNum == clientNum, $"client num does not match added client num [{server.ClientNum}:{clientNum}]");

            for (int i = clientStartIndex; i < clientStartIndex + clientNum; i++)
            {
                var e = new GameEvent()
                {
                    Type = GameEvent.EventType.Disconnect,
                    Origin = new EFClient()
                    {
                        Name = $"Player{i}",
                        NetworkId = i,
                        ClientNumber = i - 1
                    },
                    Owner = server
                };

                server.Manager.GetEventHandler().AddEvent(e);
                waiters.Enqueue(e);
            }

            while (waiters.Count > 0)
            {
                waiters.Dequeue().OnProcessed.Wait();
            }

            Assert.True(server.ClientNum == 0, "there are still clients connected");
        }

        [Fact]
        public void AddAndRemoveClientsViaRconShouldSucceed()
        {
            var server = Manager.GetServers().First();
            var waiters = new Queue<GameEvent>();

            int clientIndexStart = 1;
            int clientNum = 8;

            for (int i = clientIndexStart; i < clientNum + clientIndexStart; i++)
            {
                var e = new GameEvent()
                {
                    Type = GameEvent.EventType.Connect,
                    Origin = new EFClient()
                    {
                        Name = $"Player{i}",
                        NetworkId = i,
                        ClientNumber = i - 1,
                        IPAddress = i,
                        Ping = 50,
                        CurrentServer = server
                    },
                    Owner = server,
                };

                Manager.GetEventHandler().AddEvent(e);
                waiters.Enqueue(e);
            }

            while (waiters.Count > 0)
            {
                waiters.Dequeue().OnProcessed.Wait();
            }

            int actualClientNum = server.GetClientsAsList().Count(p => p.State == EFClient.ClientState.Connected);
            Assert.True(actualClientNum == clientNum, $"client connected states don't match [{actualClientNum}:{clientNum}");

            for (int i = clientIndexStart; i < clientNum + clientIndexStart; i++)
            {
                var e = new GameEvent()
                {
                    Type = GameEvent.EventType.Disconnect,
                    Origin = new EFClient()
                    {
                        Name = $"Player{i}",
                        NetworkId = i,
                        ClientNumber = i - 1,
                        IPAddress = i,
                        Ping = 50,
                        CurrentServer = server
                    },
                    Owner = server,
                };

                Manager.GetEventHandler().AddEvent(e);
                waiters.Enqueue(e);
            }

            while (waiters.Count > 0)
            {
                waiters.Dequeue().OnProcessed.Wait();
            }

            actualClientNum = server.ClientNum;
            Assert.True(actualClientNum == 0, "there are clients still connected");
        }


        [Fact]
        public void AddClientViaLog()
        {
            var resetEvent = new ManualResetEventSlim();
            resetEvent.Reset();

            Manager.OnServerEvent += (sender, eventArgs) =>
           {
               if (eventArgs.Event.Type ==  GameEvent.EventType.Join)
               {
                   eventArgs.Event.OnProcessed.Wait();
                   Assert.True(false);
               }
           };

            File.AppendAllText("test_mp.log", "  2:33 J;224b3d0bc64ab4f9;0;goober");


            resetEvent.Wait(5000);
        }

        [Fact]
        public void PrintCommands()
        {
            var sb = new StringBuilder();

            sb.AppendLine("|Name              |Alias|Description                                                                               |Requires Target|Syntax           |Required Level|");
            sb.AppendLine("|--------------| -----| --------------------------------------------------------| -----------------| -------------| ----------------|");

            foreach (var command in Manager.GetCommands().OrderByDescending(c => c.Permission).ThenBy(c => c.Name))
            {
                sb.AppendLine($"|{command.Name}|{command.Alias}|{command.Description}|{command.RequiresTarget}|{command.Syntax.Substring(8).EscapeMarkdown()}|{command.Permission}|");
            }

            Assert.True(false, sb.ToString());
        }
    }
}

