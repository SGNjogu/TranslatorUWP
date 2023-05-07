using RestSharp;
using RestSharp.Authenticators;
using SpeechlyTouch.Infrastructure.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Email
{
    public class EmailService : IEmailService
    {
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, string mailGunApiKey)
        {
            var response = await SendEmail(toEmail, subject, body, mailGunApiKey);

            if (response.IsSuccessful)
                return true;
            return false;
        }

        private async Task<IRestResponse> SendEmail(string toEmail, string subject, string body, string mailGunApiKey)
        {
            try
            {
                RestClient client = new RestClient();
                client.BaseUrl = new Uri("https://api.eu.mailgun.net/v3");
                client.Authenticator = new HttpBasicAuthenticator("api", mailGunApiKey);
                RestRequest request = new RestRequest();
                request.AddParameter("domain", "mail.tala.global", ParameterType.UrlSegment);
                request.Resource = "mail.tala.global/messages";
                request.AddParameter("from", "Tala <noreply@mail.tala.global>");
                request.AddParameter("to", toEmail);
                request.AddParameter("subject", subject);
                request.AddParameter("html", body);
                request.Method = Method.POST;
                return await client.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
