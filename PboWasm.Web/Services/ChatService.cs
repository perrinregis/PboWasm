using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Text.Json;

namespace PboWasm.Web.Services;

public record ChatMessage(string Text, bool IsMe, DateTime Time, bool IsSystem = false, string? ImageUrl = null);

public class ChatService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly NavigationManager _nav;
    private readonly ILocalStorageService _localStorage;
    private readonly IWebAssemblyHostEnvironment _env;

    private HubConnection? _hubConnection;
    private DotNetObjectReference<ChatService>? _dotnetRef;

    public bool IsAuthenticated { get; private set; } = false;
    public bool SignalRConnected { get; private set; } = false;
    public string P2pState { get; private set; } = "Waiting";
    
    public string MyEmail { get; private set; } = "";
    public string? SelectedUser { get; private set; } = null;
    public HashSet<string> OnlineUsers { get; private set; } = new();
    public List<ChatMessage> Messages { get; private set; } = new();
    
    public bool IsInitiator { get; private set; } = false;

    public event Action? OnStateChanged;

    public ChatService(IJSRuntime js, NavigationManager nav, ILocalStorageService localStorage, IWebAssemblyHostEnvironment env)
    {
        _js = js;
        _nav = nav;
        _localStorage = localStorage;
        _env = env;
    }

    public async Task InitializeAsync()
    {
        if (IsAuthenticated) return; // Already initialized

        try
        {
            MyEmail = await _localStorage.GetItemAsync<string>("UserEmail") ?? "";
            if (!string.IsNullOrEmpty(MyEmail)) 
            {
                IsAuthenticated = true;
                await ConnectSignalRAsync();
            }
        }
        catch { }
    }

    private async Task ConnectSignalRAsync()
    {
        _dotnetRef = DotNetObjectReference.Create(this);
        await _js.InvokeVoidAsync("webrtcInterop.initialize", _dotnetRef);

        var apiUrl = _env.Environment == "Development" ? "http://localhost:7071" : _nav.BaseUri.TrimEnd('/');

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{apiUrl}/api")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string>("ReceiveSignal", async (signalJson) =>
        {
            await HandleSignal(signalJson);
        });

        try
        {
            await _hubConnection.StartAsync();
            SignalRConnected = true;
            
            // Announce presence
            await SendSignalToServer(JsonSerializer.Serialize(new 
            { 
                type = "presence_announce", 
                senderEmail = MyEmail 
            }));
            
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to SignalR: {ex.Message}");
        }
    }

    public async Task SelectUserAsync(string targetEmail)
    {
        SelectedUser = targetEmail;
        Messages.Clear();
        P2pState = "Connecting";
        IsInitiator = true;
        
        var offerJson = await _js.InvokeAsync<string>("webrtcInterop.createOffer");

        var signal = JsonSerializer.Serialize(new 
        { 
            type = "offer", 
            data = offerJson, 
            senderEmail = MyEmail,
            targetEmail = SelectedUser
        });
        await SendSignalToServer(signal);

        AddSystemMessage($"Appel envoyé à {SelectedUser}...");
    }

    public void BackToContacts()
    {
        SelectedUser = null;
        P2pState = "Waiting";
        Messages.Clear();
        NotifyStateChanged();
    }

    private async Task HandleSignal(string signalJson)
    {
        var signal = JsonDocument.Parse(signalJson);
        var type = signal.RootElement.GetProperty("type").GetString();
        
        var senderEmail = signal.RootElement.TryGetProperty("senderEmail", out var s) ? s.GetString() : "";
        if (senderEmail == MyEmail) return; // Ignore our own

        if (type == "presence_announce")
        {
            if (!string.IsNullOrEmpty(senderEmail))
            {
                OnlineUsers.Add(senderEmail);
                NotifyStateChanged();
                
                await SendSignalToServer(JsonSerializer.Serialize(new 
                { 
                    type = "presence_reply", 
                    senderEmail = MyEmail,
                    targetEmail = senderEmail
                }));
            }
            return;
        }
        
        if (type == "presence_reply")
        {
            var targetE = signal.RootElement.TryGetProperty("targetEmail", out var t) ? t.GetString() : "";
            if (targetE == MyEmail && !string.IsNullOrEmpty(senderEmail))
            {
                OnlineUsers.Add(senderEmail);
                NotifyStateChanged();
            }
            return;
        }

        var targetEmailWebRtc = signal.RootElement.TryGetProperty("targetEmail", out var tW) ? tW.GetString() : "";
        if (targetEmailWebRtc != MyEmail) return;

        var data = signal.RootElement.GetProperty("data").GetString()!;

        switch (type)
        {
            case "offer":
                if (SelectedUser == null || SelectedUser != senderEmail)
                {
                    SelectedUser = senderEmail;
                    IsInitiator = false;
                    Messages.Clear();
                    AddSystemMessage($"{senderEmail} vous appelle...");
                }
                
                var answerJson = await _js.InvokeAsync<string>("webrtcInterop.handleOffer", data);
                var answerSignal = JsonSerializer.Serialize(new 
                { 
                    type = "answer", 
                    data = answerJson, 
                    senderEmail = MyEmail,
                    targetEmail = senderEmail
                });
                await SendSignalToServer(answerSignal);
                break;

            case "answer":
                if (IsInitiator && SelectedUser == senderEmail)
                {
                    await _js.InvokeVoidAsync("webrtcInterop.handleAnswer", data);
                }
                break;

            case "ice-candidate":
                if (SelectedUser == senderEmail)
                {
                    await _js.InvokeVoidAsync("webrtcInterop.addIceCandidate", data);
                }
                break;
        }
    }

    private async Task SendSignalToServer(string signal)
    {
        var apiUrl = _env.Environment == "Development" ? "http://localhost:7071" : _nav.BaseUri.TrimEnd('/');
        using var http = new HttpClient();
        var content = new StringContent(signal, System.Text.Encoding.UTF8, "application/json");
        var response = await http.PostAsync($"{apiUrl}/api/SendSignal", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message) || P2pState != "open") return;

        var payload = JsonSerializer.Serialize(new { type = "text", content = message });
        var sent = await _js.InvokeAsync<bool>("webrtcInterop.sendMessage", payload);
        if (sent)
        {
            Messages.Add(new ChatMessage(message, true, DateTime.Now));
            NotifyStateChanged();
        }
    }

    public async Task SendImageFromInputAsync(string inputId)
    {
        if (P2pState != "open") return;
        
        var dataUrl = await _js.InvokeAsync<string>("webrtcInterop.sendImageFromInput", inputId);
        if (!string.IsNullOrEmpty(dataUrl))
        {
            Messages.Add(new ChatMessage("", true, DateTime.Now, false, dataUrl));
            NotifyStateChanged();
        }
        
        await _js.InvokeVoidAsync("eval", $"document.getElementById('{inputId}').value = ''");
    }

    public async Task SendBase64ImageAsync(string dataUrl)
    {
        if (P2pState != "open" || string.IsNullOrEmpty(dataUrl)) return;
        
        var payload = JsonSerializer.Serialize(new { type = "image", content = dataUrl });
        var sent = await _js.InvokeAsync<bool>("webrtcInterop.sendMessage", payload);
        if (sent)
        {
            Messages.Add(new ChatMessage("", true, DateTime.Now, false, dataUrl));
            NotifyStateChanged();
        }
    }

    private void AddSystemMessage(string text)
    {
        Messages.Add(new ChatMessage(text, false, DateTime.Now, true));
        NotifyStateChanged();
    }

    [JSInvokable]
    public async Task OnIceCandidate(string candidateJson)
    {
        if (string.IsNullOrEmpty(SelectedUser)) return;
        
        var signal = JsonSerializer.Serialize(new 
        { 
            type = "ice-candidate", 
            data = candidateJson, 
            senderEmail = MyEmail,
            targetEmail = SelectedUser
        });
        await SendSignalToServer(signal);
    }

    [JSInvokable]
    public void OnConnectionStateChanged(string state)
    {
        Console.WriteLine($"P2P State: {state}");
    }

    [JSInvokable]
    public void OnDataChannelStateChanged(string state)
    {
        P2pState = state;
        if (state == "open")
        {
            AddSystemMessage("Connexion sécurisée établie.");
        }
        else if (state == "closed" || state == "disconnected")
        {
            AddSystemMessage("Connexion terminée.");
        }
        NotifyStateChanged();
    }

    [JSInvokable]
    public void OnMessageReceived(string messageJson)
    {
        try 
        {
            var doc = JsonDocument.Parse(messageJson);
            var type = doc.RootElement.GetProperty("type").GetString();
            var content = doc.RootElement.GetProperty("content").GetString();
            
            if (type == "image") 
            {
                Messages.Add(new ChatMessage("", false, DateTime.Now, false, content));
            } 
            else 
            {
                Messages.Add(new ChatMessage(content!, false, DateTime.Now));
            }
        } 
        catch 
        {
            Messages.Add(new ChatMessage(messageJson, false, DateTime.Now));
        }
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    public async ValueTask DisposeAsync()
    {
        _dotnetRef?.Dispose();
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
