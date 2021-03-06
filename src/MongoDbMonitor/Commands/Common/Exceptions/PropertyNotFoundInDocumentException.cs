﻿using System;

namespace MongoDbMonitor.Commands.Exceptions
{
    internal class PropertyNotFoundInDocumentException : Exception
    {
        public PropertyNotFoundInDocumentException(string propertyName)
            : base($"Property: {propertyName} is not part of received MongoDb document")
        {
        }
    }
}
