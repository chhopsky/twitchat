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

            string loginstring = "PASS oauth:w317cdz8r1woof105yyalatwsjgr3i\r\nNICK fromtwitch\r\n";
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
            /*
            string capstring = "CAP REQ :twitch.tv/membership\r\n";
            Byte[] capabilities = System.Text.Encoding.ASCII.GetBytes(capstring);
            stream.Write(capabilities, 0, capabilities.Length);
            Console.WriteLine("Sent capabilities.\r\n");
            Console.WriteLine("CAP REQ :twitch.tv/membership\r\n");*/

            string joinstring = "JOIN " + channel + "\r\n";
            Byte[] join = System.Text.Encoding.ASCII.GetBytes(joinstring);
            stream.Write(join, 0, join.Length);
            Console.WriteLine("Sent channel join.\r\n");
            Console.WriteLine(joinstring);

            string announcestring = channel + "!" + channel + "@" + channel +".tmi.twitch.tv PRIVMSG " + channel + " TWITCH BRIDGE ENABLED\r\n";
            Byte[] announce = System.Text.Encoding.ASCII.GetBytes(announcestring);
            // stream.Write(announce, 0, announce.Length);
            Console.WriteLine("TWITCH CHAT BRIDGE HAS BEGUN.\r\n\r\nAnything that is typed in twitch chat will appear at your cursor.");
            Console.WriteLine("Open your chat client and start a message session with yourself.");    
            Console.WriteLine("Close this window to exit.");
            Console.WriteLine("\r\nBE CAREFUL.");

            while (true)
            {
                /*  string inputstring = Console.ReadLine();
                  Byte[] say = System.Text.Encoding.ASCII.GetBytes(":chhopsky!chhopsky@chhopsky.tmi.twitch.tv PRIVMSG #chhopsky :" + inputstring + "\r\n");
                  stream.Write(say, 0, say.Length);
                  Console.WriteLine("Sent: {0}", say);  */

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
                while (stream.DataAvailable);

                // Print out the received message to the console.
                Console.WriteLine(myCompleteMessage.ToString());
                switch (myCompleteMessage.ToString())
                {
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
                    default:
                        try { 
                        string messageParser = myCompleteMessage.ToString();
                        string[] message = messageParser.Split(':');
                        string[] preamble = message[1].Split(' ');
                        string tochat;
                        // Console.WriteLine("command = " + preamble[1] + "\r\n");

                        if (preamble[1] == "PRIVMSG")
                        {
                            string[] sendingUser = preamble[0].Split('!');
                            tochat = sendingUser[0] + ": " + message[2];
                            if (tochat.Contains("\n") == false)
                                {
                                    tochat = tochat + "\n";
                                }
                                //     Console.Write(tochat);
                                if (sendingUser[0] != "moobot" && sendingUser[0] != "whale_bot")
                                {
                                    SendKeys.SendWait(tochat.TrimEnd('\n'));
                                }
                        }
                        else if (preamble[1] == "JOIN")
                        {
                            string[] sendingUser = preamble[0].Split('!');
                            tochat = "JOINED: " + sendingUser[0];
                        //    Console.WriteLine(tochat);
                            SendKeys.SendWait(tochat.TrimEnd('\n'));
                        }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("OH SHIT SOMETHING WENT WRONG\r\n", e);
                        }
                        // Console.WriteLine("Raw output: " + message[0] + "::" + message[1] + "::" + message[2]);
                        // Console.WriteLine("You received the following message : " + myCompleteMessage);
                        break;
                }
            }

            // Close everything.
            stream.Close();
            client.Close();
            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}
