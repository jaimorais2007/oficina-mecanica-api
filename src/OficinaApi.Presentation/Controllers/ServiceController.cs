using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.UseCases.Vehicles;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OficinaApi.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceController : ControllerBase
    {
        private readonly IUseCase<NoInput, IEnumerable<ServiceDto>> _getAllServicesUseCase;
        private readonly IUseCase<Guid, ServiceDto?> _getServiceByIdUseCase;
        private readonly IUseCase<CreateServiceDto, ServiceDto> _createServiceUseCase;
        private readonly IUseCase<UpdateServiceRequest, ServiceDto> _updateServiceUseCase;
        private readonly IUseCase<Guid, bool> _deleteServiceUseCase;
        private readonly IUseCase<Guid, NoInput> _logicalDeletionServiceUseCase;


        public ServiceController(
            IUseCase<NoInput, IEnumerable<ServiceDto>> getAllServicesUseCase,
            IUseCase<Guid, ServiceDto?> getServiceByIdUseCase,
            IUseCase<CreateServiceDto, ServiceDto> createServiceUseCase,
            IUseCase<UpdateServiceRequest, ServiceDto> updateServiceUseCase,
            IUseCase<Guid, bool> deleteServiceUseCase,
            IUseCase<Guid, NoInput> logicalDeletionServiceUseCase)
        {
            _getAllServicesUseCase = getAllServicesUseCase;
            _getServiceByIdUseCase = getServiceByIdUseCase;
            _createServiceUseCase = createServiceUseCase;
            _updateServiceUseCase = updateServiceUseCase;
            _deleteServiceUseCase = deleteServiceUseCase;
            _logicalDeletionServiceUseCase = logicalDeletionServiceUseCase;
        }

        [SwaggerOperation(Summary = "Lista todos os serviços cadastrados",
                          Description = "Retorna uma lista com todos os tipos de serviços disponíveis na oficina.")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _getAllServicesUseCase.ExecuteAsync(new NoInput());
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Busca serviço por ID",
                          Description = "Retorna os dados de um serviço específico a partir do seu identificador único.")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _getServiceByIdUseCase.ExecuteAsync(id);
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            if (result.Response == null) return NotFound();
            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Cria um novo serviço",
                          Description = "Cadastra um novo tipo de serviço oferecido pela oficina com os dados informados no corpo da requisição.")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
        {
            var result = await _createServiceUseCase.ExecuteAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            return CreatedAtAction(nameof(GetById), new { id = result.Response.Id }, result.Response);
        }

        [SwaggerOperation(Summary = "Atualiza os dados de um serviço",
                          Description = "Atualiza as informações de um tipo de serviço existente a partir do seu identificador único.")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateServiceDto dto)
        {
            var result = await _updateServiceUseCase.ExecuteAsync(new UpdateServiceRequest(id, dto));
            if (!result.IsSuccess)
            {
                if (result.Messages.Any(m => m.Contains("não encontrado")))
                    return NotFound(new { Message = string.Join(", ", result.Messages) });
                return BadRequest(new { Message = string.Join(", ", result.Messages) });
            }
            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Inativa/Ativa o serviço",
          Description = "Inativa ou ativa um serviço existente a partir do identificador único.")]
        [HttpPut("{id}/LogicalDeletion")]
        public async Task<IActionResult> LogicalDeletion([FromRoute] Guid id)
        {
            var result = await _logicalDeletionServiceUseCase.ExecuteAsync(id);

            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Remove um serviço",
                          Description = "Exclui permanentemente o cadastro de um tipo de serviço a partir do seu identificador único.")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _deleteServiceUseCase.ExecuteAsync(id);
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            return Ok();
        }
    }
}
