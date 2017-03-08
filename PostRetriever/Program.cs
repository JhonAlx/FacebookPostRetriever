using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using Facebook;
using JackLeitch.RateGate;
using OfficeOpenXml;

namespace PostRetriever
{
    public class ConsoleProgram
    {
        private static string _accessToken;
        private static string _filePath;
        private static DateTime _start;
        private static DateTime _end;

        private static void Main(string[] args)
        {
            var secret = string.Empty;
            var notifyStop = true;

            //foreach (var arg in args)
            //    Console.WriteLine(arg);

            if (args.Length > 0)
                secret = args[0];

            if (secret.Equals("Hello"))
            {
                _accessToken = args[1];
                _filePath = args[2];
                _start = DateTime.Parse(args[3]);
                _end = DateTime.Parse(args[4]);

                var ranges = SplitDateRange(_start, _end, 15);

                Log("INFO", "Values loaded! Starting post download");

                using (var rg = new RateGate(1400, TimeSpan.FromHours(1)))
                {
                    foreach (var range in ranges)
                    {
                        var fb = new FacebookClient(_accessToken);
                        var batchCounter = 0;
                        dynamic parameters = new ExpandoObject();

                        parameters.limit = 25;
                        parameters.offset = batchCounter++;

                        parameters.since = range.Item1.ToString("yyyy-MM-dd");

                        parameters.until = range.Item2.ToString("yyyy-MM-dd");

                        var dt = new DataTable();

                        dt.Columns.Add(new DataColumn("datestamp"));
                        dt.Columns.Add(new DataColumn("status"));

                        while (!rg.WaitToProceed(0))
                            if (notifyStop)
                            {
                                WaitMessage();

                                if (dt.Rows.Count > 0)
                                    SaveData(dt);

                                notifyStop = false;
                            }

                        notifyStop = true;

                        Log("INFO",
                            $"Retrieving posts for range {range.Item1:yyyy-MM-dd} - {range.Item2:yyyy-MM-dd}");

                        var result = (IDictionary<string, object>) fb.Get("me/posts", parameters);

                        var postCount = ((JsonArray) result["data"]).Count;

                        Log("INFO", $"Processing {postCount} posts");

                        try
                        {
                            while (postCount > 0)
                            {
                                foreach (JsonObject item in (JsonArray) result["data"])
                                {
                                    DataRow row;

                                    var commentsBatchCounter = 0;

                                    dynamic commentParameters = new ExpandoObject();
                                    commentParameters.limit = 25;
                                    commentParameters.offset = commentsBatchCounter++;

                                    while (!rg.WaitToProceed(0))
                                        if (notifyStop)
                                        {
                                            WaitMessage();

                                            if (dt.Rows.Count > 0)
                                                SaveData(dt);

                                            notifyStop = false;
                                        }

                                    notifyStop = true;

                                    Log("INFO", $"Retrieving comments for post {item["id"]}");

                                    var commentsResult =
                                        (IDictionary<string, object>)
                                        fb.Get($"{item["id"]}/comments", commentParameters);

                                    var comments = (JsonArray) commentsResult["data"];

                                    if (comments.Count > 0)
                                    {
                                        while (comments.Count > 0)
                                        {
                                            foreach (JsonObject comment in comments)
                                            {
                                                row = dt.NewRow();

                                                row["datestamp"] = item["created_time"];

                                                if (item.ContainsKey("message"))
                                                {
                                                    row["status"] = item["message"];
                                                }
                                                else
                                                {
                                                    if (item.ContainsKey("story"))
                                                        row["status"] = item["story"];
                                                    else
                                                        row["status"] = "";
                                                }

                                                if (!dt.Columns.Contains("comment_timestamp") &&
                                                    !dt.Columns.Contains("comment") &&
                                                    !dt.Columns.Contains("user_id") &&
                                                    !dt.Columns.Contains("name"))
                                                {
                                                    dt.Columns.Add("comment_timestamp");
                                                    dt.Columns.Add("name");
                                                    dt.Columns.Add("user_id");
                                                    dt.Columns.Add("comment");
                                                }

                                                row["user_id"] = ((JsonObject) comment["from"])["id"];
                                                row["comment_timestamp"] = comment["created_time"];
                                                row["name"] = ((JsonObject) comment["from"])["name"];
                                                row["comment"] = comment["message"];

                                                var repliesBatchCounter = 0;

                                                dynamic repliesParameters = new ExpandoObject();
                                                repliesParameters.limit = 25;
                                                repliesParameters.offset = repliesBatchCounter++;

                                                while (!rg.WaitToProceed(0))
                                                    if (notifyStop)
                                                    {
                                                        WaitMessage();

                                                        if (dt.Rows.Count > 0)
                                                            SaveData(dt);

                                                        notifyStop = false;
                                                    }

                                                notifyStop = true;

                                                Log("INFO",
                                                    $"Retrieving replies for comment {comment["id"]}");

                                                var repliesResult =
                                                    (IDictionary<string, object>)
                                                    fb.Get($"{comment["id"]}/comments", repliesParameters);
                                                var replies = (JsonArray) repliesResult["data"];

                                                if (replies.Count > 0)
                                                {
                                                    while (replies.Count > 0)
                                                    {
                                                        foreach (JsonObject reply in replies)
                                                        {
                                                            if (!dt.Columns.Contains($"reply_timestamp") &&
                                                                !dt.Columns.Contains($"reply") &&
                                                                !dt.Columns.Contains($"reply_user_id") &&
                                                                !dt.Columns.Contains($"reply_name"))
                                                            {
                                                                dt.Columns.Add($"reply_timestamp");
                                                                dt.Columns.Add($"reply_user_id");
                                                                dt.Columns.Add($"reply_name");
                                                                dt.Columns.Add($"reply");
                                                            }

                                                            var newRow = dt.NewRow();
                                                            newRow.ItemArray = row.ItemArray;

                                                            row[$"reply_user_id"] = ((JsonObject) reply["from"])["id"];
                                                            row[$"reply_timestamp"] = reply["created_time"];
                                                            row[$"reply_name"] = ((JsonObject) reply["from"])["name"];
                                                            row[$"reply"] = reply["message"];

                                                            dt.Rows.Add(row);

                                                            dt.AcceptChanges();

                                                            row = newRow;
                                                        }

                                                        repliesParameters.limit = 25;
                                                        repliesParameters.offset = 25 * repliesBatchCounter++;

                                                        while (!rg.WaitToProceed(0))
                                                            if (notifyStop)
                                                            {
                                                                WaitMessage();

                                                                if (dt.Rows.Count > 0)
                                                                    SaveData(dt);

                                                                notifyStop = false;
                                                            }

                                                        notifyStop = true;

                                                        Log("INFO",
                                                            $"Checking for more replies for comment {comment["id"]}");

                                                        repliesResult =
                                                            (IDictionary<string, object>)
                                                            fb.Get($"{comment["id"]}/comments", repliesParameters);
                                                        replies = (JsonArray) repliesResult["data"];
                                                    }
                                                }
                                                else
                                                {
                                                    dt.Rows.Add(row);

                                                    dt.AcceptChanges();
                                                }
                                            }

                                            commentParameters.limit = 25;
                                            commentParameters.offset = 25 * commentsBatchCounter++;

                                            while (!rg.WaitToProceed(0))
                                                if (notifyStop)
                                                {
                                                    WaitMessage();

                                                    if (dt.Rows.Count > 0)
                                                        SaveData(dt);

                                                    notifyStop = false;
                                                }

                                            notifyStop = true;

                                            Log("INFO",
                                                $"Checking for more comments on post {item["id"]}");

                                            commentsResult =
                                                (IDictionary<string, object>)
                                                fb.Get($"{item["id"]}/comments", commentParameters);
                                            comments = (JsonArray) commentsResult["data"];
                                        }
                                    }
                                    else
                                    {
                                        row = dt.NewRow();

                                        row["datestamp"] = item["created_time"];

                                        if (item.ContainsKey("message"))
                                        {
                                            row["status"] = item["message"];
                                        }
                                        else
                                        {
                                            if (item.ContainsKey("story"))
                                                row["status"] = item["story"];
                                            else
                                                row["status"] = "";
                                        }

                                        dt.Rows.Add(row);

                                        dt.AcceptChanges();
                                    }
                                }

                                parameters.limit = 25;
                                parameters.offset = 25 * batchCounter++;

                                while (!rg.WaitToProceed(0))
                                    if (notifyStop)
                                    {
                                        WaitMessage();

                                        if (dt.Rows.Count > 0)
                                            SaveData(dt);

                                        notifyStop = false;
                                    }

                                notifyStop = true;

                                Log("INFO",
                                    $"Checking for more posts on range {range.Item1:yyyy-MM-dd} - {range.Item2:yyyy-MM-dd}");

                                result = (IDictionary<string, object>) fb.Get("me/posts", parameters);
                                postCount = ((JsonArray) result["data"]).Count;

                                if (dt.Rows.Count > 0)
                                    SaveData(dt);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log("ERROR", ex.Message + Environment.NewLine + ex.StackTrace);
                        }
                    }
                }

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Usage();
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
        }

        public string ReturnPath()
        {
            return Path.Combine(Environment.CurrentDirectory, "PostRetriever.exe");
        }
    }
}