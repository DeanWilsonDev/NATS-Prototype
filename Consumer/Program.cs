using System;
using System.Text;
using NATS.Client;
using System.Threading.Tasks;


namespace Consumer;
class Program
{

  static bool _exit = false;
  static IConnection _connection;
  
  
  static void Main(string[] args)
  {
    using (_connection = ConnectToNats())
    {
      SubscribeRequestResponse();
    }
  }


  static IConnection ConnectToNats()
  {
    ConnectionFactory factory = new();

    Options options = ConnectionFactory.GetDefaultOptions();
    options.Url = "natsL//localhost:4222";

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
  }

  static void LogMessage(string message)
  {
    Console.WriteLine($"{DateTime.Now:HH:mm:ss.fffffff} - {message}");
  }

}

