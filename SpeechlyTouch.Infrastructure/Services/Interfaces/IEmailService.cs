﻿using System.Threading.Tasks;

namespace SpeechlyTouch.Infrastructure.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string body, string mailGunApiKey);
    }
}
