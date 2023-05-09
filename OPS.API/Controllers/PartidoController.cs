﻿using Microsoft.AspNetCore.Mvc;
using OPS.Core.DAO;
using System.Threading.Tasks;

namespace OPS.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PartidoController : Controller
    {
        PartidoRepository repository;

        public PartidoController(PartidoRepository repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Consultar()
        {
            var result = await repository.Consultar();

            return Ok(result);
        }
    }
}
