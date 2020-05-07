﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoystonGame.TV;
using RoystonGame.Web.DataModels.Requests;
using RoystonGame.Web.DataModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using static System.FormattableString;

namespace RoystonGame.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "Admins")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<CurrentContentController> _logger;

        public AdminController(ILogger<CurrentContentController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public static IActionResult Get()
        {
            AdminFetchResponse response = new AdminFetchResponse
            {
                ActiveLobbies = GameManager.GetLobbies()
                    .Select(lobby =>
                        new AdminFetchResponse.Lobby
                        {
                            LobbyId = lobby?.LobbyId,
                            LobbyOwner = lobby?.Owner?.UserId,
                            ActiveDuration = DateTime.Now.Subtract(lobby?.CreationTime ?? DateTime.Now)
                        })
                    .ToList(),
                ActiveUsers = GameManager.GetUsers()
                    .Select(user =>
                        new AdminFetchResponse.User
                        {
                            UserIdentifier = user?.Identifier?.ToString(),
                            DisplayName = user?.DisplayName,
                            LobbyId = user?.LobbyId,
                            ActiveDuration = DateTime.Now.Subtract(user?.CreationTime ?? DateTime.Now),
                        })
                    .ToList(),
            };
            return new JsonResult(response);
        }

        [HttpPost]
        [Route("/Delete")]
        public static IActionResult DeleteEntities(AdministrativeActionRequest input)
        {
            int deletedEntries = 0;
            Exception lastException = null;
            foreach (string userIdentifier in input?.Users ?? new List<string>())
            {
                try
                {
                    GameManager.UnregisterUser(userIdentifier);
                    deletedEntries++;
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }

            foreach (string lobbyId in input?.Lobbies ?? new List<string>())
            {
                try
                {
                    GameManager.DeleteLobby(lobbyId);
                    deletedEntries++;
                }
                catch (Exception e)
                {
                    lastException = e;
                }
            }
            return new JsonResult(Invariant($"Deleted ({deletedEntries}) entries. Last exception: '{lastException}'"));
        }
    }
}
