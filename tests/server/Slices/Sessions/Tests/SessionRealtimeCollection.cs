using Xunit;
using Wtrfll.Server.Tests.Common;

namespace Wtrfll.Server.Tests.Slices.Sessions.Tests;

[CollectionDefinition("SessionRealtime", DisableParallelization = true)]
public sealed class SessionRealtimeCollection : ICollectionFixture<SessionApiFactory>
{
}
