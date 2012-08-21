using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LinkSpider3.Process2;
using LinkSpider3.Process2.Data;
using LinkSpider3.Process2.Extensions;
using LinkSpider3.Process2.Persistence;

namespace LinkSpider3
{
    class Program
    {

        internal class TaskState
        {
            public CollectorPool Pool;
            public VisitedUrlHistory History;
            public VisitedDomainHistory DomainHistory;
            public TldParser TldParser;
            public RobotService Robots;
            public Repository Repository;
            public Func<CollectorPool, VisitedUrlHistory, VisitedDomainHistory, string> PoolManagerHandler;
            public Action<CollectorProcessorEventArgs> ProgressHandler;
            public CancellationToken CancelToken;
            //public CountdownEvent Countdown;
            public int CollectorID;
        }

        internal class CollectorProcessorEventArgs
            : EventArgs
        {
            public string Message { get; internal set; }
            public int CollectorID { get; internal set; }
        }



        //static ManualResetEvent allDone = new ManualResetEvent(false);
        const int WORK_AREA_TOP = 3;

        static void Main(string[] args)
        {
            // arguments
            // --help
            // --provider=redis|mysql|sqlserver
            // --server=hostname
            // --port=
            // --database=
            // --uid=
            // --pwd=
            // --parallel-count
            
            int parallelCount = 15;
            string provider = "redis";
            string server = "127.0.0.1";
            string port = "6379";

            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            PrintAndClearHeaderArea();

            Console.WriteLine("Buffer width: " + Console.BufferWidth);
            Console.WriteLine("Buffer height: " + Console.BufferHeight);

            IPersistence persistence =
                PersistenceFactory.GetPersistence(provider, 
                    new Dictionary<string, string>
                    {
                        { "server", server },
                        { "port", port }
                    });

            Repository repository = new Repository(persistence);

            RobotService robots = new RobotService();
            CollectorPool pool;
            VisitedUrlHistory history;
            VisitedDomainHistory domainHistory = new VisitedDomainHistory();


            int poolCount = 0;
            Console.Write("Loading pool...");
            repository.Load(out pool);
            poolCount = pool.Count;
            Console.WriteLine("done. Found " + poolCount);
            pool.Store("jubacs.somee.com");

            Console.Write("Loading link data...");
            repository.LoadLinks();
            Console.WriteLine("done. Found " + repository.Links.Count);

            Console.Write("Loading history...");
            repository.Load(out history, DateTime.Today);
            Console.WriteLine("done");

            Console.Write("Loading TLD parser...");
            TldParser tldParser = new TldParser();
            Console.WriteLine("done");
            

            Thread.Sleep(10000);
            PrintAndClearHeaderArea();

            //TestRun1(WORK_AREA_TOP, parallelCount, repository, robots, pool, history, tldParser);

            //SynchronizationContext context = new SynchronizationContext();
            //TaskScheduler scheduler = TaskScheduler.FromCurrentSynchronizationContext();

            DateTime elapsed = DateTime.Now;
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken cancelToken = cancelTokenSource.Token;
            //CountdownEvent countdown = new CountdownEvent(parallelCount);

            Task[] tasks = new Task[parallelCount];
            for (int i = 0; i < parallelCount; i++)
            {
                TaskState state = new TaskState();
                state.Pool = pool;
                state.History = history;
                state.DomainHistory = domainHistory;
                state.TldParser = tldParser;
                state.Robots = robots;
                state.Repository = repository;
                state.PoolManagerHandler = PoolManager;
                state.ProgressHandler = CollectorProgress;
                state.CancelToken = cancelToken;
                //state.Countdown = countdown;
                state.CollectorID = i;

                tasks[i] = new Task(CollectorProcessor, state, cancelToken);
                //tasks[i].Start(scheduler);
                tasks[i].Start();
            }


            







            //allDone.WaitOne();
            WriteXY(0, WORK_AREA_TOP + parallelCount + 2, "press [ENTER] to quit");
            Console.Read();

            cancelTokenSource.Cancel(false);
            try
            {
                Task.WaitAll(tasks, cancelToken);
                //countdown.Wait(cancelToken);
            }
            catch
            {
                Console.WriteLine("Collectors cancelled.");
            }
            Thread.Sleep(3000);
           // countdown.Dispose();


            PrintAndClearHeaderArea();
            Console.WriteLine("Closing database...");
            ((IDisposable)persistence).Dispose();

            Thread.Sleep(3000);


            int total = 0;
            Array.ForEach(repository.Links.ToArray(), ld => { total += ld.Value.ChildLinks.Count; });
            Console.WriteLine("Pool count: before = {0} now = {1}", poolCount, pool.Count);
            Console.WriteLine("Links crawled: {0} in {1} seconds ({2}/sec)",
                repository.Links.Count + total, (DateTime.Now - elapsed).Seconds, 
                (repository.Links.Count + total) / (DateTime.Now - elapsed).Seconds);
            Thread.Sleep(20000);

            Environment.Exit(0);
        }

        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            // Save all objects to persistence
            // allDone.Set();
        }

        static void CollectorProgress(CollectorProcessorEventArgs e)
        {
            WriteXY(0, WORK_AREA_TOP + e.CollectorID, "{0} - {1}", e.CollectorID, e.Message);
        }

        static void CollectorProcessor(object s)
        {
            TaskState state = (TaskState)s;

            CollectorProcessorEventArgs progress = new CollectorProcessorEventArgs();
            progress.CollectorID = state.CollectorID;

            string link = state.PoolManagerHandler(state.Pool, state.History, state.DomainHistory);
            while (!link.IsNullOrEmpty() && !state.CancelToken.IsCancellationRequested)
            {
                ManualResetEvent done = new ManualResetEvent(false);
                Dictionary<string, object> properties = new Dictionary<string, object>();
                properties.Add("TldParser", state.TldParser);
                properties.Add("State", done);


                Uri uri = link.ToUri();
                progress.Message = string.Format("{0} (fetching)", uri);
                state.ProgressHandler(progress);

                WebDownloader web = new WebDownloader(uri, properties,
                    ea =>
                    {
                        progress.Message = string.Format(
                            "{0} [{1}] ({2})",
                            uri.ToUriShort(), ea.Status,
                            (ea.Exception.IsNull() ?
                                "responded: " + ea.Stream.Length + " bytes" :
                                "exception: " + ea.Exception.Message));
                        state.ProgressHandler(progress);
                        Thread.Sleep(2000);

                        if (ea.Stream.IsNull())
                        {
                            Thread.Sleep(5000);
                        }
                        else
                        {
                            HtmlProcessor processor = new HtmlProcessor(
                                uri.ToString(), ea.Stream,
                                ((TldParser)ea.Properties["TldParser"]));


                            progress.Message = string.Format(
                                "{0} (found={1})", uri, processor.Links.Count);
                            state.ProgressHandler(progress);
                            Thread.Sleep(2000);

                            int pushedLinks = 0;
                            int linkCounter = 1;
                            processor.Links.ForEach(l =>
                            {
                                progress.Message = string.Format(
                                    "{0} (processing={1} of {2})",
                                    uri,
                                    linkCounter,
                                    processor.Links.Count);
                                state.ProgressHandler(progress);

                                //if (state.Robots.IsAllowed(string.Empty, l.Href.ToUri()))
                                //{
                                    ++pushedLinks;
                                    state.Pool.Store(l.Href);

                                    state.Repository.SaveLink(
                                        uri.ToString(), l.Href, string.Empty, l);
                                    state.Repository.SaveLink(
                                        l.Href, string.Empty, uri.ToString(), l);
                                    state.History.Add(uri.ToString());
                                //}

                                ++linkCounter;
                            });

                            progress.Message = string.Format("{0} [DONE]", uri);
                            state.ProgressHandler(progress);

                            ea.Stream.Close();
                        }

                        ((ManualResetEvent)ea.Properties["State"]).Set();
                    });

                web.Download();
                done.WaitOne();


                // Fetch next link
                link = state.PoolManagerHandler(state.Pool, state.History, state.DomainHistory);
            }

            //state.Countdown.Signal();
        }

        static string PoolManager(
            CollectorPool pool, 
            VisitedUrlHistory history,
            VisitedDomainHistory domainHistory)
        {
            string link = pool.Next();
            while (history.ContainsUrl(link.ToUri().ToString()))
            {
                link = pool.Next();
            }

            // Recursively check if host is still processed
            if (domainHistory.ContainsHost(link.ToUri().ToString()))
            {
                // Store the link back to pool
                pool.Store(link.ToUri().ToString());
                return PoolManager(pool, history, domainHistory);
            }
            else
            {
                // Add to history
                history.Add(link.ToUri().ToString());
                domainHistory.Add(link.ToUri().ToString());

                return link;
            }
        }




        #region Purgatory
        private static void TestRun1(int WORK_AREA_TOP, int parallelCount, Repository repository, RobotService robots, CollectorPool pool, VisitedUrlHistory history, TldParser tldParser)
        {
            CreateCollectorPool(pool, history, parallelCount).ForEach(
                c =>
                {
                    //ManualResetEvent done = new ManualResetEvent(false);
                    Dictionary<string, object> properties = new Dictionary<string, object>();
                    properties.Add("TldParser", tldParser);


                    Uri uri = c.Link.ToUri();
                    WriteXY(0, WORK_AREA_TOP + c.SeqNo, "{0} (fetching)", uri.ToUriShort());

                    WebDownloader web = new WebDownloader(uri, properties,
                        ea =>
                        {
                            WriteXY(0, WORK_AREA_TOP + c.SeqNo,
                                "{0} [{1}] ({2})",
                                uri.ToUriShort(), ea.Status,
                                (ea.Exception.IsNull() ?
                                    "responded: " + ea.Stream.Length + " bytes" :
                                    "exception: " + ea.Exception.Message));

                            if (ea.Stream.IsNull())
                            {
                                Thread.Sleep(5000);
                            }
                            else
                            {
                                HtmlProcessor processor = new HtmlProcessor(
                                    uri.ToString(), ea.Stream,
                                    ((TldParser)ea.Properties["TldParser"]));

                                WriteXY(0, WORK_AREA_TOP + c.SeqNo,
                                    "{0} (found={1})", uri.ToUriShort(), processor.Links.Count);

                                int pushedLinks = 0;
                                int linkCounter = 1;
                                processor.Links.ForEach(l =>
                                {
                                    WriteXY(0, WORK_AREA_TOP + c.SeqNo,
                                        "{0} (found={1}, prc={2} {3} ({4}))",
                                        uri.ToUriShort(),
                                        processor.Links.Count,
                                        (l.Domain.Length > 10 ? l.Domain.Substring(0, 10) : l.Domain),
                                        l.Tld,
                                        linkCounter);

                                    if (robots.IsAllowed(string.Empty, l.Href.ToUri()))
                                    {
                                        ++pushedLinks;
                                        pool.Store(l.Href);

                                        repository.SaveLink(uri.ToString(), l.Href, string.Empty, l);
                                        repository.SaveLink(l.Href, string.Empty, uri.ToString(), l);
                                        history.Add(uri.ToString());
                                    }

                                    ++linkCounter;
                                });

                                WriteXY(0, WORK_AREA_TOP + c.SeqNo,
                                    "{0} (found={1}, added={2} links) [DONE]",
                                    uri.ToUriShort(), processor.Links.Count, pushedLinks);

                                ea.Stream.Close();
                            }

                            //((ManualResetEvent)ea.Properties["State"]).Set();
                        });

                    web.Download();
                    //done.WaitOne();
                });
        }

        static List<Collector> CreateCollectorPool(
            CollectorPool pool,
            VisitedUrlHistory history,
            int parallelCount)
        {
            List<Collector> collectors = new List<Collector>(parallelCount);
            int i = 0;
            while (i < parallelCount)
            {
                string link = pool.Next();

                if (!history.ContainsUrl(link.ToUri().ToString()))
                {
                    collectors.Add(new Collector
                    {
                        SeqNo = i,
                        Link = link
                    });

                    history.Add(link.ToUri().ToString());

                    ++i;
                }
            }

            return collectors;
        }

        private class Collector
        {
            public int SeqNo;
            public string Link;
        }
        #endregion

        #region Helpers
        private static void PrintAndClearHeaderArea()
        {
            Console.Clear();
            Console.WriteLine(new string('=', Console.BufferWidth - 1));
            Console.WriteLine("{0} v{1}", ApplicationInfo.Title, ApplicationInfo.Version.ToString());
            Console.WriteLine(new string('=', Console.BufferWidth - 1));
        }

        private static void WriteXY(int x, int y, string format, params object[] args)
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(x, y);
            string msg = string.Format(format, args);
            Console.Write(msg.PadRight(Console.BufferWidth - 1, ' '));
        }
        #endregion
    }
}
