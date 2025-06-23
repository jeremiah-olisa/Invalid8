namespace Invalid8.Core.Exceptions;

public class EventConnectionException(string message, Exception innerException) : Exception(message, innerException);