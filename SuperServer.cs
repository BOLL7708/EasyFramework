using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SuperSocket.Server;
using SuperSocket.Server.Abstractions;
using SuperSocket.Server.Host;
using SuperSocket.WebSocket.Server;
using CloseReason = SuperSocket.Connection.CloseReason;

namespace BOLL7708;

/**
 * Add the Nuget package SuperSocket.WebSocket.Server 2.x
 */
public class SuperServer
{
    public enum ServerStatus
    {
        Connected,
        Disconnected,
        Error,
        ReceivedCount,
        DeliveredCount,
        SessionCount
    }
    const int DEFAULT_PORT = 4040;
    const string DEFAULT_IP = "Any";

    private IServer? _server;

    // We are getting crashes when loading sessions from _server directly so we also store sessions here.
    private readonly ConcurrentDictionary<string, WebSocketSession> _sessions = new(); 

    private volatile int _deliveredCount = 0;
    private volatile int _receivedCount = 0;

    #region Actions

    public Action<ServerStatus, int> StatusAction = (status, sessionCount) => { };
    public Action<WebSocketSession, string> MessageReceivedAction = (session, message) => { };
    public Action<WebSocketSession?, bool, string> StatusMessageAction = (session, statusType, value) => { };

    #endregion

    public SuperServer() {}

    #region Manage

    public async Task Start(int port = DEFAULT_PORT, string ip = DEFAULT_IP)
    {
        // Stop in case of already running
        await Stop();

        // Start
        _server = BuildServer(port, ip);
        await _server.StartAsync();
        StatusAction.Invoke(_server.State == ServerState.Started ? ServerStatus.Connected : ServerStatus.Error, 0);
    }

    public async Task Stop()
    {
        if (_server != null)
        {
            foreach (var session in _sessions) {
                await session.Value.CloseAsync();
            }
            await _server.StopAsync();
            await _server.DisposeAsync();
            _server = null;
        }
        StatusAction.Invoke(ServerStatus.Disconnected, 0);
    }

    #endregion

    #region Listeners

    private void Server_NewSessionConnected(WebSocketSession session)
    {
        if(session != null)
        {
            _sessions[session.SessionID] = session;
            StatusMessageAction.Invoke(session, true, $"New session connected: {session.SessionID}");
            StatusAction(ServerStatus.SessionCount, _sessions.Count);
        }
    }

    private void Server_NewMessageReceived(WebSocketSession session, string value)
    {
        MessageReceivedAction.Invoke(session, value);
        _receivedCount++;
        StatusAction(ServerStatus.ReceivedCount, _receivedCount);
    }

    private void Server_SessionClosed(WebSocketSession session, CloseReason reason)
    {
        if(session != null)
        {
            _sessions.TryRemove(session.SessionID, out WebSocketSession? oldSession);
            var reasonName = Enum.GetName(typeof(CloseReason), reason);
            StatusMessageAction.Invoke(null, false, $"Session closed: {session.SessionID}, because: {reasonName}");
            StatusAction(ServerStatus.SessionCount, _sessions.Count);
        }
    }

    #endregion

    #region Send
    public async void SendMessage(WebSocketSession session, string message)
    {
        try
        {
            if ( _server == null || _server.State != ServerState.Started) return;
        } 
        catch(ObjectDisposedException ex)
        {
            Debug.WriteLine(ex.Message);
            return;
        }

        if (session is { Handshaked: true })
        {
            await session.SendAsync(message);
            _deliveredCount++;
            StatusAction(ServerStatus.DeliveredCount, _deliveredCount);
        }
        else SendMessageToAll(message);
    }
    public void SendMessageToAll(string message)
    {
        foreach (var session in _sessions.Values)
        {
            if (session != null) SendMessage(session, message);
        }
    }
    public void SendMessageToOthers(string senderSessionID, string message)
    {
        foreach (var session in _sessions.Values)
        {
            if (session != null && session.SessionID != senderSessionID) SendMessage(session, message);
        }
    }
    public void SendMessageToGroup(string[] sessionIDs, string message)
    {
        foreach (var session in _sessions.Values)
        {
            if (session != null && sessionIDs.Contains(session.SessionID)) SendMessage(session, message);
        }
    }
    #endregion

    #region BoilerPlate

    public IServer BuildServer(int port = DEFAULT_PORT, string ip = DEFAULT_IP)
    {
        var hostBuilder = WebSocketHostBuilder.Create();

        hostBuilder.UseWebSocketMessageHandler((session, message) =>
        {
            Server_NewMessageReceived(session, message.Message);
            return ValueTask.CompletedTask;
        });

        hostBuilder.UseSessionHandler(
            session =>
            {
                Server_NewSessionConnected(session as WebSocketSession ?? null);
                return ValueTask.CompletedTask;
            },
            (session, e) =>
            {
                Server_SessionClosed(session as WebSocketSession ?? null, e.Reason);
                return ValueTask.CompletedTask;
            }
        );

        hostBuilder.ConfigureSuperSocket(options =>
        {
            options.AddListener(new ListenOptions
            {
                Ip = ip,
                Port = port
            });
        });

        hostBuilder.ConfigureErrorHandler((session, exception) => {
            Debug.WriteLine($"Exception from Error Handler: {exception.Message}");
            return ValueTask.FromResult(false);
        });

        hostBuilder.ConfigureLogging((hostCtx, loggingBuilder) => { loggingBuilder.AddConsole(); });

        return hostBuilder.BuildAsServer();
    }

    #endregion
}