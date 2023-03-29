using System;
using System.Collections.Generic;
//using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    internal class commands
    {
        // Get the current working directory
        public static string workingDirectory = Directory.GetCurrentDirectory();
        // Initialize the staging area
        public static List<FileInfo> stagedFiles = new List<FileInfo>();

        //1
        public static void do_init()
        {
            //??????what if already exists????
            Console.WriteLine("started zit init");
            Console.WriteLine("current wkd is" + workingDirectory);

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
            Console.WriteLine("started zit status");

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
                if (!(stagedFiles.Exists(file => file.Name == fileName)))
                    Console.WriteLine(fileName);
            }

            Console.WriteLine("end of zit status");
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
                Console.WriteLine("staging now " + file);
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
            if (stagedFiles.Exists(file => file.Name == fileName))
            {
                Console.WriteLine("is already staged. " + fileName);
            }
            else
            {
                // Get a reference to the file and add it to the staging area
                FileInfo file = new FileInfo(fileName);
                stagedFiles.Add(file);
                Console.WriteLine("succesfully staged. " + fileName);

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
                    Console.WriteLine("{0} has been unstaged.", fileName);
                    return;
                }
            }
            //if we passed all the files it it didnt return it wasnt there
            Console.WriteLine("{0} has not been unstaged.", fileName);

        }
        public static void EmptyStagedFiles()
        {
            stagedFiles.Clear();
        }


        //####################################################################
        //5
        public static void do_tag(string param)
        {
            //get the message
            string[] p = param.Split(' ');
            var msg = p.Length > 1 ? string.Join(" ", param.Skip(1)) : null;
            //check
            if (p[0] != "-m") { Program.warning(3, "tag"); return; }
            if (msg == null) { Console.WriteLine("Zit: name is reqiered for tag"); return; }

            //do the tag 
            //get the commit needed to tag - the one checkedout
            Commit com = getCurCommit();
            //create the tag
            Tag t = new Tag("tag", msg);
            com.Tags = t;

        }

        //6
        public static void do_commit(string param)
        {
            //get the message
            string[] p = param.Split(' ');
            var msg = p.Length > 1 ? string.Join(" ", param.Skip(1)) : null;
            Console.WriteLine("start comit param is " + p[0]+ " msg is "+msg);
            //check
            if (p[0] != "-m") { Program.warning(3, "commmit"); return; }
            if (msg == null) { Console.WriteLine("Zit: message is reqiered for commit"); return; }
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
                Console.WriteLine("commiting now " + file);
                string filePath = Path.Combine(workingDirectory, file.Name);
               //create the contant of the file in a snapshot 
                byte[] fileContent = File.ReadAllBytes(filePath);
                string fileHash = GetHashString(fileContent);
                commit.Snapshot.Add(file.Name, fileHash);
            }
            //add the commit to the repo history


            //clear the staging area to the next modification
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
            
            Console.WriteLine("hi"); 
        }
        //14
        public static void do_unshare(string args) 
        { 
            Console.WriteLine("hi"); 
        }

        //---------------------------------------------------------

        //returnes the current repository
        static string getRepo()
        {
            return "hi";
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
        //checks if an entry is correct and exists
        static bool checkRepo(string repo) 
        { 
            if(repo!=null) return true; 
            return false;
        }
        //returnes the current user
        static string getUser() 
        { 
            return "me"; 
        }
        //finds and returnes a user entry
        static string findUser(string param)
        {
            return "hi";
        }
        //checks if an entry is correct and exists
        static bool checkUser(string repo)
        {
            if (repo != null) return true;
            return false;
        }

    }
}
