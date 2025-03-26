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
    public class PlayerController : ControllerBase
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ILogger<PlayerController> _logger;

        public PlayerController(
            IRepositoryFactory repositoryFactory,
            ILogger<PlayerController> logger)
        {
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [Route("")]
        public ActionResult<IEnumerable<Player>> Get()
        {
            //TODO: Implement DTOs to avoid exposing domain entities directly
            //TODO: Add pagination for large collections
            
            try
            {
                var repository = _repositoryFactory.CreateRepository<Player>();
                var players = repository.GetAll();
                return Ok(players);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving players");
                return StatusCode(500, "An error occurred while retrieving players");
            }
        }

        [HttpGet]
        [Route("{id}", Name = "GetPlayerById")]
        public async Task<ActionResult> GetById(int id)
        {
            //TODO: Use DTOs for data transfer instead of domain entities
            
            try
            {
                var repository = _repositoryFactory.CreateRepository<Player>();
                var player = await repository.GetByIdAsync(id);
                
                if (player == null)
                    return NotFound();
                
                return Ok(player);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving player with ID: {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the player");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Post(Player player)
        {
            //TODO: Validate input data before saving
            //TODO: Use DTOs for input and output
            
            try
            {
                if (player == null)
                    return BadRequest("Player cannot be null");
                
                var repository = _repositoryFactory.CreateRepository<Player>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var result = await repository.AddAsync(player);
                await unitOfWork.SaveChangesAsync();
                
                return CreatedAtRoute(
                    "GetPlayerById", 
                    new { id = result.Id }, 
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating player");
                return StatusCode(500, "An error occurred while creating the player");
            }
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Update(int id, Player player)
        {
            //TODO: Use DTOs for input and output
            //TODO: Add validation for player data
            
            try
            {
                if (player == null)
                    return BadRequest("Player cannot be null");

                if (id != player.Id)
                    return BadRequest("ID mismatch between route and entity");
                
                var repository = _repositoryFactory.CreateRepository<Player>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var exists = await repository.GetByIdAsync(id);
                if (exists == null)
                    return NotFound();

                await repository.UpdateAsync(player);
                await unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating player with ID: {Id}", id);
                return StatusCode(500, "An error occurred while updating the player");
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var repository = _repositoryFactory.CreateRepository<Player>();
                var unitOfWork = _repositoryFactory.CreateUnitOfWork();
                
                var player = await repository.GetByIdAsync(id);
                if (player == null)
                    return NotFound();

                await repository.DeleteAsync(id);
                await unitOfWork.SaveChangesAsync();
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting player with ID: {Id}", id);
                return StatusCode(500, "An error occurred while deleting the player");
            }
        }
    }
}
