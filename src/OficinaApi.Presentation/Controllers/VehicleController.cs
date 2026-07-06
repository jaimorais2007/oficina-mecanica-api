using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IUseCase<NoInput, IEnumerable<VehicleDto?>> _getAllVehiclesUseCase;
        private readonly IUseCase<Guid, VehicleDto?> _getVehicleByIdUseCase;
        private readonly IUseCase<CreateVehicleDto, VehicleDto> _createVehicleUseCase;
        private readonly IUseCase<UpdateVehicleRequest, VehicleDto> _updateVehicleUseCase;
        private readonly IUseCase<Guid, bool> _deleteVehicleUseCase;
        private readonly IUseCase<Guid, NoInput> _logicalDeletionVehicleUseCase;


        public VehicleController(
            IUseCase<NoInput, IEnumerable<VehicleDto?>> getAllVehiclesUseCase,
            IUseCase<Guid, VehicleDto?> getVehicleByIdUseCase,
            IUseCase<CreateVehicleDto, VehicleDto> createVehicleUseCase,
            IUseCase<UpdateVehicleRequest, VehicleDto> updateVehicleUseCase,
            IUseCase<Guid, bool> deleteVehicleUseCase,
            IUseCase<Guid, NoInput> logicalDeletionVehicleUseCase)
        {
            _getAllVehiclesUseCase = getAllVehiclesUseCase;
            _getVehicleByIdUseCase = getVehicleByIdUseCase;
            _createVehicleUseCase = createVehicleUseCase;
            _updateVehicleUseCase = updateVehicleUseCase;
            _deleteVehicleUseCase = deleteVehicleUseCase;
            _logicalDeletionVehicleUseCase = logicalDeletionVehicleUseCase;
        }

        [SwaggerOperation(Summary = "Lista todos os veículos cadastrados",
                          Description = "Retorna uma lista com todos os veículos registrados no sistema.")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _getAllVehiclesUseCase.ExecuteAsync(new NoInput());
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Busca veículo por ID",
                          Description = "Retorna os dados de um veículo específico a partir do seu identificador único.")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _getVehicleByIdUseCase.ExecuteAsync(id);
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            if (result.Response == null) return NotFound();
            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Cria um novo veículo",
                          Description = "Cadastra um novo veículo no sistema. Não é permitido cadastrar dois veículos com a mesma placa.")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
        {
            var result = await _createVehicleUseCase.ExecuteAsync(dto);
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            return CreatedAtAction(nameof(GetById), new { id = result.Response.Id }, result.Response);
        }

        [SwaggerOperation(Summary = "Atualiza os dados de um veículo",
                          Description = "Atualiza as informações de um veículo existente a partir do seu identificador único.")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVehicleDto dto)
        {
            var result = await _updateVehicleUseCase.ExecuteAsync(new UpdateVehicleRequest(id, dto));
            if (!result.IsSuccess)
            {
                if (result.Messages.Any(m => m.Contains("não encontrado")))
                    return NotFound(new { Message = string.Join(", ", result.Messages) });
                return BadRequest(new { Message = string.Join(", ", result.Messages) });
            }
            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Inativa/Ativa o veículo",
                  Description = "Inativa ou ativa um veículo existente a partir do identificador único.")]
        [HttpPut("{id}/LogicalDeletion")]
        public async Task<IActionResult> LogicalDeletion([FromRoute] Guid id)
        {
            var result = await _logicalDeletionVehicleUseCase.ExecuteAsync(id);

            return Ok(result.Response);
        }

        [SwaggerOperation(Summary = "Remove um veículo",
                          Description = "Exclui permanentemente o cadastro de um veículo a partir do seu identificador único.")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _deleteVehicleUseCase.ExecuteAsync(id);
            if (!result.IsSuccess) return BadRequest(new { Message = string.Join(", ", result.Messages) });
            return Ok();
        }
    }
}
