using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Collections;
using System.Buffers;

namespace ConsoleApp
{
    public class Repository
    {
        //the commit list
        public List<Commit> CommitHistory { get; } = new List<Commit>();
        // the list of tags and thier commits in the repository
        public Dictionary<Tag, Commit> tagedCommits { get; } = new Dictionary<Tag, Commit>();
        // the list of users and thier permissions to the repository
        public Dictionary<User, String> usersRight { get; } = new Dictionary<User, string>();

        public string path { get; set; }
        public string ID { get; set; }
        public DateTime CreatedAT { get; set; }


        public Repository(string path, User creator )
        {
    
            this.CreatedAT = DateTime.Now;
            this.ID = "";

            this.path = path;

            this.usersRight = new Dictionary<User, String>();
            usersRight.Add(creator, "owner");

            this.tagedCommits = new Dictionary<Tag, Commit>();
            this.CommitHistory = new List<Commit>();

        }
        

        /*

        private readonly string path;
        private readonly CredentialsHandler credentialsHandler;

        public Repository(string path, string username, string password)
        {
            this.path = path;

            // Set up the credentials handler for authentication
            credentialsHandler = (_url, _user, _cred) =>
                new UsernamePasswordCredentials()
                {
                    Username = username,
                    Password = password
                };
        }

        public void PushChanges()
        {
            var repo = new Repository(path)
            
                // Stage changes
                Commands.Stage(repo, "*");

                // Create new commit
                var author = new Signature("Your Name", "your.email@example.com", DateTimeOffset.Now);
                var committer = author;
                repo.Commit("Commit message", author, committer);

                // Push changes to remote repository
                var remote = repo.Network.Remotes["origin"];
                var options = new PushOptions();
                options.CredentialsProvider = credentialsHandler;
                repo.Network.Push(remote, @"refs/heads/main", options);
            
        }*/
    }

    public class User
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public User(string name, string email, string password)
        {
            this.Name = name;
            this.Email = email;
            this.Password = password;
        }
    }

    // for evry tag there is only one commit and for every commit there can be a number of tags 
    public class Tag
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public Tag(string name, string msg) 
        {
            this.Name = name;
            this.Message = msg;
            this.CreatedAt = DateTime.Now;
        }
    }

    public class Commit
    {
        //the files snapshotes
        public Dictionary<string, string> Snapshot { get; } = new Dictionary<string, string>();
        public List<Tag> Tags { get; set; } =  new List<Tag>();
        public string Message { get; set; }
        public User Author { get; set; }
        public DateTime CreatedAt { get; set; }

        public string Hash { get; private set; }
        public string ParentHash { get; private set; }


        public Commit(User author, string message)
        {
            this.Snapshot = new Dictionary<string, string>();
            this.Tags = new List<Tag>();

            this.Message = message;
            this.Author = author;            
            
            this.CreatedAt = DateTime.Now;
            this.Hash = "";
            //Hash = CalculateHash();
            this.ParentHash = "";
        }


        /*
        private string CalculateHash()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("tree " + GetTreeHash());
            sb.AppendLine("parent ");
            sb.AppendLine("author " + Environment.UserName + " <" + Environment.UserDomainName + "@" + Environment.MachineName + "> " + DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            sb.AppendLine("committer " + Environment.UserName + " <" + Environment.UserDomainName + "@" + Environment.MachineName + "> " + DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            sb.AppendLine();
            sb.Append(Message);

            return CalculateHash(sb.ToString());
        }
        private string GetTreeHash()
        {
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> file in Files)
            {
                sb.AppendLine("100644 blob " + file.Value + " " + file.Key);
            }
            return CalculateHash(sb.ToString());
        }

        private string CalculateHash(string content)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = sha1.ComputeHash(bytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }


        */

        //creates a commit copy wit the same data but new hash and current date 
        public static Commit createCommit(Commit commit)
        {
            Commit nCommit = new Commit(commit.Author, commit.Message);
            nCommit.Tags = commit.Tags;

            // Create a new dictionary and copy key-value pairs from the original dictionary
            Dictionary<string, string> newSnapshot = new Dictionary<string, string>(commit.Snapshot);
            nCommit.GetType().GetProperty("Snapshot").SetValue(nCommit, newSnapshot);

            return nCommit;
        }

        //make the commit as an entry to be added to the commit history that can be viewd by log
        public static void WriteComit(Commit commit)
        {
            //the hash
            Console.WriteLine("commit "+commit.Hash);
            Console.WriteLine("Auther: "+commit.Author);
            Console.WriteLine("Date: "+commit.CreatedAt);
            Console.WriteLine("     "+commit.Message);
            Console.WriteLine("     ");


        }

    }

}







    

    