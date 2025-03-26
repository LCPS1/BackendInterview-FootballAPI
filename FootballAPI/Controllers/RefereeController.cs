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
    public class RefereeController : ControllerBase
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ILogger<RefereeController> _logger;

        public RefereeController(
            IRepositoryFactory repositoryFactory,
            ILogger<RefereeController> logger)
        {
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route("")]
        public ActionResult<IEnumerable<Referee>> Get()
        {
            //TODO: Implement DTOs to avoid exposing domain entities directly
            //TODO: Add pagination for large result sets
            
            try
            {
                var repository = _repositoryFactory.CreateRepository<Referee>();
                var referees = repository.GetAll();
                return Ok(referees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving referees");
                return StatusCode(500, "An error occurred while retrieving referees");
            }
        }

        [HttpGet]
        [Route("{id}", Name = "GetRefereeById")]
        public async Task<ActionResult> GetById(int id)
        {
            //TODO: Use DTOs for data transfer instead of domain entities
            
            try
            {
                var repository = _repositoryFactory.CreateRepository<Referee>();
                var referee = await repository.GetByIdAsync(id);
                
                if (referee == null)
                    return NotFound();
                
                return Ok(referee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving referee with ID: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the referee");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(Referee referee)
        {
            //TODO: Validate input data before saving
            //TODO: Use DTOs for input and output
            
            try
            {
                if (referee == null)
                    return BadRequest("Referee cannot be null");
                
                var repository = _repositoryFactory.CreateRepository<Referee>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var result = await repository.AddAsync(referee);
                await unitOfWork.SaveChangesAsync();
                
                return CreatedAtRoute(
                    "GetRefereeById", 
                    new { id = result.Id }, 
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating referee");
                return StatusCode(500, "An error occurred while creating the referee");
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Update(int id, Referee referee)
        {
            //TODO: Use DTOs for input and output
            //TODO: Add validation for referee data
            
            try
            {
                if (referee == null)
                    return BadRequest("Referee cannot be null");

                if (id != referee.Id)
                    return BadRequest("ID mismatch between route and entity");
                
                var repository = _repositoryFactory.CreateRepository<Referee>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var exists = await repository.GetByIdAsync(id);
                if (exists == null)
                    return NotFound();

                await repository.UpdateAsync(referee);
                await unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating referee with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the referee");
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var repository = _repositoryFactory.CreateRepository<Referee>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var referee = await repository.GetByIdAsync(id);
                if (referee == null)
                    return NotFound();

                await repository.DeleteAsync(id);
                await unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting referee with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the referee");
            }
        }
    }
        
}
