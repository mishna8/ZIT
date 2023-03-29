// See https://aka.ms/new-console-template for more information
using System.Diagnostics.Tracing;
using System;
using System.Runtime.Intrinsics.X86;

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
            //check if the user entered a command
            if (args.Length == 0)
            {
                Console.WriteLine("Zit: Please enter a command.");
                return;
            }

            var command = args[0];
            var subCommand = args.Length > 1 ? args[1] : null;
            var parameters = args.Length > 2 ? string.Join(" ", args.Skip(2)) : null;

            //if the command is right
            if (command == "zit")
            {
                //if the subcommand is right
                if (subCommand != null)
                {
                    if (subCommand == "--help") warning(0, "none");
                    //if the parameters of the subcommand is right                        
                    else if (IsValidSubCommand(subCommand))
                    {
                        //may or may not have parameters
                        switch (subCommand)
                        {
                            case "clone": { commands.do_clone(parameters); break; }
                            case "pull": { commands.do_pull(parameters); break; }
                            case "push": { commands.do_push(parameters); break; }
                            default: {  break; }//no worning - check in each func
                        }
                        
                        //the no parameters group 
                        if (parameters == null)
                        {
                            switch (subCommand)
                            {
                                case "init": { commands.do_init(); break; }
                                case "add": { commands.do_add(); break; }
                                case "log": { commands.do_log(); break; }
                                case "status": { commands.do_status(); break; }
                                case "getall": { commands.do_getall(); break; }
                                default: { warning(3, subCommand); break; }
                            }
                        }
                        //the parameters are checked in each command with its own conditions
                        else if(parameters != null)
                        {
                            switch (subCommand)
                            {
                                case "commit": { commands.do_commit(parameters); break; }
                                case "tag": { commands.do_tag(parameters); break; }
                                case "checkout": { commands.do_checkout(parameters); break; }
                                case "revert": { commands.do_revert(parameters); break; }
                                case "share": { commands.do_share(parameters); break; }
                                case "unshare": { commands.do_unshare(parameters); break; }
                                default: { warning(3, subCommand); break; }
                            }
                        }
                    }
                    else warning(2, subCommand);
                }
                else if (subCommand == null) { warning(0, "none"); }
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


        public static void warning(int code, string word)
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
                        "clone [-r <repo>]    Clone a repository into a new directory\r\n" +
                        "init      Create an empty Zit repository or reinitialize an existing one\r\n\r\n" +
                        "work on the current change \r\n" +
                        "add       Add file contents to the index\r\n" +
                        "examine the history and state \r\n" +
                        "log       Show commit logs\r\n" +
                        "status    Show the working tree status\r\n\r\n" +
                        "grow, mark and tweak your common history\r\n" +
                        //"branch    List, create, or delete branches\r\n" +
                        //"merge     Join two or more development histories together\r\n" +
                        "commit [-m <msg>]   Record changes to the repository\r\n" +
                        "tag   [-m <msg>    Create, list, delete or verify a tag object signed with GPG\r\n" +
                        "revert [-n <commit>|-t <tag>| -h head~<number>]   make the specified commit as the last \r\n" +
                        "checkout [-n <commit>|-t <tag>| -h head~<number>] go to the specified commit\r\n\r\n" +
                        "collaborate \r\n" +
                        "pull  [-r <repo>]    Fetch from and integrate with another repository or a local branch\r\n" +
                        "push  [-r <repo>]    Update remote refs along with associated objects\r\n\r\n" +
                        "administration functions\r\n" +
                        "getall      view all the repositories\r\n" +
                        "share [-r <repo> -u <user> -a <right>]    give permissions on the repository\r\n" +
                        "unshare [-r <repo> -u <user> -a <right>]   revoke permissions on the repository\r\n");
                        break;
                    }
            }
        }


    }
}