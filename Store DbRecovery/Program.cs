using ConsoleTableExt;
using CsvHelper;
using Newtonsoft.Json;
using Store_DbRecovery.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Store_DbRecovery
{
    public class Program
    {
        static SqlConnection sqlConn;

        static void Main(string[] args)
        {
            string debugFile = Path.Combine($"debug.{DateTime.Now:yyyyMMddTHHmmss}.log");
            TextWriterTraceListener[] listeners = new TextWriterTraceListener[] {
                new TextWriterTraceListener(debugFile),
            };

            Debug.Listeners.AddRange(listeners);
            Debug.AutoFlush = true;

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (args.Length == 0)
            {
                Console.WriteLine("Input File (.csv|.json) Path!");
            }
            else
            {
                string fP = args[0];
                if (File.Exists(args[0]))
                {
                    IEnumerable<Backup> data;
                    try
                    {
                        if (Path.GetExtension(fP).Equals(".json", StringComparison.InvariantCultureIgnoreCase))
                        {
                            data = JsonConvert.DeserializeObject<IEnumerable<Backup>>(File.ReadAllText(args[0])).OrderByDescending(x => x.CreatedAt);
                        }
                        else if (Path.GetExtension(fP).Equals(".csv", StringComparison.InvariantCultureIgnoreCase))
                        {
                            using (var reader = new StreamReader(fP))
                            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                            {
                                data = csv.GetRecords<Backup>().OrderByDescending(x => x.CreatedAt).ToList();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid File Extension!");
                            Console.ReadKey();
                            return;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Invalid File Data!");
                        Console.ReadKey();
                        return;
                    }
                    string connStr;
                    do
                    {
                        Console.Clear();
                        Console.WriteLine("Enter the following required information:");
                        Console.Write("\tServer: ");
                        string server = Console.ReadLine();
                        Console.Write("\tUsername: ");
                        string username = Console.ReadLine();
                        Console.Write("\tPassword: ");
                        string password = Console.ReadLine();
                        connStr = $"Data Source={server};User ID={username};Password={password};Encrypt=False;Connection Timeout=5;";
                        try
                        {
                            sqlConn = new SqlConnection(connStr);
                            sqlConn.Open();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                    while (sqlConn.State != System.Data.ConnectionState.Open);
                    var tableData = data.ConvertToTableData();
                    int select = -1;
                    do
                    {
                        Console.Clear();
                        Console.WriteLine($"Server: {sqlConn.DataSource}\n");
                        ConsoleTableBuilder.From(tableData).WithTitle("Backup History").WithColumn("#", "Name", "Type", "Created At", "Desc").ExportAndWriteLine();
                        Console.Write("\nChoose # to recovery: ");
                        try
                        {
                            select = (int)uint.Parse(Console.ReadLine());
                        }
                        catch
                        {
                            select = -1;
                        }
                        if (!(select >= 1 && select <= data.Count()))
                            select = -1;
                    } while (select == -1);
                    select -= 1;
                    DateTime curr = data.ElementAt(select).CreatedAt;
                    List<Backup> result = new List<Backup>();
                    foreach (var b in data)
                    {
                        if (curr >= b.CreatedAt)
                        {
                            result.Add(b);
                            if (b.Type.Equals("FULL", StringComparison.InvariantCultureIgnoreCase))
                                break;
                        }
                    }
                    result = result.OrderBy(x => x.CreatedAt).ToList();
                    bool isValid = result.IsValidBackup(sqlConn);
                    if (isValid && result.Restore(sqlConn))
                    {
                        Console.WriteLine("Restore Complete!");
                    }
                    else
                    {
                        Console.WriteLine("Restore Fail!");
                        new Task(() => { Process.Start(new ProcessStartInfo(debugFile) { UseShellExecute = true }); }).RunSynchronously();
                    }
                }
            }
            Console.Write("Press Enter To Exit!");
            Console.ReadLine();
        }
    }
}
