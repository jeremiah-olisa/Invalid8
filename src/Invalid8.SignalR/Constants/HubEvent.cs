using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invalid8.SignalR.Constants;

public record HubEvent(string Name)
{
    public static readonly HubEvent CacheInvalidated = new("CacheInvalidated");
    public static readonly HubEvent CacheUpdated = new("CacheUpdated");

    public override string ToString() => Name;
}