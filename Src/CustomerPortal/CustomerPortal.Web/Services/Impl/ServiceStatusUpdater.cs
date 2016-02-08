using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using apcurium.MK.Common.Diagnostic;
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
        private readonly ILogger _logger;
        public int MaxParallelism = 16;

        private readonly HttpClient _client;

        public ServiceStatusUpdater(ILogger logger) : this(new EmailSender(), new MongoRepository<Company>(), new MongoRepository<CompanyServerStatus>(), logger)
        {
        }

        public ServiceStatusUpdater(IEmailSender emailSender, IRepository<Company> companyRepository, IRepository<CompanyServerStatus> serviceStatusRepository, ILogger logger)
        {
            _emailSender = emailSender;
            _companyRepository = companyRepository;
            _serviceStatusRepository = serviceStatusRepository;
            _logger = logger;

            _client = new HttpClient();

            _client.DefaultRequestHeaders.Add("User-Agent", "CustomerPortal");
            _client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            _client.DefaultRequestHeaders.AcceptCharset.ParseAdd("utf-8");
            _client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip");
            _client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("deflate");
        }

        public void UpdateServiceStatus()
        {
            var companies = _companyRepository
                // We only get the companies that have active websites.
                .Where(company => company.Status == AppStatus.Production ||
                                  company.Status == AppStatus.TestingNewVersion ||
                                  company.CompanyKey.Equals("ArroDemo") ||
                                  company.CompanyKey.Equals("ArroNT") ||
                                  company.CompanyKey.Equals("Apcurium")
                )
                .ToArray();

            Parallel.ForEach(companies,
                new ParallelOptions { MaxDegreeOfParallelism = MaxParallelism },
                async company =>
                {
                    var updateCompanyStatus = await UpdateCompanyStatus(company);

                    if (updateCompanyStatus.Id.HasValueTrimmed())
                    {
                        _serviceStatusRepository.Update(updateCompanyStatus);
                    }
                    else
                    {
                        updateCompanyStatus.Id = Guid.NewGuid().ToString();
                        _serviceStatusRepository.Add(updateCompanyStatus);
                    }

                });
        }

        private string GetUrlFromCompany(Company company)
        {
            var isArro = company.CompanyKey.Equals("Arro", StringComparison.InvariantCultureIgnoreCase) ||
                         company.CompanyKey.Equals("ArroKiosk", StringComparison.InvariantCultureIgnoreCase);

            return company.CompanyKey.Equals("Apcurium", StringComparison.InvariantCultureIgnoreCase)
                ? "http://test.taxihail.biz:8181/Apcurium/"
                : "https://api.{0}.com/{1}/".InvariantCultureFormat(isArro ? "goarro" : "taxihail", company.CompanyKey);

        }

        private async Task<CompanyServerStatus> UpdateCompanyStatus(Company company)
        {
            var url = GetUrlFromCompany(company);

            var response = await Task.Run(() => CheckServer(url, true)).ConfigureAwait(false);

            return HandleServiceStatusResponse(company.CompanyKey, company.CompanyName, response, url);
        }

        private CompanyServerStatus HandleServiceStatusResponse(string companyKey, string companyName, ServiceStatusResponse serviceStatusResponse, string url)
        {
            var companyServerStatus = _serviceStatusRepository
                .FirstOrDefault(status => status.CompanyKey == companyKey && status.IsProduction == serviceStatusResponse.IsProduction);

            var emailSentForCurrentError = SendEmailIfNeeded(serviceStatusResponse, companyName, url, companyServerStatus);

            if (companyServerStatus == null)
            {
                companyServerStatus =  new CompanyServerStatus
                {
                    IsProduction = serviceStatusResponse.IsProduction,
                    CompanyName = companyName,
                    CompanyKey = companyKey
                };
            }

            companyServerStatus.ServiceStatus = serviceStatusResponse.ServiceStatus;
            companyServerStatus.IsApiAvailable = serviceStatusResponse.IsApiAvailable;
            companyServerStatus.IsServerAvailable = serviceStatusResponse.IsServerAvailable;
            companyServerStatus.IsEmailSentForCurrentError = emailSentForCurrentError;
            companyServerStatus.HasAuthenticationError = serviceStatusResponse.HasAuthenticationError;

            return companyServerStatus;
        }

        private bool SendEmailIfNeeded(ServiceStatusResponse serviceStatus, string companyName, string url, CompanyServerStatus previousStatus)
        {
            if (serviceStatus.ServiceStatus.SelectOrDefault(response => response.IsServerHealthy(), !serviceStatus.IsApiAvailable))
            {
                return false;
            }

            if (!previousStatus.SelectOrDefault(status => status.IsEmailSentForCurrentError))
            {
                try
                {
                    _emailSender.SendServiceStatusEmail(companyName, url, serviceStatus.ServiceStatus, serviceStatus.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex);
                    return false;
                }


            }
            return true;
        }
        
        private async Task<ServiceStatusResponse> CheckServer(string url, bool isProduction)
        {
            if (!url.HasValueTrimmed())
            {
                return null;
            }

            try
            {
                var timeout = TimeSpan.FromMinutes(4);


                var cts = new CancellationTokenSource(timeout);

                var hostPresenceResult = await _client.GetAsync(url+ "api/popularaddresses", cts.Token);

                if (!hostPresenceResult.IsSuccessStatusCode)
                {
                    return new ServiceStatusResponse
                    {
                        IsProduction = isProduction,
                        StatusCode = hostPresenceResult.StatusCode,
                        IsServerAvailable = hostPresenceResult.StatusCode != HttpStatusCode.NotFound
                    };
                }

                cts.CancelAfter(timeout);

                var authenticationResult = await _client.PostAsJsonAsync(url + "api/auth/credentials", new
                {
                    UserName = "taxihail@apcurium.com",
                    Password = "1l1k3B4n4n@",
                }, cts.Token);

                if (!authenticationResult.IsSuccessStatusCode)
                {
                    return new ServiceStatusResponse
                    {
                        IsProduction = isProduction,
                        StatusCode = authenticationResult.StatusCode,
                        HasAuthenticationError = true,
                        IsServerAvailable = true
                    };
                }

                var authData = await FromJson<AuthResponse>(authenticationResult.Content);

                var request = new HttpRequestMessage(HttpMethod.Get, url + "api/status");

                request.Headers.Add("Cookie", "ss-opt=perm; ss-pid=" + authData.SessionId);

                cts.CancelAfter(timeout);

                var serviceStatusResult = await _client.SendAsync(request, cts.Token).ConfigureAwait(false);

                if (!serviceStatusResult.IsSuccessStatusCode)
                {
                    return new ServiceStatusResponse
                    {
                        IsProduction = isProduction,
                        StatusCode = serviceStatusResult.StatusCode,
                        IsApiAvailable = serviceStatusResult.StatusCode != HttpStatusCode.NotFound,
                        IsServerAvailable = true
                    };
                }

                var status = await FromJson<ServiceStatus>(serviceStatusResult.Content);

                return new ServiceStatusResponse
                {
                    IsProduction = isProduction,
                    StatusCode = serviceStatusResult.StatusCode,
                    ServiceStatus = status,
                    IsServerAvailable = true,
                    IsApiAvailable = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);

                return new ServiceStatusResponse
                {
                    IsProduction = isProduction,
                    StatusCode = HttpStatusCode.RequestTimeout
                };
            }
            
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

            public bool IsApiAvailable { get; set; }

            public bool IsServerAvailable { get; set; }

            public bool HasAuthenticationError { get; set; }
        }
    }
}