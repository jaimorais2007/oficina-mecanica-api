using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicle = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicle);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
        {
            var result = await _vehicleService.CreateVehicleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleDto dto)
        {
            var result = await _vehicleService.UpdateVehicleAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _vehicleService.DeleteVehicleAsync(id);
            return Ok();
        }
    }
}
