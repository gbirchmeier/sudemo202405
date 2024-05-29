using QuickFix;
using QuickFix.Transport;

namespace Client;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("usage: dotnet run CONFIG_FILENAME");
            Environment.Exit(2);
        }

        string file = args[0];

        try
        {
            SessionSettings settings = new SessionSettings(file);
            var myApp = new ClientApp();
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            SocketInitiator initiator = new SocketInitiator(myApp, storeFactory, settings, logFactory);

            initiator.Start();
            myApp.Run();
            initiator.Stop();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
    }
}
