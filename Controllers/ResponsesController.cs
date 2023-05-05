﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Exp.Data;
using Exp.Models;
using Microsoft.AspNetCore.Authorization;
using OpenAI_API.Completions;
using OpenAI_API;
using Microsoft.Identity.Client;

namespace Exp.Controllers
{
    public class ResponsesController : Controller
    {
        private readonly ExpContext _context;

        public ResponsesController(ExpContext context)
        {
            _context = context;
        }

        // GET: Responses
        [Authorize]
        public async Task<IActionResult> Index()
        {
              return _context.Response != null ? 
                          View(await _context.Response.ToListAsync()) :
                          Problem("Entity set 'ExpContext.Response'  is null.");
        }

        // GET: Responses/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Response == null)
            {
                return NotFound();
            }

            var response = await _context.Response
                .FirstOrDefaultAsync(m => m.Id == id);
            if (response == null)
            {
                return NotFound();
            }

            return View(response);
        }

        // GET: Responses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Responses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,UserName,Prompt")] Response response)
        {
            if (ModelState.IsValid)
            {
                string apiKey = "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";
                string answer = string.Empty;
                var openai = new OpenAIAPI(apiKey);
                CompletionRequest completion = new CompletionRequest();
                completion.Prompt = response.Prompt;
                completion.Model = OpenAI_API.Models.Model.DavinciText;
                completion.MaxTokens = 4000;
                var result = openai.Completions.CreateCompletionsAsync(completion);
                if (result != null)
                {
                    foreach (var item in result.Result.Completions)
                    {
                        answer = item.Text;
                    }
                }

                response.Result = answer;

                response.Id = Guid.NewGuid();
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            _context.Add(response);

            return View(response);
        }

        // GET: Responses/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Response == null)
            {
                return NotFound();
            }

            var response = await _context.Response.FindAsync(id);
            if (response == null)
            {
                return NotFound();
            }
            return View(response);
        }

        // POST: Responses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Date,UserName,Result")] Response response)
        {
            if (id != response.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(response);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResponseExists(response.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(response);
        }

        // GET: Responses/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Response == null)
            {
                return NotFound();
            }

            var response = await _context.Response
                .FirstOrDefaultAsync(m => m.Id == id);
            if (response == null)
            {
                return NotFound();
            }

            return View(response);
        }

        // POST: Responses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Response == null)
            {
                return Problem("Entity set 'ExpContext.Response'  is null.");
            }
            var response = await _context.Response.FindAsync(id);
            if (response != null)
            {
                _context.Response.Remove(response);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ResponseExists(Guid id)
        {
          return (_context.Response?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        public IActionResult Test()
        {
            return View();
        }
    }
}
