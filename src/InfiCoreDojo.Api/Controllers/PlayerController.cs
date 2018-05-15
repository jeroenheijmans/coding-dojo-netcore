﻿using System;
using System.Collections.Generic;
using System.Linq;
using InfiCoreDojo.Api.DTO;
using InfiCoreDojo.DataAccess;
using InfiCoreDojo.Domain;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace InfiCoreDojo.Api.Controllers
{
    [Route("api/v1/[controller]")]
    public class PlayerController : Controller
    {
        private readonly IPlayerDal playerDal;
        private readonly ILevelDal levelDal;

        public PlayerController(IPlayerDal playerDal, ILevelDal levelDal)
        {
            this.playerDal = playerDal ?? throw new ArgumentNullException(nameof(playerDal));
            this.levelDal = levelDal ?? throw new ArgumentNullException(nameof(levelDal));
        }

        [HttpGet("{name}")]
        public Player Get(string name)
        {
            Log.Logger.Information("Asking for player {Name}", name);

            var player = playerDal.FindByName(name);

            if (player != null)
            {
                return player;
            }

            player = new Player(Guid.NewGuid(), name, GetStartingLevel().Id);

            playerDal.Upsert(player);

            return player;
        }

        [HttpGet("{name}/current-level")]
        public Level GetCurrentLevel(string name)
        {
            Log.Logger.Information("Retrieving current level for player {Name}", name);

            var player = Get(name);
            return levelDal.Get(player.CurrentLevelId);
        }
        
        [HttpPost("choose")]
        public ApiCommandResult Choose([FromBody] Choice choice)
        {
            Log.Logger.Information("Making player choice {@Choice}", choice);

            var player = playerDal.FindByName(choice.PlayerName);

            // TODO: Validate if the player *can* do this move...
            // That is, don't let the .First(...) call throw an exception because 
            // it's not there, instead handle that graciously by accusing the
            // player to be a CHEATER! :D
            var level = levelDal.Get(player.CurrentLevelId);
            var answer = level.Answers.First(a => a.Id == choice.AnswerId);

            player.MoveTo(answer.TargetLevelId);

            playerDal.Upsert(player);

            return new ApiCommandResult { Success = true } ;
        }

        [HttpPost("restart")]
        public ApiCommandResult Restart([FromBody] Reset reset)
        {
            Log.Logger.Warning("Executing reset command {@Reset}", reset);

            var player = playerDal.FindByName(reset.PlayerName);

            if (player == null)
            {
                return new ApiCommandResult { Success = false };
            }

            player.CurrentLevelId = GetStartingLevel().Id;

            playerDal.Upsert(player);

            return new ApiCommandResult { Success = true };
        }

        private Level GetStartingLevel()
        {
            // TODO: Select a random starting level, possibly in another service
            return levelDal
                .Query()
                .First(l => l.IsStartingLevel);
        }

        // TODO: PlayerController CrUD operations
        // Left as an exercise for the reader (see LevelController for examples)
    }
}