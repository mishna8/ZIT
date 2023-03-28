using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZIT
{
    public class Commit
    {
        public int Hash { get; set; }
        public string Author { get; set; }
        public string Message { get; set; }
        public int ParentHash { get; set; }
        public string[] Tags { get; set; }

        public Commit(int hash, string author, string message, int parentHash, string[] tags)
        {
            Hash = hash;
            Author = author;
            Message = message;
            ParentHash = parentHash;
            Tags = tags;
        }
    }

}
