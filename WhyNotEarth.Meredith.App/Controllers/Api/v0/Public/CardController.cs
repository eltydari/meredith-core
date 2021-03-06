using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WhyNotEarth.Meredith.App.Results.Api.v0.Public.Page;
using WhyNotEarth.Meredith.Data.Entity;

namespace WhyNotEarth.Meredith.App.Controllers.Api.v0.Public
{
    [ApiVersion("0")]
    [Route("api/v0/cards")]
    [ProducesErrorResponseType(typeof(void))]
    public class CardController : ControllerBase
    {
        private readonly MeredithDbContext _meredithDbContext;

        public CardController(MeredithDbContext meredithDbContext)
        {
            _meredithDbContext = meredithDbContext;
        }

        [Returns200]
        [Returns404]
        [HttpGet("{id}/related")]
        public async Task<ActionResult<StoryResult>> Related(int id)
        {
            var card = await _meredithDbContext.Cards.FirstOrDefaultAsync(c => c.Id == id);

            if (card is null)
            {
                return NotFound();
            }

            var result = new StoryResult(card.Id, card.Text, card.CallToAction, card.CallToActionUrl,
                card.BackgroundUrl, card.PosterUrl, card.CardType);

            return Ok(result);
        }
    }
}