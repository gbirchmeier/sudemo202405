using QuickFix;
using QuickFix.Fields;

namespace Client;

public class ClientApp : MessageCracker, IApplication {
    private Session? _session = null;

    private void Puts(string s) {
        Console.Write("\n>>> " + s + "\n_? ");
    }

    public void Run() {
        while (true) {
            try {
                char action = QueryAction();
                if (action == '1')
                    TransmitNewOrderSingle();
                else if (action == '2')
                    TransmitAdvertisement();
                else if (action == 'q' || action == 'Q')
                    break;
            } catch (Exception e) {
                string s = "Message Not Sent: " + e.Message;
                s += "\nStackTrace: " + e.StackTrace;
                Puts(s);
            }
        }

        Console.WriteLine("Program shutdown.  (There may be a delay while logouts are exchanged.)");
    }

    private char QueryAction() {
        Puts("==MENU==\n"
             + "1) Send a nonsense NewOrderSingle (35=D), so we get a ExecReport in response\n"
             + "2) Send a nonsense Advertisement (35=7), so we get a rejection\n"
             + "Q) Quit"
        );

        HashSet<string> validActions = ["1", "2", "q", "Q"];

        string cmd = Console.ReadLine()!.Trim();
        if (cmd.Length != 1 || validActions.Contains(cmd) == false)
            throw new Exception("Invalid action");

        return cmd.ToCharArray()[0];
    }

    public void OnCreate(SessionID sessionId) {
        _session = Session.LookupSession(sessionId);
    }

    public void OnLogon(SessionID sessionId) {
        Puts("Logon - " + sessionId);
    }

    public void OnLogout(SessionID sessionId) {
        Puts("Logout - " + sessionId);
    }

    public void FromAdmin(Message message, SessionID sessionId) {
        Puts("Received admin message, type " + message.Header.GetString(Tags.MsgType));
    }

    public void ToAdmin(Message message, SessionID sessionId) {
        if (message.Header.GetString(Tags.MsgType) == MsgType.LOGON) {
            message.SetField(new Username("batman"));
            message.SetField(new Password("gotham123"));
        }
    }

    public void ToApp(Message message, SessionID sessionId) {
    }

    public void FromApp(Message message, SessionID sessionId) {
        try {
            Crack(message, sessionId);
        } catch (Exception ex) {
            string s = "==Cracker exception==\n";
            s += ex + "\n";
            s += ex.StackTrace;
            Puts(s);
        }
    }

    public void OnMessage(QuickFix.FIX50SP2.ExecutionReport m, SessionID s) {
        Puts("Received execution report");
    }

    private void SendMessage(Message m) {
        if (_session != null)
            _session.Send(m);
        else
            Puts("Can't send message: session not created."); // This probably won't ever happen.
    }

    private void TransmitNewOrderSingle() {
        QuickFix.FIX50SP2.NewOrderSingle msg = new QuickFix.FIX50SP2.NewOrderSingle(
            new ClOrdID("woooot"),
            new Side(Side.BUY),
            new TransactTime(DateTime.Now),
            new OrdType(OrdType.MARKET));

        msg.Set(new Symbol("IBM"));
        msg.Set(new OrderQty(99));

        SendMessage(msg);
    }

    private void TransmitAdvertisement() {
        QuickFix.FIX50SP2.Advertisement msg = new QuickFix.FIX50SP2.Advertisement(
            new AdvId("spam"),
            new AdvTransType("luncheon meat"),
            new AdvSide(AdvSide.SELL),
            new Quantity(1000000));

        msg.Set(new Symbol("SPAM"));

        SendMessage(msg);
    }
}
