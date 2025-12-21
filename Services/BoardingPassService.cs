using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Services;

namespace Ticket_Booking.Services
{
    /// <summary>
    /// Service for generating boarding passes as PDF and sending them via email
    /// </summary>
    public class BoardingPassService : IBoardingPassService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly MailService _mailService;
        private readonly IConfiguration _configuration;
        private const string BOARDING_PASS_STORAGE_PATH = "boarding-passes";
        private const string DEFAULT_GATE = "A04";

        public BoardingPassService(
            IWebHostEnvironment environment,
            MailService mailService,
            IConfiguration configuration)
        {
            _environment = environment;
            _mailService = mailService;
            _configuration = configuration;
            
            // Set QuestPDF license (free for non-commercial use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Generates a PDF boarding pass for a ticket
        /// </summary>
        public async Task<string> GenerateBoardingPassAsync(Ticket ticket)
        {
            if (string.IsNullOrEmpty(ticket.PNR))
            {
                throw new ArgumentException("Ticket must have a PNR code to generate boarding pass.");
            }

            // Ensure ticket has related data loaded
            if (ticket.Trip == null || ticket.User == null)
            {
                throw new InvalidOperationException("Ticket must have Trip and User loaded.");
            }

            // Create directory structure: wwwroot/boarding-passes/{PNR}/
            var pnrFolder = Path.Combine(_environment.WebRootPath ?? "", BOARDING_PASS_STORAGE_PATH, ticket.PNR);
            Directory.CreateDirectory(pnrFolder);

            // Generate filename: boarding-pass-{PNR}-{timestamp}.pdf
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var fileName = $"boarding-pass-{ticket.PNR}-{timestamp}.pdf";
            var filePath = Path.Combine(pnrFolder, fileName);
            var relativePath = Path.Combine(BOARDING_PASS_STORAGE_PATH, ticket.PNR, fileName).Replace("\\", "/");

            // Calculate times
            var departureTime = ticket.Trip.DepartureTime;
            var arrivalTime = departureTime.AddMinutes(ticket.Trip.EstimatedDuration);
            var boardingTime = departureTime.AddMinutes(-45);
            var gate = DEFAULT_GATE; // Can be made configurable later

            // Generate PDF using QuestPDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Content()
                        .Column(column =>
                        {
                            // Header: Airline/Company Name
                            column.Item()
                                .PaddingBottom(10)
                                .AlignCenter()
                                .Text(ticket.Trip.Company.Name)
                                .FontSize(18)
                                .Bold();

                            // Title: BOARDING PASS
                            column.Item()
                                .PaddingBottom(15)
                                .AlignCenter()
                                .Text("BOARDING PASS")
                                .FontSize(24)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);

                            // Passenger Information Section
                            column.Item()
                                .PaddingBottom(10)
                                .BorderBottom(1)
                                .Column(passengerColumn =>
                                {
                                    passengerColumn.Item().Text("PASSENGER").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                    passengerColumn.Item().Text(ticket.User.FullName).FontSize(14).Bold();
                                    passengerColumn.Item().Text(ticket.User.Email).FontSize(10).FontColor(Colors.Grey.Darken1);
                                });

                            // Flight Information Section
                            column.Item()
                                .PaddingVertical(10)
                                .Row(row =>
                                {
                                    row.RelativeColumn().Column(flightColumn =>
                                    {
                                        flightColumn.Item().Text("FROM").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                        flightColumn.Item().Text(ticket.Trip.FromCity).FontSize(12).Bold();
                                        flightColumn.Item().Text(departureTime.ToString("dd MMM yyyy")).FontSize(9);
                                        flightColumn.Item().Text(departureTime.ToString("HH:mm")).FontSize(11).Bold();
                                    });

                                    row.RelativeColumn().Column(arrowColumn =>
                                    {
                                        arrowColumn.Item().AlignCenter().Text("→").FontSize(20).Bold();
                                    });

                                    row.RelativeColumn().Column(flightColumn =>
                                    {
                                        flightColumn.Item().Text("TO").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                        flightColumn.Item().Text(ticket.Trip.ToCity).FontSize(12).Bold();
                                        flightColumn.Item().Text(arrivalTime.ToString("dd MMM yyyy")).FontSize(9);
                                        flightColumn.Item().Text(arrivalTime.ToString("HH:mm")).FontSize(11).Bold();
                                    });
                                });

                            // Details Grid
                            column.Item()
                                .PaddingVertical(10)
                                .BorderTop(1)
                                .BorderBottom(1)
                                .Grid(grid =>
                                {
                                    grid.Columns(3);

                                    grid.Item().Column(detailColumn =>
                                    {
                                        detailColumn.Item().Text("SEAT").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                        detailColumn.Item().Text(ticket.SeatNumber).FontSize(14).Bold().FontColor(Colors.Blue.Darken2);
                                        detailColumn.Item().Text(ticket.SeatClass.ToString()).FontSize(9);
                                    });

                                    grid.Item().Column(detailColumn =>
                                    {
                                        detailColumn.Item().Text("GATE").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                        detailColumn.Item().Text(gate).FontSize(14).Bold();
                                    });

                                    grid.Item().Column(detailColumn =>
                                    {
                                        detailColumn.Item().Text("BOARDING").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                        detailColumn.Item().Text(boardingTime.ToString("HH:mm")).FontSize(14).Bold();
                                    });
                                });

                            // Booking Information
                            column.Item()
                                .PaddingVertical(10)
                                .Row(infoRow =>
                                {
                                    infoRow.RelativeColumn().Column(infoColumn =>
                                    {
                                        infoColumn.Item().Text("PNR").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                        infoColumn.Item().Text(ticket.PNR ?? "N/A").FontSize(12).Bold().FontFamily("Courier");
                                    });

                                    infoRow.RelativeColumn().Column(infoColumn =>
                                    {
                                        infoColumn.Item().Text("BOOKING ID").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                        infoColumn.Item().Text($"#{ticket.Id.ToString("D6")}").FontSize(12).Bold();
                                    });

                                    infoRow.RelativeColumn().Column(infoColumn =>
                                    {
                                        infoColumn.Item().Text("FLIGHT").FontSize(8).Bold().FontColor(Colors.Grey.Darken1);
                                        infoColumn.Item().Text(ticket.Trip.PlaneName).FontSize(12).Bold();
                                    });
                                });

                            // QR Code (if available)
                            if (!string.IsNullOrEmpty(ticket.QrCode))
                            {
                                column.Item()
                                    .PaddingTop(10)
                                    .AlignCenter()
                                    .Width(150)
                                    .Height(150)
                                    .Image($"https://api.qrserver.com/v1/create-qr-code/?size=150x150&data={ticket.QrCode}")
                                    .FitArea();
                            }

                            // Footer
                            column.Item()
                                .PaddingTop(15)
                                .AlignCenter()
                                .Text("Please arrive at the gate at least 30 minutes before departure")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Darken1)
                                .Italic();
                        });
                });
            });

            // Generate and save PDF
            document.GeneratePdf(filePath);

            return relativePath;
        }

        /// <summary>
        /// Sends boarding pass via email to the ticket holder
        /// </summary>
        public async Task SendBoardingPassEmailAsync(Ticket ticket, string boardingPassPath)
        {
            if (string.IsNullOrEmpty(ticket.User?.Email))
            {
                throw new ArgumentException("Ticket must have a valid user email.");
            }

            var fullPath = Path.Combine(_environment.WebRootPath ?? "", boardingPassPath);
            if (!System.IO.File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Boarding pass file not found: {fullPath}");
            }

            // Create email with attachment
            var subject = $"Your Boarding Pass - {ticket.PNR}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Your Boarding Pass is Ready!</h2>
                    <p>Dear {ticket.User.FullName},</p>
                    <p>Your boarding pass for flight {ticket.Trip?.FromCity} to {ticket.Trip?.ToCity} is attached.</p>
                    <p><strong>PNR Code:</strong> {ticket.PNR}</p>
                    <p><strong>Seat:</strong> {ticket.SeatNumber} ({ticket.SeatClass})</p>
                    <p><strong>Departure:</strong> {ticket.Trip?.DepartureTime:dd MMM yyyy HH:mm}</p>
                    <p>Please arrive at the gate at least 30 minutes before departure.</p>
                    <p>Safe travels!</p>
                </body>
                </html>";

            // Send email with attachment using async method
            using var smtpClient = new System.Net.Mail.SmtpClient(_configuration["Smtp:Host"])
            {
                Port = int.Parse(_configuration["Smtp:Port"] ?? "587"),
                Credentials = new System.Net.NetworkCredential(_configuration["Smtp:Username"], _configuration["Smtp:Password"]),
                EnableSsl = true,
            };

            using var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(_configuration["Smtp:FromEmail"] ?? "noreply@skybooking.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(ticket.User.Email);

            // Attach boarding pass PDF
            using var attachment = new System.Net.Mail.Attachment(fullPath);
            attachment.ContentDisposition.FileName = $"BoardingPass-{ticket.PNR}.pdf";
            mailMessage.Attachments.Add(attachment);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}

