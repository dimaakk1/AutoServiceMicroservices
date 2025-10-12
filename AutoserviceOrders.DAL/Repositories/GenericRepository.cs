using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoserviceOrders.DAL.Repositories.Interfaces;
using Dapper;

namespace AutoserviceOrders.DAL.Repositories
{
    public class GenericRepository<T> where T : class
    {
        protected readonly IDbConnection _connection;
        protected readonly IDbTransaction _transaction;
        protected readonly string _tableName;
        protected readonly string _primaryKey;

        public GenericRepository(IDbConnection connection, string tableName, string primaryKey, IDbTransaction transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
            _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            _primaryKey = primaryKey ?? throw new ArgumentNullException(nameof(primaryKey));
        }

        public virtual async Task<int> AddAsync(T entity)
        {
            var properties = GetProperties(entity).Where(p => p.Name != _primaryKey).ToList();
            var columns = string.Join(", ", properties.Select(p => p.Name));
            var values = string.Join(", ", properties.Select(p => "@" + p.Name));

            string sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({values})";
            return await _connection.ExecuteAsync(sql, entity, transaction: _transaction);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            string sql = $"SELECT * FROM {_tableName}";
            return await _connection.QueryAsync<T>(sql, transaction: _transaction);
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            string sql = $"SELECT * FROM {_tableName} WHERE {_primaryKey} = @Id";
            return await _connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id }, transaction: _transaction);
        }

        public virtual async Task<int> UpdateAsync(T entity)
        {
            var properties = GetProperties(entity).Where(p => p.Name != _primaryKey).ToList();
            var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

            string sql = $"UPDATE {_tableName} SET {setClause} WHERE {_primaryKey} = @{_primaryKey}";
            return await _connection.ExecuteAsync(sql, entity, transaction: _transaction);
        }

        public virtual async Task<int> DeleteAsync(int id)
        {
            string sql = $"DELETE FROM {_tableName} WHERE {_primaryKey} = @Id";
            return await _connection.ExecuteAsync(sql, new { Id = id }, transaction: _transaction);
        }

        protected IEnumerable<PropertyInfo> GetProperties(object obj)
        {
            return obj.GetType().GetProperties().Where(p => p.CanRead && p.CanWrite);
        }
    }
}
