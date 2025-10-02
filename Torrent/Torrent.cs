using static CLIVapour.TorrentCore.TorrentSources;
using static CLIVapour.Core.Utilities;
using static CLIVapour.TorrentCore.TorrentUtilities;
using System.Text;
using System.Net;
using CLIVapour.Web;
using System.Globalization;
using CLIVapour.Core;

namespace CLIVapour.TorrentCore;
internal static class TorrentListing {
    internal static Tuple<string, string>[] PCGTGameList = [];
    internal static string[] KaOSGameList = [];
    internal static readonly System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
    // DateTime refuses to correctly parse ru-RU d MMM yyyy date, so translate it first
    internal static string[] RussianMonths = ["янв", "фев", "мар", "апр", "мая", "июн", "июл", "авг", "сен", "окт", "ноя", "дек"];
    internal static string[] EnglishMonths = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"]; }

internal class ResultTorrent : IComparable {
    internal TorrentSource Source { get; set; }
    internal string Name { get; set; }
    internal string Url { get; set; }
    internal string TorrentUrl { get; set; }
    internal string PublishDate = "";
    internal DateTime PublishDateTime { get; set; }
    internal bool SafeAnyway { get; set; }

    public int CompareTo(ResultTorrent other) => PublishDateTime.CompareTo(other.PublishDateTime);
    public int CompareTo(object? other) => other is ResultTorrent t ? PublishDateTime.CompareTo(t.PublishDateTime) : -1;

    internal static async Task<ResultTorrent> TorrentFromUrl(TorrentSource source, string Url, string Name) {
        ResultTorrent t = new(TorrentSource.Unknown, "");
        try {
            /*if (Cache.IsTorrentCached(Url)) {
                HandleLogging($"Returning torrent {Url} from cache");
                ResultTorrent cached = Cache.LoadCachedTorrent(Url);
                if (cached != null) return cached; }*/

            string html = await WebCore.GetWebString(Url);
            switch (source) {
                case TorrentSource.PCGamesTorrents:
                    t = new ResultTorrent(Name, Url, GetBetween(html, "uk-card-hover\"><a href=\"", "\""), source);
                    string _ = GetAfter(html, "<time");
                    t.PublishDate = GetAfter(GetBetween(_, ">", "<"), ", ").Replace("+0000", string.Empty);
                    // Cache.CacheTorrent(t);
                    return t;

                case TorrentSource.KaOs:
                    HandleLogging($"[KaOs] Processing forum post {Url}");
                    // KaOs is a forum where uploaders use various formats. trying to introduce compatibility with these formats is hell.
                    string trurl = "";

                    // magnet url
                    HandleLogging($"[KaOs] Processing post torrent {Url}");
                    if (html.IndexOf("Filehost Mirrors") != -1)
                        trurl = GetBetween(GetAfter(html, "Filehost Mirrors"), "<a href=\"", "\"");
                    if (trurl.Length == 0) {
                        string[] lineHtml = html.Split('\n');
                        foreach (string line in lineHtml) {
                            string lwr = line.ToLower();
                            if ((lwr.Contains("magnet") || lwr.Contains("torrent")) && lwr.Contains("href"))
                                trurl = GetBetween(line, "href=\"", "\""); }}
                    HandleLogging($"[KaOs] found torrent");
                    t = new ResultTorrent(Name, Url, trurl, source);
                    if (string.IsNullOrWhiteSpace(trurl)) return null;

                    string __ = GetAfter(html, "<time");
                    t.PublishDate = GetBetween(__, ">", "<");
                    t.PublishDateTime = DateTime.ParseExact(GetBefore(t.PublishDate, ","), "dd MMM yyyy", TorrentListing.Culture);

                    // Cache.CacheTorrent(t);
                    return t;

                case TorrentSource.Unknown:
                default:
                    return new ResultTorrent(source, ""); }
            } catch (Exception ex) { HandleException($"ResultTorrent.TorrentFromUrl({source}, {Url}, {Name})", ex); }
        return t; }

    internal ResultTorrent(string Name, string Url, string TorrentUrl, TorrentSource Source, string PublishDate = "") { 
        this.Name = Name; this.Url = Url; this.TorrentUrl = TorrentUrl; this.Source = Source; this.PublishDate = PublishDate;
        if (!string.IsNullOrWhiteSpace(TorrentUrl)) SafeAnyway = TorrentUrl.Contains("paste.kaoskrew.org"); }

    internal ResultTorrent(TorrentSource Source, string JSON) {
        this.Source = Source;
        try {
            switch (Source) {
                case TorrentSource.PCGamesTorrents:
                case TorrentSource.FitgirlRepacks:
                case TorrentSource.SteamRIP:
                case TorrentSource.GOG:
                    Url = GetBetween(JSON, "<guid isPermaLink=\"false\">", "</guid>");
                    Name = FixRSSUnicode(GetBetween(JSON, "<title>", "</title>"));
                    PublishDate = GetBetween(JSON, "<pubDate>", "</pubDate>");
                    try {
                        PublishDateTime = DateTime.ParseExact(GetBefore(GetBetween(PublishDate, ", ", " +"), " "), "dd MMM yyyy", TorrentListing.Culture); 
                    } catch (Exception) {}
                break; }

            switch (Source) {
                case TorrentSource.PCGamesTorrents:
                    TorrentUrl = GetBetween(GetAfter(JSON, "TORRENT"), "a href=\"", "\""); // needs to load url shortener page then bypass waiting period
                    break;
                    
                case TorrentSource.FitgirlRepacks:
                    TorrentUrl = $"magnet:{GetBetween(JSON, "a href=\"magnet:", "\"")}"; // direct magnet
                    break;

                case TorrentSource.SteamRIP:
                    TorrentUrl = GetBetween(GetBetween(JSON, "clearfix", "</item"), "<p style=\"text-align: center;\"><a href=\"", "\"");
                    break;

                case TorrentSource.SevenGamers:
                    Url = GetBetween(JSON, "<a itemprop=\"url\" href=\"", "\"");
                    Name = GetBetween(JSON, "title=\"", "\"");
                    TorrentUrl = $"{Url}{(Url.EndsWith('/')?"":"/")}#torrent"; // needs to load page, download page then .torrent
                    break;

                case TorrentSource.GOG:
                    TorrentUrl = Url;
                    break;

                case TorrentSource.Xatab:
                    Url = "https://byxatab.com" + GetBetween(JSON, "href=\"https://byxatab.com", "\"");
                    if (Url == "https://byxatab.com/games/torrent_igry/licenzii/kniga-zakazov-rg-gogfan/30-1-0-1734") break; // prevent possible incorrect download
                    Name = GetBetween(JSON, $"{Url}\">", "</a>");
                    TorrentUrl = Url;
                    PublishDate = GetBetween(JSON, "<div class=\"entry__info-categories\">", "</div>");
                    string[] RusMonths = TorrentListing.RussianMonths;
                    string[] EngMonths = TorrentListing.EnglishMonths;
                    for (int i = 0; i < RusMonths.Length; i++)
                        PublishDate = PublishDate.Replace(RusMonths[i], EngMonths[i]);
                    PublishDateTime = DateTime.ParseExact(GetBefore(PublishDate, ","), "d MMMM yyyy", TorrentListing.Culture);
                    break;

                case TorrentSource.Unknown:
                default:
                    Url = ""; Name = ""; TorrentUrl = ""; PublishDate = "";
                    break; }
            } catch (Exception ex) { HandleException($"ResultTorrent.ResultSource({Source}, JSON)", ex); }}
            
    internal async Task<string> GetMagnet() {
        switch (Source) {
            case TorrentSource.PCGamesTorrents:
                // string Button = GetBetween(await WebCore.GetWebString(TorrentUrl, 7000), "Goroi_n_Create_Button(\"", "\")");
                // string Button = TorrentUrl; url-generator.php?url=
                // string Button = GetBetween(GetAfter(await WebCore.GetWebString(TorrentUrl, 7000), "DOWNLOAD LINKS"), "<a href=\"", "\"");
                string Button = GetBetween(GetAfter(await WebCore.GetWebString(TorrentUrl, 7000), "function generateDownloadUrl()"), "='", "'"); //function generateDownloadUrl()
                string Domain = GetBefore(TorrentUrl, "/") + "/get-url.php?url=";

                return WebUtility.HtmlDecode("magnet:?xt=" + GetBetween(await WebCore.GetWebString($"{Domain}{WebInternals.DecodeBlueMediaFiles(Button)}", 7000), "'magnet:?xt=", "'"));

            case TorrentSource.FitgirlRepacks:
                return WebUtility.HtmlDecode(TorrentUrl);

            case TorrentSource.SevenGamers:
                return WebUtility.HtmlDecode($"https://www.seven-gamers.com/fm/{GetBetween(await WebCore.GetWebString(GetBetween(await WebCore.GetWebString(TorrentUrl, 7000), "<a class=\"maxbutton-2 maxbutton maxbutton-torrent\" target=\"_blank\" rel=\"nofollow noopener\" href=\"", "\"")), "<a class=\"btn btn-primary main-btn py-3 d-flex w-100\" href=\"", "\"")}");

            case TorrentSource.KaOs:
                // despite all my rage, even if held at gunpoint i would refuse to try to find a stupid linkvertise bypass
                if (SafeAnyway) {
                    // omg! hooray! this url doesnt use linkvertise! how awesome is that??
                    return WebUtility.HtmlDecode(TorrentUrl); }
                return Url;

            case TorrentSource.SteamRIP:
                // pending megadb bypass
                // incredibly unlikely that it'll get one
                return WebUtility.HtmlDecode(Url);

            case TorrentSource.GOG:
                return WebUtility.HtmlDecode(Encoding.UTF8.GetString(Convert.FromBase64String(GetBetween(GetAfter(await WebCore.GetWebString(TorrentUrl, 7000), "Download Here"), "?url=", "\""))));

            case TorrentSource.Xatab:
                //return $"https://byxatab.com/index.php?do=download{WebUtility.HtmlDecode(GetBetween(await WebCore.GetWebString(TorrentUrl, 7000), "<a href=\"https://byxatab.com/index.php?do=download", "\""))}";
                Tuple<string, HttpResponseMessage> XatabResponse = await WebCore.GetWebStringWithResponse(TorrentUrl, 7000);
                string[] Cookies = (string[])XatabResponse.Item2.Headers.GetValues("set-cookie");
                if (Cookies.Length == 0) {
                    Console.WriteLine($"{Program.RED} Failed to get Xatab PHP session cookie from '{TorrentUrl}'{Program.GREY}");
                    return null; }
                
                return MagnetFromTorrent(Url, await WebCore.GetWebBytes($"https://byxatab.com/index.php?do=download{WebUtility.HtmlDecode(GetBetween(XatabResponse.Item1, "<a href=\"https://byxatab.com/index.php?do=download", "\""))}", TorrentUrl, GetUntil(Cookies[0], ";")));

            case TorrentSource.Unknown:
            default:
                throw new Exception($"Torrent source '{TorrentUrl}' is unknown or unsupported"); }}}