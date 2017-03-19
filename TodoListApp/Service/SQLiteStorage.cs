using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Todoist.Net.Models;

namespace TodoListApp.Service
{
    public class SQLiteStorage : IDisposable
    {

        private DbConnection _connection;

        public SQLiteStorage()
        {
            DbProviderFactory fact = DbProviderFactories.GetFactory("System.Data.SQLite");
            _connection = fact.CreateConnection();
            var dbFileDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var dbFile = Path.Combine(dbFileDirectory, "test.s3db");
            if (!File.Exists(dbFile))
            {
                File.Copy(Path.Combine(dbFileDirectory, "test_.s3db"), dbFile);
            }
            _connection.ConnectionString = "Data Source=" + dbFile;
        }

        public async Task ConnectAsync()
        {
            await _connection.OpenAsync();
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            List<Item> result = new List<Item>();
            using (DbCommand command = _connection.CreateCommand())
            {
                command.CommandText = "select * from items";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            long id = reader.GetInt64(0);
                            string content = reader.GetString(1);
                            Item item = new Item(content);
                            item.Id = new ComplexId(id);
                            result.Add(item);
                        }
                    }
                }
            }
            return result;
        }

        public async Task<int> RemoveAllAsync()
        {
            int result = -1;
            try
            {
                using (DbTransaction transaction = _connection.BeginTransaction())
                {
                    using (DbCommand command = _connection.CreateCommand())
                    {
                        command.CommandText = "delete from items";
                        result = await command.ExecuteNonQueryAsync();
                    }
                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                //TODO: Exception handling
            }
            return result;
        }

        public async Task<int> RemoveRangeAsync(IEnumerable<Item> items)
        {
            int result = -1;
            if (items.Count() > 0)
            {
                try
                {
                    using (DbTransaction transaction = _connection.BeginTransaction())
                    {
                        using (DbCommand command = _connection.CreateCommand())
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.Append("delete from items where id in (");
                            foreach (var item in items)
                            {
                                sb.Append(item.Id.ToString());
                                sb.Append(",");
                            }
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append(")");
                            command.CommandText = sb.ToString();
                            result = await command.ExecuteNonQueryAsync();
                        }
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Exception handling
                }
            }
            return result;
        }

        public async Task<int> AddRangeAsync(IEnumerable<Item> items)
        {
            int result = -1;
            if (items.Count() > 0)
            {
                try
                {
                    using (DbTransaction transaction = _connection.BeginTransaction())
                    {
                        using (DbCommand command = _connection.CreateCommand())
                        {
                            DbParameter parameterId = new SQLiteParameter("@id");
                            DbParameter parameterContent = new SQLiteParameter("@content");

                            command.CommandText = "insert into items (id, content) VALUES(@id, @content)";
                            command.Parameters.Add(parameterId);
                            command.Parameters.Add(parameterContent);

                            foreach (var item in items)
                            {
                                parameterId.Value = item.Id.ToString();
                                parameterContent.Value = item.Content;
                                result = result + await command.ExecuteNonQueryAsync();
                            }
                        }
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Exception handling
                }
            }

            return result;
        }

        public async Task<int> UpdateRange(IEnumerable<Item> items)
        {
            int result = -1;
            if (items.Count() > 0)
            {
                try
                {
                    using (DbTransaction transaction = _connection.BeginTransaction())
                    {
                        using (DbCommand command = _connection.CreateCommand())
                        {
                            DbParameter parameterId = new SQLiteParameter("@id");
                            DbParameter parameterContent = new SQLiteParameter("@content");

                            command.CommandText = "update items set content=@content where id=@id";
                            command.Parameters.Add(parameterId);
                            command.Parameters.Add(parameterContent);

                            foreach (var item in items)
                            {
                                parameterId.Value = item.Id.ToString();
                                parameterContent.Value = item.Content;
                            }
                            result = await command.ExecuteNonQueryAsync();
                        }
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Exception handling
                }
            }
            return result;
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
