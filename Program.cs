using Microsoft.AnalysisServices.AdomdClient;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace AasClient
{
    class Program
    {
        private const string TENANT_ID = "###";
        private const string CLIENT_ID = "###";
        private const string CLIENT_SECRET = "###";
        private const string DOMAIN = "###";
        private const string SSAS_URL = "###";

        private static string query = "";
        private static int queryCount = 1;
        private static bool printQueryResult = false;

        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Please insert the query to run");
                Console.Write("DAX query: ");
                query = Console.ReadLine();
                Console.WriteLine();
                Console.Write("Please insert an integer how many times to run the query: ");
                queryCount = int.Parse(Console.ReadLine());
                Console.Write("Do you want to print the query result (0 - NO, 1 - YES)?: ");
                printQueryResult = int.Parse(Console.ReadLine()) == 1;

                Action[] taskArray = new Action[queryCount];
                for (int i = 0; i < taskArray.Length; i++)
                {
                    taskArray[i] = () => ReadFromAzureAS();
                }

                Parallel.Invoke(taskArray);

                Console.WriteLine("****Complete***");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Executes query against the AAS.
        /// </summary>
        private static void ReadFromAzureAS()
        {
            using (AdomdConnection connection = new AdomdConnection(ConstructConnectionString()))
            {
                connection.Open();
                ProcessResult(connection);
                connection.Close();
            } // using connection
        }

        /// <summary>
        /// Constructs connection string used to connect to AAS.
        /// </summary>
        /// <returns>Connection string to AAS.</returns>
        private static string ConstructConnectionString()
        {
            return $"Provider=MSOLAP;Data Source=asazure://{SSAS_URL}/<AAS cluster name>;Initial Catalog= <Catalog name>;User ID=app:{CLIENT_ID};Password={CLIENT_SECRET};";
        }

        /// <summary>
        /// Constructs command to query AAS.
        /// </summary>
        /// <param name="connection">Connection to AAS cluster.</param>
        /// <returns>Command to execute against AAS.</returns>
        private static AdomdCommand ConstructCommand(AdomdConnection connection)
        {
            //Create a command, using given connection
            AdomdCommand command = connection.CreateCommand();
            command.CommandText = query;
            return command;
        }

        /// <summary>
        /// Reads from AAS and displays results in the Console.
        /// </summary>
        /// <param name="connection">Connection to AAS cluster.</param>
        private static void ProcessResult(AdomdConnection connection)
        {
            // Create new stopwatch.
            Stopwatch stopwatch = new Stopwatch();

            // Begin timing.
            stopwatch.Start();

            using (AdomdDataReader reader = ConstructCommand(connection).ExecuteReader())
            {
                int count = 0;
                if (printQueryResult)
                {
                    count = ProcessQueryResult(reader);
                }

                // Stop timing.
                stopwatch.Stop();

                // Write result.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Thread ID: {0} \n Time elapsed: {1}ms", Thread.CurrentThread.ManagedThreadId, stopwatch.ElapsedMilliseconds);

                if (printQueryResult)
                {
                    Console.WriteLine("Total number of rows: {0}", count);
                }
            }
        }

        /// <summary>
        /// Prints query result.
        /// </summary>
        private static int ProcessQueryResult(AdomdDataReader reader)
        {
            int count = 0;

            // print field names in the query result
            for (int j = 0; j < reader.FieldCount; ++j)
            {
                Console.Write(reader.GetName(j) + (j == reader.FieldCount - 1 ? "" : ", "));
            }

            Console.WriteLine();

            // print field values in the query result;
            while (reader.Read())
            {
                ++count;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.Write(reader[i] + (i == reader.FieldCount - 1 ? "" : ", "));
                }

                Console.WriteLine();
            }

            return count;
        }
    }
}