using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Writely.Models;
using Writely.Services;

namespace Writely.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntriesController : ControllerBase
    {
        private readonly ILogger<EntriesController> _logger;
        private readonly IEntryService _entryService;

        public EntriesController(ILogger<EntriesController> logger, IEntryService entryService)
        {
            _logger = logger;
            _entryService = entryService;
        }

        public async Task<IActionResult> GetById(long entryId)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> Add(NewEntry newEntry)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> Update(EntryUpdate update)
        {
            throw new NotImplementedException();
        }

        public async Task<IActionResult> Delete(long entryId)
        {
            throw new NotImplementedException();
        }
    }
}