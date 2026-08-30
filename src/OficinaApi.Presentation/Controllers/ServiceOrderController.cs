using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServiceOrdersController : ControllerBase
{
    private readonly IUseCase<NoInput, IEnumerable<ServiceOrderDto>> _getAllServiceOrdersUseCase;
    private readonly IUseCase<Guid, ServiceOrderDto?> _getServiceOrderByIdUseCase;
    private readonly IUseCase<Guid, ServiceOrderStatusDto?> _getServiceOrderByStatusUseCase;
    private readonly IUseCase<CreateServiceOrderDto, ServiceOrderDto> _createServiceOrderUseCase;
    private readonly IUseCase<StartDiagnosticsRequest, ServiceOrderDto> _startDiagnosticsUseCase;
    private readonly IUseCase<FinishAnalysisRequest, ServiceOrderDto> _finishAnalysisUseCase;
    private readonly IUseCase<AddPartToServiceOrderRequest, ServiceOrderDto> _addPartToServiceOrderUseCase;
    private readonly IUseCase<AddServiceToServiceOrderRequest, ServiceOrderDto> _addServiceToServiceOrderUseCase;
    private readonly IUseCase<ApproveServiceOrderRequest, ServiceOrderDto> _approveServiceOrderUseCase;
    private readonly IUseCase<FinishExecutionRequest, ServiceOrderDto> _finishExecutionUseCase;
    private readonly IUseCase<DeliverServiceOrderRequest, ServiceOrderDto> _deliverServiceOrderUseCase;
    private readonly IUseCase<RefuseServiceOrderRequest, ServiceOrderDto> _refuseServiceOrderUseCase;
    private readonly IUseCase<Guid, IEnumerable<ServiceOrderPeddingStockDto>> _getServiceOrderPendingStocksUseCase;
    private readonly IUseCase<NoInput, double> _getAverageDurationUseCase;

    public ServiceOrdersController(
        IUseCase<NoInput, IEnumerable<ServiceOrderDto>> getAllServiceOrdersUseCase,
        IUseCase<Guid, ServiceOrderDto?> getServiceOrderByIdUseCase,
        IUseCase<Guid, ServiceOrderStatusDto?> getServiceOrderByStatusUseCase,
        IUseCase<CreateServiceOrderDto, ServiceOrderDto> createServiceOrderUseCase,
        IUseCase<StartDiagnosticsRequest, ServiceOrderDto> startDiagnosticsUseCase,
        IUseCase<FinishAnalysisRequest, ServiceOrderDto> finishAnalysisUseCase,
        IUseCase<AddPartToServiceOrderRequest, ServiceOrderDto> addPartToServiceOrderUseCase,
        IUseCase<AddServiceToServiceOrderRequest, ServiceOrderDto> addServiceToServiceOrderUseCase,
        IUseCase<ApproveServiceOrderRequest, ServiceOrderDto> approveServiceOrderUseCase,
        IUseCase<FinishExecutionRequest, ServiceOrderDto> finishExecutionUseCase,
        IUseCase<DeliverServiceOrderRequest, ServiceOrderDto> deliverServiceOrderUseCase,
        IUseCase<Guid, IEnumerable<ServiceOrderPeddingStockDto>> getServiceOrderPendingStocksUseCase,
        IUseCase<NoInput, double> getAverageDurationUseCase,
        IUseCase<RefuseServiceOrderRequest, ServiceOrderDto> refuseServiceOrderUseCase)
    {
        _getAllServiceOrdersUseCase = getAllServiceOrdersUseCase;
        _getServiceOrderByIdUseCase = getServiceOrderByIdUseCase;
        _getServiceOrderByStatusUseCase = getServiceOrderByStatusUseCase;
        _createServiceOrderUseCase = createServiceOrderUseCase;
        _startDiagnosticsUseCase = startDiagnosticsUseCase;
        _finishAnalysisUseCase = finishAnalysisUseCase;
        _addPartToServiceOrderUseCase = addPartToServiceOrderUseCase;
        _addServiceToServiceOrderUseCase = addServiceToServiceOrderUseCase;
        _approveServiceOrderUseCase = approveServiceOrderUseCase;
        _finishExecutionUseCase = finishExecutionUseCase;
        _deliverServiceOrderUseCase = deliverServiceOrderUseCase;
        _getServiceOrderPendingStocksUseCase = getServiceOrderPendingStocksUseCase;
        _getAverageDurationUseCase = getAverageDurationUseCase;
        _refuseServiceOrderUseCase = refuseServiceOrderUseCase;
    }

    [SwaggerOperation(Summary = "Lista todas as ordens de serviço",
                      Description = "Retorna todas as ordens de serviço cadastradas no sistema.")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _getAllServiceOrdersUseCase.ExecuteAsync(new NoInput());
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Busca ordem de serviço por ID",
                      Description = "Retorna os dados completos de uma ordem de serviço, incluindo serviços realizados, peças utilizadas e veículo associado.")]
    [HttpGet("{id}")] 
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getServiceOrderByIdUseCase.ExecuteAsync(id);
        if (!result.IsSuccess)
        {
            if (result.Messages.Any(m => m.Contains("não encontrada")))
                return NotFound(new { message = string.Join(", ", result.Messages) });
            return BadRequest(new { message = string.Join(", ", result.Messages) });
        }
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Consulta o status da ordem de serviço",
                  Description = "Retorna o id e o status atual da ordem de serviço.")]
    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetByIdForStatus(Guid id)
    {
        var result = await _getServiceOrderByStatusUseCase.ExecuteAsync(id);
        if (!result.IsSuccess)
        {
            if (result.Messages.Any(m => m.Contains("não encontrada")))
                return NotFound(new { message = string.Join(", ", result.Messages) });
            return BadRequest(new { message = string.Join(", ", result.Messages) });
        }
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Cria uma nova ordem de serviço",
                      Description = "Abre uma nova OS no sistema. É possível informar o veículo, os serviços a utilizar.")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceOrderDto dto)
    {
        var result = await _createServiceOrderUseCase.ExecuteAsync(dto);
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return CreatedAtAction(nameof(GetById), new { id = result.Response.Id }, result.Response);
    }

    [SwaggerOperation(Summary = "Move uma ordem de serviço para analise",
                      Description = "Move uma ordem de serviço para o status de análise técnica.")]
    [HttpPost("{id}/start-analysis")]
    public async Task<IActionResult> MoveToAnalysis(Guid id)
    {
        var result = await _startDiagnosticsUseCase.ExecuteAsync(new StartDiagnosticsRequest(id));
        if (!result.IsSuccess)
        {
            if (result.Messages.Any(m => m.Contains("não encontrada")))
                return NotFound(new { message = string.Join(", ", result.Messages) });
            return UnprocessableEntity(new { message = string.Join(", ", result.Messages) });
        }
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Move uma ordem de serviço para execução",
                      Description = "Move uma ordem de serviço para o status de execução, indicando que os trabalhos começaram.")]
    [HttpPost("{id}/finish-analysis")]
    public async Task<IActionResult> FinishAnalysis(Guid id)
    {
        var result = await _finishAnalysisUseCase.ExecuteAsync(new FinishAnalysisRequest(id));
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Adiciona uma peça a ordem de serviço",
                      Description = "Adiciona uma peça a uma ordem de serviço existente.")]
    [HttpPost("{id}/parts")]
    public async Task<IActionResult> AddPartToServiceOrder(Guid id, [FromBody] AddPartDto dto)
    {
        var result = await _addPartToServiceOrderUseCase.ExecuteAsync(new AddPartToServiceOrderRequest(id, dto));
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Adiciona um serviço a ordem de serviço",
                      Description = "Adiciona um serviço a uma ordem de serviço existente.")]
    [HttpPost("{id}/services")]
    public async Task<IActionResult> AddServiceToServiceOrder(Guid id, [FromBody] AddServiceDto dto)
    {
        var result = await _addServiceToServiceOrderUseCase.ExecuteAsync(new AddServiceToServiceOrderRequest(id, dto));
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Aprova uma ordem de serviço",
                      Description = "Move uma ordem de serviço para o status de execução, indicando que foi aprovada.")]
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveServiceOrder(Guid id)
    {
        var result = await _approveServiceOrderUseCase.ExecuteAsync(new ApproveServiceOrderRequest(id));
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Finaliza a execução de uma ordem de serviço",
                      Description = "Move uma ordem de serviço para o status de finalizada, indicando que a execução foi concluída.")]
    [HttpPost("{id}/finish-execution")]
    public async Task<IActionResult> FinishExecution(Guid id)
    {
        var result = await _finishExecutionUseCase.ExecuteAsync(new FinishExecutionRequest(id));
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Entrega uma ordem de serviço",
                      Description = "Move uma ordem de serviço para o status de entregue, indicando que foi entregue ao cliente.")]
    [HttpPost("{id}/deliver")]
    public async Task<IActionResult> DeliverServiceOrder(Guid id)
    {
        var result = await _deliverServiceOrderUseCase.ExecuteAsync(new DeliverServiceOrderRequest(id));
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }


    [SwaggerOperation(Summary = "Recusa uma ordem de serviço",
                  Description = "Altera uma ordem de serviço para o status de recusado, indicando que foi recusado pelo cliente.")]
    [HttpPost("{id}/refuse")]
    public async Task<IActionResult> RefuseServiceOrder(Guid id)
    {
        var result = await _refuseServiceOrderUseCase.ExecuteAsync(new RefuseServiceOrderRequest(id));
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }


    [SwaggerOperation(Summary = "Lista os estoques pendentes da ordem de serviço",
                      Description = "Retorna uma lista de estoques pendentes relacionados a uma ordem de serviço.")]
    [HttpGet("{id}/pending-stocks")]
    public async Task<IActionResult> GetServiceOrderPendingStocks(Guid id)
    {
        var result = await _getServiceOrderPendingStocksUseCase.ExecuteAsync(id);
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return Ok(result.Response);
    }

    [SwaggerOperation(Summary = "Relatorio de duração média de um serviço",
                        Description = "Retorna a duração média de um tipo de serviço com base nas ordens de serviço finalizadas.")]
    [HttpGet("average-duration")]
    public async Task<IActionResult> GetAverageDuration()
    {
        var result = await _getAverageDurationUseCase.ExecuteAsync(new NoInput());
        if (!result.IsSuccess) return BadRequest(new { message = string.Join(", ", result.Messages) });
        return Ok(new { AverageDurationInDays = result.Response });
    }
}
