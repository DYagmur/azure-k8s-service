﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace SampleApp.Pages
{
    [IgnoreAntiforgeryToken(Order = 1001)]//Not a real anti-forgery token, but we don't want to send it to the server
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        public IEnumerable<WeatherForecast> Forecasts { get; private set; } = new List<WeatherForecast>();
        public IEnumerable<string> Files { get; private set; } = new List<string>();

        public string Environment { get; set; }

        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _configuration = configuration;

        }

        public async Task OnGet()
        {
            try
            {
                Environment = _configuration["ASPNETCORE_ENVIRONMENT"];
                var request = new HttpRequestMessage(HttpMethod.Get, "weatherforecast");
                var client = _clientFactory.CreateClient("backend");

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseStream = await response.Content.ReadAsStringAsync();
                    Forecasts = JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(responseStream, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"!!!API ERROR!!! - {ex.Message}");
            }

            LoadFiles();
        
        }

        public async Task OnPostAsync()
        {
            try
            {
                if (UploadedFile == null || UploadedFile.Length == 0)
                {
                    return;
                }
                _logger.LogInformation($"Uploading {UploadedFile.FileName}.");
                
                string targetFileName = $"{Path.Join(_configuration.GetValue<string>("UploadDirectory"))}/{UploadedFile.FileName}";

                using (var stream = new FileStream(targetFileName, FileMode.Create))
                {
                    await UploadedFile.CopyToAsync(stream);
                }
            }
            catch (System.Exception ex)
            {
                
                _logger.LogError($"!!!UPLOAD ERROR!!! - {ex.Message}");
            }

        }

        private void LoadFiles(){
            try
            {
                
                Files = Directory.GetFiles($"{Path.Join(_configuration.GetValue<string>("UploadDirectory"))}");
 
            }
            catch (System.Exception ex)
            {
                
                _logger.LogError($"!!!FILE LIST ERROR!!! - {ex.Message}");;
            }
        }
    }
}
