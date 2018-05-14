﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfiCoreDojo.Domain;

namespace InfiCoreDojo.DataAccess.InMemory
{
    public class InMemoryLevelDal : ILevelDal
    {
        public InMemoryLevelDal()
        {
        }

        public IQueryable<Level> Query()
        {
            return InMemoryDatabase.Instance.Levels.AsQueryable();
        }

        public Level Get(Guid id)
        {
            return InMemoryDatabase.Instance.Levels.FirstOrDefault(i => i.Id == id)
                ?? throw new KeyNotFoundException($"Level {id} does not exist");
        }

        public void Upsert(Level level)
        {
            throw new NotImplementedException();
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public void Delete(Level level)
        {
            this.Delete(level.Id);
        }
    }
}