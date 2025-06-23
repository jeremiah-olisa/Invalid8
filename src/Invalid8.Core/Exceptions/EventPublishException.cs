namespace Invalid8.Core.Exceptions;

public class EventPublishException(string message, Exception innerException) : Exception(message, innerException);