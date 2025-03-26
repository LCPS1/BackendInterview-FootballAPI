using FootballAPI.Core.Entities;
using Microsoft.AspNetCore.Identity;
using FootballAPI.Infraestructure.Data;
using Microsoft.AspNetCore.Mvc;
using FootballAPI.Core.Interfaces;
using FootballAPI.Core.Interfaces.Factories;

namespace FootballAPI.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ILogger<MatchController> _logger;

        public MatchController(
            IRepositoryFactory repositoryFactory,
            ILogger<MatchController> logger)
        {
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route("")]
        public ActionResult<IEnumerable<Match>> Get()
        {
            //TODO: Implement DTOs to avoid exposing domain entities directly
            //TODO: Add pagination for large result sets
            
            try
            {
                var repository = _repositoryFactory.CreateRepository<Match>();
                var matches = repository.GetAll();
                return Ok(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving matches");
                return StatusCode(500, "An error occurred while retrieving matches");
            }
        }

        [HttpGet]
        [Route("{id}", Name = "GetMatchById")]
        public async Task<ActionResult> GetById(int id)
        {
            //TODO: Use DTOs for data transfer instead of domain entities
            
            try
            {
                var repository = _repositoryFactory.CreateRepository<Match>();
                var match = await repository.GetByIdAsync(id);
                
                if (match == null)
                    return NotFound();
                
                return Ok(match);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving match with ID: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the match");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(Match match)
        {
            //TODO: Validate input data before saving
            //TODO: Use DTOs for input and output
            //TODO: Verify that referenced entities exist
            
            try
            {
                if (match == null)
                    return BadRequest("Match cannot be null");
                
                var repository = _repositoryFactory.CreateRepository<Match>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var result = await repository.AddAsync(match);
                await unitOfWork.SaveChangesAsync();
                
                return CreatedAtRoute(
                    "GetMatchById", 
                    new { id = result.Id }, 
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating match");
                return StatusCode(500, "An error occurred while creating the match");
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Update(int id, Match match)
        {
            //TODO: Use DTOs for input and output
            //TODO: Add validation for match data (dates, status, etc.)
            
            try
            {
                if (match == null)
                    return BadRequest("Match cannot be null");

                if (id != match.Id)
                    return BadRequest("ID mismatch between route and entity");
                
                var repository = _repositoryFactory.CreateRepository<Match>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var exists = await repository.GetByIdAsync(id);
                if (exists == null)
                    return NotFound();

                await repository.UpdateAsync(match);
                await unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating match with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the match");
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var repository = _repositoryFactory.CreateRepository<Match>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var match = await repository.GetByIdAsync(id);
                if (match == null)
                    return NotFound();

                await repository.DeleteAsync(id);
                await unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting match with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the match");
            }
        }

       
    }
}
