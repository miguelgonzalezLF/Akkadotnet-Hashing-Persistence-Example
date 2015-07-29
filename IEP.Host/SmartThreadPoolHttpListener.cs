using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Amib.Threading;
using Gurock.SmartInspect;
using ServiceStack.Common.Web;
using ServiceStack.Text;
using ServiceStack.WebHost.Endpoints;

namespace IEP.Host
{
    public abstract class SmartThreadPoolHttpListener : AppHostHttpListenerBase
    {
        private readonly AutoResetEvent _listenForNextRequest = new AutoResetEvent(false);
        private readonly SmartThreadPool _threadPoolManager;
        private const int IdleTimeout = 300;

        protected SmartThreadPoolHttpListener(int poolSize = 500)
        {
            _threadPoolManager = new SmartThreadPool(IdleTimeout, poolSize);
        }

        protected SmartThreadPoolHttpListener(string serviceName, params Assembly[] assembliesWithServices)
            : this(serviceName, 500, assembliesWithServices) { }

        protected SmartThreadPoolHttpListener(string serviceName, int poolSize, params Assembly[] assembliesWithServices)
            : base(serviceName, assembliesWithServices)
        {
            _threadPoolManager = new SmartThreadPool(IdleTimeout, poolSize);
        }

        protected SmartThreadPoolHttpListener(string serviceName, string handlerPath, params Assembly[] assembliesWithServices)
            : this(serviceName, handlerPath, 500, assembliesWithServices) { }

        protected SmartThreadPoolHttpListener(string serviceName, string handlerPath, int poolSize, params Assembly[] assembliesWithServices)
            : base(serviceName, handlerPath, assembliesWithServices)
        {
            _threadPoolManager = new SmartThreadPool(IdleTimeout, poolSize);
        }

        private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (disposed) return;

            lock (this)
            {
                if (disposed) return;

                if (disposing)
                    _threadPoolManager.Dispose();

                // new shared cleanup logic
                disposed = true;

                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Starts the Web Service
        /// </summary>
        /// <param name="urlBase">
        /// A Uri that acts as the base that the server is listening on.
        /// Format should be: http://127.0.0.1:8080/ or http://127.0.0.1:8080/somevirtual/
        /// Note: the trailing slash is required! For more info see the
        /// HttpListener.Prefixes property on MSDN.
        /// </param>
        public override void Start(string urlBase)
        {
            if (IsStarted)
                return;

            if (Listener == null)
                Listener = new HttpListener();

            Listener.Prefixes.Add(urlBase);

            IsStarted = true;
            Listener.Start();

            ThreadPool.QueueUserWorkItem(Listen);
        }

        // Loop here to begin processing of new requests.
        private void Listen(object state)
        {
            while (Listener.IsListening)
            {
                if (Listener == null) return;

                try
                {
                    Listener.BeginGetContext(ListenerCallback, Listener);
                    _listenForNextRequest.WaitOne();
                }
                catch (Exception ex)
                {
                    SiAuto.Main.LogException("Listen()", ex);
                    return;
                }
                if (Listener == null) return;
            }
        }

        // Handle the processing of a request in here.
        private void ListenerCallback(IAsyncResult asyncResult)
        {
            var listener = asyncResult.AsyncState as HttpListener;
            HttpListenerContext context;

            if (listener == null) return;
            var isListening = listener.IsListening;

            try
            {
                if (!isListening)
                {
                    return;
                }
                // The EndGetContext() method, as with all Begin/End asynchronous methods in the .NET Framework,
                // blocks until there is a request to be processed or some type of data is available.
                context = listener.EndGetContext(asyncResult);
            }
            catch (Exception ex)
            {
                // You will get an exception when httpListener.Stop() is called
                // because there will be a thread stopped waiting on the .EndGetContext()
                // method, and again, that is just the way most Begin/End asynchronous
                // methods of the .NET Framework work.

                //string errMsg = ex + ": " + isListening;
                //_log.Warn(errMsg);

                SiAuto.Main.LogException(ex);
                return;
            }
            finally
            {
                // Once we know we have a request (or exception), we signal the other thread
                // so that it calls the BeginGetContext() (or possibly exits if we're not
                // listening any more) method to start handling the next incoming request
                // while we continue to process this request on a different thread.
                _listenForNextRequest.Set();
            }

            //_log.InfoFormat("{0} Request : {1}", context.Request.UserHostAddress, context.Request.RawUrl);

            RaiseReceiveWebRequest(context);

            _threadPoolManager.QueueWorkItem(() =>
            {
                try
                {
                    ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    var error = string.Format("Error this.ProcessRequest(context): [{0}]: {1}", ex.GetType().Name, ex.Message);
                    SiAuto.Main.LogError(error);

                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("{");
                        sb.AppendLine("\"ResponseStatus\":{");
                        sb.AppendFormat(" \"ErrorCode\":{0},\n", ex.GetType().Name.EncodeJson());
                        sb.AppendFormat(" \"Message\":{0},\n", ex.Message.EncodeJson());
                        sb.AppendFormat(" \"StackTrace\":{0}\n", ex.StackTrace.EncodeJson());
                        sb.AppendLine("}");
                        sb.AppendLine("}");

                        context.Response.StatusCode = 500;
                        context.Response.ContentType = ContentType.Json;
                        byte[] sbBytes = sb.ToString().ToUtf8Bytes();
                        context.Response.OutputStream.Write(sbBytes, 0, sbBytes.Length);
                        context.Response.Close();
                    }
                    catch (Exception errorEx)
                    {
                        error = string.Format("Error this.ProcessRequest(context)(Exception while writing error to the response): [{0}]: {1}",
                            errorEx.GetType().Name, errorEx.Message);
                        SiAuto.Main.LogError(error);
                    }
                }
            });
        }
    }
}