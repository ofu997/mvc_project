using System;
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
using MailKit.Net.Smtp;
using MimeKit;
using System.Net;
using System.Net.Mail;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
//using Azure;

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

            var userEmail = User.Claims.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

              return _context.Response != null ? 
                          View(await _context.Response.OrderByDescending(x => x.Date).Where(x => x.UserName == userEmail).ToListAsync()) :
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
        public async Task<IActionResult> Create([Bind("UserName,Prompt")] Response response)
        {
            if (ModelState.IsValid)
            {
                string apiKey = Decipher( new int[] 
                    { 8, 16, 53, 54, 59, 10, // sk-16q
                    52, 51, 60, 16, 36, 24, // AB7kQc
                    43, 59, 35, 55, 36, 17, // J6R2Qj
                    27, 39, 31, 62, 13, 33, // ZNV9nT
                    56, 51, 15, 25, 16, 47, // 3BlbkF
                    43, 62, 48, 11, 49, 18, // J9EqDi J9EpDi
                    62, 19, 52, 27, 3, 28, // 9hAZxY
                   8, 38, 57, 2, 9, 49, 37, // sO4yrDP 
                    57, 33 } // 4T
                );

                DateTime currentTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
                response.Date = currentTime;

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
                _context.Add(response);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }


            return View(response);
        }

        public string Decipher(int[] encryption)
        {
            Dictionary<int, char> Encryptions = new Dictionary<int, char>()
            {
                { 1,'z' }, { 27, 'Z'},
                { 2, 'y' }, { 28, 'Y'},
                { 3, 'x' }, { 29, 'X'},
                { 4, 'w' }, { 30, 'W'},
                { 5, 'v' }, { 31, 'V'},
                { 6, 'u' }, { 32, 'U'},
                { 7, 't' }, { 33, 'T'},
                { 8, 's' }, { 34, 'S'},
                { 9, 'r' }, { 35, 'R'},
                { 10, 'q' }, { 36, 'Q'},
                { 11, 'p' }, { 37, 'P'},
                { 12, 'o' }, { 38, 'O'},
                { 13, 'n' }, { 39, 'N'},
                { 14, 'm' }, { 40, 'M'},
                { 15, 'l' }, { 41, 'L'},
                { 16, 'k' }, { 42, 'K'},
                { 17, 'j' }, { 43, 'J'},
                { 18, 'i' }, { 44, 'I'},
                { 19, 'h' }, { 45, 'H'},
                { 20, 'g' }, { 46, 'G'},
                { 21, 'f' }, { 47, 'F'},
                { 22, 'e' }, { 48, 'E'},
                { 23, 'd' }, { 49, 'D'},
                { 24, 'c' }, { 50, 'C'},
                { 25, 'b' }, { 51, 'B'},
                { 26, 'a' }, { 52, 'A'},

                { 53,  '-'},
                { 54, '1' },
                { 55, '2' },
                { 56, '3' },
                { 57, '4' },
                { 58, '5' },
                { 59, '6' },
                { 60, '7' },
                { 61, '8' },
                { 62, '9' },
                { 63, '0' },
                { 64, '.' }
            };
            var resultString = "";
            foreach (var ch in encryption)
            {
                resultString += Encryptions[ch];
            }
            return resultString; 
        }

        // GET: Responses/SendChat/5
        public async Task<IActionResult> SendChat(Guid? id)
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

        // POST: Responses/SendChat/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendChat(Guid id, [Bind("Id")] Response formResponse)
        {
            if (id != formResponse.Id)
            {
                return NotFound();
            }


                try
                {
                    var response = await _context.Response
                    .FirstOrDefaultAsync(m => m.Id == id);
                string userName = "trees73@gmail.com";
                    string password = "puvqmjarhklfsbnu";

                    string subject = response?.Prompt.Length > 50 ? string.Concat(response.Prompt.AsSpan(0, 49), "...") : response.Prompt;

                    ICredentialsByHost credentials = new NetworkCredential(userName, password);

                    System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient()
                    {
                        Host = "smtp.gmail.com",
                        Credentials = credentials,
                        Port = 587,
                        EnableSsl = true
                    };

                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(userName);
                    mail.To.Add(string.IsNullOrEmpty(response.UserName)? userName : response.UserName);
                    mail.Subject = $"Your requested AI chat";
                    mail.Body = $"{response.Prompt} \r\n {response.Result}";

                    smtpClient.Send(mail);

                    response.Sent = true;
                    _context.Update(response);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResponseExists(formResponse.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));


            //using (var client = new SmtpClient())
            //{
            //    //client.Connect = "";
            //    client.Connect("smtp.gmail.com");
            //    client.Authenticate("trees73@gmail.com", 
            //        Decipher(new int[] { 8, 22, 18, 8, 22, 18, 20, 19, 7, 64})
            //    );

            //    var bodyBuilder = new BodyBuilder
            //    {
            //        HtmlBody = $"<p>{response.Prompt}</p><p>{response.Result}</p>",
            //        TextBody = $"<p>{response.Prompt}</p> \r\n <p>{response.Result}</p>",
            //    };

            //    var message = new MimeMessage
            //    {
            //        Body = bodyBuilder.ToMessageBody()
            //    };
            //    message.From.Add(new MailboxAddress("No reply (sendchat)", "trees73@gmail.com"));
            //    message.To.Add(new MailboxAddress("testing01", response.UserName));
            //    message.Subject = "Your requested AI chat";
            //    client.Send(message);

            //    client.Disconnect(true); 
            //}
        }

        public async Task<IActionResult> SendChatFunction (Guid? id)
        {
            var response = await _context.Response
                .FirstOrDefaultAsync(m => m.Id == id);

            if (response == null)
            {
                return NotFound();
            }
            else
            {
                string userName = "trees73@gmail.com";
                string password = "puvqmjarhklfsbnu";
                string subject = response.Prompt.Length > 50 ? string.Concat(response.Prompt.AsSpan(0, 49), "...") : response.Prompt;
                ICredentialsByHost credentials = new NetworkCredential(userName, password);

                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    Credentials = credentials,
                    Port = 587,
                    EnableSsl = true
                };

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(userName);
                mail.To.Add(string.IsNullOrEmpty(response.UserName) ? userName : response.UserName);
                mail.Subject = "Your requested AI chat : ";
                mail.Body = $"{response.Prompt} \r\n {response.Result}";

                smtpClient.Send(mail);
            }

            return RedirectToAction(nameof(Index));
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
