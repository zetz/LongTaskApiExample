using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using LongTaskApiExample.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using LongTaskApiExample.Providers;

namespace LongTaskApiExample.Controllers
{
    [Route("[controller]")]
    public class BgJobControler : ControllerBase
    {
        LongTaskApiProvider _longTaskApiProvider;

        public BgJobControler(LongTaskApiProvider longTaskApiProvider)
        {
            _longTaskApiProvider = longTaskApiProvider;
        }


        [HttpGet]
        public Task<List<Message>> Get()
        {
            return _longTaskApiProvider.GetJobList();
        }

        [HttpPost]
        public async Task<string> Regist(string msg, int count, double delay)
        {
            return await _longTaskApiProvider.RegistJob(msg, count, delay);
        }
        

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Delete(string id)
        {
            await _longTaskApiProvider.CancelJob(id);
            return NoContent();
        }
    }
}
