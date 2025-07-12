using Invalid8.Core.Models;
using MediatR;

namespace Invalid8.MediatR.Events;

public record MediatRCacheUpdatedEvent(CacheUpdatedEvent Event) : INotification;