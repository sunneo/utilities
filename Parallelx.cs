using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities
{
    public class Parallelx
    {
        public enum ParallelForScheduler
        {
            Block = 0,
            Cyclic = 1,
            RuntimeLoadBalance = 2
        }
        
        /// <summary>
        /// Helper method to dispose a list of AsyncTask objects
        /// Call this after parallel operations complete to free resources
        /// </summary>
        /// <param name="tasks">List of tasks to dispose</param>
        public static void DisposeTasks(List<AsyncTask> tasks)
        {
            if (tasks != null)
            {
                foreach (var task in tasks)
                {
                    if (task != null && !task.IsDisposed)
                    {
                        task.Dispose();
                    }
                }
            }
        }
        
        private void WaitForAsyncTasks(List<AsyncTask> tasks, bool wait)
        {
            if (wait)
            {
                for (int i = 0; i < tasks.Count; ++i)
                {
                    tasks[i].Join();
                }
            }
        }
        #region Kernel Algorithms
        public static int Concurrency = Environment.ProcessorCount;
        public List<AsyncTask> RuntimeLoadBalanceFor(int lowerBound, int upperBound, Action<int, CancellationTokenSource, int> action, CancellationTokenSource cancellationSource, bool wait)
        {
            List<AsyncTask> ret = new List<AsyncTask>();
            object locker = new object();
            int sharedIdx = lowerBound;
            int threadCount = Concurrency;
            for (int i = 0; i < threadCount; ++i)
            {
                AsyncTask task = new AsyncTask(() =>
                {
                    int load = lowerBound;
                    while (true)
                    {
                        if (cancellationSource != null && cancellationSource.IsCancellationRequested)
                        {
                            // cancelled
                            break;
                        }
                        // fetch next load
                        lock (locker)
                        {
                            load = sharedIdx;
                            ++sharedIdx;
                        }
                        if (load >= upperBound)
                        {
                            break;
                        }
                        // consume load
                        action(load, cancellationSource, i);
                    }
                });
                task.Start(false);
                ret.Add(task);
            }
            WaitForAsyncTasks(ret, wait);
            return ret;
        }
        private void BlockParitionRunner(int start, int end, Action<int, CancellationTokenSource, int> action, int threadid, CancellationTokenSource cancellationSource = null)
        {
            for (int i = start; i < end; ++i)
            {
                if (cancellationSource != null && cancellationSource.IsCancellationRequested)
                {
                    break;
                }
                action(i, cancellationSource, threadid);
            }
        }
        private void CyclicParitionRunner(int start, int end, int step, Action<int, CancellationTokenSource, int> action, int threadid, CancellationTokenSource cancellationSource = null)
        {
            for (int i = start; i < end; i += step)
            {
                if (cancellationSource != null && cancellationSource.IsCancellationRequested)
                {
                    break;
                }
                action(i, cancellationSource, threadid);
            }
        }
        public List<AsyncTask> CyclicPartitionFor(int lowerBound, int upperBound, Action<int, CancellationTokenSource, int> action, CancellationTokenSource cancellationSource, bool wait)
        {
            int len = upperBound - lowerBound + 1;
            int threadCount = Concurrency;
            int partLen = len / threadCount;
            if (len % threadCount > 0)
            {
                partLen += 1;
            }
            List<AsyncTask> ret = new List<AsyncTask>();
            for (int i = 0; i < threadCount; ++i)
            {
                int start = lowerBound + i;
                int end = upperBound;
                Action<AsyncTask> actionTask = (task) =>
                {
                    int id = (int)task.DynamicFields["ID"];
                    CyclicParitionRunner(start, end, threadCount, action, id, cancellationSource);
                };
                AsyncTask thread = new AsyncTask(actionTask);
                thread.DynamicFields["ID"] = i;
                thread.Start(false);
                ret.Add(thread);
            }
            WaitForAsyncTasks(ret, wait);
            return ret;
        }
        public List<AsyncTask> BlockPartitionFor(int lowerBound, int upperBound, Action<int, CancellationTokenSource, int> action, CancellationTokenSource cancellationSource, bool wait)
        {
            int len = upperBound - lowerBound + 1;
            int threadCount = Concurrency;
            int partLen = len / threadCount;
            if (len % threadCount > 0)
            {
                partLen += 1;
            }
            List<AsyncTask> ret = new List<AsyncTask>();
            for (int i = 0; i < threadCount; ++i)
            {
                int start = lowerBound + i * partLen;
                int end = start + partLen;
                if (end > upperBound)
                {
                    end = upperBound;
                }
                Action<AsyncTask> actionTask = (task) =>
                {
                    int id = (int)task.DynamicFields["ID"];
                    BlockParitionRunner(start, end, action, id, cancellationSource);
                };
                AsyncTask thread = new AsyncTask(actionTask);
                thread.DynamicFields["ID"] = i;
                thread.Start(false);
                ret.Add(thread);
            }
            WaitForAsyncTasks(ret, wait);
            return ret;
        }
        #endregion


        #region LowerBound+UpperBound with Index,Cancellation Wrapper

        public List<AsyncTask> BlockPartitionFor(int lowerBound, int upperBound, Action<int, CancellationTokenSource> action, bool wait = false)
        {
            Action<int, CancellationTokenSource, int> actionProxy = (i, dummy, dummyid) =>
            {
                action(i, dummy);
            };
            return BlockPartitionFor(lowerBound, upperBound, actionProxy, null, wait);
        }

        public List<AsyncTask> CyclicPartitionFor(int lowerBound, int upperBound, Action<int, CancellationTokenSource> action, bool wait = false)
        {
            Action<int, CancellationTokenSource, int> actionProxy = (i, dummy, dummyid) =>
            {
                action(i, dummy);
            };
            return CyclicPartitionFor(lowerBound, upperBound, actionProxy, null, wait);
        }

        #endregion

        public List<AsyncTask> BlockPartitionFor(int lowerBound, int upperBound, Action<int> action, bool wait = false)
        {
            Action<int, CancellationTokenSource, int> actionProxy = (i, dummy, dummyid) =>
            {
                action(i);
            };
            return BlockPartitionFor(lowerBound, upperBound, actionProxy, null, wait);
        }

        public List<AsyncTask> BlockPartitionFor(int upperBound, Action<int> action, bool wait = false)
        {
            return BlockPartitionFor(0, upperBound, action, wait);
        }

        public List<AsyncTask> CyclicPartitionFor(int upperBound, Action<int, CancellationTokenSource> action, bool wait = false)
        {
            return CyclicPartitionFor(0, upperBound, action, wait);
        }
        public List<AsyncTask> CyclicPartitionFor(int upperBound, Action<int> action, bool wait = false)
        {
            return CyclicPartitionFor(0, upperBound, action, wait);
        }
        public List<AsyncTask> CyclicPartitionFor(int lowerBound, int upperBound, Action<int> action, bool wait = false)
        {
            Action<int, CancellationTokenSource, int> actionProxy = (i, dummy, dummyid) =>
            {
                action(i);
            };
            return CyclicPartitionFor(lowerBound, upperBound, actionProxy, null, wait);
        }
        public List<AsyncTask> RuntimeLoadBalanceFor(int lowerBound, int upperBound, Action<int> action, bool wait = false)
        {
            Action<int, CancellationTokenSource, int> actionProxy = (i, dummy, dummyid) =>
            {
                action(i);
            };
            return RuntimeLoadBalanceFor(lowerBound, upperBound, actionProxy, null, wait);
        }
        public List<AsyncTask> RuntimeLoadBalanceFor(int upperBound, Action<int> action, bool wait = false)
        {
            return RuntimeLoadBalanceFor(0, upperBound, action, wait);
        }


        public static List<AsyncTask> For(int lowerbound, int upperBound, Action<int, CancellationTokenSource, int> action, bool wait = false, ParallelForScheduler scheduler = ParallelForScheduler.Block)
        {
            switch (scheduler)
            {
                default:
                case ParallelForScheduler.Block: return new Parallelx().BlockPartitionFor(lowerbound, upperBound, action, new CancellationTokenSource(), wait);
                case ParallelForScheduler.Cyclic: return new Parallelx().CyclicPartitionFor(lowerbound, upperBound, action, new CancellationTokenSource(), wait);
                case ParallelForScheduler.RuntimeLoadBalance: return new Parallelx().RuntimeLoadBalanceFor(lowerbound, upperBound, action, new CancellationTokenSource(), wait);
            }

        }
        public static List<AsyncTask> For(int lowerbound, int upperBound, Action<int, CancellationTokenSource, int> action, CancellationTokenSource cancellationSource, bool wait = false, ParallelForScheduler scheduler = ParallelForScheduler.Block)
        {
            switch (scheduler)
            {
                default:
                case ParallelForScheduler.Block: return new Parallelx().BlockPartitionFor(lowerbound, upperBound, action, cancellationSource, wait);
                case ParallelForScheduler.Cyclic: return new Parallelx().CyclicPartitionFor(lowerbound, upperBound, action, cancellationSource, wait);
                case ParallelForScheduler.RuntimeLoadBalance: return new Parallelx().RuntimeLoadBalanceFor(lowerbound, upperBound, action, cancellationSource, wait);
            }
        }

        public static List<AsyncTask> For(int lowerbound, int upperBound, Action<int> action, bool wait = false, ParallelForScheduler scheduler = ParallelForScheduler.Block)
        {
            switch (scheduler)
            {
                default:
                case ParallelForScheduler.Block: return new Parallelx().BlockPartitionFor(lowerbound, upperBound, action, wait);
                case ParallelForScheduler.Cyclic: return new Parallelx().CyclicPartitionFor(lowerbound, upperBound, action, wait);
                case ParallelForScheduler.RuntimeLoadBalance: return new Parallelx().RuntimeLoadBalanceFor(lowerbound, upperBound, action, wait);
            }
        }

        public static List<AsyncTask> For(int upperBound, Action<int> action, bool wait = false, ParallelForScheduler scheduler = ParallelForScheduler.Block)
        {
            switch (scheduler)
            {
                default:
                case ParallelForScheduler.Block: return new Parallelx().BlockPartitionFor(upperBound, action, wait);
                case ParallelForScheduler.Cyclic: return new Parallelx().CyclicPartitionFor(upperBound, action, wait);
                case ParallelForScheduler.RuntimeLoadBalance: return new Parallelx().RuntimeLoadBalanceFor(upperBound, action, wait);
            }

        }

        public static List<AsyncTask> Foreach<T>(IList<T> upperBound, Action<T> action, bool wait = false, ParallelForScheduler scheduler = ParallelForScheduler.Block)
        {
            Action<int> actionProxy = (i) =>
            {
                action(upperBound[i]);
            };
            switch (scheduler)
            {
                default:
                case ParallelForScheduler.Block: return new Parallelx().BlockPartitionFor(upperBound.Count, actionProxy, wait);
                case ParallelForScheduler.Cyclic: return new Parallelx().CyclicPartitionFor(upperBound.Count, actionProxy, wait);
                case ParallelForScheduler.RuntimeLoadBalance: return new Parallelx().RuntimeLoadBalanceFor(upperBound.Count, actionProxy, wait);
            }
        }
        public static List<AsyncTask> Foreach<T>(IList<T> upperBound, Action<T, int> action, bool wait = false, ParallelForScheduler scheduler = ParallelForScheduler.Block)
        {
            Action<int, CancellationTokenSource, int> actionProxy = (i, dummy, id) =>
            {
                action(upperBound[i], id);
            };
            switch (scheduler)
            {
                default:
                case ParallelForScheduler.Block: return new Parallelx().BlockPartitionFor(0, upperBound.Count, actionProxy, null, wait);
                case ParallelForScheduler.Cyclic: return new Parallelx().CyclicPartitionFor(0, upperBound.Count, actionProxy, null, wait);
                case ParallelForScheduler.RuntimeLoadBalance: return new Parallelx().RuntimeLoadBalanceFor(0, upperBound.Count, actionProxy, null, wait);
            }
        }
        public static T Reduction<T>(IList<T> list, Func<T, T, T> reductionOperation)
        {
            T[] threadStorage = new T[Concurrency];
            Action<T, int> action = (val, processorId) =>
            {
                threadStorage[processorId] = reductionOperation(threadStorage[processorId], val);
            };
            Foreach(list, action, true);
            T ret = threadStorage[0];
            for (int i = 1; i < threadStorage.Length; ++i)
            {
                ret = reductionOperation(ret, threadStorage[i]);
            }
            return ret;
        }
        public static void Main()
        {
            List<double> rands = new List<double>();
            Locker locker = new Locker();
            Random rand = new Random();
            for (int i = 0; i < 10000000; ++i)
            {
                rands.Add(rand.NextDouble());
            }
            DateTime dtStart = DateTime.Now;
            double ret = 0;
            for (int i = 0; i < rands.Count; ++i)
            {
                ret += rands[i];
            }
            Console.WriteLine("Regular Reduction takes {0} sec, result={1}", DateTime.Now.Subtract(dtStart).TotalSeconds, ret);

            ret = 0;
            dtStart = DateTime.Now;
            ret = Parallelx.Reduction(rands, (a, b) => a + b);
            Console.WriteLine("Parallelx Reduction takes {0} sec, result={1}", DateTime.Now.Subtract(dtStart).TotalSeconds, ret);
            Console.Read();
        }
    }
}
