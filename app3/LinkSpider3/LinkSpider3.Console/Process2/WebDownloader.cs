using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

using LinkSpider3.Process2.Extensions;

namespace LinkSpider3.Process2
{
    public class WebDownloader
    {
        internal class RequestState
        {
            const int BUFFER_SIZE = 1024;
            public HttpWebRequest Request;
            public HttpWebResponse Response;
            public Stream StreamResponse;
            public Exception Exception;
            public Action<WebDownloaderEventArgs> Completed;
            public Uri OriginalUri;
            public bool IsTimeout;
            public IDictionary<string, object> Properties;
            public HttpStatusCode Status;

            public RequestState()
            {
                Request = null;
                Response = null;
                StreamResponse = null;
                IsTimeout = false;
            }
        }


        public class WebDownloaderEventArgs
            : EventArgs
        {
            public Stream Stream { get; internal set; }
            public Exception Exception { get; internal set; }
            public Uri ResponseUri { get; internal set; }
            public bool IsTimeout { get; internal set; }
            public IDictionary<string, object> Properties { get; internal set; }
            public HttpStatusCode Status { get; internal set; }
        }




        //public static ManualResetEvent AllDone = new ManualResetEvent(false);
        const int BUFFER_SIZE = 1024;
        const int DEFAULT_TIMEOUT = 2 * 60 * 1000; // 2 mins

        Uri uri;
        Action<WebDownloaderEventArgs> completed;
        IDictionary<string, object> properties;

        public WebDownloader(
            Uri uri, 
            IDictionary<string, object> properties,
            Action<WebDownloaderEventArgs> completed)
        {
            this.uri = uri;
            this.completed = completed;
            this.properties = properties;
        }


        public void Download()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.uri);
                request.Timeout = 500; // Wait for .5 sec only

                RequestState state = new RequestState();
                state.Request = request;
                state.Completed = this.completed;
                state.OriginalUri = this.uri;
                state.Properties = this.properties;

                IAsyncResult result = (IAsyncResult)request.BeginGetResponse(
                    new AsyncCallback(ResponseCallback), state);

                ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle,
                    new WaitOrTimerCallback(TimeoutCallback), state,
                    DEFAULT_TIMEOUT, true);
            }
            catch (Exception ex)
            {
                completed(new WebDownloaderEventArgs
                {
                    Exception = ex,
                    Stream = null,
                    ResponseUri = this.uri,
                    IsTimeout = false,
                    Properties = this.properties,
                    Status = HttpStatusCode.Unused 
                });
            }
        }

        private static void ResponseCallback(IAsyncResult result)
        {
            RequestState state = (RequestState)result.AsyncState;

            try
            {
                HttpWebRequest request = state.Request;
                state.Response = (HttpWebResponse)request.EndGetResponse(result);
                state.Status = state.Response.StatusCode;

                Stream s = state.Response.GetResponseStream();

                MemoryStream ms = new MemoryStream();
                s.CopyTo(ms);
                s.Close();
                ms.Seek(0, SeekOrigin.Begin);

                state.StreamResponse = ms;
            }
            catch (Exception ex)
            {
                state.Exception = ex;
                state.StreamResponse = null;
            }
            finally
            {
                if (!state.Response.IsNull())
                    state.Response.Close();

                state.Completed(new WebDownloaderEventArgs
                {
                    Exception = state.Exception,
                    Stream = state.StreamResponse,
                    ResponseUri = (state.Response.IsNull() ? state.OriginalUri : state.Response.ResponseUri),
                    IsTimeout = state.IsTimeout,
                    Properties = state.Properties,
                    Status = state.Status
                });
            }
        }

        private static void TimeoutCallback(object state, bool timedOut)
        {
            if (timedOut)
            {
                RequestState rs = state as RequestState;
                if (!rs.Request.IsNull())
                {
                    rs.Request.Abort();
                    rs.IsTimeout = true;
                }
            }
        }


    }
}
