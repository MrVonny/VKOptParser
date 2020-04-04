using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class VKMessage
    {
        public VKMessage(string author, string vkID, string message, DateTime dateTime)
        {
            VKID = vkID;
            Date = dateTime;
            Author = author;
            Message = message;
        }

        public VKMessage(string author, string vkID, string message, DateTime dateTime, List<VKMessage> forwarded, List<string> attachments)
        {
            VKID = vkID;
            Date = dateTime;
            Author = author;
            Message = message;
            Forwarded = forwarded;
            Attachments = attachments;
        }

        public DateTime Date { get; set; }
        public string Author { get; set; }
        public string VKID { get; set; }
        public string Message { get; set; }
        public List<VKMessage> Forwarded { get; set; }
        public List<string> Attachments { get; set; }
    }
}
