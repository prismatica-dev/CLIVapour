using System.Net;
using CLIVapour.Core;
using CLIVapour.TorrentCore;
using static CLIVapour.TorrentCore.TorrentSources;

namespace CLIVapour;
public class Program {
    public static string NORMAL  = Console.IsOutputRedirected ? "" : "\x1b[39m";
    public static string RED     = Console.IsOutputRedirected ? "" : "\x1b[91m";
    public static string GREEN   = Console.IsOutputRedirected ? "" : "\x1b[92m";
    public static string YELLOW  = Console.IsOutputRedirected ? "" : "\x1b[93m";
    public static string BLUE    = Console.IsOutputRedirected ? "" : "\x1b[94m";
    public static string MAGENTA = Console.IsOutputRedirected ? "" : "\x1b[95m";
    public static string CYAN    = Console.IsOutputRedirected ? "" : "\x1b[96m";
    public static string GREY    = Console.IsOutputRedirected ? "" : "\x1b[97m";

    public static string[] SourceColour = { RED, CYAN, GREEN, YELLOW, RED, MAGENTA, BLUE, BLUE, BLUE };

    internal static List<ResultTorrent> Results = new();
    internal static int Searched = 0;
    internal const int WaitDelay = 50;
    internal const int WaitIncrement = 1000 / WaitDelay;
    internal static int MaxDelay = 30;
    internal static bool FilterResults = true;
    internal static bool UseDisabled = false;
    internal static bool Sort = true;
    internal static bool Verbose = false;
    internal static bool Quiet = false;
    internal static bool NoExtended = false;
    internal static bool AlwaysExtended = false;
    internal static List<string> SearchFilter = new();

    public static void ParseArgument(string Argument) {
        try {
            string p = string.Empty;
            if (Argument.Contains('=')) {
                string[] s = Argument.Split('=');
                Argument = s[0];
                p = s[1]; }
            // Argument = Argument.ToLower(TorrentListing.Culture);
            if (string.IsNullOrWhiteSpace(Argument)) return;
            switch (Argument) {
                case "--timeout":
                    MaxDelay = int.Parse(p);
                    return;
                case "-f":
                case "--no-filter":
                    FilterResults = false;
                    return;
                case "--filtering":
                    FilterResults = bool.Parse(p);
                    return;
                case "--search-all":
                    UseDisabled = true;
                    return;
                case "--no-sort":
                    Sort = false;
                    return;
                case "--sorting":
                    Sort = bool.Parse(p);
                    return;
                case "-U":
                case "--no-update":
                case "--no-updates":
                case "--exclude-update":
                case "--exclude-updates":
                    SearchFilter.Add("update");
                    return;
                case "-T":
                case "--strict-no-update":
                case "--strict-no-updates":
                case "--strict-exclude-update":
                case "--strict-exclude-updates":
                    SearchFilter.Add("update v");
                    return;
                case "--exclude":
                    SearchFilter.Add(p.Trim().ToLower(TorrentListing.Culture));
                    return;
                case "-R":
                case "--exclude-rune":
                    SearchFilter.Add("rune");
                    return;
                case "-C":
                case "--exclude-codex":
                    SearchFilter.Add("codex");
                    return;
                case "-E":
                case "--exclude-empress":
                case "--exclude-mould":
                    SearchFilter.Add("empress");
                    return;
                case "-S":
                case "--exclude-skidrow":
                    SearchFilter.Add("skidrow");
                    return;
                case "-P":
                case "--exclude-plaza":
                    SearchFilter.Add("plaza");
                    return;
                case "-a":
                case "--all":
                    MaxDelay = 60;
                    FilterResults = false;
                    UseDisabled = true;
                    return;
                case "-v":
                case "--verbose":
                    Verbose = true;
                    return;
                case "-q":
                case "--quiet":
                    Quiet = true;
                    return;
                case "-s":
                case "--simple":
                    NoExtended = true;
                    return;
                case "-e":
                case "--extended":
                    AlwaysExtended = true;
                    return;
                default:
                    if (Argument.Length > 1 && Argument.StartsWith('-')) {
                        bool Valid = true;
                        string su = Argument.Substring(1);
                        foreach (char c in su) {
                            bool v = false;
                            foreach (char c1 in ValidShorthands)
                                if (c1 == c) { v = true; break; }
                            if (!v) { Valid = false; break; }}
                        if (Valid) {
                            // Valid, execute shorthands
                            foreach (char c in su)
                                switch (c) {
                                    case 'e':
                                        AlwaysExtended = true;
                                        NoExtended = false;
                                        continue;
                                    case 's':
                                        NoExtended = true;
                                        AlwaysExtended = false;
                                        continue;
                                    case 'q':
                                        Quiet = true;
                                        Verbose = false;
                                        continue;
                                    case 'v':
                                        Quiet = false;
                                        Verbose = true;
                                        continue;
                                    case 'f':
                                        FilterResults = false;
                                        continue;
                                    case 'a':
                                        MaxDelay = 60;
                                        FilterResults = false;
                                        UseDisabled = true;
                                        continue;
                                    case 'R':
                                        SearchFilter.Add("rune");
                                        continue;
                                    case 'C':
                                        SearchFilter.Add("codex");
                                        continue;
                                    case 'S':
                                        SearchFilter.Add("skidrow");
                                        continue;
                                    case 'E':
                                        SearchFilter.Add("empress");
                                        continue;
                                    case 'P':
                                        SearchFilter.Add("plaza");
                                        continue;
                                    case 'U':
                                        SearchFilter.Add("update");
                                        continue;
                                    case 'T':
                                        SearchFilter.Add("update v");
                                        continue;  }
                            return; }}
                    Console.WriteLine($"{RED}Unknown argument '{Argument}'{GREY}");
                    return; }
        } catch (Exception ex) { Console.WriteLine($"{RED}Failed to parse argument '{Argument}'{GREY}"); }}

    public static char[] ValidShorthands = { 'e', 's', 'q', 'v', 'a', 'f', 'R', 'C', 'S', 'E', 'U', 'P' };
    public static async Task WaitForResults(int Target) {
        long s = DateTime.UtcNow.Ticks;
        for (int i = 0; i < MaxDelay * WaitIncrement; i++) {
            Console.CursorLeft = 0;
            Console.Write($"{GREEN}<=======>" + $"   Searched {Searched:N0} / {Target:N0} ({new TimeSpan(DateTime.UtcNow.Ticks - s).TotalSeconds:N1}s) ".PadRight(28) + "<=======>");
            if (Searched >= Target) { Console.WriteLine(); return; }
            await Task.Delay(WaitDelay); }
        Console.WriteLine($"\n{RED} Search Timeout"); }

    internal static bool IsSourceEnabled(TorrentSource Source) 
        => SourceScores[Source].Item3 == Implementation.Enabled || (UseDisabled && SourceScores[Source].Item3 != Implementation.Unimplemented);

    public static async Task Main(string[] args) {
        string _ = string.Empty;
        if (args.Length == 0) {
            Console.Write("Torrent Search: ");
            string? s = Console.ReadLine();
            if (s == null) return;
            _ = s.Trim().Replace("  ", " ").Replace("  ", " ");
            Console.WriteLine($"{GREEN} Searching for '{_}'{GREY}"); }
        else if (args.Length == 1) {
            string z = args[0];
            // Add extra hidden options to aid confused users guessing command
            if (z == "--help" || z == "-help" || z == "--h" || z == "-h") {
                Console.WriteLine(
                    $"{CYAN}CLIVapour Help{GREY}\n" +
                    $"{MAGENTA} Running CLIVapour with no arguments (eg. clivapour){GREY} will prompt the user for a search query with default settings\n" +
                    $"{MAGENTA} Running CLIVapour with one argument (eg. clivapour gamename){GREY} will use the provided search query with default settings\n" +
                    $"{MAGENTA} Running CLIVapour with more than one argument (eg. clivapour gamename --timeout=60 -no-filter){GREY} will use the first argument as the search query, and parse remaining arguments as settings.\n\n" +
                    $"{CYAN}Valid setting arguments\n" +
                    $" {MAGENTA}-h, --help              {GREY}Shows this menu and terminates without searching\n" +
                    $" {MAGENTA}-a, --all               {GREY}Equivalent to {MAGENTA}--no-filter --search-all --timeout=60{GREY}\n" +
                    $" {MAGENTA}-v, --verbose           {GREY}Enables verbose logging\n" +
                    $" {MAGENTA}-q, --quiet             {GREY}Disables error and magnet link logging\n" +
                    $" {MAGENTA}-s, --simple            {GREY}Forces search to never use additional torrent searching methods\n" +
                    $" {MAGENTA}-e, --extended          {GREY}Forces search to always use additional torrent searching methods\n" +
                    $" {MAGENTA}    --search-all        {GREY}Enables searching error-prone and non-ad-bypassed sources\n" +
                    $" {MAGENTA}    --no-sort           {GREY}Disables newest-oldest sorting\n" +
                    $" {MAGENTA}    --override=boolean  {GREY}Forces search to never/always use extended searching methods     {YELLOW}[DEFAULT: AUTO]{GREY}\n" +
                    $" {MAGENTA}    --sorting=boolean   {GREY}Disables/enables newest-oldest sorting                           {YELLOW}[DEFAULT: TRUE]{GREY}\n" +
                    $" {MAGENTA}    --timeout=int       {GREY}Sets the maximum search timeout to the provided int (in seconds) {YELLOW}[DEFAULT: 30]{GREY}\n\n" +
                    $"{CYAN}Valid filter arguments\n" +
                    $" {MAGENTA}-f, --no-filter         {GREY}Disables additional relevance filtering\n" +
                    $" {MAGENTA}-U, --no-updates        {GREY}Excludes labelled UPDATE torrents from results {RED}(prone to false positives){GREY}\n" +
                    $" {MAGENTA}-T, --strict-no-updates {GREY}Excludes labelled UPDATE vX.X.X torrents from results {RED}(prone to false negatives){GREY}\n" +
                    $" {MAGENTA}-R, --exclude-rune      {GREY}Excludes labelled RUNE torrents from results\n" +
                    $" {MAGENTA}-C, --exclude-codex     {GREY}Excludes labelled CODEX torrents from results\n" +
                    $" {MAGENTA}-S, --exclude-skidrow   {GREY}Excludes labelled SKIDROW torrents from results\n" +
                    $" {MAGENTA}-P, --exclude-plaza     {GREY}Excludes labelled PLAZA torrents from results\n" +
                    $" {MAGENTA}-E, --exclude-empress,   \n" +
                    $" {MAGENTA}    --exclude-mould     {GREY}Excludes labelled EMPRESS torrents from results\n" +
                    $" {MAGENTA}    --exclude=string    {GREY}Excludes results containingthe specified string {RED}(no spaces){GREY} from results\n" +
                    $" {MAGENTA}    --filtering=boolean {GREY}Disables/enables additional relevance filtering {YELLOW}[DEFAULT: TRUE]{GREY}");
                return; }
            _ = z.Trim().Replace("  ", " ").Replace("  ", " ");
            Console.WriteLine($"{GREEN} Searching for '{_}'{GREY}"); }
        else {
            string GameName = string.Empty;
            List<string> Args = new();
            for (int i = 0; i < args.Length; i++) {
                string a = args[i];
                if (!a.StartsWith('-')) GameName += a + ' ';
                else Args.Add(a); }

            foreach (string arg in Args)
                ParseArgument(arg.Trim());

            _ = GameName.Trim().Replace("  ", " ").Replace("  ", " ");
            Console.WriteLine($"{GREEN} Searching for '{_}'{GREY}");  }

        if (string.IsNullOrWhiteSpace(_)) {
            Console.WriteLine($"{RED} Nothing to search!{GREY}");
            return; }

        // Determine target sources
        int Target = 0;
        foreach (TorrentSource source in Enum.GetValues(typeof(TorrentSource)))
            if (IsSourceEnabled(source)) Target++;
        bool Extended = _.Length > 7;
        if (AlwaysExtended) Extended = true;
        else if (NoExtended) Extended = false;
        if (Extended) Target *= 2;

        // Extended Search
        if (Extended)
            foreach (TorrentSource source in Enum.GetValues(typeof(TorrentSource))) {
                if (!IsSourceEnabled(source)) continue;
                Task<List<Task<ResultTorrent>>> getresults = TorrentUtilities.GetExtendedResults(source, _);
                List<Task> GetTasks = [];
                Task gettask = getresults.ContinueWith(async (results) => {

                    foreach (Task<ResultTorrent> torrenttask in results.Result) {
                        Task gettask2 = torrenttask.ContinueWith((r) => {

                            ResultTorrent torrent = r.Result;
                            if (SearchFilter.Count == 0)
                                Results.Add(torrent);
                            else {
                                bool p = true;
                                foreach (string s in SearchFilter)
                                    if (torrent.Name.ToLower(TorrentListing.Culture).Contains(s)) {
                                        p = false; break; }
                                if (p) Results.Add(torrent); }});

                        // Task.Run(() => gettask2);
                        GetTasks.Add(gettask2); }
                    await Task.WhenAll(GetTasks);
                    await Task.Delay(250);
                    // Task.Delay(100);
                    Searched++; });

                Task.Run(() => getresults); }

        // Standard Search
        foreach (TorrentSource source in Enum.GetValues(typeof(TorrentSource))) {
            if (!IsSourceEnabled(source)) continue;
            Task<List<ResultTorrent>> getresults = TorrentUtilities.GetResults(source, _);
            Task gettask = getresults.ContinueWith((results) => {
                foreach (ResultTorrent torrent in results.Result) {
                    // do stuff
                    if (SearchFilter.Count == 0)
                        Results.Add(torrent);
                    else {
                        bool p = true;
                        foreach (string s in SearchFilter)
                            if (torrent.Name.ToLower(TorrentListing.Culture).Contains(s)) {
                                p = false; break; }
                        if (p) Results.Add(torrent); }}
                Searched++; });
            Task.Run(() => getresults); }

        // Thread.Sleep(5000);
        await WaitForResults(Target);

        ResultTorrent[] Torrents = Results.ToArray();
        if (Sort) Array.Sort(Torrents);
        int c = Torrents.Length;
        if (Torrents.Length == 0) {
            Console.WriteLine($"{RED} No results found!");
            return; }
        for (int i = 0; i < Torrents.Length; i++) {
            ResultTorrent t = Torrents[i];
            if (t.PublishDateTime != null)
                Console.WriteLine($"{MAGENTA}{c-i} {SourceColour[(int)t.Source]}{t.Source}{GREY}/{t.Name} {YELLOW}({t.PublishDateTime.ToString("MMMM d, yyyy")}){GREY}");
            else Console.WriteLine($"{MAGENTA}{c-i} {SourceColour[(int)t.Source]}{t.Source}{GREY}/{t.Name}"); }

        Console.WriteLine($"{GREEN}==>{GREY} Torrents to magnet (eg: 1 2 3, 1-3 or ^4)");
        Console.Write($"{GREEN}==>{GREY} ");
        string m = Console.ReadLine().Trim();
        try {
            if (m.Contains(' ')) {
                // Multiple specified manually
                string[] sp = m.Split(' ');
                int[] a = new int[sp.Length];
                for (int i = 0; i < a.Length; i++)
                    a[i] = c - int.Parse(sp[i]);
                foreach (int b in a)
                    await Magnet(Torrents[b], c-b); }

            else if (m.StartsWith('^')) {
                // Exclusion specified
                int a = c-int.Parse(Utilities.GetAfter(m, "^"));
                if (a < 0 || a > c) throw new IndexOutOfRangeException();
                for (int i = 0; i < Torrents.Length; i++)
                    if (i != a) await Magnet(Torrents[i], c-i); }

            else if (m.Contains('-')) {
                // Range specified
                string[] b = m.Split('-');
                if (b.Length != 2) throw new InvalidCastException();
                int a1 = c - int.Parse(b[0]);
                int a2 = c - int.Parse(b[1]);
                if (a2 < a1) {
                    int _a1 = a1; a1 = a2; a2 = _a1; }
                for (int i = a1; i <= a2; i++) {
                    await Magnet(Torrents[i], c-i); }}

            else {
                // One specified
                int a = int.Parse(m);
                await Magnet(Torrents[c-a], a); }

        } catch (Exception ex) {
            Console.WriteLine($"{RED} Exception parsing specifier (Error '{ex.Message}')"); }
        Console.WriteLine(" All done!"); }
            
    internal static async Task Magnet(ResultTorrent Torrent, int Index) {
        string Magnet = string.Empty;
        if (!Quiet) Console.WriteLine($"{GREY}Getting magnet for torrent {MAGENTA}{Index} {SourceColour[(int)Torrent.Source]}[{Torrent.Source}]{NORMAL}/{Torrent.Name} {YELLOW}({Torrent.PublishDateTime.ToString("MMMM d, yyyy")}){GREY}");
        try {
            if ((Torrent.Source == TorrentSource.KaOs && !Torrent.SafeAnyway) || Torrent.Source == TorrentSource.SteamRIP) {
                Console.WriteLine($"{MAGENTA}{Index} {YELLOW}Current torrent '{Torrent.Url}' is not fully implemented. Opening page URL '{Torrent.Url}'");
                Utilities.OpenUrl(Torrent.Url);
                return; }
            Magnet = await Torrent.GetMagnet();
            if (Magnet.ToLower().Contains("%2f")) Magnet = WebUtility.UrlDecode(Magnet);
        } catch (Exception ex) { 
            Utilities.HandleException($"{MAGENTA}{Index} {RED}Main.Magnet() [Get Magnet]", ex); }
        try {
            if (!string.IsNullOrWhiteSpace(Magnet)) {
                Utilities.HandleLogging($"{MAGENTA}{Index} {GREY}Opening magnet url '{Magnet}'");
                if (Utilities.OpenUrl(Magnet) && !Quiet)
                    Console.WriteLine($"{MAGENTA}{Index} {GREEN}Opened magnet url '{Magnet}'{GREY}");
                else if (Quiet) Console.WriteLine($"{MAGENTA}{Index} {RED}Failed to open magnet url '{Magnet}'{GREY}"); }
            else Console.WriteLine($"{MAGENTA}{Index} {RED}Could not retrieve magnet url from torrent {SourceColour[(int)Torrent.Source]}[{Torrent.Source}]{NORMAL}/{Torrent.Name}{GREY}");
        } catch (Exception ex) { 
            Utilities.HandleException($"{MAGENTA}{Index} {SourceColour[(int)Torrent.Source]}[{Torrent.Source}]{NORMAL}/{Torrent.Name} hit an exception", ex); }}}