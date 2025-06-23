namespace Invalid8.Core.Exceptions;

/// <summary>
/// Exception thrown when a mutation operation fails
/// </summary>
public class MutationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the MutationException class
    /// </summary>
    public MutationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the MutationException class with a specified error message
    /// </summary>
    public MutationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the MutationException class with a specified error message and inner exception
    /// </summary>
    public MutationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}