using System.Net;
using System.Text;
using System.Threading;

namespace StatsTracker.Util;

public class HttpSSE
{
  private HttpListener? listener;
  private string? currentJson = null;
  private Thread? serverThread;
  private const int PORT = 2145;
  private readonly ManualResetEvent dayFinishedSignaler = new ManualResetEvent(false);

  public void Start()
  {
    listener = new HttpListener();
    listener.Prefixes.Add($"http://localhost:{PORT}/");
    listener.Start();

    serverThread = new Thread(ListenLoop) { IsBackground = true };
    serverThread.Start();

    StatsTracker.Logger.LogInfo($"Stat server running at http://localhost:{PORT}");
  }

  public void Stop()
  {
    listener?.Stop();
    serverThread?.Join();

    StatsTracker.Logger.LogInfo($"Stat server stopped");
  }

  public void PublishStats(string json)
  {
    currentJson = json;
    dayFinishedSignaler.Set();
  }

  public void Reset()
  {
    currentJson = null;
    dayFinishedSignaler.Reset();
  }

  private void ListenLoop()
  {
    //This should never be null (I think)
    while (listener!.IsListening)
    {
      try
      {
        var context = listener.GetContext();
        ThreadPool.QueueUserWorkItem(_ => HandleRequest(context));
      }
      catch (HttpListenerException) { break; }
    }
  }

  private void HandleRequest(HttpListenerContext context)
  {
    var response = context.Response;
    response.StatusCode = 200;
    response.ContentType = "text/event-stream";
    response.Headers.Add("Cache-Control", "no-cache");
    response.Headers.Add("Access-Control-Allow-Origin", "*");

    dayFinishedSignaler.WaitOne();

    var data = "{\"Stats\": " + currentJson + "}\n\n";
    var buffer = Encoding.UTF8.GetBytes(data);
    response.OutputStream.Write(buffer, 0, buffer.Length);
    response.OutputStream.Flush();
    response.Close();
  }
}
