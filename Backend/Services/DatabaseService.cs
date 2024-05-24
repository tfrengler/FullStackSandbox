using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace FullStackSandbox.Services
{
    public sealed class DatabaseService
    {
        public DatabaseService(FileInfo databaseFile)
        {
            ArgumentNullException.ThrowIfNull(databaseFile);
            Debug.Assert(!databaseFile.IsReadOnly);

            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlite3());

            ConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = databaseFile.FullName,
                ForeignKeys = true,
                Mode = SqliteOpenMode.ReadWrite
            }.ToString();
        }

        private readonly string ConnectionString;

        public IDbConnection CreateConnection() => new SqliteConnection(ConnectionString);

        /*
            var DB = DataBase.Get();
            var SqlCommand = DB.CreateCommand();

            // Start a local transaction.
            var Transaction = connection.BeginTransaction();

            // Must assign both transaction object and connection
            // to Command object for a pending local transaction
            SqlCommand.Connection = connection;
            SqlCommand.Transaction = Transaction;

            SqlCommand.CommandText = @"
                INSERT INTO [Quests] (Id,Category,Name,Type,Side,Level,RequiredLevel)
                SELECT @QuestId,@Category,@Name,@Type,@Side,@Level,@RequiredLevel
                WHERE NOT EXISTS(SELECT 1 FROM [Quests] WHERE [Id] = @QuestId)
            ";

            SqlCommand.Parameters.Add(new SqliteParameter
            {
                ParameterName = "QuestId",
                DbType = DbType.Int32,
                Value = CurrentQuest.id
            });
            SqlCommand.Parameters.Add(new SqliteParameter
            {
                ParameterName = "Category",
                DbType = DbType.Int32,
                Value = CurrentQuest.category
            });
            SqlCommand.Parameters.Add(new SqliteParameter
            {
                ParameterName = "Name",
                DbType = DbType.String,
                Value = CurrentQuest.name
            });
            SqlCommand.Parameters.Add(new SqliteParameter
            {
                ParameterName = "Type",
                DbType = DbType.Int32,
                Value = CurrentQuest.type
            });
            SqlCommand.Parameters.Add(new SqliteParameter
            {
                ParameterName = "Side",
                DbType = DbType.Int32,
                Value = CurrentQuest.side
            });
            SqlCommand.Parameters.Add(new SqliteParameter
            {
                ParameterName = "Level",
                DbType = DbType.Int32,
                Value = CurrentQuest.level
            });
            SqlCommand.Parameters.Add(new SqliteParameter
            {
                ParameterName = "RequiredLevel",
                DbType = DbType.Int32,
                Value = CurrentQuest.reqlevel
            });

            bool Inserted = false;
            DB.Open();

            try
            {
                Inserted = SqlCommand.ExecuteNonQuery() > 0;
            }
            catch(SqliteException)
            {
                // Attempt to roll back the transaction.
                try
                {
                    Transaction.Rollback();
                }
                catch (Exception ex2)
                {
                    // This catch block will handle any errors that may have occurred
                    // on the server that would cause the rollback to fail, such as
                    // a closed connection.
                    Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                    Console.WriteLine("  Message: {0}", ex2.Message);
                }
            }
            finally
            {
                DB?.Close();
            }
        */
    }
}