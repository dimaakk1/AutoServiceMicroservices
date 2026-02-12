using AutoServiceUsers.BLL.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoServiceUsers.BLL.Services
{
    public class EmailService : IEmailService
    {
        public Task SendEmailAsync(string to, string subject, string body)
        {
            Console.WriteLine("====== EMAIL ======");
            Console.WriteLine($"TO: {to}");
            Console.WriteLine($"SUBJECT: {subject}");
            Console.WriteLine($"BODY:\n{body}");
            Console.WriteLine("===================");

            return Task.CompletedTask;
        }
    }
}
