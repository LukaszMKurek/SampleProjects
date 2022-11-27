using Dapper;
using LockingInSqlServer.Utils;

namespace LockingInSqlServer;

public static class ExampleDatabase1
{
    public static async Task CreateDatabase(bool enableReadCommittedSnapshot)
    {
        await using var connection = await ConnectionHelper.GetSqlConnection();

        var n = await connection.ExecuteAsync($$"""
            
                IF (EXISTS (SELECT * 
                                 FROM INFORMATION_SCHEMA.TABLES 
                                 WHERE TABLE_SCHEMA = 'dbo' 
                                 AND  TABLE_NAME = 'Tab1'))
                BEGIN
                    DROP TABLE [dbo].[Tab1];
                    DROP TABLE [dbo].[Tab2];
                END

                CREATE TABLE [dbo].[Tab1](
                    [Id] [bigint] NOT NULL,
                    [Value] [nvarchar](50) NOT NULL,
                    [Tab2Id] [bigint] NULL,
                    CONSTRAINT [PK_Tab1] PRIMARY KEY CLUSTERED 
                    (
                        [Id] ASC
                    )
                );

                CREATE NONCLUSTERED INDEX [IX_Value] ON [dbo].[Tab1]
                (
                    [Value] ASC
                );

                CREATE NONCLUSTERED INDEX [IX_Tab2Id] ON [dbo].[Tab1]
                (
                    [Tab2Id] ASC
                );

                CREATE TABLE [dbo].[Tab2](
                    [Id] [bigint] NOT NULL,
                    [Value] [nvarchar](50) NOT NULL,
                    CONSTRAINT [PK_Tab2] PRIMARY KEY CLUSTERED 
                    (
                        [Id] ASC
                    )
                );

                CREATE NONCLUSTERED INDEX [IX_Value] ON [dbo].[Tab2]
                (
                    [Value] ASC
                );

                ALTER TABLE [dbo].[Tab1] WITH CHECK ADD CONSTRAINT [FK_Tab1_Tab2] FOREIGN KEY([Tab2Id]) REFERENCES [dbo].[Tab2] ([Id]);
                ALTER TABLE [dbo].[Tab1] CHECK CONSTRAINT [FK_Tab1_Tab2]; 
                           

                -- this options radicaly change behaviour of database
                -- if any other connection is active then this command will run forever
                ALTER DATABASE {{connection.Database}} SET READ_COMMITTED_SNAPSHOT {{(enableReadCommittedSnapshot ? "ON" : "OFF")}} 
                           
    
            """);

        for (int i = 0; i < 1024 * 32; i++)
        {
            await connection.ExecuteAsync("INSERT INTO [dbo].[Tab2] ([Id], [Value]) VALUES (@Id, @Value)", new { Id = i, Value = $"Val_{i}" });
            await connection.ExecuteAsync("INSERT INTO [dbo].[Tab1] ([Id], [Value], [Tab2Id]) VALUES (@Id, @Value, @Tab2Id)", new { Id = i, Value = $"Val_{i}", Tab2Id = i });
        }
    }
}