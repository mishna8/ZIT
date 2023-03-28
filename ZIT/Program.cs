// See https://aka.ms/new-console-template for more information
using System.Diagnostics.Tracing;
using System;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Welcom To ZIT:");
                string input = Console.ReadLine();
                if (input == "quit")
                {
                    break;
                }
                string[] inputArr = input.Split(' ');
                ParseInput(inputArr);
            }
        }

        static void ParseInput(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Zit: Please enter a command.");
                return;
            }

            var command = args[0];
            var subCommand = args.Length > 1 ? args[1] : null;
            var parameters = args.Length > 2 ? string.Join(" ", args.Skip(2)) : null;

            if (command == "zit")
            {
                if (subCommand != null)
                {
                    if (subCommand == "--help") warning(0, "none");
                    else if (IsValidSubCommand(subCommand))
                    {
                        Console.WriteLine($"Executing subcommand '{subCommand}'");
                    }
                    else warning(2, subCommand);
                }
                if (subCommand == null) warning(0, "none");
            }
            else
            {
                warning(1, command);
            }
        }

        static bool IsValidSubCommand(string subCommand)
        {
            var validSubCommands = new List<string> { "init", "log", "status", "add", "commit", "checkout" ,
                                                    "revert", "tag", "clone","push", "pull",
                                                        "getall", "share", "unshare"};
            return validSubCommands.Contains(subCommand);
        }


        static void warning(int code, string word)
        {
            switch (code)
            {
                case 1:
                    {
                        //when the command does no start with zit
                        Console.WriteLine("'" + word + "' is not recognized as an internal or external command.");
                        break;
                    }
                case 2:
                    {
                        //when the sub command is wrong 
                        Console.WriteLine("zit: '" + word + "' is not a zit command. See 'zit --help'.");
                        break;
                    }
                case 3:
                    {
                        //when the parameters for the command are wrong
                        Console.WriteLine("zit: not a zit argument. See 'zit --help'.");
                        break;
                    }
                case 0:
                    {
                        // the --help argument 
                        Console.WriteLine("usage: zit  [--help] <command> [<args>]\r\n\r\n" +
                        "enter quit to exit\r\n\r\n" +
                        "These are common Zit commands used in various situations:\r\n\r\n" +
                        "start a working area \r\n" +
                        "clone     Clone a repository into a new directory\r\n" +
                        "init      Create an empty Zit repository or reinitialize an existing one\r\n\r\n" +
                        "work on the current change \r\n" +
                        "add       Add file contents to the index\r\n" +
                        "examine the history and state \r\n" +
                        "log       Show commit logs\r\n" +
                        "status    Show the working tree status\r\n\r\n" +
                        "grow, mark and tweak your common history\r\n" +
                        //"branch    List, create, or delete branches\r\n" +
                        //"merge     Join two or more development histories together\r\n" +
                        "commit    Record changes to the repository\r\n" +
                        "revert    make the specified commit \r\n" +
                        "tag       Create, list, delete or verify a tag object signed with GPG\r\n\r\n" +
                        "collaborate \r\n" +
                        "pull      Fetch from and integrate with another repository or a local branch\r\n" +
                        "push      Update remote refs along with associated objects\r\n\r\n" +
                        "administration functions\r\n" +
                        "getall      view all the repositories\r\n" +
                        "share     give permissions on the repository\r\n" +
                        "unshare      revoke permissions on the repository\r\n");
                        break;
                    }
            }
        }


    }
}