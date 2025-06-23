namespace Invalid8.Core.Exceptions;

public class EventSubscriptionException(string message, Exception innerException) : Exception(message, innerException);