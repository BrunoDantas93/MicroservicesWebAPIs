using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MicroservicesHelpers.Services;
public class InstagramServices
{
    private IInstaApi _instaApi;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public InstagramServices(IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<bool> Login(string username, string password)
    {

        try
        {
            _instaApi = InstaApiBuilder.CreateBuilder()
                .SetUser(new UserSessionData { UserName = username, Password = password })
                .Build();

            var loginResult = await _instaApi.LoginAsync();

            return loginResult.Succeeded;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<bool> PostImage(IFormFile imageFile, string caption)
    {
        try
        {
            if (_instaApi == null || !_instaApi.IsUserAuthenticated)
            {
                throw new InvalidOperationException("Usuário não autenticado. Realize o login antes de postar.");
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("Nenhum arquivo de imagem fornecido.");
            }

            using (var stream = new MemoryStream())
            {
                await imageFile.CopyToAsync(stream);

                var mediaImage = new InstaImageUpload
                {
                    ImageBytes = stream.ToArray()
                };

                var result = await _instaApi.MediaProcessor.UploadPhotoAsync(mediaImage, caption);

                if (result.Succeeded)
                {
                    Console.WriteLine("Imagem postada com sucesso!");
                }
                else
                {
                    Console.WriteLine($"Falha ao postar imagem. Erro: {result.Info.Message}");
                }

                return result.Succeeded;
            }


            // Salva temporariamente a imagem no servidor
            //var tempImagePath = SaveTempImage(imageFile);

            //var tempPath = Path.Combine(_webHostEnvironment.ContentRootPath, "TempImages");
            //Directory.CreateDirectory(tempPath);

            //var tempFileName = $"{Guid.NewGuid()}_temp_image.jpg";
            //var tempFilePath = Path.Combine(tempPath, tempFileName);

            //using (var stream = new FileStream(tempFilePath, FileMode.Create))
            //{
            //    imageFile.CopyTo(stream);
            //}

            //var mediaImage = new InstaImageUpload
            //{
            //    ImageBytes = await File.ReadAllBytesAsync(tempFilePath)
            //};

            //var result = await _instaApi.MediaProcessor.UploadPhotoAsync(mediaImage, caption);

            //return result.Succeeded;

            //var mediaImage = new InstaImageUpload
            //{
            //   // Uri = $"C:\\app\\TempImages\\{tempFileName}",
            //    ImageBytes = await File.ReadAllBytesAsync($"\\app\\TempImages\\{tempFileName}"),
            //};
            //// Add user tag (tag people)
            //mediaImage.UserTags.Add(new InstaUserTagUpload
            //{

            //    Username = "webapisgrp8",
            //    X = 0.5,
            //    Y = 0.5
            //});

            //var result = await _instaApi.MediaProcessor.UploadPhotoAsync(mediaImage, caption);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    //public async Task<bool> PostImage(IFormFile imageFile, string caption)
    //{
    //    try
    //    {
    //        if (_instaApi == null || !_instaApi.IsUserAuthenticated)
    //        {
    //            throw new InvalidOperationException("Usuário não autenticado. Realize o login antes de postar.");
    //        }

    //        using (var stream = new MemoryStream())
    //        {
    //            await imageFile.CopyToAsync(stream);

    //            var mediaImage = new InstaImageUpload
    //            {
    //                ImageBytes = stream.ToArray(),
    //            };

    //            var result = await _instaApi.MediaProcessor.UploadPhotoAsync(mediaImage, caption);

    //            return result.Succeeded;
    //        }
    //        //return result.Succeeded;
    //    }
    //    catch (Exception ex)
    //    {

    //        throw ex;
    //    }
    //}

    private string SaveTempImage(IFormFile imageFile)
    {
        var tempPath = Path.Combine("/app/TempImages"); // Caminho dentro do contêiner Docker
        Directory.CreateDirectory(tempPath);

        var tempFileName = $"{Guid.NewGuid()}_temp_image.jpg";
        var tempFilePath = Path.Combine(tempPath, tempFileName);

        using (var stream = new FileStream(tempFilePath, FileMode.Create))
        {
            imageFile.CopyTo(stream);
        }

        return tempFilePath;
    }


    private async Task<byte[]> DownloadImageBytesAsync(string imageUrl)
    {
        using (var httpClient = new HttpClient())
        {
            return await httpClient.GetByteArrayAsync(imageUrl);
        }
    }

}
