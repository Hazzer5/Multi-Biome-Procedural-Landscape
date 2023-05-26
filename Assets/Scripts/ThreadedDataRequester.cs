using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using System.Threading;
using System;

public class ThreadedDataRequester : MonoBehaviour
{
    static ThreadedDataRequester instance;
    Queue<ThreadInfo> DataThreadInfoQueue = new Queue<ThreadInfo>();
    CustomSampler sampler;

    void Awake() {
        instance = FindObjectOfType<ThreadedDataRequester> ();
        sampler = CustomSampler.Create("MyCustomSampler");
    }

    public static void RequestData(Func<object> dataGenerator, Action<object> callback) {
        ThreadStart threadStart = delegate {
            Profiler.BeginThreadProfiling("My threads", "Data Thread");
            instance.DataThread (dataGenerator, callback);
            Profiler.EndThreadProfiling();
        };

        new Thread (threadStart).Start ();
    }

    void DataThread(Func<object> dataGenerator, Action<object> callback) {
        sampler.Begin();
        object data = dataGenerator();
        lock (DataThreadInfoQueue) {
            DataThreadInfoQueue.Enqueue(new ThreadInfo (callback, data));
        }
        sampler.End();
    }


    void Update() {
        lock (DataThreadInfoQueue) {
            if (DataThreadInfoQueue.Count > 0) {
                for (int i = 0; i < DataThreadInfoQueue.Count; i++) {
                    ThreadInfo threadInfo = DataThreadInfoQueue.Dequeue ();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
        }
    }

    
    
    struct ThreadInfo {
        public readonly Action<object> callback;
        public readonly object parameter;

        public ThreadInfo (Action<object> callback, object parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
