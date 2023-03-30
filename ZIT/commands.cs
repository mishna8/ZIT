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
            string[] flags = Program.ParseParameters(param);
            
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
            string[] flags = Program.ParseParameters(param);

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
        public static void do_share(string args) 
        {
            permissions(args, 1);

        }
        //14
        public static void do_unshare(string args) 
        {
            permissions(args, 0);
        }
        public static void permissions(string param,int type)
        {
            bool allowed = false;
            string user = getUser();
            string repo = null;
            string account = null;
            string right = null;
            string[] flags = Program.ParseParameters(param);

            //chack parameters
            if (flags[0] == "r")
            {
                repo = flags[1];
                if (flags[2] == "u")
                {
                    account = flags[3];
                    if (flags[4] == "a" && checkRight(flags[5])) right = flags[5];
                    else if (flags[4] == "0") { Console.WriteLine("Zit: specify a permission"); return; }
                    else { Program.warning(3, "shares"); return; }
                }
                else if (flags[2] == "0") { Console.WriteLine("Zit: a user is reqiered for permission managment"); return; }
                else if (flags[2] != "0") { Program.warning(3, "shares"); return; }
            }
            //if there is no repo then its current 
            else if (flags[0] == "u")
            {
                repo = getRepo(); account = flags[1];
                if (flags[2] == "a" && checkRight(flags[3])) right = flags[3];
                else if (flags[2] == "0") { Console.WriteLine("Zit: specify a permission"); return; }
                else { Program.warning(3, "shares"); return; }
            }

            //find the user in the user table and find his permissions for the repo 
            //var user_right
            //find the account in the user table and find his permisions for the repo
            //if not exists set to none
            //var acc_right
            // if (acc_right!=owner && (user_right == owner)||(user_right == full)) allowed = true;
            // if (acc_right==owner && user_right == owner) allowed = true;
            //every other case the user doent have correct permissions to continue
            //
            //also check - when unshare if the permission exists to remove
            //if( type = 0 && acc_right != right ) { Console.WriteLine("Zit: the account does not have this permission to revoke"); return; }


            //if( allowed )
            //{
            //  if( type = 1) //share
            //      access the permissions and change it or if not exists create a new entry
            //  else( type = 0 ) //unshare
            //  {
            //      access the permissions and change it or remove entry
            //  }
            //}
            //else Console.WriteLine("Zit: not sufficient  permissions"); return;
        }
        static bool checkRight(string right)
        {
            if (right == "full" || right == "owner" || right == "write" || right == "read") return true;
            return false;
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

    }
}