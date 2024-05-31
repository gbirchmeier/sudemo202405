using QuickFix;
using QuickFix.Fields;

namespace Client;

public class ClientApp : MessageCracker, IApplication {

    private string _scenario;

    private bool _doShutdown = false;

    public Session? MySession { get; set; }

    public ClientApp(string scenario) {
        _scenario = scenario;
    }

    public void Run() {
        while (true) {
            Thread.Sleep(200);
            if (_doShutdown) {
                _doShutdown = false;
                break;
            }
        }

        Console.WriteLine("Program shutdown.  (There may be a delay while logouts are exchanged.)");
    }

    public void OnCreate(SessionID sessionId) {
        MySession = Session.LookupSession(sessionId);
    }

    public void OnLogon(SessionID sessionId) {
    }

    public void OnLogout(SessionID sessionId) {
    }

    private int _firstSeqNo = -1; // for scenario 3

    public void FromAdmin(Message message, SessionID sessionId) {
        string msgType = message.Header.GetString(Tags.MsgType);
        int seqNum = message.Header.GetInt(Tags.MsgSeqNum);

        switch (_scenario) {
            case "1a":
                if (msgType == MsgType.LOGOUT
                    && message.IsSetField(Tags.SessionStatus)
                    && message.GetInt(Tags.SessionStatus) == SessionStatus.PASSWORD_EXPIRED) {
                    Console.WriteLine("Scenario 1a (password expired) successful");
                    _doShutdown = true;
                }
                break;

            case "1b":
                if (msgType == MsgType.LOGON
                    && message.IsSetField(Tags.SessionStatus)
                    && message.GetInt(Tags.SessionStatus) == SessionStatus.SESSION_PASSWORD_CHANGED) {
                    Console.WriteLine("Scenario 1b (password changed) successful");
                    _doShutdown = true;
                }
                break;

            case "3":
                if (_firstSeqNo < 0)
                    _firstSeqNo = seqNum;

                Console.WriteLine($"Received {msgType} with SeqNum {seqNum}");

                if (seqNum - _firstSeqNo >= 2) {
                    Console.WriteLine("Scenario 3: Logging out. Will logon with reset.");
                    Console.WriteLine();
                    // every logon is a reset, don't need to change anything
                    _scenario = "3-after-reset";
                    _doShutdown = true;
                }
                break;

            case "3-after-reset":
                Console.WriteLine($"Received {msgType} with SeqNum {seqNum}");

                // ignore Logout, it's from the previous logon
                if (msgType == MsgType.LOGOUT)
                    break;

                Console.WriteLine(seqNum == 1
                    ? "Scenario 3 was successfully reset!"
                    : "Scenario 3 FAILED. SeqNum was not reset.");
                _doShutdown = true;
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
        if (MySession != null)
            MySession.Send(m);
        else
            Console.WriteLine("Can't send message: session not created."); // This probably won't ever happen.
    }
}
