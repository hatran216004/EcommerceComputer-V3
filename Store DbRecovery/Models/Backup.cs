using System;

namespace Store_DbRecovery.Models
{
    public struct Backup
    {
        public DateTime CreatedAt { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Desc { get; set; }
        public string Type { get; set; }
    }
}