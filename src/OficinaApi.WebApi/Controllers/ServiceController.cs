using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.WebApi.Controllers
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

        [SwaggerOperation(Summary = "Lista todos os serviços cadastrados",
                          Description = "Retorna uma lista com todos os tipos de serviços disponíveis na oficina.")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicle = await _serviceManagementService.GetAllServicesAsync();
            return Ok(vehicle);
        }

        [SwaggerOperation(Summary = "Busca serviço por ID",
                          Description = "Retorna os dados de um serviço específico a partir do seu identificador único.")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var vehicle = await _serviceManagementService.GetServiceByIdAsync(id);
            if (vehicle == null) return NotFound();
            return Ok(vehicle);
        }

        [SwaggerOperation(Summary = "Cria um novo serviço",
                          Description = "Cadastra um novo tipo de serviço oferecido pela oficina com os dados informados no corpo da requisição.")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
        {
            var result = await _serviceManagementService.CreateServiceAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [SwaggerOperation(Summary = "Atualiza os dados de um serviço",
                          Description = "Atualiza as informações de um tipo de serviço existente a partir do seu identificador único.")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceDto dto)
        {
            var result = await _serviceManagementService.UpdateServiceAsync(id, dto);
            return Ok(result);
        }

        [SwaggerOperation(Summary = "Remove um serviço",
                          Description = "Exclui permanentemente o cadastro de um tipo de serviço a partir do seu identificador único.")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _serviceManagementService.DeleteServiceAsync(id);
            return Ok();
        }
    }
}
