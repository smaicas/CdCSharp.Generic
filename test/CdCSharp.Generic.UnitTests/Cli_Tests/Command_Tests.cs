namespace CdCSharp.Generic.UnitTests.Cli_Tests;
public class CommandTests
{
    public class TestArgs
    {
        [Arg("required", IsRequired = true)]
        public string Required { get; set; } = null!;

        [Arg("optional")]
        public string? Optional { get; set; }

        [Arg("flag", HasValue = false)]
        public bool Flag { get; set; }
    }

    [Fact]
    public async Task ExecuteAsync_WithValidArgs_CallsHandler()
    {
        // Arrange
        bool handlerCalled = false;
        Cli cli = new Cli()
            .Command<TestArgs>("test")
                .OnExecute(args =>
                {
                    Assert.Equal("value", args.Required);
                    Assert.Equal("optional", args.Optional);
                    Assert.True(args.Flag);
                    handlerCalled = true;
                    return Task.CompletedTask;
                });

        // Act
        await cli.ExecuteAsync(new[] {
            "test",
            "-required", "value",
            "-optional", "optional",
            "-flag"
        });

        // Assert
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredArg_ThrowsException()
    {
        // Arrange
        Cli cli = new Cli()
            .Command<TestArgs>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act & Assert
        Exception exception = await Record.ExceptionAsync(() =>
            cli.ExecuteAsync(new[] { "test" }));

        CliException cliException = Assert.IsType<CliException>(exception);
        Assert.Equal("Missing required argument: -required", cliException.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidArgValue_ThrowsException()
    {
        // Arrange
        Cli cli = new Cli()
            .Command<NumericArgs>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act & Assert
        Exception exception = await Record.ExceptionAsync(() =>
            cli.ExecuteAsync(new[] { "test", "-number", "notanumber" }));

        CliException cliException = Assert.IsType<CliException>(exception);
        Assert.Equal("Invalid value for -number: notanumber", cliException.Message);
    }

    public class NumericArgs
    {
        [Arg("number")]
        public int Number { get; set; }
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingValue_ThrowsException()
    {
        // Arrange
        Cli cli = new Cli()
            .Command<TestArgs>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act & Assert
        Exception exception = await Record.ExceptionAsync(() =>
            cli.ExecuteAsync(new[] { "test", "-required" }));

        CliException cliException = Assert.IsType<CliException>(exception);
        Assert.Equal("Missing value for -required", cliException.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WithAlias_CallsHandler()
    {
        // Arrange
        bool handlerCalled = false;
        Cli cli = new Cli()
            .Command<Empty>("test")
                .WithAlias("alias")
                .OnExecute(_ =>
                {
                    handlerCalled = true;
                    return Task.CompletedTask;
                });

        // Act
        await cli.ExecuteAsync(new[] { "alias" });

        // Assert
        Assert.True(handlerCalled);
    }

    [Fact]
    public async Task ExecuteAsync_WithErrorHandler_HandlesException()
    {
        // Arrange
        Exception? caught = null;
        Cli cli = new Cli()
            .WithErrorHandler(ex => caught = ex)
            .Command<TestArgs>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act
        await cli.ExecuteAsync(new[] { "test" });

        // Assert
        Assert.NotNull(caught);
        Assert.IsType<CliException>(caught);
    }
}