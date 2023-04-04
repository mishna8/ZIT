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
using System.Data.SqlClient;

namespace ConsoleApp
{
    internal class commands
    {
        //all the sources of data:
        // Get the current working directory
        public static string workingDirectory = Directory.GetCurrentDirectory();
        
        // Initialize the staging area
        public static List<FileInfo> stagedFiles = new List<FileInfo>();
        // the commit history is a list in every repository
        // the repository reg
        public static List<Repository> RepoDB = new List<Repository>();

        // conflict resolution checking list 
        public static List<FileInfo> dif = new List<FileInfo>();

        //1
        public static void do_init()
        {
            // check if already exists, for zit write a worning
            /* Git reinitialize the repository. the existing Git history, 
             * including all previous commits and branches, will be erased. 
             */
            string zitDirectoryPath = Path.Combine(workingDirectory, ".zit");
            if (Directory.Exists(zitDirectoryPath))
            {
                Console.WriteLine("Zit: a zit repository is already initialized in this directory");
                return; 
            }
            //create the directory
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

            Console.WriteLine("Initialized empty Zit repository in " + zitDirectoryPath);

            // create the new repo 
            User creator = getUser();
            Repository newRepo = new Repository(zitDirectoryPath, creator);

            //add the repo to the repo list
            RepoDB.Add(newRepo);
        }

        // get the value of the commit written as current in the head file
        public static string GetHead()
        {
            string value = null;
            //get the .zit directory
            string zitDirectoryPath = Path.Combine(workingDirectory, ".zit");
            //get the head file from this directory
            string headFilePath = Path.Combine(zitDirectoryPath, "HEAD");
            try
            {
                using (StreamReader sr = new StreamReader(headFilePath))
                {
                    value = sr.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading file: " + e.Message);
            }

            return value;
        }
        // update the head file value to be the new commit
        public static void SetHead(string newReference)
        {
            //get the .zit directory
            string zitDirectoryPath = Path.Combine(workingDirectory, ".zit");
            //get the head file from this directory
            string headFilePath = Path.Combine(zitDirectoryPath, "HEAD");
            try
            {
                using (StreamWriter sw = new StreamWriter(headFilePath))
                {
                    sw.WriteLine(newReference);
                }
            }   
            catch (Exception e)
            {
                Console.WriteLine("Error updating HEAD reference: " + e.Message);
             }
        }


        //2
        public static void do_log()
        {
            //access the current repo
            Repository repo = getRepo();
            //access the commit table for this repo
            Console.WriteLine("Commit History :");
            foreach (Commit commitEntry in repo.CommitHistory)
            {
              Commit.WriteComit(commitEntry);        
            }
        }

        //---------------------------------------------------------------
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

        //4
        public static void do_add()
        {
            // Get all files in the working directory
            string[] files = Directory.GetFiles(workingDirectory);
            // Stage each file
            foreach (string file in files)
            {
                StageFile(file);
            }

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


            //get the commit needed to tag - the one checkedout
            Commit com = getCurCommit();
            //create the tag
            Tag t = new Tag(n, m);
            // there is only one commit for a tag check if this tag is available
            //get the repo 
            Repository repo = getRepo();
            if (repo.tagedCommits.ContainsKey(t))
            {
                Commit commit = repo.tagedCommits[t];
                Console.WriteLine("Zit: tag is already asigned to " + commit);
            }
            else
            {
                // there are many tags to a commit, so we add it to the list 
                com.Tags.Add(t);
                repo.tagedCommits.Add(t, com);
                Console.WriteLine("Zit: tag successfully asigned to " + com);
            }          
        }

        //6
        public static void do_commit(string param)
        {           
            string msg = null;
            string[] flags = Program.ParseParameters(param);

            //check the message
            if (flags[0] == "m") msg = flags[1];
            else if (flags[0] == "0") { Console.WriteLine("Zit: a message is reqiered for commit"); return; }
            else { Program.warning(3, "commit"); return; }
            if (flags[2] != "0" || flags[3] != "0" || flags[4] != "0" || flags[5] != "0") { Program.warning(3, "commit"); return; }
            //get the author
            User auther = getUser();

            // Create a new commit object
            Commit commit = new Commit(auther, msg);
            
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

            //set the new commit as the last and current
            SetHead(commit.Hash);
            Commit.WriteComit(commit);

            //add the commit to the repo history
            Repository repo = getRepo();
            repo.CommitHistory.Add(commit);

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
           //find the commit we revert to 
           if (param == null) { Console.WriteLine("Zit: a commit reference is requierd"); return; }
           Commit com = getCommitHash(param);
           //check if the commit is valid


           //create the new reverted commit
           //will only copy the data details, but with unique system details like time & hash
           Commit revCom = Commit.createCommit(com);

            //set the new commit as the last and current
            SetHead(revCom.Hash);
            Commit.WriteComit(revCom);

            //add the commit to the repo history
            Repository repo = getRepo();
            repo.CommitHistory.Add(revCom);
        }
        //8
        public static void do_checkout(string param)
        {
            //find the commit to checkout
            if (param == null) { Console.WriteLine("Zit: a commit reference is requierd"); return; }
            Commit com = getCommitHash(param);
            //check if the commit is valid
            //if (com.Author == "system_nullCommit") { Console.WriteLine("Zit: could not find your commit"); return; }
            
            //set the new commit as the last and current
            SetHead(com.Hash);
            Commit.WriteComit(com);

        }

        //-------------------------------------------------------------------
        //returns the current commit that is on head
        static Commit getCurCommit()
        {
            string h = GetHead();
            Commit com = getCommitHash(h);
            return com;
        }
        //returns the commit that is requested 
        static Commit getCommitHash(string param)
        {
            Console.WriteLine("|" + param + "|");
            Commit com;

            //get the reference
            string firstFourChars = param.Substring(0, 4);
            //if its a ref relatred to head
            if (firstFourChars == "head")
            {
                char Char = param[5]; 
                int n = Int32.Parse(Char.ToString()); 
                Console.WriteLine(n); 

            }
            //else its a tag or a hash, so access the history and search
            else if (true)
            {
                if (true)
                {
                    Console.WriteLine("this is your commit by tag");
                    /*foreach (tag  in db)
                    {
                        if (tag == param)
                        { 
                            Commit c = createCommit( get the commit of this tag ) ; 
                            return c;
                        }
                    }*/
                }
                //if we passed all the tags and none matches then check tags
                else
                {
                    Console.WriteLine("this is your commit by hash");
                    /*foreach (hash  in the hash db)
                   {
                       if (hash == param)
                       {
                           Commit c = createCommit( get the commit of this tag );
                           return c;
                       }
                   }*/
                }
            }
            //else if it doesnt exist then its a wrong parameter
            else { Program.warning(3, "commit"); }

                      
            return com;
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
            
            
        }
        //11
        public static void do_pull(string param) 
        { 
            Console.WriteLine("hi"); 
        }
        public static bool checkConflict()
        {
            bool conflict = false;
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
            return conflict;
        }
        
        //-----------------------------------------------------------
        //12
        public static void do_getall() 
        {
            User u = getUser();
            List<Repository> yourRepo = new List<Repository>();

            foreach (Repository r in RepoDB)
            {
                if (r.usersRight.ContainsKey(u))
                {
                    yourRepo.Add(r);
                    Console.WriteLine("ID: " + r.ID);
                    Console.WriteLine("path: " + r.path);
                    Console.WriteLine("     ");
                }
            }
        }

        /*public static List<string> FindRepositoriesByUser(string connectionString, string user)
        {
            var repositories = new List<string>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand($"SELECT Name FROM Repositories WHERE Users LIKE '%{user}%'", connection);

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    repositories.Add(reader["Name"].ToString());
                }

                reader.Close();
            }

            return repositories;
        }
        */

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
            bool exists = false;
            Repository repo = null;
            string who = null;
            string right = "none";
            string[] flags = Program.ParseParameters(param);

            //chack parameters
            //first we get a repo
            if (flags[0] == "r")
            {                
                //check the repository 
                foreach (Repository r in RepoDB)
                {
                    if (r.ID == flags[1])
                    {
                        exists = true;
                        repo = r; break;
                    }
                }
                if (exists == false) {Console.WriteLine("Zit: the repository doesnt exists"); return; }
                if (flags[2] == "u")
                {
                    who = flags[3];
                    if (flags[4] == "a" && checkRight(flags[5])) right = flags[5];
                    else if (flags[4] == "0") { Console.WriteLine("Zit: specify a permission"); return; }
                    else { Program.warning(3, "shares"); return; }
                }
                else if (flags[2] == "0") { Console.WriteLine("Zit: a user is reqiered for permission managment"); return; }
                else if (flags[2] != "0") { Program.warning(3, "shares"); return; }
            }
            //if there is no repo then its the current repo
            else if (flags[0] == "u")
            {
                repo = getRepo(); who = flags[1];
                if (flags[2] == "a" && checkRight(flags[3])) right = flags[3];
                else if (flags[2] == "0") { Console.WriteLine("Zit: specify a permission"); return; }
                else { Program.warning(3, "shares"); return; }
            }
            else { Program.warning(3, "shares"); return; }

            bool allowed = false;
            User user = getUser();
            string user_right = null;
            User account;
            string acc_right = "none";
            //find the user's permissions for the repo
            if (repo.usersRight.ContainsKey(user))
            {
                user_right = repo.usersRight[user];
            }
            //find the account's permissions 
            foreach (var ac in repo.usersRight)
            {
                if (ac.Key.Name == who)
                {
                    account = ac.Key;
                    acc_right = acc.Value; break;
                }
            }
            //the only cses where the permisssions are sufficient to change them
            if (acc_right!="owner" && (user_right == "owner" || user_right == "full")) allowed = true;
            if (acc_right=="owner" && user_right == "owner") allowed = true;          
            //also check - when unshare if the permission exists to remove
            if( (type == 0) && (acc_right != right) ) { Console.WriteLine("Zit: the account does not have this permission to revoke"); return; }
            if( allowed )
            {
              if( type == 1) //share
                    repo.usersRight[account]=right;
              else ( type == 0 ) //unshare
              {
                //      access the permissions and change it or remove entry
              }
            }
            else { Console.WriteLine("Zit: not sufficient permissions"); return;}
        }
        static bool checkRight(string right)
        {
            if (right == "full" || right == "owner" || right == "write" || right == "read") return true;
            return false;
        }
        
        //---------------------------------------------------------              

        //returnes the current repository
        static Repository getRepo()
        {
            User u = new User("a", "b", "c");
            Repository r = new Repository(workingDirectory, u);
            return r;
        }        


        //returnes the current user
        static User getUser() 
        {
            User u = new User("a", "b", "c");
            return u; 
        }
        //checks if a user entry is correct and exists
        static bool checkUser(string user)
        {
            if (user == "user") return true;
            return false;
        }

    }
}