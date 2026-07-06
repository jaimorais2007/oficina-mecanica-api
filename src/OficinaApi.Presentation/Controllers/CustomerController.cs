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
    public class CustomerController : ControllerBase
    {
        private readonly IUseCase<NoInput, IEnumerable<CustomerDto>> _getAllCustomersUseCase;
        private readonly IUseCase<Guid, CustomerDto?> _getCustomerByIdUseCase;
        private readonly IUseCase<CreateCustomerDto, CustomerDto> _createCustomerUseCase;
        private readonly IUseCase<UpdateCustomerRequest, CustomerDto> _updateCustomerUseCase;
        private readonly IUseCase<Guid, bool> _deleteCustomerUseCase;
        private readonly IUseCase<Guid, NoInput> _logicalDeletionCustomerUseCase;


        public CustomerController(
            IUseCase<NoInput, IEnumerable<CustomerDto>> getAllCustomersUseCase,
            IUseCase<Guid, CustomerDto?> getCustomerByIdUseCase,
            IUseCase<CreateCustomerDto, CustomerDto> createCustomerUseCase,
            IUseCase<UpdateCustomerRequest, CustomerDto> updateCustomerUseCase,
            IUseCase<Guid, bool> deleteCustomerUseCase,
            IUseCase<Guid, NoInput> logicalDeletionCustomerUseCase)
        {
            _getAllCustomersUseCase = getAllCustomersUseCase;
            _getCustomerByIdUseCase = getCustomerByIdUseCase;
            _createCustomerUseCase = createCustomerUseCase;
            _updateCustomerUseCase = updateCustomerUseCase;
            _deleteCustomerUseCase = deleteCustomerUseCase;
            _logicalDeletionCustomerUseCase = logicalDeletionCustomerUseCase;
        }

        [SwaggerOperation(Summary = "Lista todos os clientes cadastrados",
                          Description = "Retorna uma lista com todos os clientes registrados no sistema.")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _getAllCustomersUseCase.ExecuteAsync(new NoInput());
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Busca cliente por ID",
                          Description = "Retorna os dados de um cliente específico a partir do seu identificador único.")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _getCustomerByIdUseCase.ExecuteAsync(id);
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            if (result.Response == null) return NotFound();
            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Cria um novo cliente",
                          Description = "Cadastra um novo cliente no sistema com os dados informados no corpo da requisição.")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            var result = await _createCustomerUseCase.ExecuteAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            return CreatedAtAction(nameof(GetById), new { id = result.Response.Id }, result.Response);
        }

        [SwaggerOperation(Summary = "Atualiza os dados de um cliente",
                          Description = "Atualiza as informações de um cliente existente a partir do seu identificador único.")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
        {
            var result = await _updateCustomerUseCase.ExecuteAsync(new UpdateCustomerRequest(id, dto));
            if (!result.IsSuccess)
            {
                if (result.Messages.Any(m => m.Contains("não encontrado")))
                    return NotFound(new { Message = string.Join(", ", result.Messages) });
                return BadRequest(new { Message = string.Join(", ", result.Messages) });
            }
            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Inativa/Ativa o cliente",
          Description = "Inativa ou ativa um cliente existente a partir do identificador único.")]
        [HttpPut("{id}/LogicalDeletion")]
        public async Task<IActionResult> LogicalDeletion([FromRoute] Guid id)
        {
            var result = await _logicalDeletionCustomerUseCase.ExecuteAsync(id);

            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Remove um cliente",
                          Description = "Exclui permanentemente o cadastro de um cliente a partir do seu identificador único.")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _deleteCustomerUseCase.ExecuteAsync(id);
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            return NoContent();
        }
    }
}
