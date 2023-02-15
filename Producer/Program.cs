using System.Text;
using NATS.Client;

namespace Producer;

class Program
{

  static readonly int _messageCount = 25;
  static readonly int _sendIntervalMs = 100;
  const string AllowedOptions = "12Qq";
  
  static IConnection _connection;
  
  
  static void Main(string[] args)
  {
    bool exit = false;
    
    using (_connection = ConnectToNats())
    {
      while (!exit)
      {
        Console.Clear();
        
        Console.WriteLine("NATS Demo Producer");
        Console.WriteLine("==================");
        Console.WriteLine("Select Mode:");
        Console.WriteLine("1) Request / Response (explicit)");
        Console.WriteLine("2) Request / Response (implicit)");
        Console.WriteLine("q) Quit");

        ConsoleKeyInfo input;
        do
        {
          input = Console.ReadKey(true);
        } while (!AllowedOptions.Contains(input.KeyChar));

        switch (input.KeyChar)
        {
          case '1':
            RequestResponseExplicit();
            break;
          case '2':
            RequestResponseImplicit();
            break;
          case 'q':
            case 'Q':
            exit = true;
            continue;
        }
        
        Console.WriteLine();
        Console.WriteLine("Done. Press any key to continue...");
        Console.ReadKey(true);
        Clear();
      }
      
      _connection.Drain(5000);
    }
  }

  static IConnection ConnectToNats()
  {
    ConnectionFactory factory = new();

    Options? options = ConnectionFactory.GetDefaultOptions();
    options.Url = "nats://localhost:4222";

    return factory.CreateConnection(options);
  }

  static void RequestResponseExplicit()
  {
    Console.Clear();
    Console.WriteLine("Request/Response (explicit) demo");
    Console.WriteLine("================================");

    for (int i = 1; i <= _messageCount; i++)
    {
      string replySubject = $"_INBOX.{Guid.NewGuid():N}";
      ISyncSubscription subscription = _connection.SubscribeSync(replySubject);
      subscription.AutoUnsubscribe(1);

      string message = $"Message {i}";
      
      Console.WriteLine($"Sending: {message}");
      
      // send with reply subject. NOTE: Messages must be sent as Byte arrays
      byte[] data = Encoding.UTF8.GetBytes(message);
      
      _connection.Publish("nats.demo.requestresponse", replySubject, data);
      
      // wait for response in reply subject
      Msg? response = subscription.NextMessage(5000);

      string responseMsg = Encoding.UTF8.GetString(response.Data);
      Console.WriteLine($"Response: {responseMsg}");
      
      Thread.Sleep(_sendIntervalMs);
    }
  }

  static void RequestResponseImplicit()
  {
    Console.Clear();
    Console.WriteLine("Request/Response (implicit) demo");
    Console.WriteLine("================================");
    for (int i = 1; i <= _messageCount; i++)
    {
      string message = $"Message {i}";

      Console.WriteLine($"Sending: {message}");

      byte[] data = Encoding.UTF8.GetBytes(message);
      
      Msg? response =
        _connection.Request("nats.demo.requestresponse", data, 5000);

      string responseMsg = Encoding.UTF8.GetString(response.Data);
      
      Console.Write($"Response: {responseMsg}");
      
      Thread.Sleep(_sendIntervalMs);
    }
  }


  static void Clear()
  {
    Console.Clear();
    _connection.Publish("nats.demo.clear", null);
  }
}