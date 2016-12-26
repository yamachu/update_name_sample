using System;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using CoreTweet;
using CoreTweet.Streaming;

namespace ConsoleApplication
{
    public class TwitterBot
    {
        private Tokens MyToken;
        private const string MyScreenName = "Your ScreenName => ex. y_chu5";

        public TwitterBot()
        {
            var CONSUMER_KEY = "Write Your Consumer Key";
            var CONSUMER_SECRET = "Write Your Consumer Secret";
            var OAUTH_ACCESS_TOKEN = "Write Your Access Token";
            var OAUTH_ACCESS_SECRET = "Write Your Access Secret";

            MyToken = Tokens.Create(CONSUMER_KEY, CONSUMER_SECRET,
                                    OAUTH_ACCESS_TOKEN, OAUTH_ACCESS_SECRET);
        }

        public void Run()
        {
            var stream = MyToken.Streaming.UserAsObservable();

            stream.OfType<StatusMessage>()
            .Where(x => x.Type == MessageType.Create)
            .Select(x => x.Status)
            .Subscribe(ParseTweet);
        }

        private async void ParseTweet(Status status)
        {
            var update_name_pattern = $@"^@{MyScreenName}\s+update_name\s+(?<next_name>.+)$";
            var re = new Regex(update_name_pattern, RegexOptions.IgnoreCase);

            var match = re.Match(status.Text);
            if (!match.Success) return;

            var next_name = match.Groups["next_name"].Value;
            System.Console.WriteLine($"Try run update_name! => {status.User.ScreenName}: {status.Text}");

            try
            {
                await MyToken.Account.UpdateProfileAsync(new {name = next_name});
            }
            catch(CoreTweet.TwitterException ex)
            {
                System.Console.WriteLine(ex);
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Start my Twitter Bot!");
            var bot = new TwitterBot();
            bot.Run();

            Console.ReadKey(true);
        }
    }
}
