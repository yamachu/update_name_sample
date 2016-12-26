using System;
using System.Linq;
using System.Reactive.Linq;
using CoreTweet;
using CoreTweet.Streaming;

namespace ConsoleApplication
{
    public class TwitterBot
    {
        private Tokens MyToken;

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
            .Subscribe(x => {
                System.Console.WriteLine($"{x.Status.User.ScreenName}: {x.Status.Text}");
            });
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
