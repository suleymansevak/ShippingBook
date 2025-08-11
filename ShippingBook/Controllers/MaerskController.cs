using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShippingBook.Core.Entities;
using ShippingBook.Services.Abstract;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ShippingBook.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaerskController : Controller
    {
        private readonly IMaerskService _maerskService;
        public MaerskController(IMaerskService maerskService)
        {
            _maerskService = maerskService;
        }

        [HttpPost("GetTable")]
        public async Task<BaseResponse<MaerskResponse>> GetTable(MaerskRequest request) =>
             await _maerskService.GetTable(request);
    }
}

