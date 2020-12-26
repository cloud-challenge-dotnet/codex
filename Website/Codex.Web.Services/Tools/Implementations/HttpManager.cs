using Codex.Models.Exceptions;
using Codex.Web.Services.Models;
using Codex.Web.Services.Tools.Interfaces;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Codex.Web.Services.Tools.Implementations
{
    public class HttpManager : IHttpManager
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IApplicationData _applicationData;

        public HttpManager(
            HttpClient httpClient,
            IApplicationData applicationData,
            NavigationManager navigationManager
        )
        {
            _httpClient = httpClient;
            _applicationData = applicationData;
            _navigationManager = navigationManager;

            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                IgnoreNullValues = true
            };
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            _jsonSerializerOptions.Converters.Add(new ObjectIdConverter());
        }

        public async Task<T> GetAsync<T>(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return await SendRequestAsync<T>(request);
        }

        public async Task PostAsync(string uri, object value)
        {
            var request = CreateRequest(HttpMethod.Post, uri, value);
            await SendRequestAsync(request);
        }

        public async Task<T> PostAsync<T>(string uri, object value)
        {
            var request = CreateRequest(HttpMethod.Post, uri, value);
            return await SendRequestAsync<T>(request);
        }

        public async Task PutAsync(string uri, object value)
        {
            var request = CreateRequest(HttpMethod.Put, uri, value);
            await SendRequestAsync(request);
        }

        public async Task<T> PutAsync<T>(string uri, object value)
        {
            var request = CreateRequest(HttpMethod.Put, uri, value);
            return await SendRequestAsync<T>(request);
        }

        public async Task DeleteAsync(string uri)
        {
            var request = CreateRequest(HttpMethod.Delete, uri);
            await SendRequestAsync(request);
        }

        public async Task<T> DeleteAsync<T>(string uri)
        {
            var request = CreateRequest(HttpMethod.Delete, uri);
            return await SendRequestAsync<T>(request);
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, string uri, object? value = null)
        {
            var request = new HttpRequestMessage(method, uri);
            if (value != null)
                request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
            return request;
        }

        private async Task SendRequestAsync(HttpRequestMessage request)
        {
            AddAuthHeader(request);

            // send request
            using var response = await _httpClient.SendAsync(request);

            await HandleErrorsAsync(response);
        }

        private async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
        {
            AddAuthHeader(request);

            using var response = await _httpClient.SendAsync(request);

            await HandleErrorsAsync(response);
            
            return await response.Content.ReadFromJsonAsync<T>(_jsonSerializerOptions)
                ?? throw new TechnicalException("Invalid server response");
        }

        private void AddAuthHeader(HttpRequestMessage request)
        {
            // add jwt auth header if user is logged in and request is to the api url
            var auth = _applicationData.Auth;
            var tenantId = _applicationData.TenantId;
            var isApiUrl = !request.RequestUri!.IsAbsoluteUri;
            if (auth?.Token != null && isApiUrl)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
            }
            if (tenantId != null)
            {
                request.Headers.Add("tenantId", tenantId);
            }
        }

        private async Task HandleErrorsAsync(HttpResponseMessage response)
        {
            // auto logout on 401 response
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("account/logout");
                return;
            }

            // throw exception on error response
            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                switch (statusCode)
                {
                    case >= 400 and < 500:
                        {
                            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                            throw new FunctionnalException(message: problemDetails?.Title ?? "Unknow Error", code: problemDetails!.Code);
                        }
                    case >= 500 and < 600:
                    default:
                        {
                            try
                            {
                                var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
                                throw new TechnicalException(message: problemDetails?.Title ?? "Unknow Error", code: problemDetails!.Code);
                            }
                            catch
                            {
                                throw new TechnicalException(message: "Unknow Error");
                            }
                        }
                }
            }
        }
    }
}
