using QuickFix;
using QuickFix.Fields;

namespace Server;

public class ServerApp : MessageCracker, IApplication {
    private const decimal DefaultMarketPrice = 10;

    private int _orderId = 0;
    private int _execId = 0;

    private string GenOrderId() {
        return (++_orderId).ToString();
    }

    private string GenExecId() {
        return (++_execId).ToString();
    }

    public void FromApp(Message message, SessionID sessionId) {
        Crack(message, sessionId);
    }

    public void ToApp(Message message, SessionID sessionId) {
        Console.WriteLine("OUT: " + message);
    }

    public void FromAdmin(Message message, SessionID sessionId) {
    }

    public void OnCreate(SessionID sessionId) {
    }

    public void OnLogout(SessionID sessionId) {
    }

    public void OnLogon(SessionID sessionId) {
    }

    public void ToAdmin(Message message, SessionID sessionId) {
    }

    public void OnMessage(QuickFix.FIX50SP2.NewOrderSingle n, SessionID s) {
        Console.WriteLine("* Got a NewOrderSingle.  Responding with an ExecutionReport.");

        Symbol symbol = n.Symbol;
        Side side = n.Side;
        OrdType ordType = n.OrdType;
        OrderQty orderQty = n.OrderQty;
        Price price = new Price(DefaultMarketPrice);
        ClOrdID clOrdId = n.ClOrdID;

        switch (ordType.getValue()) {
            case OrdType.LIMIT:
                price = n.Price;
                if (price.Obj == 0)
                    throw new IncorrectTagValue(price.Tag);
                break;
            case OrdType.MARKET: break;
            default: throw new IncorrectTagValue(ordType.Tag);
        }

        QuickFix.FIX50SP2.ExecutionReport exReport = new QuickFix.FIX50SP2.ExecutionReport(
            new OrderID(GenOrderId()),
            new ExecID(GenExecId()),
            new ExecType(ExecType.FILL),
            new OrdStatus(OrdStatus.FILLED),
            side,
            new LeavesQty(0),
            new CumQty(orderQty.getValue()))
        {
            ClOrdID = clOrdId,
            Symbol = symbol,
            AvgPx = new AvgPx(price.getValue()),
            OrderQty = orderQty,
            LastQty = new LastQty(orderQty.getValue()),
            LastPx = new LastPx(price.getValue())
        };

        if (n.IsSetAccount())
            exReport.SetField(n.Account);

        try {
            Session.SendToTarget(exReport, s);
        } catch (SessionNotFound ex) {
            Console.WriteLine("==session not found exception!==");
            Console.WriteLine(ex.ToString());
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }

    public void OnMessage(QuickFix.FIX50SP2.News n, SessionID s) {
        Console.WriteLine("* Got a News.  Headline: " + n.Headline.getValue());
    }

    public void OnMessage(QuickFix.FIX50SP2.BusinessMessageReject n, SessionID s) {
        Console.WriteLine("***The initiator rejected one of my messages.");
    }
}
