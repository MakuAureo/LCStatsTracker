using System.Net;
using System.Text;
using System.Threading;

namespace StatsTracker.Util;

public class HttpSSE
{
  private HttpListener? listener;
  private string? current_json = null;
  private Thread? server_thread;
  private const int PORT = 54321;
  private readonly ManualResetEvent day_finished_signaler = new ManualResetEvent(false);

  public void Start()
  {
    listener = new HttpListener();
    listener.Prefixes.Add($"http://localhost:{PORT}/");
    listener.Start();

    server_thread = new Thread(ListenLoop) { IsBackground = true };
    server_thread.Start();

    StatsTracker.Logger.LogInfo($"Stat server running at http://localhost:{PORT}/stats");
  }

  public void Stop()
  {
    listener?.Stop();
    server_thread?.Join();
  }

  public void PublishStats(string json)
  {
    current_json = json;
    day_finished_signaler.Set();
  }

  public void Reset()
  {
    current_json = null;
    day_finished_signaler.Reset();
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

    day_finished_signaler.WaitOne();

    var data = $"data: {current_json}\n\n";
    var buffer = Encoding.UTF8.GetBytes(data);
    response.OutputStream.Write(buffer, 0, buffer.Length);
    response.OutputStream.Flush();
    response.Close();
  }
}
