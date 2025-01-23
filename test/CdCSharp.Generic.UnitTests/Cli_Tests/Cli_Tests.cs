namespace CdCSharp.Generic.UnitTests.Cli_Tests;

public class Empty { }

public class Cli_Tests
{
    [Fact]
    public async Task ExecuteAsync_WithNoArgs_ShowsHelp()
    {
        // Arrange
        StringWriter output = new();
        Console.SetOut(output);

        Cli cli = new Cli()
            .WithDescription("Test CLI")
            .Command<Empty>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act
        await cli.ExecuteAsync(Array.Empty<string>());

        // Assert
        string help = output.ToString();
        Assert.Contains("Test CLI", help);
        Assert.Contains("test", help);
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    public async Task ExecuteAsync_WithHelpFlag_ShowsHelp(string helpFlag)
    {
        // Arrange
        StringWriter output = new();
        Console.SetOut(output);

        Cli cli = new Cli()
            .Command<Empty>("test")
                .OnExecute(_ => Task.CompletedTask);

        // Act
        await cli.ExecuteAsync(new[] { helpFlag });

        // Assert
        Assert.Contains("test", output.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCommand_ThrowsException()
    {
        // Arrange
        Cli cli = new();
        Exception exception = await Record.ExceptionAsync(() =>
            cli.ExecuteAsync(new[] { "unknown" }));

        // Assert
        CliException cliException = Assert.IsType<CliException>(exception);
        Assert.Equal("Unknown command: unknown", cliException.Message);
    }
}


