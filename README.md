# sudemo202405

Two apps: a client (initiator) and a server (acceptor)

## "Server" app

No special parameters.  Just run it and leave it up.

```
cd Server
dotnet run ../../cfg/server.cfg
```

## "Client" app

Takes one "scenario" parameter.  The client will start, run the scenario, and shut down.

* It has `ResetOnLogon=Y` so it always starts at 1.  (I don't think I need to explain this.)
* `HeartBtInt` is 10 seconds

```
cd Client
dotnet run ../../cfg/server.cfg scenario=1a
```

### Scenarios:

* "1a" - Login with expired password "LLL".  Server will respond with appropriate logout.
* "1b" - Login with Password (554) and NewPassword (925).  Server will respond with appropriate login.
    * The counterparty docs treat these as a combined scenario, but they're functionally independent to each other.
* "3" - SeqNum reset scenario:
    1. Client logs on, waits for a few heartbeats to run the SeqNum up
    2. Client disconnects
    3. Client reconnects.  Config `ResetOnLogon` means it will include `ResetSeqNumFlag=Y`.
    4. Client observes that the SeqNum was reset by Server and terminates.
    5. (NOTE: this scenario is a little redundant -- The client's default `ResetOnLogon=Y` config means
       that this behavior is demonstrated whenever this Client connects to Server for any reason!)
