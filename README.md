# twitchat is the basics of a Twitch bot in .NET Csharp

Why, you ask?  Well, so it can run natively on Windows and be tampered with and customized by users.  Twitch chat is NOT IRC - the interface that TMI provides is "irc-like" and does not strictly conform to the RFCs.  This is fine and a lot of clients will work around it but none of the existing .NET C# IRC libraries will.

I wrote this in an afternoon in raw TCP sockets because I needed to, and it's my first time using it or Visual Studio so although the code is not as pretty or elegant as it should be, it does what I couldn't find a library to do - give me the basic tools to implement my own functionality.

I hope you find it useful, and while I don't suggest you create anything downstream from this as it will probably change in dependency-breaking ways, please credit me if you do use it.

Kappa and chhDogross,
chhopsky
