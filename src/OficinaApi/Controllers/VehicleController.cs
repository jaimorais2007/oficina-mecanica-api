using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [SwaggerOperation(Summary = "Lista todos os veículos cadastrados",
                          Description = "Retorna uma lista com todos os veículos registrados no sistema.")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicle = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicle);
        }

        [SwaggerOperation(Summary = "Busca veículo por ID",
                          Description = "Retorna os dados de um veículo específico a partir do seu identificador único.")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        [SwaggerOperation(Summary = "Cria um novo veículo",
                          Description = "Cadastra um novo veículo no sistema. Não é permitido cadastrar dois veículos com a mesma placa.")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
        {
            var result = await _vehicleService.CreateVehicleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [SwaggerOperation(Summary = "Atualiza os dados de um veículo",
                          Description = "Atualiza as informações de um veículo existente a partir do seu identificador único.")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleDto dto)
        {
            var result = await _vehicleService.UpdateVehicleAsync(id, dto);
            return Ok(result);
        }

        [SwaggerOperation(Summary = "Remove um veículo",
                          Description = "Exclui permanentemente o cadastro de um veículo a partir do seu identificador único.")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _vehicleService.DeleteVehicleAsync(id);
            return Ok();
        }
    }
}
