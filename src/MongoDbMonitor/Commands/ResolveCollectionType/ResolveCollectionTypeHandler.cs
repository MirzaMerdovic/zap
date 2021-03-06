﻿using MediatR;
using Microsoft.Extensions.Caching.Memory;
using MongoDbMonitor.Commands.Common.Responses;
using MongoDbMonitor.Commands.Exceptions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDbMonitor.Commands.ResolveCollectionType
{
    internal class ResolveCollectionTypeHandler : IRequestHandler<ResolveCollectionTypeRequest, ProcessingStatusResponse>
    {
        private const string VALUES_PROPERTY_NAME = "Values";
        private const string SEND_METHOD_NAME = "Send";

        private readonly IMediator _mediator;
        private readonly IMemoryCache _cache;

        public ResolveCollectionTypeHandler(IMediator mediator, IMemoryCache cache)
        {
            _mediator = mediator;
            _cache = cache;
        }

        public async Task<ProcessingStatusResponse> Handle(ResolveCollectionTypeRequest request, CancellationToken cancellationToken)
        {
            var key = $"{request.AssemblyName}-{request.HandlerRequestFullQualifiedName}";

            object instance = _cache.Get(key);

            instance ??= _cache.Set(key, CreateInstance(request));

            var method =
                typeof(ISender)
                    .GetMethods()
                    .First(
                        x =>
                            x.Name == SEND_METHOD_NAME &&
                            x.IsGenericMethod);

            dynamic send = method.MakeGenericMethod(typeof(ProcessingStatusResponse));

            ProcessingStatusResponse response = await send.Invoke(_mediator, new[] { instance, cancellationToken });

            return response;
        }

        private static object CreateInstance(ResolveCollectionTypeRequest request)
        {
            var instance = CreateRequestInstance(request.AssemblyName, request.HandlerRequestFullQualifiedName);

            var type = instance.GetType();

            var valuesProperty = type.GetProperty(VALUES_PROPERTY_NAME);

            if (valuesProperty == null)
                throw new MissingRequiredPropertyException(request.HandlerRequestFullQualifiedName, VALUES_PROPERTY_NAME);

            valuesProperty.SetValue(instance, request.Values);

            return instance;
        }

        private static object CreateRequestInstance(string assemblyName, string requestFullQualifiedName)
        {
            try
            {
                var instance = Activator.CreateInstance(assemblyName, requestFullQualifiedName)?.Unwrap();

                return instance;
            }
            catch(Exception ex)
            {
                throw new InvalidRequestTypeException(assemblyName, requestFullQualifiedName, ex);
            }
        }
    }
}
