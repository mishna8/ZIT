using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZIT;

namespace ConsoleApp
{
    internal class commands
    {
        //1
        public static void do_init() 
        { 
            Console.WriteLine("hi"); 
        }
        //2
        public static void do_log() 
        { 
            Console.WriteLine("hi"); 
        }
        //3
        public static void do_status()
        { 
            Console.WriteLine("hi"); 
        }
        //4
        public static void do_add()
        { 
            Console.WriteLine("hi"); 
        }
        
        //5
        public static void do_commit(string param)
        {
            //get the message
            string[] p = param.Split(' ');
            var msg = p.Length > 1 ? string.Join(" ", param.Skip(1)) : null;
            //check
            if (p[0] != "-m") { Program.warning(3, "commmit"); return; }
            if (msg == null) { Console.WriteLine("Zit: message is reqiered for commit"); return; }

            //do the commit 
        }
        //6
        public static void do_tag(string param) 
        {
            //get the message
            string[] p = param.Split(' ');
            var msg = p.Length > 1 ? string.Join(" ", param.Skip(1)) : null;
            //check
            if (p[0] != "-m") { Program.warning(3, "tag"); return; }
            if (msg == null) { Console.WriteLine("Zit: name is reqiered for tag"); return; }

            //do the tag 
        }

        //7
        public static void do_checkout(string param)
        {
            Commit com = getCommit(param);


        }
        //8
        public static void do_revert(string param) 
        {
           Commit com = getCommit(param);

        }

        //9
        public static void do_clone(string param)
        {
            //if there are parameters 
            var lnk = " ";
            
        }
        //10
        public static void do_push(string param) 
        { 
            Console.WriteLine("hi"); 
        }
        //11
        public static void do_pull(string param) 
        { 
            Console.WriteLine("hi"); 
        }
        
        //12
        public static void do_getall() 
        { 
            Console.WriteLine("hi"); 
        }

        //there are to be 3 parameters that may be or may not be 
        //13
        public static void do_share(string args) 
        { 
            
            Console.WriteLine("hi"); 
        }
        //14
        public static void do_unshare(string args) 
        { 
            Console.WriteLine("hi"); 
        }

        

        static Commit getCommit(string param)
        {
            //get the reference
            string[] p = param.Split(' ');
            var c = p.Length > 1 ? string.Join(" ", param.Skip(1)) : null;
            //check
            if (c == null) { Console.WriteLine("Zit: please specify another commit"); return null; }

            //find it and return it
            //look for the hash in the commit table
            if (p[0] == "-n") 
            {

            }
            //look for the tag in the tag table and get its hash
            else if (p[0] == "-t") 
            {
                
            }
            //isolate the number from the string and go back 
            else if(p[0] == "-h") 
            {
                var x = getNum(c);
                if(x==-1) return null;
                for (int i=0; i<=x;i++)
                {

                }
            }
            else { Program.warning(3, "tag"); return null; }

        }
        static int getNum(string input)
        {
            //find the number
            string[] parts = input.Split('~');
            string num = parts[1];

            // parse the number string as an integer
            if (int.TryParse(num, out int x))
            {
                // Return the extracted number 
                return x;
            }
            else
            {
                Console.WriteLine("Zit: not a correct reference.");
                return -1; 
            }
        }


        static string getRepo(string param)
        {
            var lnk = " ";
            if (param != null)
            {
                string[] p = param.Split(' ');
                lnk = p.Length > 1 ? string.Join(" ", param.Skip(1)) : " ";
                //check
                if (p[0] != "-r") { Program.warning(3, "clone"); return null; }
                if (lnk == null) { Console.WriteLine("Zit: please provide a repository with -r"); return; }
            }
            if (lnk != " ") { checkRepo(lnk); }
        }
        static bool checkRepo(string repo) 
        { 
            if(repo!=null) return true; 
            return false;
        }


    }
}
