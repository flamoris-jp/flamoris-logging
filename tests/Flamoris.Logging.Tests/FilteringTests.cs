namespace Flamoris.Logging.Tests;

public sealed class FilteringTests
{
    [Theory]
    [InlineData("debug", 4)]
    [InlineData("info", 3)]
    [InlineData("warn", 2)]
    [InlineData("error", 1)]
    public void Global_threshold_filters_all_levels(string configuredLevel, int expectedCount)
    {
        var sink = new MemorySink();
        var logger = TestLogger.Create(new LoggingOptions { Level = configuredLevel, Outputs = [] }, [sink]);
        logger.Debug("app", "debug"); logger.Info("app", "info");
        logger.Warn("app", "warn"); logger.Error("app", "error");
        Assert.Equal(expectedCount, sink.Events.Count);
    }

    [Fact]
    public void Most_specific_category_then_parent_then_global_wins()
    {
        var sink = new MemorySink();
        var options = new LoggingOptions { Level = "error", Outputs = [] };
        options.Categories["mcp"] = "warn";
        options.Categories["mcp.transport"] = "debug";
        var logger = TestLogger.Create(options, [sink]);
        logger.Info("mcp.query", "blocked");
        logger.Warn("mcp.query", "parent");
        logger.Debug("mcp.transport.http", "specific");
        logger.Warn("ui", "blocked");
        logger.Error("ui", "global");
        Assert.Equal(["parent", "specific", "global"], sink.Events.Select(item => item.Message));
    }

    [Fact]
    public void Invalid_level_falls_back_to_debug_and_reports_warning()
    {
        var normalized = Internal.ConfigurationNormalizer.Normalize(
            new LoggingOptions { Level = "verbose", Outputs = [] });
        Assert.Equal(LogLevel.Debug, normalized.Level);
        Assert.Contains(normalized.Warnings, item => item.Contains("Invalid global level"));
    }
}
