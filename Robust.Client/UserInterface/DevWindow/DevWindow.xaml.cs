﻿using System.Linq;
using JetBrains.Annotations;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Console;
using Robust.Shared.IoC;

namespace Robust.Client.UserInterface
{
    [GenerateTypedNameReferences]
    public sealed partial class DevWindow : Control
    {
        public DevWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            RobustXamlLoader.Load(this);

            TabContainer.SetTabTitle(DebugConsole, "Debug Console");
            TabContainer.SetTabTitle(UI, "User Interface");
            TabContainer.SetTabTitle(Perf, "Profiling");
        }
    }

    [UsedImplicitly]
    internal sealed class TestWindowCommand : IConsoleCommand
    {
        public string Command => "devwindow";
        public string Description => "A";
        public string Help => "A";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var clyde = IoCManager.Resolve<IClyde>();
            var monitor = clyde.EnumerateMonitors().First();
            if (args.Length > 0)
            {
                var id = int.Parse(args[0]);
                monitor = clyde.EnumerateMonitors().Single(m => m.Id == id);
            }

            var window = clyde.CreateWindow(new WindowCreateParameters
            {
                //Maximized = true,
                Title = "Robust Debug Window",
                //Monitor = monitor,
            });
            var root = IoCManager.Resolve<IUserInterfaceManager>().CreateWindowRoot(window);
            window.DisposeOnClose = true;

            var control = new DevWindow();

            root.AddChild(control);
        }
    }
}
