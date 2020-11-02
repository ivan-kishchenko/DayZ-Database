﻿using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace DayzDatabaseServer
{
    public class Database
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly string name;

        private readonly SQLiteConnection connection;

        public Database(string name)
        {
            this.name = name;

            var storageFolder = GetStorageFolder();
            if (!Directory.Exists(storageFolder))
            {
                Directory.CreateDirectory(storageFolder);
                logger.Info($"Directory '{storageFolder}' created.");
            }

            var databasePath = Path.Combine(storageFolder, $"{name}.db");
            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
                logger.Info($"Database '{name}' created.");
            }

            var builder = new SQLiteConnectionStringBuilder();
            builder.DataSource = databasePath;
            builder.Version = 3;
            builder.DefaultIsolationLevel = System.Data.IsolationLevel.Serializable;
            builder.JournalMode = SQLiteJournalModeEnum.Wal;

            connection = new SQLiteConnection(builder.ConnectionString);
            connection.Open();
            logger.Info($"Database '{name}' successfully loaded.");
        }

        public string Query(string query)
        {
            try
            {
                if (logger.IsDebugEnabled) logger.Debug($"Database '{name}' query start: {query}");

                var command = new SQLiteCommand(query, this.connection);
                var reader = command.ExecuteReader();
                var result = SerializeResult(reader);

                if (logger.IsDebugEnabled) logger.Debug($"Database '{name}' query end: {result}");

                return result;
            }
            catch(Exception ex)
            {
                logger.Error($"Database '{name}' query failed: {ex.Message}");
                throw;
            }
        }

        public string Transaction(string transactionData)
        {
            int queryId = 0;
            var result = string.Empty;
            var transaction = this.connection.BeginTransaction(System.Data.IsolationLevel.Serializable);
            var command = new SQLiteCommand()
            {
                Connection = connection,
                Transaction = transaction
            };

            try
            {
                if (logger.IsDebugEnabled) logger.Debug($"Database '{name}' transaction start: {transactionData}");

                var queries = JsonConvert.DeserializeObject<string[]>(transactionData);
                for (queryId = 0; queryId < queries.Length; queryId++)
                {
                    command.CommandText = queries[queryId];
                    if (queryId == queries.Length - 1)
                    {
                        result = SerializeResult(command.ExecuteReader());
                    }
                    else
                    {
                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();

                if (logger.IsDebugEnabled) logger.Debug($"Database '{name}' transaction end: {result}");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                logger.Error($"Database '{name}' transaction failed on query {queryId}: {ex.Message}");
                throw;
            }

            return result;
        }

        private string SerializeResult(SQLiteDataReader reader)
        {
            if (!reader.HasRows)
            {
                return string.Empty;
            }

            var results = new List<string[]>();
            while (reader.Read())
            {
                var row = new string[reader.FieldCount];
                for (int col = 0; col < reader.FieldCount; col++)
                {
                    var value = reader[col];
                    row[col] = value != null ? value.ToString() : string.Empty;
                }

                results.Add(row);
            }
            return JsonConvert.SerializeObject(results, Formatting.None);
        }

        private string GetStorageFolder()
        {
            var profilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "profiles");
            if (Directory.Exists(profilesFolder))
            {
                return Path.GetFullPath(Path.Combine(profilesFolder, "storage"));
            }
            else
            {
                return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage"));
            }
        }
    }
}
