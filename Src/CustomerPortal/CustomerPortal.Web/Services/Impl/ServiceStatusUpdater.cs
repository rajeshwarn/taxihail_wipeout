using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Web.Entities;
using MongoRepository;

namespace CustomerPortal.Web.Services.Impl
{
    public class ServiceStatusUpdater : IServiceStatusUpdater
    {
        private readonly IEmailSender _emailSender;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<CompanyServerStatus> _serviceStatusRepository; 

        private readonly HttpClient _client;

        public ServiceStatusUpdater() : this(new EmailSender(), new MongoRepository<Company>(), new MongoRepository<CompanyServerStatus>())
        {
        }

        public ServiceStatusUpdater(IEmailSender emailSender, IRepository<Company> companyRepository, IRepository<CompanyServerStatus> serviceStatusRepository)
        {
            _emailSender = emailSender;
            _companyRepository = companyRepository;
            _serviceStatusRepository = serviceStatusRepository;

            _client = new HttpClient();

            _client.DefaultRequestHeaders.Add("User-Agent", "CustomerPortal");
            _client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            _client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
            _client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
            _client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");
        }

        public async Task UpdateServiceStatus()
        {
            var companies = _companyRepository
                // We only get the companies that have active websites.
                .Where(company => company.Status == AppStatus.Production ||
                    company.Status == AppStatus.DemoSystem || 
                    company.Status == AppStatus.Test ||
                    company.Status == AppStatus.TestingNewVersion
                )
                .Select(async company => await UpdateCompanyStatus(company.CompanyKey, company.CompanyName).ConfigureAwait(false))
                .ToArray();

            var companyServerStatus = await Task.WhenAll(companies).ConfigureAwait(false);

            var groupedCompanyServerStatus = companyServerStatus
                .SelectMany(status => status)
                .GroupBy(status => status.Id == null);

            foreach (var groupedStatus in groupedCompanyServerStatus)
            {
                if (groupedStatus.Key)
                {
                    _serviceStatusRepository.Add(groupedStatus.Select(status =>
                    {
                        status.Id = Guid.NewGuid().ToString();
                        return status;
                    }));
                }
                else
                {
                    _serviceStatusRepository.Update(groupedStatus);
                }
            }
        }


        private async Task<CompanyServerStatus[]> UpdateCompanyStatus(string companyKey, string companyName)
        {
            var productionUrl = companyKey.Equals("Arro", StringComparison.InvariantCultureIgnoreCase)
                ? "https://api.goarro.com/Arro/api/"
                : "https://api.taxihail.com/{0}/api/".InvariantCultureFormat(companyKey);

            var stagingUrl = companyKey.Equals("Arro", StringComparison.InvariantCultureIgnoreCase)
                ? string.Empty
                : "https://staging.taxihail.com/{0}/api/".InvariantCultureFormat(companyKey);

            var responses = await Task.WhenAll(CheckServer(productionUrl, true), CheckServer(stagingUrl, false)).ConfigureAwait(false);

            return responses
                .Where(serverStatus => serverStatus != null)
                .Select(serviceStatusResponse =>
                {
                    var url = serviceStatusResponse.IsProduction ? productionUrl : stagingUrl;

                    return HandleServiceStatusResponse(companyKey, companyName, serviceStatusResponse, url);
                })
                .ToArray();
        }

        private CompanyServerStatus HandleServiceStatusResponse(string companyKey, string companyName, ServiceStatusResponse serviceStatusResponse, string url)
        {
            var companyServerStatus = _serviceStatusRepository
                .FirstOrDefault(status => status.CompanyKey == companyKey && status.IsProduction == serviceStatusResponse.IsProduction);

            SendEmailIfNeeded(serviceStatusResponse, companyName, url, companyServerStatus);

            if (companyServerStatus == null)
            {
                return new CompanyServerStatus
                {
                    ServiceStatus = serviceStatusResponse.ServiceStatus,
                    IsProduction = serviceStatusResponse.IsProduction,
                    CompanyName = companyName,
                    CompanyKey = companyKey,
                    HasNoStatusApi = serviceStatusResponse.HasNoStatusApi
                };
            }

            companyServerStatus.ServiceStatus = serviceStatusResponse.ServiceStatus;
            companyServerStatus.HasNoStatusApi = serviceStatusResponse.HasNoStatusApi;

            return companyServerStatus;
        }

        private void SendEmailIfNeeded(ServiceStatusResponse serviceStatus, string companyName, string url, CompanyServerStatus previousStatus)
        {
            if (serviceStatus.StatusCode == HttpStatusCode.OK ||
                        serviceStatus.ServiceStatus.SelectOrDefault(response => response.IsServerHealthy(), serviceStatus.HasNoStatusApi))
            {
                return;
            }

            if (previousStatus != null && !previousStatus.ServiceStatus.IsServerHealthy())
            {
                // We sent the email already.
                return;
            }

            _emailSender.SendServiceStatusEmail(companyName, url, serviceStatus.ServiceStatus, serviceStatus.StatusCode); 
        }
        
        private async Task<ServiceStatusResponse> CheckServer(string url, bool isProduction)
        {
            if (!url.HasValueTrimmed())
            {
                return null;
            }

            var authenticationReslt = await _client.PostAsJsonAsync(url + "/auth/credentials", new
            {
                UserName = "taxihail@apcurium.com",
                Password = "1l1k3B4n4n@",
            });

            if (!authenticationReslt.IsSuccessStatusCode)
            {
                return new ServiceStatusResponse
                {
                    IsProduction = isProduction,
                    StatusCode = authenticationReslt.StatusCode
                };
            }

            var authData = await FromJson<AuthResponse>(authenticationReslt.Content);

            var request = new HttpRequestMessage(HttpMethod.Get, url + "status");

            request.Headers.Add("Cookie", "ss-opt=perm; ss-pid=" + authData.SessionId);

            var serviceStatusResult = await _client.SendAsync(request);


            if (!serviceStatusResult.IsSuccessStatusCode)
            {
                return new ServiceStatusResponse
                {
                    IsProduction = isProduction,
                    StatusCode = serviceStatusResult.StatusCode,
                    HasNoStatusApi = authenticationReslt.StatusCode == HttpStatusCode.NotFound
                };
            }

            var status = await FromJson<ServiceStatus>(serviceStatusResult.Content);

            return new ServiceStatusResponse
            {
                IsProduction = isProduction,
                StatusCode = serviceStatusResult.StatusCode,
                ServiceStatus = status
            };
        }

        private async Task<TValue> FromJson<TValue>(HttpContent httpContent)
        {
            var content = await httpContent.ReadAsStringAsync();

            return content.FromJson<TValue>();
        }

        private class AuthResponse
        {
            public string SessionId { get; set; }
        }

        private class ServiceStatusResponse
        {
            public HttpStatusCode StatusCode { get; set; }
            public ServiceStatus ServiceStatus { get; set; }
            public bool IsProduction { get; set; }

            public bool HasNoStatusApi { get; set; }
        }
    }
}