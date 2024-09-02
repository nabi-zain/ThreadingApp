using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace ThreadingApp
{
    class Program
    {
        private static List<int> globalList = new List<int>();
        private static readonly object lockObject = new object();
        private static volatile bool stopThreads = false;

        static void Main(string[] args)
        {
            Thread oddNumberThread = new Thread(GenerateOddNumbers);
            Thread primeNumberThread = new Thread(GeneratePrimeNumbers);
            oddNumberThread.Start();
            primeNumberThread.Start();

            Thread evenNumberThread = null;

            while (!stopThreads)
            {
                lock (lockObject)
                {
                    if (globalList.Count == 250000 && evenNumberThread == null)
                    {
                        evenNumberThread = new Thread(GenerateEvenNumbers);
                        evenNumberThread.Start();
                    }
                }
            }

            oddNumberThread.Join();
            primeNumberThread.Join();
            if (evenNumberThread != null) evenNumberThread.Join();

            SortAndDisplayResults();
        }

        private static void GenerateOddNumbers()
        {
            Random random = new Random();
            while (!stopThreads)
            {
                int oddNumber = random.Next(1, 1000000) * 2 + 1;
                AddToGlobalList(oddNumber);
            }
        }

        private static void GeneratePrimeNumbers()
        {
            int number = 2;
            while (!stopThreads)
            {
                if (IsPrime(number))
                {
                    AddToGlobalList(-number);
                }
                number++;
            }
        }

        private static void GenerateEvenNumbers()
        {
            int evenNumber = 0;
            while (!stopThreads)
            {
                AddToGlobalList(evenNumber);
                evenNumber += 2;
            }
        }

        private static void AddToGlobalList(int number)
        {
            lock (lockObject)
            {
                if (globalList.Count < 1000000)
                {
                    globalList.Add(number);
                    if (globalList.Count == 1000000)
                    {
                        stopThreads = true;
                    }
                }
            }
        }

        private static bool IsPrime(int number)
        {
            if (number <= 1) return false;
            for (int i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0) return false;
            }
            return true;
        }

        private static void SortAndDisplayResults()
        {
            globalList.Sort();
            int oddCount = globalList.Count(n => n % 2 != 0);
            int evenCount = globalList.Count(n => n % 2 == 0);

            Console.WriteLine($"Odd Count: {oddCount}");
            Console.WriteLine($"Even Count: {evenCount}");

            SerializeToBinary();
            SerializeToXml();
        }

        private static void SerializeToBinary()
        {
            using (FileStream fs = new FileStream("globalList.bin", FileMode.Create))
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                formatter.Serialize(fs, globalList);
            }
        }

        private static void SerializeToXml()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<int>));
            using (FileStream fs = new FileStream("globalList.xml", FileMode.Create))
            {
                serializer.Serialize(fs, globalList);
            }
        }
    }
}

