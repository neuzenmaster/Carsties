using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams)
        {
            var query = DB.PagedSearch<Item>();

            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }

            switch (searchParams.OrderBy)
            {
                case "make":
                    query.Sort(x => x.Ascending(a => a.Make)).Sort(x => x.Ascending(a => a.Model));
                    break;
                case "new":
                    query.Sort(x => x.Descending(a => a.CreatedAt));
                    break;
                default:
                    query.Sort(x => x.Ascending(a => a.AuctionEnd));
                    break;
            }

            switch (searchParams.FilterBy)
            {
                case "finished":
                    query.Match(x => x.AuctionEnd < DateTime.UtcNow);
                    break;
                case "endingSoon":
                    query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow);
                    break;
                default:
                    query.Match(x => x.AuctionEnd > DateTime.UtcNow);
                    break;
            }

            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query.Match(x => x.Seller == searchParams.Seller);
            }

            if (!string.IsNullOrEmpty(searchParams.Winner))
            {
                query.Match(x => x.Winner == searchParams.Winner);
            }

            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);



            var results = await query.ExecuteAsync();

            return Ok(new
            {
                results = results.Results,
                pageCount = results.PageCount,
                totalCount = results.TotalCount
            });
        }
    }
}
