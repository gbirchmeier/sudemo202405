using QuickFix;

namespace Server;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length != 1) {
            Console.WriteLine("usage: dotnet run CONFIG_FILENAME");
            Environment.Exit(2);
        }

        try {
            SessionSettings settings = new SessionSettings(args[0]);
            IApplication myApp = new ServerApp();
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);
            ThreadedSocketAcceptor acceptor = new ThreadedSocketAcceptor(myApp, storeFactory, settings, logFactory);

            acceptor.Start();
            Console.WriteLine("Acceptor is waiting for clients.");
            Console.WriteLine("press <enter> to quit");
            Console.Read();
            acceptor.Stop();
        } catch (Exception e) {
            Console.WriteLine("==FATAL ERROR==");
            Console.WriteLine(e.ToString());
        }
    }
}
