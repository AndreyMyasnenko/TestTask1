using System;

namespace TransactionManager
{
    static class Program
    {
        private const string EXIT_COMMAND = "exit";

        static void Main(string[] args)
        {
            TransactionProcessor transactionProcessor = new TransactionProcessor();

            while (true)
            {
                string input = Console.ReadLine().Trim();

                if (string.Equals(input, EXIT_COMMAND, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                try
                {
                    var result = transactionProcessor.Process(input);

                    Console.Write(result.Message);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Произошла непредвиденная ошибка");
                    LogError(e);
                }
            }
        }

        private static void LogError(Exception e)
        {
            //TODO: log exception proper way

            Console.WriteLine(e.Message);
        }
    }
}
