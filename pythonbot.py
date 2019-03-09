import sys
import irc.bot
import requests
import json
import pprint

# This python chatbot does a bunch of things, but is currently configured to take messages from twitch chat
# and punt them into a discord channel for some reason. I made this initially to help chat get through
# the great firewall of china for LPL, but now serves as a template to make your own stuff.
# this is all based on irc.bot.SingleServerIRCBot, the documentation for which is available here: 
# https://pypi.org/project/irc/
# https://python-irc.readthedocs.io/en/latest/irc.html
# full event list here, anything that starts with "on_" https://github.com/jaraco/irc/blob/master/irc/bot.py

class TwitchBot(irc.bot.SingleServerIRCBot):
    def __init__(self, username, client_id, token, channel):
        self.client_id = client_id
        self.token = token
        self.channel = '#' + channel

        # Get the channel id, we will need this for v5 API calls
        url = 'https://api.twitch.tv/kraken/users?login=' + channel
        headers = {'Client-ID': client_id, 'Accept': 'application/vnd.twitchtv.v5+json'}
        r = requests.get(url, headers=headers).json()
        self.channel_id = r['users'][0]['_id']

        # Create IRC bot connection
        server = 'irc.chat.twitch.tv'
        port = 6667
        print('Connecting to ' + server + ' on port ' + str(port) + '...')
        irc.bot.SingleServerIRCBot.__init__(self, [(server, port, 'oauth:'+token)], username, username)

# things that happen when you connect to chat

    def on_welcome(self, c, e):
        print('Joining ' + self.channel)

        # You must request specific capabilities before you can use them
        c.cap('REQ', ':twitch.tv/membership')
        c.cap('REQ', ':twitch.tv/tags')
        c.cap('REQ', ':twitch.tv/commands')
        c.join(self.channel)

# this function will run any time a messages is posted in twitch chat. if you want to do anything like increment counters
# or other funky stuff that involves reading every message, this is where to do it. there are triggers for basically every
# irc action in the definitions, which you can look up 

    def on_pubmsg(self, c, e):
        chatmessage = e.source.split("!")
        chat = "<" + chatmessage[0] + "> " + e.arguments[0]
        url = "https://your_discord_webhook_url_here"
        headers = {'Content-Type': 'application/json'}
        message = {'content': chat }
        r = requests.post(url, headers=headers, data=json.dumps(message))
        # If a chat message starts with an exclamation point, try to run it as a command
        if e.arguments[0][:1] == '!':
            cmd = e.arguments[0].split(' ')[0][1:]
            print('Received command: ' + cmd)
            self.do_command(e, cmd)
        return

# this defines commands, anything prefixed with an exclamation point will be interpreted as a command. checking game
# currently being played etc

    def do_command(self, e, cmd):
        c = self.connection

        # Poll the API to get current game.
        if cmd == "game":
            url = 'https://api.twitch.tv/kraken/channels/' + self.channel_id
            headers = {'Client-ID': self.client_id, 'Accept': 'application/vnd.twitchtv.v5+json'}
            r = requests.get(url, headers=headers).json()
            c.privmsg(self.channel, r['display_name'] + ' is currently playing ' + r['game'])

        # Poll the API the get the current status of the stream
        elif cmd == "title":
            url = 'https://api.twitch.tv/kraken/channels/' + self.channel_id
            headers = {'Client-ID': self.client_id, 'Accept': 'application/vnd.twitchtv.v5+json'}
            r = requests.get(url, headers=headers).json()
            c.privmsg(self.channel, r['display_name'] + ' channel title is currently ' + r['status'])

        # Provide basic information to viewers for specific commands
        elif cmd == "raffle":
            message = "This is an example bot, replace this text with your raffle text."
            c.privmsg(self.channel, message)
        elif cmd == "schedule":
            message = "This is an example bot, replace this text with your schedule text."
            c.privmsg(self.channel, message)

        # The command was not recognized
        else:
            c.privmsg(self.channel, "Did not understand command: " + cmd)

def main():
#    if len(sys.argv) != 5:
#        print("Usage: twitchbot <username> <client id> <token> <channel>")
#        sys.exit(1)

    username  = "definitelynotchhopsky"
    client_id = "your_client_id_here"
    token     = "your_auth_token_here"
    channel   = "rustythecaster"

    bot = TwitchBot(username, client_id, token, channel)
    bot.start()

if __name__ == "__main__":
    main()
