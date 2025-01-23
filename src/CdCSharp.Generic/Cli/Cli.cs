using System.Reflection;
using System.Text;

namespace CdCSharp.Generic;

public class Cli
{
    private readonly Dictionary<string, ICommand> _commands = new(StringComparer.OrdinalIgnoreCase);
    private Action<Exception>? _errorHandler;
    private string? _description;

    public Cli WithErrorHandler(Action<Exception> handler)
    {
        _errorHandler = handler;
        return this;
    }

    public Cli WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public CommandBuilder<TArgs> Command<TArgs>(string name) where TArgs : new() =>
        new(name, this);

    public async Task ExecuteAsync(string[] args)
    {
        try
        {
            if (args.Length == 0 || args[0] is "-h" or "--help")
            {
                ShowHelp();
                return;
            }

            string commandName = args[0];
            if (!_commands.TryGetValue(commandName, out ICommand? command))
            {
                throw new CliException($"Unknown command: {commandName}");
            }

            await command.ExecuteAsync(args[1..]);
        }
        catch (Exception ex) when (_errorHandler != null)
        {
            _errorHandler(ex);
        }
    }

    internal void AddCommand(string name, ICommand command) =>
        _commands[name] = command;

    private void ShowHelp()
    {
        if (_description != null)
            Console.WriteLine(_description + Environment.NewLine);

        Console.WriteLine("Commands:");
        foreach (ICommand? cmd in _commands.Values.DistinctBy(x => x.GetType()))
            Console.WriteLine($"  {cmd.GetHelp()}");
    }
}

public class CommandBuilder<TArgs> where TArgs : new()
{
    private readonly string _name;
    private readonly Cli _cli;
    private readonly List<string> _aliases = [];
    private string? _description;
    private Func<TArgs, Task>? _handler;

    public CommandBuilder(string name, Cli cli)
    {
        _name = name;
        _cli = cli;
    }

    public CommandBuilder<TArgs> WithAlias(string alias)
    {
        _aliases.Add(alias);
        return this;
    }

    public CommandBuilder<TArgs> WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public Cli OnExecute(Func<TArgs, Task> handler)
    {
        _handler = handler;
        Command<TArgs> command = new(_name, _aliases, _description, _handler);
        _cli.AddCommand(_name, command);
        foreach (string alias in _aliases)
            _cli.AddCommand(alias, command);
        return _cli;
    }

    public Cli OnExecute(Action<TArgs> handler) => OnExecute(args => { handler(args); return Task.CompletedTask; });
}

public class Command<TArgs> : ICommand where TArgs : new()
{
    private readonly string _name;
    private readonly IReadOnlyList<string> _aliases;
    private readonly string? _description;
    private readonly Func<TArgs, Task> _handler;

    public Command(string name, IReadOnlyList<string> aliases, string? description, Func<TArgs, Task> handler)
    {
        _name = name;
        _aliases = aliases;
        _description = description;
        _handler = handler;
    }

    public async Task ExecuteAsync(string[] args)
    {
        TArgs? parsedArgs = ParseArgs(args);
        await _handler(parsedArgs);
    }

    public string GetHelp()
    {
        StringBuilder help = new(_name);
        if (_aliases.Any())
            help.Append($" (aliases: {string.Join(", ", _aliases)})");

        PropertyInfo[] props = typeof(TArgs).GetProperties();
        if (props.Length > 0)
        {
            help.Append(" [args:");
            foreach (PropertyInfo? prop in props)
            {
                ArgAttribute attr = prop.GetCustomAttribute<ArgAttribute>();
                help.Append($" {attr?.Name ?? prop.Name}");
            }
            help.Append(']');
        }

        if (_description != null)
            help.Append($" - {_description}");

        return help.ToString();
    }

    private TArgs ParseArgs(string[] args)
    {
        TArgs result = new();
        Dictionary<string, (PropertyInfo Property, ArgAttribute ArgAttribute)> props = typeof(TArgs).GetProperties()
            .Select(p => (Property: p, Attribute: p.GetCustomAttribute<ArgAttribute>()))
            .Where(x => x.Attribute != null)
            .ToDictionary(
                x => x.Attribute!.Name,
                x => (x.Property, x.Attribute!),
                StringComparer.OrdinalIgnoreCase
            );

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (!arg.StartsWith("-") || !props.TryGetValue(arg[1..], out (PropertyInfo Property, ArgAttribute ArgAttribute) prop))
                throw new CliException($"Unknown argument: {arg}");

            string value = prop.ArgAttribute.HasValue ?
                (i + 1 < args.Length ? args[++i] : throw new CliException($"Missing value for {arg}")) :
                "true";

            try
            {
                object convertedValue = Convert.ChangeType(value, prop.Property.PropertyType);
                prop.Property.SetValue(result, convertedValue);
            }
            catch
            {
                throw new CliException($"Invalid value for {arg}: {value}");
            }
        }

        foreach ((string name, (PropertyInfo prop, ArgAttribute attr)) in props)
        {
            if (attr.IsRequired && prop.GetValue(result) == null)
                throw new CliException($"Missing required argument: -{name}");
        }

        return result;
    }
}

public interface ICommand
{
    Task ExecuteAsync(string[] args);
    string GetHelp();
}

[AttributeUsage(AttributeTargets.Property)]
public class ArgAttribute : Attribute
{
    public string Name { get; }
    public bool IsRequired { get; init; }
    public bool HasValue { get; init; } = true;

    public ArgAttribute(string name) => Name = name;
}

public class CliException : Exception
{
    public CliException(string message) : base(message) { }
}