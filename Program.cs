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
            if (!status.InReplyToUserId.HasValue) {
                return;
            }

            var update_name_pattern = $@"^@{MyScreenName}\s+update_name\s+(?<next_name>.+)$";
            var re = new Regex(update_name_pattern, RegexOptions.IgnoreCase);

            var match = re.Match(status.Text);
            if (!match.Success) return;

            var next_name = match.Groups["next_name"].Value;
            System.Console.WriteLine($"Try run update_name! => {status.User.ScreenName}: {status.Text}");

            var si = new System.Globalization.StringInfo(next_name);
            
            if (si.LengthInTextElements > 20)
            {
                System.Console.WriteLine("名前が20文字を超えています");
                return;
            }

            try
            {
                await MyToken.Account.UpdateProfileAsync(new {name = next_name});
            }
            catch(CoreTweet.TwitterException ex)
            {
                System.Console.WriteLine(ex);
                return;
            }
            var escaped_name = ReplyEscapedString(next_name);
            var escaped_my_name = ReplyEscapedString(status.User.Name);

            try
            {
                await MyToken.Statuses.UpdateAsync(
                    new {
                        status = $"{escaped_my_name} さん(@{status.User.ScreenName})により {escaped_name} に改名しました！",
                        in_reply_to_status_id = status.Id
                    });
            }
            catch(CoreTweet.TwitterException ex)
            {
                System.Console.WriteLine(ex);
                return;
            }
        }

        private string ReplyEscapedString(string str) {
            return str.Replace("@", "@" + char.ConvertFromUtf32(0x200B))
                        .Replace("#", "#" + char.ConvertFromUtf32(0x200B))
                        .Replace("$", "$" + char.ConvertFromUtf32(0x200B));
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
