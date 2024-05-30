using QuickFix;
using QuickFix.Fields;

namespace Server;

public class ServerApp : MessageCracker, IApplication
{
    enum LogonState {
        Ok = 0,
        PwExpired = 1,
        PwChanged = 2
    }
    private LogonState _logonState = LogonState.Ok;


    public void FromApp(Message message, SessionID sessionId) {
        Crack(message, sessionId);
    }

    public void ToApp(Message message, SessionID sessionId) {
        Console.WriteLine("OUT: " + message);
    }

    public void FromAdmin(Message message, SessionID sessionId) {
        if (message.Header.GetString(Tags.MsgType) == MsgType.LOGON) {
            if (message.GetString(Tags.Password) == "LLL") {
                if (message.IsSetField(Tags.NewPassword)) {
                    _logonState = LogonState.PwChanged;
                    Console.WriteLine(">>> Detected scenario 1b: Password change");
                } else {
                    _logonState = LogonState.PwExpired;
                    Console.WriteLine(">>> Detected scenario 1a: Password expired");
                    throw new RejectLogon("Password Expired");
                }
            }
        }
    }

    public void ToAdmin(Message message, SessionID sessionId) {
        if (message.Header.GetString(Tags.MsgType) == MsgType.LOGON) {
            if (_logonState == LogonState.PwChanged) {
                message.SetField(new SessionStatus(SessionStatus.SESSION_PASSWORD_CHANGED));
            }
        }

        if (message.Header.GetString(Tags.MsgType) == MsgType.LOGOUT) {
            if (_logonState == LogonState.PwExpired) {
                // Text is already "Rejected Logon Attempt: Password Expired"
                //   (Can overwrite that message here if desired)
                message.SetField(new SessionStatus(SessionStatus.PASSWORD_EXPIRED));
            }
        }

        _logonState = LogonState.Ok;
    }

    public void OnCreate(SessionID sessionId) {
    }

    public void OnLogout(SessionID sessionId) {
    }

    public void OnLogon(SessionID sessionId) {
    }
}
