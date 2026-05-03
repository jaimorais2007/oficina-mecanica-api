using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OficinaApi.Application.DTOs;
using OficinaApi.Application.Interfaces;
using OficinaApi.Application.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace OficinaApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [SwaggerOperation(Summary = "Lista todos os clientes cadastrados",
                          Description = "Retorna uma lista com todos os clientes registrados no sistema.")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customer = await _customerService.GetAllCustomersAsync();
            return Ok(customer);
        }

        [SwaggerOperation(Summary = "Busca cliente por ID",
                          Description = "Retorna os dados de um cliente específico a partir do seu identificador único.")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound();
            return Ok(customer);
        }

        [SwaggerOperation(Summary = "Cria um novo cliente",
                          Description = "Cadastra um novo cliente no sistema com os dados informados no corpo da requisição.")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
        {
            var result = await _customerService.CreateCustomerAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [SwaggerOperation(Summary = "Atualiza os dados de um cliente",
                          Description = "Atualiza as informações de um cliente existente a partir do seu identificador único.")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto)
        {
            var result = await _customerService.UpdateCustomerAsync(id, dto);
            return Ok(result);
        }

        [SwaggerOperation(Summary = "Remove um cliente",
                          Description = "Exclui permanentemente o cadastro de um cliente a partir do seu identificador único.")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _customerService.DeleteCustomerAsync(id);
            return NoContent();
        }
    }
}
