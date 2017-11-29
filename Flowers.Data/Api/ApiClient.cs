using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Flowers.Api
{
    public interface IApiClient
    {
        Dictionary<string, string> DefaultQueryParameters { get; set; }

        Task<ApiResponse<T>> Execute<T>(ApiRequest<T> request);
    }

    public class ApiClient : IApiClient
    {
        private HttpClient _httpClient;
        private Dictionary<string, string> _defaultQueryParameters;

        public Dictionary<string, string> DefaultQueryParameters 
        {
            get => _defaultQueryParameters;
            set => _defaultQueryParameters = value;
        }

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _defaultQueryParameters = new Dictionary<string, string>();
        }

        public async Task<ApiResponse<T>> Execute<T>(ApiRequest<T> request)
        {
            try
            {
                request.HttpRequest.RequestUri = AppendDefaultQueryParameters(request.HttpRequest.RequestUri);
                var httpResponse = await _httpClient.SendAsync(request.HttpRequest).ConfigureAwait(false);

                if (httpResponse.IsSuccessStatusCode)
                {
                    var deserializedRespone = await request.Parser(httpResponse).ConfigureAwait(false);

                    return new ApiResponse<T>(deserializedRespone, httpResponse);
                }
                else
                {
                    throw new ApiResponseException { HttpResponse = httpResponse };
                }
            }
            catch (ApiParsingException apiParsingException)
            {
                throw apiParsingException;
            }
            catch (Exception exception)
            {
                throw new ApiException(exception.Message, exception);
            }
        }

        private Uri AppendDefaultQueryParameters(Uri uri)
        {
            if (!_defaultQueryParameters.Any())
            {
                return uri;
            }

            var newUriString = uri.OriginalString + (uri.OriginalString.Contains("?") ? "&" : "?");

            foreach (var keyValuePair in _defaultQueryParameters)
            {
                newUriString += $"{keyValuePair.Key}={keyValuePair.Value}&";   
            }

            newUriString = newUriString.Trim('&');

            return new Uri(newUriString, UriKind.RelativeOrAbsolute);
        }
    }

    public class ApiRequest<T>
    {
        public Func<HttpResponseMessage, Task<T>> Parser;
        public Func<HttpResponseMessage, ApiResponseException> ExceptionParser = responseMessage => new ApiResponseException { HttpResponse = responseMessage };

        public HttpRequestMessage HttpRequest { get; protected set; }

        public ApiRequest()
        {
            Parser = ParseHttpResponse;
        }

        private async Task<T> ParseHttpResponse(HttpResponseMessage httpResponse)
        {
            var errors = new List<string>();

            var responseString = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            var deserializedRespone = JsonConvert.DeserializeObject<T>(responseString,
                                                                       new JsonSerializerSettings
                                                                       {
                                                                           Error = (sender, args) =>
                                                                           {
                                                                               errors.Add(args.ErrorContext.Error.Message);
                                                                               args.ErrorContext.Handled = true;
                                                                           }
                                                                       });

            if (errors.Any())
            {
                throw new ApiParsingException { Errors = new ReadOnlyCollection<string>(errors) };
            }
            else
            {
                return deserializedRespone;
            }
        }
    }

    public class ApiResponse<T>
    {
        public T DeserializedResponse { get; set; }
        public HttpResponseMessage HttpResponse { get; set; }

        public ApiResponse(T deserializedResponse, HttpResponseMessage httpResponse)
        {
            DeserializedResponse = deserializedResponse;
            HttpResponse = httpResponse;
        }
    }

    public class ApiParsingException : ApiResponseException
    {
        public ReadOnlyCollection<String> Errors { get; set; }
    }

    public class ApiResponseException : ApiException
    {
        public HttpResponseMessage HttpResponse { get; set; }
    }

    public class ApiException : Exception
    {
        public ApiException() { }

        public ApiException(string message, Exception inner) : base(message, inner) { }
    }

    public sealed class Void { }
}
