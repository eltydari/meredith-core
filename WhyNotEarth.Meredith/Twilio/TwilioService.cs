﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using WhyNotEarth.Meredith.Data.Entity;
using WhyNotEarth.Meredith.Data.Entity.Models;

namespace WhyNotEarth.Meredith.Twilio
{
    internal class TwilioService : ITwilioService
    {
        private readonly MeredithDbContext _dbContext;
        private readonly TwilioOptions _options;

        public TwilioService(IOptions<TwilioOptions> options, MeredithDbContext dbContext)
        {
            _dbContext = dbContext;
            _options = options.Value;
        }

        public async Task SendAsync(int shortMessageId)
        {
            var shortMessage = await _dbContext.ShortMessages.FirstOrDefaultAsync(item => item.Id == shortMessageId);

            await SendAsync(shortMessage);
        }

        public async Task SendAsync(ShortMessage message)
        {
            if (message.SentAt != null)
            {
                return;
            }

            await SendCoreAsync(message);

            message.SentAt = DateTime.UtcNow;

            if (message.Id == default)
            {
                _dbContext.ShortMessages.Add(message);
            }
            else
            {
                _dbContext.ShortMessages.Update(message);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task SendCoreAsync(ShortMessage message)
        {
            TwilioClient.Init(_options.AccountSid, _options.AuthToken);

            message.From = GetPhoneNumber(message.From, message.IsWhatsApp).ToString();
            message.To = GetPhoneNumber(message.To, message.IsWhatsApp).ToString();

            var result = await MessageResource.CreateAsync(
                body: message.Body,
                from: message.From,
                to: message.To
            );
        }

        private PhoneNumber GetPhoneNumber(string phoneNumber, bool isWhatsApp)
        {
            if (isWhatsApp)
            {
                return new PhoneNumber($"whatsapp:{phoneNumber}");
            }

            return new PhoneNumber(phoneNumber);
        }
    }
}