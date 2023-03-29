using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
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
        public string Message { get; set; }
        public string Author { get; set; }
        public Tag Tags { get; set; }
        public string Hash { get; private set; }
        public string ParentHash { get; private set; }


        public Commit(string author, string message, Tag tags, Dictionary<string, string> Snapshot)
        {

            this.Author = author;
            this.Message = message;
            this.Tags = tags;

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

        public static Commit createCommit(Commit commit)
        {
            Commit nCommit = new Commit(commit.Author, commit.Message, commit.Tags, commit.Snapshot);
            return nCommit;
        }
        /*
        //make the commit as an entry to be added to the commit history that can be viewd by log
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Author: {Author}");
            sb.AppendLine($"Message: {Message}");
            public CommitObject(Dictionary<string, string> files, string message)
            {
                Files = files;
                Message = message;
            }
        }*/
    }

}







    

    