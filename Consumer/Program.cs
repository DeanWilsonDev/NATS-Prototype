using System;
using System.Text;
using NATS.Client;
using System.Threading.Tasks;


namespace Consumer;

class Program
{
  static bool _exit = false;
  static IConnection _connection;
  const int FiveSeconds = 5000;

  static void Main(string[] args)
  {
    using (_connection = ConnectToNats())
    {
      SubscribeRequestResponse();
      SubscribeClear();
      
      Console.Clear();
      Console.WriteLine($"Connected to {_connection.ConnectedUrl}.");
      Console.WriteLine("Consumer Started");
      Console.ReadKey(true);
      _exit = true;

      _connection.Drain(FiveSeconds);
    }
  }


  static IConnection ConnectToNats()
  {
    ConnectionFactory factory = new();

    Options options = ConnectionFactory.GetDefaultOptions();
    options.Url = "nats://localhost:4222";

    return factory.CreateConnection(options);
  }

  static void SubscribeRequestResponse()
  {
    EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
    {
      string data = Encoding.UTF8.GetString(args.Message.Data);
      LogMessage(data);

      string? replySubject = args.Message.Reply;

      if (replySubject == null) return;

      byte[] responseData = Encoding.UTF8.GetBytes($"ACK for {data}");
      _connection.Publish(replySubject, responseData);
    };
    _connection.SubscribeAsync("nats.demo.requestresponse",
      "request-response-queue", handler);
  }

  static void LogMessage(string message)
  {
    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fffffff} - {message}");
  }

  static void SubscribeClear()
  {
    EventHandler<MsgHandlerEventArgs> handler = (sender, args) =>
    {
      Console.Clear();
    };

    _connection.SubscribeAsync("nats.demo.clear", handler);
  }
}