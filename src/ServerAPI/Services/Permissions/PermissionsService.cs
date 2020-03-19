using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using GenerateTables.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServerAPI.Extensions;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    [UsedImplicitly]
    public class PermissionsService : IPermissionsService
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly AAtims _context;

        public PermissionsService(IConfiguration configuration, AAtims context)
        {
            _client.BaseAddress = new Uri(configuration.GetValue<string>("JwtIssuerOptions:Audience"));
            _context = context;
        }

        public async Task<string> FunctionPermissionCheck(FunctionPermissionCheck functionPermissionCheck)
        {
            _client.DefaultRequestHeaders.Add("Authorization", functionPermissionCheck.AuthenticationHeader);
            _client.DefaultRequestHeaders.Add("FunctionPermissionCheck", "true");
            string responseContent;
            HttpResponseMessage response;
            switch (functionPermissionCheck.RequestType)
            {
                case "GET":
                    response = await _client.GetAsync($"/{functionPermissionCheck.ApiPath}");
                    responseContent = await response.Content.ReadAsStringAsync();
                    break;
                case "POST":
                    response = await _client.PostAsync($"/{functionPermissionCheck.ApiPath}", new JsonContent(new{}));
                    responseContent = await response.Content.ReadAsStringAsync();
                    break;
                case "PUT":
                    response = await _client.PutAsync($"/{functionPermissionCheck.ApiPath}", new JsonContent(new { }));
                    responseContent = await response.Content.ReadAsStringAsync();
                    break;
                default:
                    response = await _client.DeleteAsync($"/{functionPermissionCheck.ApiPath}");
                    responseContent = await response.Content.ReadAsStringAsync();
                    break;
            }

            if (string.IsNullOrEmpty(responseContent))
            {
                responseContent = "Permission Granted";
            }
            return responseContent;

        }

        public bool FunctionPermissionConditionCheck(string condition, HttpContext httpContext)
        {
            string body = null;
            if (httpContext.Request.Method == HttpMethods.Post
                && httpContext.Request.Body.CanRead)
            {
                httpContext.Request.EnableRewind();

                var stream = new StreamReader(httpContext.Request.Body);
                body = stream.ReadToEnd();
                httpContext.Request.Body.Position = 0;
            }

            Type type = this.GetType();
            MethodInfo method = type.GetMethod(condition);
            return (bool)method.Invoke(this, new object[] { body });
        }

        public bool IsBookingComplete(string body)
        {
            PersonIdentity person = JsonConvert.DeserializeObject<PersonIdentity>(body);
            bool result = false;
            Incarceration incarceration = _context.Incarceration
                .Where(w => w.Inmate.Person.PersonId == person.PersonId && w.ReleaseOut == null)
                .SingleOrDefault();
            if (incarceration == null) {
                result = false;
            }
            else {
                result = Convert.ToBoolean(incarceration.BookCompleteFlag);
            }
            return result;
        }
    }
}
