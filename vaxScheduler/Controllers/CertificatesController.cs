﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using vaxScheduler.Data;
using vaxScheduler.Data.Model;
using vaxScheduler.models;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificatesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CertificatesController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpPost("upload")]
public IActionResult UploadCertificate(IFormFile file, string userId)
{
    if (file == null || file.Length == 0)
    {
        return BadRequest("No file uploaded");
    }
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    var maxFileSize = 10 * 1024 * 1024;

    if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
    {
        return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and GIF files are allowed.");
    }

    if (file.Length > maxFileSize)
    {
        return BadRequest("File size exceeds the limit of 10MB.");
    }
    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
    var filePath = Path.Combine( fileName); // Use the class-level variable directly here

    try
    {
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
        var certificate = new Certificate { Name = fileName, FilePath = filePath, AppUserId = userId };
        _context.Certificates.Add(certificate);
        _context.SaveChanges();

        return Ok(new { message = "Certificate uploaded successfully", certificateId = certificate.Id });
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save certificate information to the database.");
    }
}




        [HttpGet("{userId}/photo")]
        public IActionResult GetCertificatePhotoByUserId(string userId)
        {
            var certificate = _context.Certificates.FirstOrDefault(c => c.AppUserId == userId);

            if (certificate == null)
            {
                return NotFound("Certificate not found for the specified user ID.");
            }
            var fileBytes = System.IO.File.ReadAllBytes(certificate.FilePath);
            return File(fileBytes, "image/jpeg");
        }
    }
}