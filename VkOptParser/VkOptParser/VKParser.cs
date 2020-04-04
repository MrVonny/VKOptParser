using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    static class VKParser
    {
    
        public static List<VKMessage> GetVKMessages(StreamReader streamReader)
        {
            List<VKMessage> vKMessages = new List<VKMessage>();

            DateTime dateTime=new DateTime();
            List<VKMessage> forwardedMessages=new List<VKMessage>();
            List<string> attachments = new List<string>();
            string author="";
            string vkID="";
            string message = "";
            string line="";
            line = streamReader.ReadLine();
            while (!streamReader.EndOfStream)
            {
                message = "";
                var splitLine = line.Split(' ', '(', ')');

                author = splitLine[0] + " " + splitLine[1];

                vkID = splitLine[2];
                vkID = vkID.Remove(vkID.IndexOf('['), 1);
                vkID = vkID.Remove(vkID.IndexOf(']'), 1);

                var time = splitLine[4].Split(':');
                var date = splitLine[6].Split('/');
                dateTime = new DateTime(Convert.ToInt32(date[2]),
                    Convert.ToInt32(date[1]),
                    Convert.ToInt32(date[0]),
                    Convert.ToInt32(time[0]),
                    Convert.ToInt32(time[1]),
                    Convert.ToInt32(time[2])
                    );

                while(true)
                {


                    if ((line = streamReader.ReadLine()) == "" && !streamReader.EndOfStream)
                    {
                        continue;
                    }
                    else if (!streamReader.EndOfStream && line[0] == '\t')
                    {
                        List<string> forwardedStr = new List<string>();
                        line=line.Remove(0, 1);
                        forwardedStr.Add(line);
                        while (((line=streamReader.ReadLine())!="") && line[0] == '\t')
                        {
                            
                            line=line.Remove(0, 1);
                            forwardedStr.Add(line);
                        }
                        forwardedMessages = GetVKMessages(forwardedStr.ToArray());
                    } 
                    else if (line == "Attachments:[")
                    {
                        attachments = new List<string>();
                        while (true)
                        {
                            if ((line=streamReader.ReadLine()) == "]") break;
                            else {
                                attachments.Add(line);
                            }
                        }

                    }
                    else if (IsHeader(line))
                    {
                        break;
                    }
                    else
                    {
                        message += line;
                    }
                    if (streamReader.EndOfStream) break;
                }
                
                vKMessages.Add(new VKMessage(author, vkID, message, dateTime, forwardedMessages, attachments));
                //Console.WriteLine(vKMessages.Last().Message);
                
            }
            vKMessages.Add(new VKMessage(author, vkID, message, dateTime, forwardedMessages, attachments));
            // Console.WriteLine(vKMessages.Last().Message);
            return vKMessages;
        }
        public static List<VKMessage> GetVKMessages(string[] data)
        {
            List<VKMessage> vKMessages = new List<VKMessage>();

            DateTime dateTime = new DateTime();
            List<VKMessage> forwardedMessages = new List<VKMessage>();
            List<string> forwardedStr = new List<string>();
            List<string> attachments = new List<string>();
            string author = "";
            string vkID = "";
            string message = "";
            string line = "";

            ReadState state = ReadState.Header;
            
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == "") continue;
                line = data[i];
                if(state==ReadState.Attachments && line!="]")
                {
                    attachments.Add(line);
                    continue;
                } else if(line=="]")
                {
                    state = ReadState.NotStated;
                } else if(state==ReadState.Forwarded && line[0]!='\t')
                {
                    forwardedMessages = GetVKMessages(forwardedStr.ToArray());
                }
                if (IsHeader(line))
                {
                    state = ReadState.Header;
                }
                else if (line == "Attachments:[")
                {
                    state = ReadState.Attachments;
                }
                else if (line[0] == '\t')
                {
                    state = ReadState.Forwarded;
                }
                else
                {
                    state = ReadState.Message;
                }

                switch(state)
                {
                    case ReadState.Header:
                        if(vKMessages.Count>0) vKMessages.Add(new VKMessage(author, vkID, message, dateTime, forwardedMessages, attachments));
                        var splitLine = line.Split(' ', '(', ')');

                        author = splitLine[0] + " " + splitLine[1];

                        vkID = splitLine[2];
                        vkID = vkID.Remove(vkID.IndexOf('['), 1);
                        vkID = vkID.Remove(vkID.IndexOf(']'), 1);

                        var time = splitLine[4].Split(':');
                        var date = splitLine[6].Split('/');
                        dateTime = new DateTime(Convert.ToInt32(date[2]),
                            Convert.ToInt32(date[1]),
                            Convert.ToInt32(date[0]),
                            Convert.ToInt32(time[0]),
                            Convert.ToInt32(time[1]),
                            Convert.ToInt32(time[2])
                            );
                        break;
                    case ReadState.Message:
                        message += line;
                        break;
                    case ReadState.Forwarded:
                        forwardedStr.Add(line.Remove(0, 1));
                        break;
                    case ReadState.Attachments:

                        break;
                }
            }
            vKMessages.Add(new VKMessage(author, vkID, message, dateTime, forwardedMessages, attachments));
            return vKMessages;
        }
   
        
        private static bool IsHeader(string str) //Some Name [someID] (12:55:13  01/05/2019): - True
        {
            try
            {
                var splitStr = str.Split(' ','(',')');
                return IsID(splitStr[2]) && IsDate(splitStr[4]+" "+splitStr[6]);
            }
            catch(Exception ex)
            {
                return false;
            }

        }
        private static bool IsID(string str) // [someID] - True
        {
            try
            {
                return (str[0] == '[') && (str[str.Length - 1] == ']');
            }
            catch
            {
                return false;
            }
            
        }
        private static bool IsDate(string str) //22:13:33  28/09/2018 - True
        {
            try
            {
                var splitLine = str.Split(' ');
                var time = splitLine[0].Split(':');
                var date = splitLine[1].Split('/');           
                DateTime dateTime = new DateTime(Convert.ToInt32(date[2]),
                        Convert.ToInt32(date[1]),
                        Convert.ToInt32(date[0]),
                        Convert.ToInt32(time[0]),
                        Convert.ToInt32(time[1]),
                        Convert.ToInt32(time[2])
                        );
                return true;
            }
            catch
            {
                return false;
            }
        }
        enum ReadState
        {
            Header = 0,
            Message,
            Attachments,
            Forwarded,
            NotStated
        }

    }

    
}
