using System;
using System.Linq;
using System.Reflection;

public class CheatConsole
{
    public static Action<ConsoleCommands.ConsoleCommandResult> OnCommandExecuted;

    public static void ExecuteCommand(ConsoleCommandName command, string[] arguments)
    {
        UnityEngine.Debug.Log("Executing console comand " + command);
        var commandTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                           where t.IsClass && t.Namespace == "ConsoleCommands"
                           select t;

        var commandInstance = Activator.CreateInstance(commandTypes.First(t => t.Name == command.ToString())) as ConsoleCommands.ConsoleCommand;
        var result = commandInstance.Execute(arguments);
        OnCommandExecuted?.Invoke(result);
    }
}