﻿using System;
using Core.Serializers;
using StackExchange.Redis;

namespace AzureSqlDatabaseStressTestTool
{
    public class StackExchangeRedisAdapter : TestingDbAdapter
    {
        private static readonly ISerializer Serializer = SerializerFactory.Create(SerializerType.ProtocolBuffers);
        private static volatile int _counter;
        private readonly ConnectionMultiplexer _connection;

        public StackExchangeRedisAdapter(string connectionString)
            : base(connectionString)
        {
            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public override void DropAndCreateTable()
        {
        }

        public override void Insert(Testing entity)
        {
            entity.Id = _counter++;

            var db = _connection.GetDatabase();
            var json = Serializer.Serialize(entity);
            var key = TestingConstants.RedisKeyPrefix + entity.Id;
            db.StringSet(key, json);
        }

        public override Testing Select()
        {
            var db = _connection.GetDatabase();
            var id = new Random().Next(100);
            var key = TestingConstants.RedisKeyPrefix + id;
            var json = (string)db.StringGet(key);
            return string.IsNullOrEmpty(json) ? null : Serializer.Deserialize<Testing>(json);
        }
    }
}
