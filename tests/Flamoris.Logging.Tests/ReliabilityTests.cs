namespace Flamoris.Logging.Tests;

public sealed class ReliabilityTests
{
    [Fact]
    public void Failure_in_one_output_does_not_block_another()
    {
        var diagnostics = new List<string>();
        var healthy = new MemorySink();
        var logger = TestLogger.Create(new LoggingOptions(), [new ThrowingSink(), healthy], diagnostics.Add);
        Assert.Null(Record.Exception(() => logger.Info("app", "delivered")));
        Assert.Single(healthy.Events);
        Assert.Contains(diagnostics, item => item.Contains("Logging output failed"));
    }

    [Fact]
    public async Task Concurrent_logging_does_not_drop_events()
    {
        var sink = new MemorySink();
        var logger = TestLogger.Create(new LoggingOptions(), [sink]);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(worker => Task.Run(() =>
        {
            for (var index = 0; index < 250; index++)
                logger.Debug("render", "frame", new Dictionary<string, object?>
                    { ["worker"] = worker, ["index"] = index });
        })));
        Assert.Equal(2_000, sink.Events.Count);
    }

    [Fact]
    public void Sensitive_properties_are_redacted_by_default()
    {
        var sink = new MemorySink();
        var logger = TestLogger.Create(new LoggingOptions(), [sink]);
        logger.Info("mcp.auth", "authorized",
            new Dictionary<string, object?> { ["token"] = "secret", ["client"] = "chatgpt" });
        Assert.Equal("[REDACTED]", sink.Events[0].Properties["token"]);
        Assert.Equal("chatgpt", sink.Events[0].Properties["client"]);
    }
}
