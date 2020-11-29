using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {

        public GenresController(IApplicationDbContext dbContext, IApplicationReadDbConnection readDbConnection, IApplicationWriteDbConnection writeDbConnection)
        {
            _dbContext = dbContext;
            _readDbConnection = readDbConnection;
            _writeDbConnection = writeDbConnection;
        }

        public IApplicationDbContext _dbContext { get; }
        public IApplicationReadDbConnection _readDbConnection { get; }
        public IApplicationWriteDbConnection _writeDbConnection { get; }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = $"select * from genres";
            var genres = await _readDbConnection.QueryAsync<Genres>(query);
            return Ok(genres);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var genres = await _dbContext.Genres.Where(a => a.GenreId == id).ToListAsync();
            return Ok(genres);
        }

        [HttpGet("name")]
        public async Task<IActionResult> GetByName([FromBody] GenresDto name)
        {
            var genres = await _dbContext.Genres.Where(a => a.Name == name.Name).ToListAsync();
            if (genres.Count == 0) return NotFound(new { msg = "No Data Found" });
            return Ok(genres);
        }


        [HttpPost]
        public async Task<IActionResult> AddNew(GenresDto genresDto)
        {
            _dbContext.Connection.Open();
            using (var transaction = _dbContext.Connection.BeginTransaction())
            {
                try
                {
                    _dbContext.Database.UseTransaction(transaction as DbTransaction);
                    bool GenresExists = await _dbContext.Genres.AnyAsync(a => a.Name == genresDto.Name);
                    if (GenresExists) return BadRequest(new { msg = "Genres Already Exists" });


                    var addGenresQuery = $"INSERT INTO genres(Name) VALUES('{genresDto.Name}');SELECT last_insert_rowid()";
                    var genresId = await _writeDbConnection.QuerySingleAsync<int>(addGenresQuery, transaction: transaction);

                    if (genresId == 0) return BadRequest(new { msg = "Genres could not be inserted" });

                    await _dbContext.SaveChangesAsync(default);
                    transaction.Commit();
                    return Ok(new { id = genresId });
                }
                catch (Exception exp)
                {
                    transaction.Rollback();
                    return StatusCode((int)HttpStatusCode.InternalServerError, new { msg = "unexpected error occurred." + exp.Message });
                }
                finally
                {
                    _dbContext.Connection.Close();
                }
            }
        }


    }
}
