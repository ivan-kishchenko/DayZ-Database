﻿using CommandLine;
using System;

namespace DayzDatabaseServer
{
    public class CommandLineOptions
    {
        [Option('p', "port", Required = false, Default = Constants.DEFAULT_SERVER_PORT, HelpText = "Sets the server port.")]
        public int Port { get; set; }

        [Option("no-logs", Required = false, Default = false, HelpText = "Disable logging to file and output.")]
        public bool NoLogs { get; set; }

        [Option("debug", Required = false, Default = false, HelpText = "Enable debug logging (Print all queries to the log).")]
        public bool Debug { get; set; }

        public CommandLineOptions() { }

        public static void Parse(string[] args, Action<CommandLineOptions> result)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed(result);
        }
    }
}
