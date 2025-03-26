using FootballAPI.Core.Entities;
using Microsoft.AspNetCore.Identity;
using FootballAPI.Infraestructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FootballAPI.Core.Interfaces.Factories;

namespace FootballAPI.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(
            IRepositoryFactory repositoryFactory,
            ILogger<ManagerController> logger)
        {
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route("")]
        public ActionResult<IEnumerable<Manager>> Get()
        {
            //TODO: Implement DTOs to avoid exposing domain entities directly
            //TODO: Add pagination for large result sets
            
            try
            {
                var repository = _repositoryFactory.CreateRepository<Manager>();
                var managers = repository.GetAll();
                return Ok(managers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving managers");
                return StatusCode(500, "An error occurred while retrieving managers");
            }
        }

        [HttpGet]
        [Route("{id}", Name = "GetManagerById")]
        public async Task<ActionResult> GetById(int id)
        {
            //TODO: Use DTOs for data transfer instead of domain entities
            
            try
            {
                var repository = _repositoryFactory.CreateRepository<Manager>();
                var manager = await repository.GetByIdAsync(id);
                
                if (manager == null)
                    return NotFound();
                
                return Ok(manager);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manager with ID: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the manager");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(Manager manager)
        {
            //TODO: Validate input data before saving
            //TODO: Use DTOs for input and output
            
            try
            {
                if (manager == null)
                    return BadRequest("Manager cannot be null");
                
                var repository = _repositoryFactory.CreateRepository<Manager>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var result = await repository.AddAsync(manager);
                await unitOfWork.SaveChangesAsync();
                
                return CreatedAtRoute(
                    "GetManagerById", 
                    new { id = result.Id }, 
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manager");
                return StatusCode(500, "An error occurred while creating the manager");
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Update(int id, Manager manager)
        {
            //TODO: Use DTOs for input and output
            //TODO: Add validation for manager data
            
            try
            {
                if (manager == null)
                    return BadRequest("Manager cannot be null");

                if (id != manager.Id)
                    return BadRequest("ID mismatch between route and entity");
                
                var repository = _repositoryFactory.CreateRepository<Manager>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var exists = await repository.GetByIdAsync(id);
                if (exists == null)
                    return NotFound();

                await repository.UpdateAsync(manager);
                await unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating manager with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the manager");
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var repository = _repositoryFactory.CreateRepository<Manager>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var manager = await repository.GetByIdAsync(id);
                if (manager == null)
                    return NotFound();

                await repository.DeleteAsync(id);
                await unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting manager with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the manager");
            }
        }
    }
}
