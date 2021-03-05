﻿using MediatR;
using Microsoft.Extensions.Logging;
using MongoDbMonitor.Commands.Exceptions;
using MongoDbMonitor.Commands.ExtractDocumentIdentifier;

namespace MongoDbMonitor.Commands.Common.ExceptionHandlers.ExtractDocumentIdentifier
{
    internal class InvalidObjectIdExceptionHandler<TRequest> :
        ExtractDocumentIdentifierRequestExceptionHandler<TRequest, InvalidObjectIdException>
        where TRequest : ExtractDocumentIdentifierRequest
    {
        public InvalidObjectIdExceptionHandler(
            IMediator mediator,
            ILogger<InvalidObjectIdExceptionHandler<TRequest>> logger) :
            base(mediator, logger)
        {
        }
    }
}
