using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Net.Sockets;

namespace chatrig
{
    class chatrig
    {
        private static byte[] data;

        static void Main(string[] args)
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Int32 port = 6667;
            TcpClient client = new TcpClient("irc.twitch.tv", port);
            string channel = "#awesomejoey";

            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();

            NetworkStream stream = client.GetStream();

            // Send the message to the connected TcpServer. 

            string loginstring = "PASS oauth:YOUROAUTHGOESHERE\r\nNICK YOURNICKNAMEHERE\r\n";
            Byte[] login = System.Text.Encoding.ASCII.GetBytes(loginstring);
            stream.Write(login, 0, login.Length);
            Console.WriteLine("Sent login.\r\n");
            Console.WriteLine(loginstring);

            // Receive the TcpServer.response.
            // Buffer to store the response bytes.
            data = new Byte[512];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("Received WELCOME: \r\n\r\n{0}", responseData);

            // send message to join channel

            string joinstring = "JOIN " + channel + "\r\n";
            Byte[] join = System.Text.Encoding.ASCII.GetBytes(joinstring);
            stream.Write(join, 0, join.Length);
            Console.WriteLine("Sent channel join.\r\n");
            Console.WriteLine(joinstring);
            
            // PMs the channel to announce that it's joined and listening
            // These three lines are the example for how to send something to the channel

            string announcestring = channel + "!" + channel + "@" + channel +".tmi.twitch.tv PRIVMSG " + channel + " BOT ENABLED\r\n";
            Byte[] announce = System.Text.Encoding.ASCII.GetBytes(announcestring);
            stream.Write(announce, 0, announce.Length);
            
            // Lets you know its working
            
            Console.WriteLine("TWITCH CHAT HAS BEGUN.\r\n\r\nr.");
            Console.WriteLine("\r\nBE CAREFUL.");

            while (true)
            {
                
                // build a buffer to read the incoming TCP stream to, convert to a string

                byte[] myReadBuffer = new byte[1024];
                StringBuilder myCompleteMessage = new StringBuilder();
                int numberOfBytesRead = 0;

                // Incoming message may be larger than the buffer size.
                do
                {
                    try { numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length); }
                    catch (Exception e)
                    {
                        Console.WriteLine("OH SHIT SOMETHING WENT WRONG\r\n", e);
                    }

                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                }
                
                // when we've received data, do Things
                
                while (stream.DataAvailable);

                // Print out the received message to the console.
                Console.WriteLine(myCompleteMessage.ToString());
                switch (myCompleteMessage.ToString())
                {
                    // Every 5 minutes the Twitch server will send a PING, this is to respond with a PONG to keepalive
                    
                    case "PING :tmi.twitch.tv\r\n":
                        try { 
                        Byte[] say = System.Text.Encoding.ASCII.GetBytes("PONG :tmi.twitch.tv\r\n");
                        stream.Write(say, 0, say.Length);
                        Console.WriteLine("Ping? Pong!");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("OH SHIT SOMETHING WENT WRONG\r\n", e);
                        }
                        break;
                        
                    // If it's not a ping, it's probably something we care about.  Try to parse it for a message.
                    default:
                        try { 
                        string messageParser = myCompleteMessage.ToString();
                        string[] message = messageParser.Split(':');
                        string[] preamble = message[1].Split(' ');
                        string tochat;

                        // This means it's a message to the channel.  Yes, PRIVMSG is IRC for messaging a channel too
                        if (preamble[1] == "PRIVMSG")
                        {
                            string[] sendingUser = preamble[0].Split('!');
                            tochat = sendingUser[0] + ": " + message[2];
                            
                            // sometimes the carriage returns get lost (??)
                            if (tochat.Contains("\n") == false)
                                {
                                    tochat = tochat + "\n";
                                }
                                
                                // Ignore some well known bots
                                if (sendingUser[0] != "moobot" && sendingUser[0] != "whale_bot")
                                {
                                    SendKeys.SendWait(tochat.TrimEnd('\n'));
                                }
                        }
                        // A user joined.
                        else if (preamble[1] == "JOIN")
                        {
                            string[] sendingUser = preamble[0].Split('!');
                            tochat = "JOINED: " + sendingUser[0];
                        //    Console.WriteLine(tochat);
                            SendKeys.SendWait(tochat.TrimEnd('\n'));
                        }
                        }
                        // This is a disgusting catch for something going wrong that keeps it all running.  I'm sorry.
                        catch (Exception e)
                        {
                            Console.WriteLine("OH SHIT SOMETHING WENT WRONG\r\n", e);
                        }
                        
                        // Uncomment the following for raw message output for debugging
                        //
                        // Console.WriteLine("Raw output: " + message[0] + "::" + message[1] + "::" + message[2]);
                        // Console.WriteLine("You received the following message : " + myCompleteMessage);
                        break;
                }
            }

            // Close everything.  Should never happen because you gotta close the window.
            stream.Close();
            client.Close();
            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}
