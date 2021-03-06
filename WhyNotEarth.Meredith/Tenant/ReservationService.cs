using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using WhyNotEarth.Meredith.Data.Entity;
using WhyNotEarth.Meredith.Data.Entity.Models;
using WhyNotEarth.Meredith.Email;
using WhyNotEarth.Meredith.Exceptions;
using WhyNotEarth.Meredith.Tenant.Models;
using WhyNotEarth.Meredith.Twilio;

namespace WhyNotEarth.Meredith.Tenant
{
    public class ReservationService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly MeredithDbContext _dbContext;
        private readonly SendGridService _sendGridService;
        private readonly TenantReservationNotification _tenantReservationNotification;

        public ReservationService(IBackgroundJobClient backgroundJobClient,
            TenantReservationNotification tenantReservationNotification, MeredithDbContext dbContext,
            SendGridService sendGridService)
        {
            _backgroundJobClient = backgroundJobClient;
            _tenantReservationNotification = tenantReservationNotification;
            _dbContext = dbContext;
            _sendGridService = sendGridService;
        }

        public async Task ReserveAsync(string tenantSlug, TenantReservationModel model, User user)
        {
            var tenant = await _dbContext.Tenants
                .Include(item => item.Owner)
                .Include(item => item.Company)
                .FirstOrDefaultAsync(item => item.Slug == tenantSlug);

            if (tenant is null)
            {
                throw new RecordNotFoundException($"Tenant {tenantSlug} not found");
            }

            if (tenant.Company is null)
            {
                throw new RecordNotFoundException($"Tenant: {tenantSlug} is not connected to any company");
            }

            if (tenant.Owner is null)
            {
                throw new RecordNotFoundException($"Tenant: {tenantSlug} does not have an owner");
            }

            if (model.WhatsappNotification == true)
            {
                var shortMessages = _tenantReservationNotification.GetWhatsAppMessage(tenant, model, user);

                _dbContext.ShortMessages.AddRange(shortMessages);
                await _dbContext.SaveChangesAsync();

                foreach (var shortMessage in shortMessages)
                {
                    _backgroundJobClient.Enqueue<ITwilioService>(service =>
                        service.SendAsync(shortMessage.Id));
                }
            }
            else
            {
                var emailMessage = _tenantReservationNotification.GetEmailMessage(tenant, model, user);

                await _sendGridService.SendEmailAsync(emailMessage);
            }
        }
    }
}