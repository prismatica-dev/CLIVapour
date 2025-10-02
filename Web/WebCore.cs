using System.IO.Compression;
using System.Net;
using CLIVapour.Core;
using static CLIVapour.Web.WebInternals;

namespace CLIVapour.Web;
internal static class WebCore {
    internal const int Timeout = 35;
    internal static Dictionary<string, DateTime> LastTimeout = [];
    internal static async Task<string> GetWebString(string Url, int MaxTimeout = 3500, bool FullSpoof = true, Uri Referrer = null) {
        Utilities.HandleLogging($"[HTTP-GET-0] '{Url}'");
        string baseUrl = GetBaseUrl(Url);
        if (LastTimeout.ContainsKey(baseUrl)) {
            if ((DateTime.Now - LastTimeout[baseUrl]) < TimeSpan.FromMilliseconds(Timeout))
                Utilities.HandleLogging($"GetWebString({Url}, {MaxTimeout}, {FullSpoof}) delayed for >={(DateTime.Now - LastTimeout[baseUrl]).TotalMilliseconds + 10:N2}ms");
            while ((DateTime.Now - LastTimeout[baseUrl]) < TimeSpan.FromMilliseconds(Timeout))
                await Task.Delay((int)Math.Ceiling((DateTime.Now - LastTimeout[baseUrl]).TotalMilliseconds) + 10);
            LastTimeout[baseUrl] = DateTime.Now;
        } else LastTimeout.Add(baseUrl, DateTime.Now);
            
        Utilities.HandleLogging($"[HTTP-GET-1] '{Url}'");
        using (HttpClientHandler handler = new() { AllowAutoRedirect = true, UseProxy = false, PreAuthenticate = false }) {
            using (HttpClient client = new(handler) { Timeout = TimeSpan.FromMilliseconds(MaxTimeout) }) {
                AddHeaders(client.DefaultRequestHeaders, Url, FullSpoof);
                if (Url.StartsWith("https://byxatab.com")) {
                    // Add old theme cookie
                    client.DefaultRequestHeaders.Add("Cookie", "dle_skin=Torrentino"); }
                if (Referrer != null)
                    client.DefaultRequestHeaders.Referrer = Referrer;
                try {
                    Utilities.HandleLogging($"[HTTP-GET-2] '{Url}'");
                    HttpResponseMessage response = await client.GetAsync(Url);
                    response.EnsureSuccessStatusCode();

                    string content = "";
                    using (Stream decompressedStream = await response.Content.ReadAsStreamAsync()) {
                        Stream decompressionStream = null;
                        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                            decompressionStream = new GZipStream(decompressedStream, CompressionMode.Decompress);
                        else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
                            decompressionStream = new DeflateStream(decompressedStream, CompressionMode.Decompress);

                        // Read the decompressed content as a string
                        content = await new StreamReader(decompressionStream ?? decompressedStream).ReadToEndAsync(); }
                    // string content = await response.Content.ReadAsStringAsync();
                    Utilities.HandleLogging($"[HTTP-GET-3] Done getting '{Url}'");
                    return content; }
                catch (TaskCanceledException ex) { Utilities.HandleException($"WebCore.GetWebString({Url}) [Cancellation Token {ex.CancellationToken.IsCancellationRequested}]", ex); }
                catch (Exception ex) { Utilities.HandleException($"WebCore.GetWebString({Url})", ex); }}}
        return ""; }
    internal static async Task<Tuple<string, HttpResponseMessage>> GetWebStringWithResponse(string Url, int MaxTimeout = 3500, bool FullSpoof = true, Uri Referrer = null) {
        Utilities.HandleLogging($"[HTTP-rGET-0] '{Url}'");
        string baseUrl = GetBaseUrl(Url);
        if (LastTimeout.ContainsKey(baseUrl)) {
            if ((DateTime.Now - LastTimeout[baseUrl]) < TimeSpan.FromMilliseconds(Timeout))
                Utilities.HandleLogging($"GetWebString({Url}, {MaxTimeout}, {FullSpoof}) delayed for >={(DateTime.Now - LastTimeout[baseUrl]).TotalMilliseconds + 10:N2}ms");
            while ((DateTime.Now - LastTimeout[baseUrl]) < TimeSpan.FromMilliseconds(Timeout))
                await Task.Delay((int)Math.Ceiling((DateTime.Now - LastTimeout[baseUrl]).TotalMilliseconds) + 10);
            LastTimeout[baseUrl] = DateTime.Now;
        } else LastTimeout.Add(baseUrl, DateTime.Now);
            
        Utilities.HandleLogging($"[HTTP-rGET-1] '{Url}'");
        using (HttpClientHandler handler = new() { AllowAutoRedirect = true, UseProxy = false, PreAuthenticate = false }) {
            using (HttpClient client = new(handler) { Timeout = TimeSpan.FromMilliseconds(MaxTimeout) }) {
                AddHeaders(client.DefaultRequestHeaders, Url, FullSpoof);
                if (Url.StartsWith("https://byxatab.com")) {
                    // Add old theme cookie
                    client.DefaultRequestHeaders.Add("Cookie", "dle_skin=Torrentino"); }
                if (Referrer != null)
                    client.DefaultRequestHeaders.Referrer = Referrer;
                try {
                    Utilities.HandleLogging($"[HTTP-rGET-2] '{Url}'");
                    HttpResponseMessage response = await client.GetAsync(Url);
                    response.EnsureSuccessStatusCode();

                    string content = "";
                    using (Stream decompressedStream = await response.Content.ReadAsStreamAsync()) {
                        Stream decompressionStream = null;
                        if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                            decompressionStream = new GZipStream(decompressedStream, CompressionMode.Decompress);
                        else if (response.Content.Headers.ContentEncoding.Contains("deflate"))
                            decompressionStream = new DeflateStream(decompressedStream, CompressionMode.Decompress);

                        // Read the decompressed content as a string
                        content = await new StreamReader(decompressionStream ?? decompressedStream).ReadToEndAsync(); }
                    // string content = await response.Content.ReadAsStringAsync();
                    Utilities.HandleLogging($"[HTTP-rGET-3] Done getting '{Url}'");
                    return new Tuple<string, HttpResponseMessage>(content, response); }
                catch (TaskCanceledException ex) { Utilities.HandleException($"WebCore.GetWebStringWithResponse({Url}) [Cancellation Token {ex.CancellationToken.IsCancellationRequested}]", ex); }
                catch (Exception ex) { Utilities.HandleException($"WebCore.GetWebStringWithResponse({Url})", ex); }}}
        return new Tuple<string, HttpResponseMessage>(string.Empty, null); }

    internal static async Task<byte[]> GetWebBytes(string Url, string Referer = null, string Cookie = null) {
        Utilities.HandleLogging($"[HTTP-GET-BYTES-0] '{Url}'");
        string baseUrl = GetBaseUrl(Url);
        if (LastTimeout.ContainsKey(baseUrl)) {
            if ((DateTime.Now - LastTimeout[baseUrl]) < TimeSpan.FromMilliseconds(Timeout))
                Utilities.HandleLogging($"GetWebBytes({Url}) delayed for >={(DateTime.Now - LastTimeout[baseUrl]).TotalMilliseconds + 10:N2}ms");
            while ((DateTime.Now - LastTimeout[baseUrl]) < TimeSpan.FromMilliseconds(Timeout))
                await Task.Delay((int)Math.Ceiling((DateTime.Now - LastTimeout[baseUrl]).TotalMilliseconds) + 10);
            LastTimeout[baseUrl] = DateTime.Now;
        } else LastTimeout.Add(baseUrl, DateTime.Now);

        try {
            Utilities.HandleLogging($"[HTTP-GET-BYTES-1] '{Url}'");
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Url);
            req.Method = "GET";
            req.UserAgent = GetRandomUserAgent();
            if (Referer != null) req.Referer = Referer;
            if (Cookie != null) { 
                req.Headers.Add("Cookie", Cookie);
                Utilities.HandleLogging($"Using Xatab PHP session cookie '{Cookie}'"); }
            req.AllowAutoRedirect = false;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Host = Utilities.GetBefore(Utilities.GetAfter(Url, "://"), "/");
            req.Headers.Add("Sec-Fetch-Dest", "document"); // top level user navigation
            req.Headers.Add("Sec-Fetch-Mode", "navigate"); // indicates HTML origin
            req.Headers.Add("Sec-Fetch-Site", "none"); // header that can only be sent by a user
            req.Headers.Add("Sec-Fetch-User", "?1"); // another one
            req.Headers.Add("TE", "trailers");
            req.Headers.Add("Upgrade-Insecure-Requests", "1"); /* https only */
            req.Headers.Add("Priority", "u=0, i");
            req.KeepAlive = true;
            req.AutomaticDecompression = DecompressionMethods.All;
            Utilities.HandleLogging($"[HTTP-GET-BYTES-2] '{Url}'");
            using (MemoryStream ms = new()) {
                (await req.GetResponseAsync()).GetResponseStream().CopyTo(ms);
                return ms.ToArray(); }
        } catch (Exception ex) { Utilities.HandleException($"WebCore.GetWebBytes({Url})", ex); }
        return []; }
    
    internal static string GetBaseUrl(string Url) { 
        if (string.IsNullOrWhiteSpace(Url)) return "";
        try { return new Uri(Url).GetLeftPart(UriPartial.Authority); } 
        catch (Exception ex) { Utilities.HandleException($"WebCore.GetBaseUrl({Url})", ex); return null; }}}
