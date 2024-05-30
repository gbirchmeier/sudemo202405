using QuickFix;
using QuickFix.Fields;

namespace Client;

public class ClientApp : MessageCracker, IApplication {

    private readonly string _scenario;

    private bool _doShutdown = false;

    private Session? _session;

    public ClientApp(string scenario) {
        _scenario = scenario;
    }

    public void Run() {
        while (true) {
            Thread.Sleep(200);
            if (_doShutdown)
                break;
        }

        Console.WriteLine("Program shutdown.  (There may be a delay while logouts are exchanged.)");
    }

    public void OnCreate(SessionID sessionId) {
        _session = Session.LookupSession(sessionId);
    }

    public void OnLogon(SessionID sessionId) {
    }

    public void OnLogout(SessionID sessionId) {
    }

    public void FromAdmin(Message message, SessionID sessionId) {
        switch (message.Header.GetString(Tags.MsgType)) {
            case MsgType.LOGON:
                if (message.IsSetField(Tags.SessionStatus) && message.GetInt(Tags.SessionStatus) == SessionStatus.SESSION_PASSWORD_CHANGED) {
                    if(_scenario == "1b")
                        Console.WriteLine("Scenario 1b (password changed) successful");
                    _doShutdown = true;
                }
                break;

            case MsgType.LOGOUT:
                if (message.IsSetField(Tags.SessionStatus) && message.GetInt(Tags.SessionStatus) == SessionStatus.PASSWORD_EXPIRED) {
                    if(_scenario == "1a")
                        Console.WriteLine("Scenario 1a (password expired) successful");
                    _doShutdown = true;
                }
                break;
        }
    }

    public void ToAdmin(Message message, SessionID sessionId) {
        switch (_scenario) {
            case "1a":
                if (message.Header.GetString(Tags.MsgType) == MsgType.LOGON) {
                    message.SetField(new Username("testuser"));
                    message.SetField(new Password("LLL"));
                }
                break;
            case "1b":
                if (message.Header.GetString(Tags.MsgType) == MsgType.LOGON) {
                    message.SetField(new Username("testuser"));
                    message.SetField(new Password("LLL"));
                    message.SetField(new NewPassword("MMM"));
                }
                break;
        }
    }

    public void ToApp(Message message, SessionID sessionId) {
    }

    public void FromApp(Message message, SessionID sessionId) {
        try {
            Crack(message, sessionId);
        } catch (Exception ex) {
            Console.WriteLine(ex);
            throw;
        }
    }

    private void SendMessage(Message m) {
        if (_session != null)
            _session.Send(m);
        else
            Console.WriteLine("Can't send message: session not created."); // This probably won't ever happen.
    }
}
