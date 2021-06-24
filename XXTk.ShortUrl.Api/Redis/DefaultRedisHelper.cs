using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XXTk.ShortUrl.Api.Redis
{
    public class DefaultRedisHelper : IDisposable
    {
        private static readonly ConcurrentDictionary<string, ConnectionMultiplexer> _connections = new();

        private bool _hasDisposed = false;
        private readonly ConfigurationOptions _configurationOptions;
        private readonly string _instanceName;
        private readonly int _defaultDB;

        public DefaultRedisHelper(RedisOptions redisOptions)
        {
            _configurationOptions = ConfigurationOptions.Parse(redisOptions.ConnectionString);
            _instanceName = redisOptions.InstanceName;
            _defaultDB = redisOptions.DefaultDB;
        }

        private ConnectionMultiplexer GetConnection()
        {
            return _connections.GetOrAdd(_instanceName, p => ConnectionMultiplexer.Connect(_configurationOptions));
        }

        public IDatabase GetDatabase()
        {
            return GetConnection().GetDatabase(_defaultDB);
        }

        public IServer GetServer(int endPointsIndex = 0)
        {
            return GetConnection().GetServer(_configurationOptions.EndPoints[endPointsIndex]);
        }

        public ISubscriber GetSubscriber()
        {
            return GetConnection().GetSubscriber();
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_hasDisposed) return;

            if (isDisposing)
            {
                if (_connections.Any())
                {
                    foreach (var connection in _connections.Values)
                    {
                        connection.Close();
                    }
                }
            }

            _hasDisposed = true;
        }
    }
}
