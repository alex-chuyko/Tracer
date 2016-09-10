using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace lab1Test
{
    class Program
    {
        static void Main(string[] args)
        {
            MyTracer.StartTrace();
            myMethod();
            Thread myThread = new Thread(myMethod2);
            myThread.Start();
            MyTracer.StopTrace();
            myThread.Join();
            MyTracer.PrintToConsole();
            MyTracer.BuildXml();
            Console.Read();
        }

        public static void myMethod()
        {
            MyTracer.StartTrace();
            Thread.Sleep(2);
            MyTracer.StopTrace();
        }

        public static void myMethod2()
        {
            MyTracer.StartTrace();
            Thread.Sleep(3);
            Thread th = new Thread(TestClass.testMethod);
            th.Start();
            MyTracer.StopTrace();
            th.Join();
        }
    }

    static class TestClass
    {

        public static void testMethod()
        {
            MyTracer.StartTrace();
            Thread.Sleep(5);
            oneMethod();
            MyTracer.StopTrace();
        }

        public static void oneMethod()
        {
            MyTracer.StartTrace();
            MyTracer.StopTrace();
            MyTracer.StartTrace();
            int i = 7555341;
            while (i > 1000) { i--; }
            MyTracer.StopTrace();
        }
    }
}
