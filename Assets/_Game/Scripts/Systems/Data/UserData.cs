using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Systems.Data
{
    
    using Domain.Entities;
    using Domain.Exceptions;
    using Domain.Interfaces;

    /// <summary>
    /// 저장/로드 대상이 되는 유저 데이터 루트 클래스입니다.
    /// </summary>
    [Serializable]
    public class UserData : IDataValidatable
    {
        #region Fields

        public int version = 1;

        // 파생 타입(LifeData, ObstacleData, ProjectileData)을 유지하기 위해 SerializeReference를 사용합니다.
        [SerializeReference] public List<EntityData> entities = new();

        #endregion
        #region Properties

        public int Version => version;
        public IReadOnlyList<EntityData> Entities => entities;

        #endregion
        #region Constructors

        public UserData()
        {
        }

        #endregion
        #region Methods

        public void Initialize()
        {
            version = Math.Max(1, version);
            entities ??= new List<EntityData>();
        }

        public void AddEntity(EntityData entity)
        {
            if(entity == null) return;
            entities ??= new List<EntityData>();
            entities.Add(entity);
        }

        public bool RemoveEntityById(string entityId)
        {
            if(string.IsNullOrEmpty(entityId) || entities == null) return false;

            for(int i = 0; i < entities.Count; i++)
            {
                if(entities[i] != null && entities[i].id == entityId)
                {
                    entities.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public EntityData FindEntityById(string entityId)
        {
            if(string.IsNullOrEmpty(entityId) || entities == null) return null;

            for(int i = 0; i < entities.Count; i++)
            {
                EntityData entity = entities[i];
                if(entity != null && entity.id == entityId)
                    return entity;
            }

            return null;
        }

        public void ValidateData()
        {
            if(version <= 0)
                throw new DataValidityViolationException("Invalid value assigned to UserData Version.");

            if(entities == null)
                throw new DataValidityViolationException("Entity list cannot be null.");

            HashSet<string> ids = new();
            foreach(EntityData entity in entities)
            {
                if(entity == null)
                    throw new DataValidityViolationException("Entity data cannot be null.");

                entity.ValidateData();

                if(ids.Add(entity.id) == false)
                    throw new DataValidityViolationException($"Duplicated Entity ID detected: {entity.id}");
            }
        }

        #endregion
    }
}
