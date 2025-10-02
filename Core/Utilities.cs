using System.Diagnostics;
using System.Text.RegularExpressions;

namespace CLIVapour.Core {
    internal partial class Utilities {
        internal static int GetLevenshteinDistance(string String, string Destination) {
            int length1 = String.Length;
            int length2 = Destination.Length;
            int[,] matrix = new int[length1 + 1, length2 + 1];

            if (length1 == 0) return length2;
            if (length2 == 0) return length1;
            for (int i = 0; i <= length1; i++) matrix[i, 0] = i;
            for (int j = 0; j <= length2; j++) matrix[0, j] = j;
            for (int i = 1; i <= length1; i++)
                for (int j = 1; j <= length2; j++)
                    matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), matrix[i - 1, j - 1] + ((Destination[j - 1] == String[i - 1]) ? 0 : 1));
            return matrix[length1, length2]; }

        internal static bool OpenUrl(string Url) {
            try { Process.Start(new ProcessStartInfo(Url) { UseShellExecute = true, Verb = "open" }); return true;
            } catch (Exception ex) { HandleException($"Utilities.OpenUrl({Url})", ex); return false; }}

        internal static string GetBetween(string String, string BetweenStart, string BetweenEnd) {
            try {
                if (String == null || BetweenStart == null || BetweenEnd == null) return "";
                int Start, End;
                if (String.Contains(BetweenStart) && String.Contains(BetweenEnd))
                    if (String.Substring(String.IndexOf(BetweenStart)).Contains(BetweenEnd))
                        try {
                            Start = String.IndexOf(BetweenStart, 0) + BetweenStart.Length;
                            End = String.IndexOf(BetweenEnd, Start);
                            string _ = String.Substring(Start, End - Start);
                            return String.Substring(Start, End - Start);
                        } catch (ArgumentOutOfRangeException) { return ""; }
                    else return String.Substring(String.IndexOf(BetweenStart) + BetweenStart.Length);
                else return "";
            } catch (Exception ex) { 
                HandleException($"Utilities.GetBetween({String}, {BetweenStart}, {BetweenEnd})", ex); 
                return ""; }}
        internal static string GetUntil(string String, string Until) {
            try {
                if (String == null || Until == null || !String.Contains(Until)) return String;
                try { return String.Substring(0, String.IndexOf(Until));
                } catch (ArgumentOutOfRangeException) { return ""; }
            } catch (Exception ex) { 
                HandleException($"Utilities.GetUntil({String}, {Until})", ex); 
                return String; }}
        internal static string GetBefore(string String, string Before) {
            try {
                if (String == null || Before == null || !String.Contains(Before)) return String;
                try { return String.Substring(0, String.LastIndexOf(Before));
                } catch (ArgumentOutOfRangeException) { return ""; }
            } catch (Exception ex) { 
                HandleException($"Utilities.GetBefore({String}, {Before})", ex); 
                return String; }}
        internal static string GetAfter(string String, string After) {
            try {
                if (String == null || After == null || !String.Contains(After)) return String;
                try { 
                    int after = String.IndexOf(After) + After.Length;
                    return String[after..];
                } catch (ArgumentOutOfRangeException) { return ""; }
            } catch (Exception ex) { 
                HandleException($"Utilities.GetAfter({String}, {After})", ex); 
                return String; }}

        internal static bool ExceptionHandlerException = false;
        internal static void HandleLogging(string Log, bool IgnoreLog = false, bool IgnoreException = false) {
            try {
                string logformat = $"{(IgnoreLog?"":$"[{DateTime.Now}]")} {Log}";
                if (Program.Verbose) Console.WriteLine(logformat);
            } catch (Exception ex) { if (!IgnoreException) HandleException($"Utilities.HandleLogging({Log}, {IgnoreLog})", ex, IgnoreLog); }}

        internal static void HandleException(string Cause, Exception Result, bool IgnoreLog = false) { 
            try {
                bool Unloggable = false;
                if (Result != null)
                    if (Result.Message.StartsWith("Unable to translate Unicode character"))
                        Unloggable = true;

                string logformat = Unloggable?$"[{DateTime.Now}] An unwritable source threw the exception '{Result?.Message}'\nStack Trace: '{Result.StackTrace}'":$"[{DateTime.Now}] {Cause} threw exception '{Result?.Message}'\nStack Trace: '{Result.StackTrace}'";
                if (!Program.Quiet) Console.WriteLine(logformat);

                Console.WriteLine($"[ERROR] {logformat}");
            } catch (Exception ex) { 
                if (!ExceptionHandlerException) {
                    ExceptionHandlerException = true;
                    Console.WriteLine("[META-ERROR] Error throwing error. Cannot list source."); }}}

        [GeneratedRegex("[^a-zA-Z0-9]")]
        internal static partial Regex alphanumeric { get; }
        [GeneratedRegex("[^a-zA-Z0-9 ]")]
        internal static partial Regex AlphanumericSpace { get; }
        [GeneratedRegex("[^0-9]")]
        internal static partial Regex numeric { get; }
        internal static string FilterAlphanumericSpace(string unfilteredString) => AlphanumericSpace.Replace(unfilteredString, string.Empty);
        internal static string FilterAlphanumeric(string unfilteredString) => alphanumeric.Replace(unfilteredString, string.Empty);
        internal static string FilterNumeric(string unfilteredString) => numeric.Replace(unfilteredString, string.Empty); }}
