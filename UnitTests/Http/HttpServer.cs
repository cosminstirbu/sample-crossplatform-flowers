using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests.Http
{
    public class HttpServer : HttpMessageHandler
    {
        public Queue<RequestWithTask> RequestsQueue { get; set; } = new Queue<RequestWithTask>();

        public Queue<ResponseWithException> ResponsesQueue { get; } = new Queue<ResponseWithException>();

        public class RequestWithTask
        {
            public HttpRequestMessage HttpRequest { get; set; }
            public TaskCompletionSource<HttpResponseMessage> TaskCompletionSource { get; set; }
        }

        public class ResponseWithException
        {
            public HttpResponseMessage HttpResponse { get; set; }
            public Exception Exception { get; set; }

            public static ResponseWithException OK()
            {
                return new ResponseWithException { HttpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK) };
            }

            public static ResponseWithException WithException(string message = "")
            {
                return new ResponseWithException { Exception = new Exception(message) };
            }
        }

        public void ConsumeRequests()
        {
            while (ResponsesQueue.Count != 0 && RequestsQueue.Count != 0)
            {
                var response = ResponsesQueue.Dequeue();
                var request = RequestsQueue.Dequeue();

                if (response.Exception != null)
                {
                    request.TaskCompletionSource.SetException(response.Exception);
                }
                else
                {
                    request.TaskCompletionSource.SetResult(response.HttpResponse);
                }
            }
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var taskCompletionSource = new TaskCompletionSource<HttpResponseMessage>();

            RequestsQueue.Enqueue(new RequestWithTask { HttpRequest = request, TaskCompletionSource = taskCompletionSource });

            return taskCompletionSource.Task;
        }
    }
}
