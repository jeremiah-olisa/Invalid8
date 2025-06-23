namespace Invalid8.Core;

/// <summary>
/// Event arguments for event provider connection state changes
/// </summary>
public class EventProviderConnectionEventArgs : EventArgs
{
    /// <summary>
    /// The new connection state
    /// </summary>
    public required bool IsConnected { get; set; }
    
    /// <summary>
    /// The previous connection state
    /// </summary>
    public required bool WasConnected { get; set; }
    
    /// <summary>
    /// Error that caused disconnection, if any
    /// </summary>
    public Exception? Error { get; set; }
    
    /// <summary>
    /// Timestamp when the connection state changed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}