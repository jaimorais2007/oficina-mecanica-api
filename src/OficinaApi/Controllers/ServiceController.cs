using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;

namespace OficinaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceManagementService _serviceManagementService;

        public ServiceController(IServiceManagementService serviceManagementService)
        {
            _serviceManagementService = serviceManagementService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicle = await _serviceManagementService.GetAllServicesAsync();
            return Ok(vehicle);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var vehicle = await _serviceManagementService.GetServiceByIdAsync(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
        {
            var result = await _serviceManagementService.CreateServiceAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceDto dto)
        {
            var result = await _serviceManagementService.UpdateServiceAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _serviceManagementService.DeleteServiceAsync(id);
            return Ok();
        }
    }
}
