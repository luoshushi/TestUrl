using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestUrl
{
    public class Logs
    {
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public DateTime CreatedTime { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
        public int? UserID { get; set; }

    }
}