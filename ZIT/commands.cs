using System;
using System.Collections.Generic;
//using System.IO;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleApp
{
    internal class commands
    {
        // Get the current working directory
        public static string workingDirectory = Directory.GetCurrentDirectory();
        // Initialize the staging area
        public static List<FileInfo> stagedFiles = new List<FileInfo>();
        // a list of all the files exists in one place and not the other to prepare for pull/push
        public static List<FileInfo> dif = new List<FileInfo>();

        //1
        public static void do_init()
        {
            //??????what if already exists????
            // Create a new directory called ".zit"
            string zitDirectoryPath = Path.Combine(workingDirectory, ".zit");
            Directory.CreateDirectory(zitDirectoryPath);

            // Create subdirectories inside the .zit directory
            string[] subdirectoryNames = { "hooks", "info", "objects", "refs" };
            foreach (string subdirectoryName in subdirectoryNames)
            {
                string subdirectoryPath = Path.Combine(zitDirectoryPath, subdirectoryName);
                Directory.CreateDirectory(subdirectoryPath);
            }

            // Create a new file called "HEAD" inside the .zit directory
            string headFilePath = Path.Combine(zitDirectoryPath, "HEAD");
            File.Create(headFilePath);

            //??????head is file?????????

            Console.WriteLine("Initialized empty Zit repository in " + zitDirectoryPath);
            
            //add the repo to the repo reg with the creator user as owner
        }

        //2
        public static void do_log()
        {
            //access the current repo
            string repo = getRepo();
            //access the commit table for this repo
            Console.WriteLine("Commit History :");
            //foreach (commit commitEntry in the commits group of th reg )
            //{
            //make a string of it 
            //string commitString = ToString(commitEntry);
            //Console.WriteLine(commitString);
            //}
        }

        //3
        public static void do_status()
        {
            // Get all files in the working directory
            string[] files = Directory.GetFiles(workingDirectory);
            //display all files
            Console.WriteLine("Tracked Files:");
            foreach (FileInfo file in stagedFiles)
            {
                Console.WriteLine(file.Name);
            }
            Console.WriteLine("Untracked Files:");
            foreach (string fileName in files)
            {
                if (!(stagedFiles.Exists(file => file.FullName == fileName)))
                    Console.WriteLine(fileName);
            }
        }

        //-------------------------------------------------
        //4
        public static void do_add()
        {
            Console.WriteLine("started zit add");

            // Get all files in the working directory
            string[] files = Directory.GetFiles(workingDirectory);
            // Stage each file
            foreach (string file in files)
            {
                StageFile(file);
            }

            Console.WriteLine("returned to zit add");

            // Remove for debug 
            //EmptyStagedFiles();
            //UnstageFile("");

        }
        static void StageFile(string fileName)
        {
            // Check if the file is already staged
            if (stagedFiles.Exists(file => file.FullName == fileName))
            {
                Console.WriteLine(fileName + " is already staged.");
            }
            else
            {
                // Get a reference to the file and add it to the staging area
                FileInfo file = new FileInfo(fileName);
                stagedFiles.Add(file);
                Console.WriteLine(file.Name + " succesfully staged.");

            }
        }
        static void UnstageFile(string fileName)
        {
            // Check if the file is not staged
            
            foreach (FileInfo file in stagedFiles)
            {
                if (file.Name == fileName)
                {
                    // Remove the file from the staging area
                    stagedFiles.Remove(file);
                    Console.WriteLine(fileName + " has been unstaged.");
                    return;
                }
            }
            //if we passed all the files it it didnt return it wasnt there
            Console.WriteLine(fileName + " is not staged , failed to unstage");

        }
        public static void EmptyStagedFiles()
        {
            stagedFiles.Clear();
        }


        //####################################################################
       
        //5
        public static void do_tag(string param)
        {
            string n = null;
            string m = null;
            string[] flags = ParseParameters(param);
            
            //check the parameters
            if (flags[0] == "m") m = flags[1];
            else if (flags[0] == "a") n = flags[1];
            else if (flags[0] == "0") { Console.WriteLine("Zit: a name or message is reqiered for tag"); return; }
            else { Program.warning(3, "tag"); return; }
            if (flags[2] == "m") m = flags[3];
            else if (flags[2] == "a") n = flags[3];
            else if (flags[2] != "0") { Program.warning(3, "tag"); return; }
            if (flags[4] != "0" && flags[5] != "0") { Program.warning(3, "tag"); return; }

            //do the tag 
            //get the commit needed to tag - the one checkedout
            Commit com = getCurCommit();
            //create the tag
            Tag t = new Tag(n, m);
            com.Tags = t;
        }

        //6
        public static void do_commit(string param)
        {           
            string msg = null;
            string[] flags = ParseParameters(param);

            //check the parameters
            if (flags[0] == "m") msg = flags[1];
            else if (flags[0] == "0") { Console.WriteLine("Zit: a message is reqiered for commit"); return; }
            else { Program.warning(3, "commit"); return; }
            if (flags[2] != "0" || flags[3] != "0" || flags[4] != "0" || flags[5] != "0") { Program.warning(3, "commit"); return; }
            Console.WriteLine("start comit param is |" + flags[0] + "|" + msg + "|");
            
            //get the author
            string auther = getUser();
            Console.WriteLine(" auther is " + auther);

            //create a new objects for the commit 
            Dictionary<string, string> FileSnapshot = new Dictionary<string, string>();
            Tag t = new Tag(null, null);
            // Create a new commit object
            Commit commit = new Commit(auther, msg, t, FileSnapshot);
            Console.WriteLine("here1");

            // Create a snapshot of each file and add it to the commit object
            foreach (FileInfo file in stagedFiles)
            {
                Console.WriteLine("commiting now " + file+ "|"+ file.Name );
                string filePath = Path.Combine(workingDirectory, file.Name);
               //create the contant of the file in a snapshot 
                byte[] fileContent = File.ReadAllBytes(filePath);
                string fileHash = GetHashString(fileContent);
                commit.Snapshot.Add(file.Name, fileHash);
            }
            //add the commit to the repo history


            //clear the staging area to the next modification?
            /*foreach (FileInfo file in stagedFiles)
            {
                UnstageFile(file.Name);
            }
            */
        }
        static string GetHashString(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(data);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        //7
        public static void do_revert(string param) 
        {
            //get the commit hash number 
           Commit com = getCommitHash(param);
            //create the new reverted commit
           Commit revCom = Commit.createCommit(com);
            //add the commit to the repo history
        }
        //8
        public static void do_checkout(string param)
        {
            //get the commit hash number 
            Commit com = getCommitHash(param);
            //move the head 

        }

        //-------------------------------------------------------------------
        //returns the current commit that is being checkedout
        static Commit getCurCommit()
        {
            Dictionary<string, string> FileSnapshot = new Dictionary<string, string>();
            Tag t = new Tag(null, null);
            Commit commit1 = new Commit("a", "m", t, FileSnapshot);
            return commit1;
        }
        static Commit getCommitHash(string param)
        {
            //get the reference
            string[] p = param.Split(' ');
            var c = p.Length > 1 ? string.Join(" ", param.Skip(1)) : null;
            //check
            if (c == null) { Console.WriteLine("Zit: please specify another commit"); return null; }
            /*
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
            else if (p[0] == "-h")
            {
                var x = getNum(c);
                if (x == -1) return null;
                for (int i = 0; i <= x; i++)
                {

                }
            }
            else { Program.warning(3, "tag"); return null; }
            */
            Dictionary<string, string> FileSnapshot = new Dictionary<string, string>();
            Tag t = new Tag(null, null);
            Commit commit1 = new Commit("a", "m", t, FileSnapshot);
            return commit1;
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


        //####################################################################



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
            //check for conflicts
            //access specified remote repo's commit histroy table
            //sql compare??
            //foreach file in local commit history
            //if not exists in remote commit history
            //FileInfo file = new FileInfo(fileName);
            //dif.Add(file);
            //
            //conflict - if the remote and local are diverged 
            //Console.WriteLine("Zit: please make a pull request to update your local repository and then attempt the push request again"); 
            //extra - conflict resolution :
            //no conflict - 
            // Check if the file is already staged
            
        }
        //11
        public static void do_pull(string param) 
        { 
            Console.WriteLine("hi"); 
        }
        
        //-----------------------------------------------------------
        //12
        public static void do_getall() 
        {
            string who = getUser();
            //access the user db search the user and print all repo found 
        }

        //there are to be 3 parameters that may be or may not be 
        //13
        public static void do_share(string param) 
        {

            string repo = null;
            string user = null;
            string right = null;
            string[] flags = ParseParameters(param);

            //chack parameters
            if (flags[0] == "r")
            {
                repo = flags[1];
                if (flags[2] == "u")
                { 
                    user = flags[3];
                    if (flags[4] == "a" && checkRight(flags[5]))right= flags[5];
                    else if (flags[4] == "0") { Console.WriteLine("Zit: specify a permission"); return; }
                    else { Program.warning(3, "shares"); return; }
                }
                else if (flags[2] == "0") { Console.WriteLine("Zit: a user is reqiered for permission managment"); return; }
                else if (flags[2] != "0") { Program.warning(3, "shares"); return; }
            }
            //if there is no repo then its current 
            else if (flags[0] == "u") 
            { 
                repo = getRepo(); user = flags[1];
                if (flags[2] == "a" && checkRight(flags[3])) right = flags[3];
                else if (flags[2] == "0") { Console.WriteLine("Zit: specify a permission"); return; }
                else { Program.warning(3, "shares"); return; }
            }
            
            

        }
        //14
        public static void do_unshare(string args) 
        {
            permissions(args,0);
        }
        public static void permissions(string param,int type)
        {

        }
        static bool checkRight(string right)
        {
            if (right == "full" || right == "owner" || right == "write" || right == "read") return true;
            return false;
        }

        public static string[] ParseParameters(string param)
        {
            string[] flags = { "0", "0", "0", "0", "0", "0" };
            string[] args = param.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var p1 = args[0];
            var p2 = args.Length > 1 ? args[1] : null;
            var p3 = args.Length > 2 ? args[2] : null;
            Console.WriteLine(p1 + "|" + p2 + "|" + p3);

            if (p1 != null)
            {
                string[] s1 = p1.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                flags[0] = s1[0];
                flags[1] = s1.Length > 1 ? string.Join(" ", s1.Skip(1)) : "0";
                Console.WriteLine(flags[0] + "|" + flags[1]);
            }

            if (p2 != null)
            {
                string[] s2 = p2.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                flags[2] = s2[0];
                flags[3] = s2.Length > 1 ? string.Join(" ", s2.Skip(1)) : "0";
                Console.WriteLine(flags[2] + "|" + flags[3]);
            }

            if (p3 != null)
            {
                string[] s3 = p3.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                flags[4] = s3[0];
                flags[5] = s3.Length > 1 ? string.Join(" ", s3.Skip(1)) : "0";
                Console.WriteLine(flags[4] + "|" + flags[5]);

            }
            return flags;
        }

        //---------------------------------------------------------

        
        


        //returnes the current repository
        static string getRepo()
        {
            return "hi";
        }
        //checks if a repository entry is correct and exists
        static bool checkRepo(string repo) 
        { 
            if(repo=="repo") return true; 
            return false;
        }
        //returnes the current user
        static string getUser() 
        { 
            return "me"; 
        }
        //checks if a user entry is correct and exists
        static bool checkUser(string user)
        {
            if (user == "user") return true;
            return false;
        }



        ////////////////////////////////////////////////////////////////////////////
        ///failed attemps at prasing::
        ///the part in the commit      
        /*
        //get the message
        string[] p = param.Split(' ');
        var msg = p.Length > 1 ? string.Join("", param.Skip(2)) : null;
            //check
            if (p[0] != "-m") { Program.warning(3, "tag"); return; }
            if (msg == null) { Console.WriteLine("Zit: name is reqiered for a commit"); return; }
            else { msg = msg.TrimStart(' '); }
            Console.WriteLine("start comit param is |" + p[0] + "|" + msg + "|");
        */
        public static void ParseParams(string input, out string msg, out string name, out bool id)
        {
            // Initialize the output variables
            msg = null;
            name = null;
            id = false;

            // Split the input string into parts using whitespace as the separator
            string[] parts = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            //the first part of the parameters must be a flag 
            if (parts[0] == "-a" || parts[0] == "-m")
            {
                // Iterate over the parts of the input string
                for (int i = 0; i < parts.Length; i++)
                {
                    // Check if the current part is the -m option
                    if (parts[i] == "-m" && i < parts.Length - 1)
                    {
                        i++;
                        // Set the msg variable to the next part of the input string
                        while (parts[i] != "-a" && i < parts.Length)
                        {
                            msg += (parts[i]);
                            msg += " ";
                            if (i < parts.Length - 1) i++; else break;
                        }
                        i--;
                    }
                    // Check if the current part is the -a option
                    else if (parts[i] == "-a" && i < parts.Length - 1)
                    {
                        i++;
                        // Set the name variable to the next part of the input string
                        while (parts[i] != "-m" && i < parts.Length)
                        {
                            name += parts[i];
                            name += " ";
                            if (i < parts.Length - 1) i++; else break;
                        }
                        i--;
                    }
                }
                if (msg != null || name != null) id = true;
            }
            else Program.warning(3, "tag");

        }
        //a function that parses the 3 arguments for the share commands
        static string[] args(string input)
        {
            // Split input string by the -> delimiter
            string[] inputParts = input.Split(new string[] { "->" }, StringSplitOptions.None);

            // Extract up to three arguments: repo, user, right
            string repo = null;
            string user = null;
            string right = null;

            if (inputParts.Length >= 1)
            {
                repo = inputParts[0].Trim();
            }

            if (inputParts.Length >= 2)
            {
                user = inputParts[1].Trim();
            }

            if (inputParts.Length >= 3)
            {
                right = inputParts[2].Trim();
            }

            // Print each argument on a separate line
            Console.WriteLine("Repo: " + (repo ?? "none"));
            Console.WriteLine("User: " + (user ?? "none"));
            Console.WriteLine("Right: " + (right ?? "none"));
            string[] arg = { null, null, null };
            //check if the first argument is a repo
            if (checkRepo(inputParts[0]))
                //set it to be sent back
                arg[0] = inputParts[0];
            else
            {
                //set the repo as current 
                arg[0] = getRepo();
                //and check if the first argument is a user 
                if (checkUser(inputParts[0]))
                    //set it to be sent back
                    arg[1] = inputParts[0];
                else
                {
                    //if its not a user and not a repo that its a wrong argument
                    //client must specify repo - user - right but may ommit the repo 

                }
            }
            //else 
            //if it is put it in the 
            return inputParts;
        }
        //finds and returnes a repository entry
        static string findRepo(string param)
        {
            return "hi";
            /*
            var lnk = " ";
            if (param != null)
            {
                string[] p = param.Split(' ');
                lnk = p.Length > 1 ? string.Join(" ", param.Skip(1)) : " ";
                //check
                if (p[0] != "-r") { Program.warning(3, "clone"); return null; }
                if (lnk == null) { Console.WriteLine("Zit: please provide a repository with -r"); return; }
            }
            if (lnk != " ") { checkRepo(lnk); }*/
        }
    }
}