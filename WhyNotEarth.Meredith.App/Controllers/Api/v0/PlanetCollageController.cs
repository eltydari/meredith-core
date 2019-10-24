namespace WhyNotEarth.Meredith.App.Controllers.Api.v0
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CloudinaryDotNet;
    using CloudinaryDotNet.Actions;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using WhyNotEarth.Meredith.Cloudinary;

    [ApiVersion("0")]
    [Route("/api/v0/planetcollage")]
    [EnableCors]
    public class PlanetCollageController : Controller
    {
        protected CloudinaryOptions CloudinaryOptions { get; }

        public PlanetCollageController(IOptions<CloudinaryOptions> cloudinaryOptions)
        {
            CloudinaryOptions = cloudinaryOptions.Value;
        }

        protected List<SearchResource> GetResources(int totalCount)
        {
            var cloudinary = new Cloudinary(new Account(CloudinaryOptions.CloudName, CloudinaryOptions.ApiKey, CloudinaryOptions.ApiSecret));
            var resources = new List<SearchResource>();
            string nextCursor = null;
            do
            {
                var requestSize = Math.Min(totalCount, 500);
                var results = cloudinary.Search()
                    .Expression("folder=Bensley Website/Monday - Disruption/*")
                    .MaxResults(requestSize)
                    .NextCursor(nextCursor)
                    .Execute();
                resources.AddRange(results.Resources);
                totalCount -= 500;
                nextCursor = results.NextCursor;
                if (results.Resources.Count == 0 || results.Resources.Count != requestSize)
                {
                    break;
                }
            }
            while (totalCount > 0);
            return resources;
        }

        [HttpGet]
        [Route("")]
        public IActionResult Get()
        {
            var resources = GetResources(800);
            var clodinaryUrl = new Url(CloudinaryOptions.CloudName);
            var imageSize = 75;
            return Ok(resources.Select(r => new
            {
                Id = r.PublicId,
                Url = clodinaryUrl
                    .Transform(new Transformation()
                        .Width(imageSize)
                        .Height(imageSize)
                        .Crop("fill"))
                    .BuildUrl(r.PublicId)
            }).ToList());
        }

        [HttpPost]
        [Route("full")]
        public IActionResult FullResolution([FromBody] FullResolutionModel model)
        {
            var clodinaryUrl = new Url(CloudinaryOptions.CloudName);
            return Ok(new
            {
                Id = model.Id,
                Url = clodinaryUrl.BuildUrl(model.Id)
            });
        }

        public class FullResolutionModel
        {
            public string Id { get; set; }
        }
    }
}