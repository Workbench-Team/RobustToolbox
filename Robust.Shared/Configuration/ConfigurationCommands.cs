using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Robust.Shared.Console;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Utility;

namespace Robust.Shared.Configuration
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal sealed class CVarCommand : IConsoleCommand
    {
        public string Command => "cvar";
        public string Description => Loc.GetString("cmd-cvar-desc");

        public string Help => Loc.GetString("cmd-cvar-help");

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length is < 1 or > 2)
            {
                shell.WriteError(Loc.GetString("cmd-cvar-invalid-args"));
                return;
            }

            var configManager = IoCManager.Resolve<IConfigurationManager>();
            var name = args[0];

            if (name == "?")
            {
                var cvars = configManager.GetRegisteredCVars().OrderBy(c => c);
                shell.WriteLine(string.Join("\n", cvars));
                return;
            }

            if (!configManager.IsCVarRegistered(name))
            {
                shell.WriteError(Loc.GetString("cmd-cvar-not-registered", ("cvar", name)));
                return;
            }

            if (args.Length == 1)
            {
                // Read CVar
                var value = configManager.GetCVar<object>(name);
                shell.WriteLine(value.ToString()!);
            }
            else
            {
                // Write CVar
                var value = args[1];
                var type = configManager.GetCVarType(name);
                try
                {
                    var parsed = ParseObject(type, value);
                    configManager.SetCVar(name, parsed);
                }
                catch (FormatException)
                {
                    shell.WriteError(Loc.GetString("cmd-cvar-parse-error", ("type", type)));
                }
            }
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            var cfg = IoCManager.Resolve<IConfigurationManager>();
            if (args.Length == 1)
            {
                var helpQuestion = Loc.GetString("cmd-cvar-compl-list");

                return CompletionResult.FromHintOptions(
                    cfg.GetRegisteredCVars()
                        .Select(c => new CompletionOption(c, GetCVarValueHint(cfg, c)))
                        .Union(new[] { new CompletionOption("?", helpQuestion) })
                        .OrderBy(c => c.Value),
                    Loc.GetString("cmd-cvar-arg-name"));
            }

            var cvar = args[0];
            if (!cfg.IsCVarRegistered(cvar))
                return CompletionResult.Empty;

            var type = cfg.GetCVarType(cvar);
            return CompletionResult.FromHint($"<{type.Name}>");
        }

        private static string GetCVarValueHint(IConfigurationManager cfg, string cVar)
        {
            var flags = cfg.GetCVarFlags(cVar);
            if ((flags & CVar.CONFIDENTIAL) != 0)
                return Loc.GetString("cmd-cvar-value-hidden");

            var value = cfg.GetCVar<object>(cVar).ToString() ?? "";
            if (value.Length > 50)
                value = $"{value[..51]}…";

            return value;
        }

        private static object ParseObject(Type type, string input)
        {
            if (type == typeof(bool))
            {
                if (bool.TryParse(input, out var val))
                    return val;

                if (Parse.TryInt32(input, out var intVal))
                {
                    if (intVal == 0) return false;
                    if (intVal == 1) return true;
                }

                throw new FormatException($"Could not parse bool value: {input}");
            }

            if (type == typeof(string))
            {
                return input;
            }

            if (type == typeof(int))
            {
                return Parse.Int32(input);
            }

            if (type == typeof(float))
            {
                return Parse.Float(input);
            }

            throw new NotSupportedException();
        }
    }
}
