using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Dto;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly RestaurantContext _context;

        public TransactionController(RestaurantContext context)
        {
            _context = context;
        }

        // GET: api/Transaction
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            return await _context.Transactions.ToListAsync();
        }

        // GET: api/Transaction/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }

        // PUT: api/Transaction/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int? id, TransactionRequest req)
        {
            if (id == null || id <= 0)
            {
                return BadRequest("Invalid or missing id.");
            }

            var existingTransaction = await _context.Transactions.FindAsync(id);
            var food = await _context.Foods.FindAsync(req.FoodId);

            if (existingTransaction == null || food == null)
            {
                return NotFound();
            }

            int quantityDifference = req.Qty - existingTransaction.Qty;
            food.Stock -= quantityDifference;

            if (food.Stock < 0)
            {
                return BadRequest("Insufficient stock available.");
            }

            existingTransaction.CustomerId = req.CustomerId;
            existingTransaction.FoodId = req.FoodId;
            existingTransaction.Qty = req.Qty;
            existingTransaction.TotalPrice = food.Price * req.Qty;

            _context.Entry(existingTransaction).State = EntityState.Modified;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transaction.RollbackAsync();
                    if (!TransactionExists(id.Value))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return NoContent();
        }

        // POST: api/Transaction
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction(TransactionRequest req)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var food = await _context.Foods.FindAsync(req.FoodId);

                if (food == null || food.Stock < req.Qty)
                {
                    return BadRequest("Not enough stock available.");
                }

                var newTransaction = new Transaction
                {
                    CustomerId = req.CustomerId,
                    FoodId = req.FoodId,
                    Qty = req.Qty,
                    TotalPrice = food.Price * req.Qty,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                food.Stock -= req.Qty;

                _context.Transactions.Add(newTransaction);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return CreatedAtAction("GetTransaction", new { id = newTransaction.Id }, newTransaction);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/Transaction/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}
