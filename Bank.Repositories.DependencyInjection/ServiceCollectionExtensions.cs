﻿using System.Data.SqlClient;
using Bank.Entities;

namespace Bank.Repositories.DependencyInjection;

/// <summary>
/// Расширение методов коллекции сервисов
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавить в список сервисов репозиторории работы с сообщениями
    /// </summary>
    /// <param name="services">Список зарегистрированных сервисов</param>
    /// <param name="options">Настройки</param>
    /// <returns>Список зарегистрированных сервисов</returns>
    public static IServiceCollection AddSmsRepositories(this IServiceCollection services,
        Action<InviteMessagesRepositoryOptions> options)
    {
        services
            .Configure(options)
            .AddScoped<IDbConnection, SqlConnection>(provider =>
                new SqlConnection(provider.GetRequiredService<IOptions<InviteMessagesRepositoryOptions>>().Value
                    .ConnectionString))
            .AddScoped<IInviteMessagesRepository, InviteMessagesRepository>();

        InitialDbMigration(services);

        return services;
    }

    private static void InitialDbMigration(IServiceCollection services)
    {
        using var sp = services.BuildServiceProvider();
        using var connection = sp.GetRequiredService<IDbConnection>();

        connection.Execute($"""
        IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = '{InviteMessageEntity.TableName}')
        BEGIN
        	CREATE TABLE [dbo].[{InviteMessageEntity.TableName}] (
        		[{nameof(InviteMessageEntity.Id)}] [int] IDENTITY(1,1) NOT NULL,
        		[{nameof(InviteMessageEntity.ApiId)}] [int] NOT NULL,
        		[{nameof(InviteMessageEntity.Message)}] [nvarchar](160) NOT NULL,
        		CONSTRAINT [PK_InviteMessage] PRIMARY KEY CLUSTERED ([{nameof(InviteMessageEntity.Id)}] ASC)
        	)
        	CREATE NONCLUSTERED INDEX [IX_InviteMessage_ApiId] ON [dbo].[{InviteMessageEntity.TableName}] ([{nameof(InviteMessageEntity.ApiId)}] ASC)
        
        	CREATE TABLE [dbo].[{InviteMessagesLogEntity.TableName}] (
        		[{nameof(InviteMessagesLogEntity.Id)}] [int] IDENTITY(1,1) NOT NULL,
        		[{nameof(InviteMessagesLogEntity.SendDateTime)}] [datetime] NOT NULL,
        		[{nameof(InviteMessagesLogEntity.Phone)}] [char](11) NOT NULL,
        		[{nameof(InviteMessagesLogEntity.InviteMessageId)}] [int] NOT NULL,
        		CONSTRAINT [PK_InviteMessagesLog] PRIMARY KEY CLUSTERED ([{nameof(InviteMessagesLogEntity.Id)}] ASC)
        	)
        	CREATE NONCLUSTERED INDEX [IX_InviteMessagesLog_SendDateTime] ON [dbo].[{InviteMessagesLogEntity.TableName}] ([{nameof(InviteMessagesLogEntity.SendDateTime)}] DESC)
        
        	ALTER TABLE [dbo].[{InviteMessagesLogEntity.TableName}] WITH CHECK ADD  
        	CONSTRAINT [FK_InviteMessagesLog_InviteMessage] 
        	FOREIGN KEY([{nameof(InviteMessagesLogEntity.InviteMessageId)}]) REFERENCES [dbo].[{InviteMessageEntity.TableName}] ([Id])
        
        	ALTER TABLE [dbo].[{InviteMessagesLogEntity.TableName}] CHECK CONSTRAINT [FK_InviteMessagesLog_InviteMessage]
        END
        """);
    }
}
