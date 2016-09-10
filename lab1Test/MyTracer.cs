using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using System.Xml.Linq;


namespace lab1Test
{
    static class MyTracer
    {

        private static List<FrameList> fList = new List<FrameList>();
        private static FrameList[] frameList = new FrameList[1];
        private static Stopwatch[] arrayStopwatch = new Stopwatch[1];
        private static int counterSW = 0;
        private static double time = 0;
        public static object threadLock = new object();

        public static void StartTrace()
        {
            lock(threadLock)
            { 
                counterSW++;
                Array.Resize<Stopwatch>(ref arrayStopwatch, counterSW);
                arrayStopwatch[counterSW - 1] = Stopwatch.StartNew();

                StackTrace stackTrace = new StackTrace(true);
                StackFrame stackFrame = stackTrace.GetFrame(1);

                frameList[frameList.Length - 1] = new FrameList(stackFrame);
                frameList[frameList.Length - 1].thread = Thread.CurrentThread.ManagedThreadId;
                for (int i = frameList.Length - 2; i >= 0; i--)
                {
                    if (frameList[i].isOpened == true)
                    {
                        frameList[frameList.Length - 1].parentId = i;
                        frameList[i].childFrame[frameList[i].childFrame.Length - 1] = frameList[frameList.Length - 1];
                        Array.Resize<FrameList>(ref frameList[i].childFrame, frameList[i].childFrame.Length + 1);
                        break;
                    }
                }
                Array.Resize<FrameList>(ref frameList, frameList.Length + 1);
            }
        }

        public static void StopTrace()
        {
            lock(threadLock)
            {
                arrayStopwatch[counterSW - 1].Stop();

                frameList = FrameList.sort(frameList);

                time = arrayStopwatch[counterSW - 1].ElapsedMilliseconds;
                Array.Resize<Stopwatch>(ref arrayStopwatch, --counterSW);

                StackTrace st = new StackTrace(true);
                StackFrame sf = st.GetFrame(1);
                for (int i = 0; i < frameList.Length - 1; i++)
                {
                    if (frameList[i].frame.GetMethod().Equals(sf.GetMethod()))
                    {
                        frameList[i].time = time;
                        frameList[i].isOpened = false;
                        break;
                    }
                }
            }
        }

        public static void BuildXml()
        {
            XDocument xmlDoc = new XDocument();
            XElement xmlRoot = new XElement("root");
            XElement xmlThread;
            XAttribute threadIdAttr, threadTimeAttr;
            for(int i = 0; i < frameList.Length - 1; i++)
            {
                if (frameList[i].parentId == -1)
                {
                    xmlThread = new XElement("thread");
                    threadIdAttr = new XAttribute("id", frameList[i].thread);
                    threadTimeAttr = new XAttribute("time", threadTime(frameList[i].thread));
                    xmlThread.Add(threadIdAttr);
                    xmlThread.Add(threadTimeAttr);
                    addNodeXml(frameList[i], ref xmlThread);
                    xmlRoot.Add(xmlThread);
                }
            }
            xmlDoc.Add(xmlRoot);
            xmlDoc.Save("XMLtree.xml");
        }

        public static void addNodeXml(FrameList frame, ref XElement thread)
        {
            XElement xmlMethod = new XElement("method");
            XAttribute methodNameAttr, methodTimeAttr, methodClassAttr, methodParamAttr;
            methodNameAttr = new XAttribute("name", frame.methodName);
            methodTimeAttr = new XAttribute("time", frame.time);
            methodClassAttr = new XAttribute("package", frame.className);
            methodParamAttr = new XAttribute("paramsCount", frame.countParam);
            xmlMethod.Add(methodNameAttr);
            xmlMethod.Add(methodTimeAttr);
            xmlMethod.Add(methodClassAttr);
            xmlMethod.Add(methodParamAttr);
            for (int i = 0; i < frame.childFrame.Length - 1; i++)
            {
                addNodeXml(frame.childFrame[i], ref xmlMethod);
            }
            thread.Add(xmlMethod);
        }

        public static void PrintToConsole()
        {
            for(int i = 0; i < frameList.Length - 1; i++)
            {
                if(frameList[i].parentId == -1)
                {
                    Console.WriteLine("Thread {0}: {1} ms;", frameList[i].thread, threadTime(frameList[i].thread));

                    printTree(frameList[i], 1);
                }
            }
        }

        private static void printTree(FrameList frame, int tab)
        {
            for (int i = 0; i < tab; i++)
                Console.Write("\t");
            Console.WriteLine("Method: {0}({1} param) in {2} class: {3} ms;",
                        frame.methodName, frame.countParam, frame.className, frame.time);
            for (int i = 0; i < frame.childFrame.Length - 1; i++)
            {
                printTree(frame.childFrame[i], tab + 1);
            }
        }

        private static double threadTime(int threadId)
        {
            double time = 0;
            for(int i = 0; i < frameList.Length - 1; i++)
            {
                if((frameList[i].thread == threadId) && (frameList[i].parentId == -1))
                {
                    time += frameList[i].time;
                }
            }
            return time;
        }
    }

    class FrameList
    {
        public bool isOpened = false;
        public string methodName;
        public string className;
        public int countParam = 0;
        public int parentId = -1;
        public double time = 0;
        public int thread;
        public StackFrame frame = new StackFrame();
        public FrameList[] childFrame = new FrameList[1];


        public FrameList(StackFrame newFrame)
        {
            isOpened = true;
            frame = newFrame;
            methodName = frame.GetMethod().Name;
            className = frame.GetMethod().DeclaringType.Name;
            countParam = frame.GetMethod().GetParameters().Length;
        }

        public static FrameList[] sort(FrameList[] arr)
        {
            int temp;
            for(int i = 0; i < arr.Length - 2; i++)
            {
                if(arr[i].thread > arr[i + 1].thread)
                {
                    temp = arr[i].thread;
                    arr[i].thread = arr[i + 1].thread;
                    arr[i + 1].thread = temp;
                }
            }
            return arr;
        }
    }
}
