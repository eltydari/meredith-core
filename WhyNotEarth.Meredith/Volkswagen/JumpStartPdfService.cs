﻿using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using WhyNotEarth.Meredith.Data.Entity;
using WhyNotEarth.Meredith.Data.Entity.Models.Modules.Volkswagen;
using WhyNotEarth.Meredith.GoogleCloud;

namespace WhyNotEarth.Meredith.Volkswagen
{
    public class JumpStartPdfService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly MeredithDbContext _dbContext;
        private readonly GoogleStorageService _googleStorageService;
        private readonly PuppeteerService _puppeteerService;

        public JumpStartPdfService(MeredithDbContext dbContext, GoogleStorageService googleStorageService,
            IBackgroundJobClient backgroundJobClient, PuppeteerService puppeteerService)
        {
            _dbContext = dbContext;
            _googleStorageService = googleStorageService;
            _backgroundJobClient = backgroundJobClient;
            _puppeteerService = puppeteerService;
        }

        public async Task CreatePdfAsync(int jumpStartId)
        {
            var jumpStart = await _dbContext.JumpStarts
                .Include(item => item.Posts)
                .ThenInclude(item => item.Category)
                .ThenInclude(item => item.Image)
                .FirstOrDefaultAsync(item => item.Id == jumpStartId && item.HasPdf == false);

            if (jumpStart is null)
            {
                // We already created pdf for this one
                return;
            }

            var pdfStream = await _puppeteerService.BuildPdfAsync(jumpStart.DateTime, jumpStart.Posts);

            await UploadPdfAsync(jumpStart, pdfStream);

            jumpStart.HasPdf = true;
            _dbContext.Update(jumpStart);

            await _dbContext.SaveChangesAsync();

            _backgroundJobClient.Schedule<JumpStartEmailService>(service =>
                service.SendAsync(jumpStart.Id), DateTime.UtcNow - jumpStart.DateTime);
        }

        public Task<string> CreatePdfUrlAsync(JumpStart jumpStart)
        {
            return _googleStorageService.CreateSignedUrlAsync(GetName(jumpStart), 24);
        }

        private async Task UploadPdfAsync(JumpStart jumpStart, Stream pdfStream)
        {
            await _googleStorageService.UploadPdfAsync(GetName(jumpStart), pdfStream);
        }

        private string GetName(JumpStart jumpStart)
        {
            return $"volkswagen_pdf/{jumpStart.DateTime.Date:yyyy_MM_dd}.pdf";
        }
    }
}