using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDbFunction.Commands.ProcessItem;
using MongoDbFunction.Commands.ProcessThing;
using MongoDbMonitor;
using MongoDbMonitor.Commands.Exceptions;
using MongoDbMonitor.Commands.ProcessChangeEvent;
using MongoDbMonitor.Commands.ResolveCollectionType;
using MongoDbMonitor.Commands.SendNotification;
using MongoDbMonitor.Commands.SendSlackAlert;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MongoDbMonitorTest
{
    public class DbMonitorTests
    {
        private static IServiceCollection RegisterService()
        {
            IConfiguration configuration =
                new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "test.json"), false)
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton(configuration);

            services.AddLogging();

            services
                .AddOptions<Collection<CollectionOptions>>()
                .Configure<IConfiguration>((settings, cfg) =>
                {
                    cfg.Bind("MongoOptions:CollectionOptions", settings);
                });

            services.AddMemoryCache();

            RegisterMediator.AddMediatR(services, new[]
            {
                typeof(ProcessChangeEventRequest),
                typeof(ResolveCollectionTypeRequest),
                typeof(SendNotificationRequest),
                typeof(SendSlackAlertRequest),
                typeof(ProcessItemRequest),
                typeof(ProcessThingRequest)
            },
            new MediatRServiceConfiguration().AsScoped());

            return services;
        }

        [Fact]
        public async Task Should_Report()
        {
            using var provider = RegisterService().BuildServiceProvider();

            var data = new Dictionary<string, object>
            {
                ["_id"] = ObjectId.GenerateNewId(),
                ["name"] = "test",
                ["elapsed"] = 441
            };

            var handler = provider.GetRequiredService<IRequestHandler<ProcessChangeEventRequest, Unit>>();


            Unit response = await handler.Handle(
                new ProcessChangeEventRequest
                {
                    CollectionName = "items",
                     OperationType = ChangeStreamOperationType.Replace,
                     Values = data
                },
                CancellationToken.None);

            Assert.Equal(Unit.Value, response);
        }

        [Fact]
        public async Task Shoud_Throw_InvalidRequestTypeException()
        {
            using var provider = RegisterService().BuildServiceProvider();

            var data = new Dictionary<string, object>
            {
                ["_id"] = ObjectId.GenerateNewId(),
                ["name"] = "test",
                ["elapsed"] = 441
            };

            var handler = provider.GetRequiredService<IRequestHandler<ProcessChangeEventRequest, Unit>>();


            var exception = await Assert.ThrowsAsync<InvalidRequestTypeException>(() => handler.Handle(
                new ProcessChangeEventRequest
                {
                    CollectionName = "stuff",
                    OperationType = ChangeStreamOperationType.Replace,
                    Values = data
                },
                CancellationToken.None));
        }
    }
}