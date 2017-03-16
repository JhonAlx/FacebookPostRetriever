using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Threading;
using Facebook;
using JackLeitch.RateGate;
using OfficeOpenXml;
using Polly;
using Polly.Retry;

namespace PostRetriever
{
    public class ConsoleProgram
    {
        private static string _accessToken;
        private static string _filePath;
        private static DateTime _start;
        private static DateTime _end;

        private static DataRow _row;
        private static RetryPolicy _retryPolicy;
        private static DataTable _dt;
        private static RateGate _rg;
        private static bool _notifyStop;

        private static void Main(string[] args)
        {
            try
            {
                var secret = string.Empty;
                _notifyStop = true;

                if (args.Length > 0)
                    secret = args[0];

                if (secret.Equals("Hello"))
                {
                    _accessToken = args[1];
                    _filePath = args[2];
                    _start = DateTime.Parse(args[3]);
                    _end = DateTime.Parse(args[4]);
                    _dt = new DataTable();

                    var ranges = SplitDateRange(_start, _end, 15);

                    Log("INFO", "Values loaded! Starting post download");

                    _retryPolicy = Policy
                        .Handle<WebExceptionWrapper>()
                        .WaitAndRetry(
                            3,
                            retryAttempt => TimeSpan.FromMinutes(Math.Pow(2, retryAttempt)),
                            (e, i) =>
                            {
                                Log("ERROR", $"Caught exception {e.GetType().Name}, retrying in 5 minutes.");
                                Log("ERROR", $"{e.StackTrace}");

                                if (e.InnerException != null)
                                    Log("ERROR", $"{e.InnerException.Message}");
                            });

                    _rg = new RateGate(1400, TimeSpan.FromHours(1));

                    foreach (var range in ranges)
                        GetPostData(range);

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    _rg.Dispose();
                }
                else
                {
                    Usage();
                }
            }
            catch (Exception ex)
            {
                Log("ERROR", $"Global error {ex.GetType().Name} - {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        private static void GetPostData(Tuple<DateTime, DateTime> range)
        {
            var fb = new FacebookClient(_accessToken);
            var batchCounter = 0;
            dynamic parameters = new ExpandoObject();

            parameters.limit = 25;
            parameters.offset = batchCounter++;

            parameters.since = range.Item1.ToString("yyyy-MM-dd");

            parameters.until = range.Item2.ToString("yyyy-MM-dd");

            if (!_dt.Columns.Contains("datestamp") && !_dt.Columns.Contains("status"))
            {
                _dt.Columns.Add(new DataColumn("datestamp"));
                _dt.Columns.Add(new DataColumn("status"));
            }

            while (!_rg.WaitToProceed(0))
                if (_notifyStop)
                {
                    WaitMessage();

                    if (_dt.Rows.Count > 0)
                        SaveData(_dt);

                    _notifyStop = false;
                }

            _notifyStop = true;

            Log("INFO",
                $"Retrieving posts for range {range.Item1:yyyy-MM-dd} - {range.Item2:yyyy-MM-dd}");

            IDictionary<string, object> result = null;

            _retryPolicy.Execute(() => result = (IDictionary<string, object>) fb.Get("me/posts", parameters));

            var postCount = ((JsonArray) result["data"]).Count;

            Log("INFO", $"Processing {postCount} posts");

            try
            {
                while (postCount > 0)
                {
                    foreach (JsonObject item in (JsonArray) result["data"])
                    {
                        var commentsBatchCounter = 0;

                        dynamic commentParameters = new ExpandoObject();
                        commentParameters.limit = 25;
                        commentParameters.offset = commentsBatchCounter++;

                        while (!_rg.WaitToProceed(0))
                            if (_notifyStop)
                            {
                                WaitMessage();

                                if (_dt.Rows.Count > 0)
                                    SaveData(_dt);

                                _notifyStop = false;
                            }

                        _notifyStop = true;

                        Log("INFO", $"Retrieving comments for post {item["id"]}");

                        GetPostComments(item, fb, commentParameters,
                            commentsBatchCounter);
                    }

                    parameters.limit = 25;
                    parameters.offset = 25 * batchCounter++;

                    while (!_rg.WaitToProceed(0))
                        if (_notifyStop)
                        {
                            WaitMessage();

                            if (_dt.Rows.Count > 0)
                                SaveData(_dt);

                            _notifyStop = false;
                        }

                    _notifyStop = true;

                    Log("INFO",
                        $"Checking for more posts on range {range.Item1:yyyy-MM-dd} - {range.Item2:yyyy-MM-dd}");

                    _retryPolicy.Execute(() => result = (IDictionary<string, object>) fb.Get("me/posts", parameters));
                    postCount = ((JsonArray) result["data"]).Count;

                    if (_dt.Rows.Count > 0)
                        SaveData(_dt);
                }
            }
            catch (FacebookOAuthException)
            {
                Log("ERROR", "Limit reached");
                Log("WARNING", "Sleeping for 1 hour");
                SaveData(_dt);
                Thread.Sleep(TimeSpan.FromHours(1));
                _rg = new RateGate(1400, TimeSpan.FromHours(1));
            }
            catch (Exception ex)
            {
                Log("ERROR", $"{ex.GetType().Name} - {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                SaveData(_dt);
            }
        }

        private static void GetPostComments(IDictionary<string, object> item, FacebookClient fb,
            dynamic commentParameters, int commentsBatchCounter)
        {
            var commentsResult =
                _retryPolicy.Execute(
                    () => (IDictionary<string, object>) fb.Get($"{item["id"]}/comments", commentParameters));

            var comments = (JsonArray) commentsResult["data"];

            if (comments.Count > 0)
            {
                while (comments.Count > 0)
                {
                    foreach (JsonObject comment in comments)
                    {
                        _row = _dt.NewRow();

                        _row["datestamp"] = item["created_time"];

                        if (item.ContainsKey("message"))
                        {
                            _row["status"] = item["message"];
                        }
                        else
                        {
                            if (item.ContainsKey("story"))
                                _row["status"] = item["story"];
                            else
                                _row["status"] = "";
                        }

                        if (!_dt.Columns.Contains("comment_timestamp") &&
                            !_dt.Columns.Contains("comment") &&
                            !_dt.Columns.Contains("user_id") &&
                            !_dt.Columns.Contains("name"))
                        {
                            _dt.Columns.Add("comment_timestamp");
                            _dt.Columns.Add("name");
                            _dt.Columns.Add("user_id");
                            _dt.Columns.Add("comment");
                        }

                        _row["user_id"] = ((JsonObject) comment["from"])["id"];
                        _row["comment_timestamp"] = comment["created_time"];
                        _row["name"] = ((JsonObject) comment["from"])["name"];
                        _row["comment"] = comment["message"];

                        var repliesBatchCounter = 0;

                        dynamic repliesParameters = new ExpandoObject();
                        repliesParameters.limit = 25;
                        repliesParameters.offset = repliesBatchCounter++;

                        while (!_rg.WaitToProceed(0))
                            if (_notifyStop)
                            {
                                WaitMessage();

                                if (_dt.Rows.Count > 0)
                                    SaveData(_dt);

                                _notifyStop = false;
                            }

                        _notifyStop = true;

                        Log("INFO",
                            $"Retrieving replies for comment {comment["id"]}");

                        GetCommentReplies((IDictionary<string, object>) comment, fb, repliesParameters,
                            repliesBatchCounter);
                    }

                    commentParameters.limit = 25;
                    commentParameters.offset = 25 * commentsBatchCounter++;

                    while (!_rg.WaitToProceed(0))
                        if (_notifyStop)
                        {
                            WaitMessage();

                            if (_dt.Rows.Count > 0)
                                SaveData(_dt);

                            _notifyStop = false;
                        }

                    _notifyStop = true;

                    Log("INFO",
                        $"Checking for more comments on post {item["id"]}");

                    commentsResult =
                        _retryPolicy.Execute(
                            () => (IDictionary<string, object>) fb.Get($"{item["id"]}/comments", commentParameters));
                    comments = (JsonArray) commentsResult["data"];
                }
            }
            else
            {
                _row = _dt.NewRow();

                _row["datestamp"] = item["created_time"];

                if (item.ContainsKey("message"))
                {
                    _row["status"] = item["message"];
                }
                else
                {
                    if (item.ContainsKey("story"))
                        _row["status"] = item["story"];
                    else
                        _row["status"] = "";
                }

                _dt.Rows.Add(_row);

                _dt.AcceptChanges();
            }
        }

        private static void GetCommentReplies(IDictionary<string, object> comment, FacebookClient fb,
            dynamic repliesParameters, int repliesBatchCounter)
        {
            var repliesResult =
                _retryPolicy.Execute(
                    () => (IDictionary<string, object>) fb.Get($"{comment["id"]}/comments", repliesParameters));

            var replies = (JsonArray) repliesResult["data"];

            if (replies.Count > 0)
            {
                while (replies.Count > 0)
                {
                    foreach (JsonObject reply in replies)
                    {
                        if (!_dt.Columns.Contains("reply_timestamp") &&
                            !_dt.Columns.Contains("reply") &&
                            !_dt.Columns.Contains("reply_user_id") &&
                            !_dt.Columns.Contains("reply_name"))
                        {
                            _dt.Columns.Add("reply_timestamp");
                            _dt.Columns.Add("reply_user_id");
                            _dt.Columns.Add("reply_name");
                            _dt.Columns.Add("reply");
                        }

                        var newRow = _dt.NewRow();
                        newRow.ItemArray = _row.ItemArray;

                        _row["reply_user_id"] = ((JsonObject) reply["from"])["id"];
                        _row["reply_timestamp"] = reply["created_time"];
                        _row["reply_name"] = ((JsonObject) reply["from"])["name"];
                        _row["reply"] = reply["message"];

                        _dt.Rows.Add(_row);

                        _dt.AcceptChanges();

                        _row = newRow;
                    }

                    repliesParameters.limit = 25;
                    repliesParameters.offset = 25 * repliesBatchCounter++;

                    while (!_rg.WaitToProceed(0))
                        if (_notifyStop)
                        {
                            WaitMessage();

                            if (_dt.Rows.Count > 0)
                                SaveData(_dt);

                            _notifyStop = false;
                        }

                    _notifyStop = true;

                    Log("INFO",
                        $"Checking for more replies for comment {comment["id"]}");

                    repliesResult =
                        _retryPolicy.Execute(
                            () => (IDictionary<string, object>) fb.Get($"{comment["id"]}/comments", repliesParameters));
                    replies = (JsonArray) repliesResult["data"];
                }
            }
            else
            {
                _dt.Rows.Add(_row);

                _dt.AcceptChanges();
            }
        }

        private static void SaveData(DataTable dt)
        {
            Log("INFO", "Saving progress...");

            using (var fileStream = File.Create(_filePath))
            {
                using (var pck = new ExcelPackage(fileStream))
                {
                    var ws = pck.Workbook.Worksheets.Add("Accounts");
                    ws.Cells["A1"].LoadFromDataTable(dt, true);
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    pck.Save();
                }
            }
        }

        private static void Usage()
        {
            Log("ERROR", "This app cannot be executed directly!");
        }

        private static IEnumerable<Tuple<DateTime, DateTime>> SplitDateRange(DateTime start, DateTime end,
            int dayChunkSize)
        {
            DateTime chunkEnd;
            while ((chunkEnd = start.AddDays(dayChunkSize)) < end)
            {
                yield return Tuple.Create(start, chunkEnd);
                start = chunkEnd;
            }
            yield return Tuple.Create(start, end);
        }

        private static void WaitMessage()
        {
            Log("WARNING", "Stopping execution until allowed to proceed");
        }

        private static void Log(string type, string msg)
        {
            var status = string.Empty;

            Console.ResetColor();

            switch (type)
            {
                case "INFO":

                    status += $"[INFO] {DateTime.Now.ToString(CultureInfo.CurrentCulture)} - {msg}";

                    break;

                case "ERROR":

                    status += $"[ERROR] {DateTime.Now.ToString(CultureInfo.CurrentCulture)} - {msg}";
                    Console.ForegroundColor = ConsoleColor.Red;

                    break;

                case "WARNING":

                    status += $"[WARNING] {DateTime.Now.ToString(CultureInfo.CurrentCulture)} - {msg}";
                    Console.ForegroundColor = ConsoleColor.Yellow;

                    break;
            }

            Console.WriteLine(status);

            using (var sw = new StreamWriter("LogFile.txt", true))
            {
                sw.WriteLine(status);
            }
        }

        public string ReturnPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "PostRetriever.exe");
        }
    }
}