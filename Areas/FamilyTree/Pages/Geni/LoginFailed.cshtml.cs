using Ekmansoft.FamilyTree.WebApp.Data;
using Ekmansoft.FamilyTree.WebApp.Services;
using Ekmansoft.FamilyTree.WebTools.Data;
using Ekmansoft.FamilyTree.WebTools.Services;
//using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FamilyTreeServices.Pages
{
  [Authorize]
  public class GeniLoginFailedModel : PageModel
  {
    private static readonly TraceSource trace = new TraceSource("GeniLoginFailed", SourceLevels.Information);
    //private static GeniAppAuthenticationClass appAuthentication = new GeniAppAuthenticationClass();
    private readonly WebAppIdentity _appId;
    private readonly FamilyTreeDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private static HttpClient httpClient;
    private static HttpClientHandler clientHandler;
    public string Message { get; set; }

    public GeniLoginFailedModel(FamilyTreeDbContext context, UserManager<IdentityUser> userManager, WebAppIdentity appId)
    {
      _context = context;
      _userManager = userManager;
      _appId = appId;
    }

    private class HttpAppAuthenticationResponse
    {
      public string access_token { get; set; }
      public string refresh_token { get; set; }
      public int expires_in { get; set; }
    }

    //[TempData]
    //public string geni_access_token { get; set; }
    //[TempData]
    //public string geni_expires_in { get; set; }

    public IActionResult OnGet(string code, string expires_in, string state, string message, string redirectTarget)
    {
      Message = "Logging into Geni....";
      //geni_expires_in = expires_in;
      //geni_access_token = access_token;
      trace.TraceData(TraceEventType.Information, 0, "GeniLoginFailedModel.OnGet() code:" +
        code + " expires_in: " + expires_in + " state: " + state + " message: " + message);

      if (string.IsNullOrEmpty(code))
      {
        code = HttpContext.Session.GetString("geni_code");
        expires_in = HttpContext.Session.GetString("token_expires_in");

        trace.TraceData(TraceEventType.Warning, 0, "Error code is empty (1):" + code);
        if (string.IsNullOrEmpty(code))
        {
          return Redirect("./Login");
        }
        trace.TraceData(TraceEventType.Warning, 0, "Hmm code is almost empty:" + code);
      }
      else
      {
        HttpContext.Session.SetString("geni_code", code);
        HttpContext.Session.SetString("geni_access_token", "");
        HttpContext.Session.SetString("token_expires_in", expires_in);
        HttpContext.Session.SetString("GedcomFilename", "");
        HttpContext.Session.SetString("OriginalFilename", "");
      }
      string retryCounter = HttpContext.Session.GetString("RetryCounter");

      if (string.IsNullOrEmpty(retryCounter))
      {
        retryCounter = "1";
      }
      else
      {
        Message += " (retry " + retryCounter + ")";
        int retryCounterValue = Convert.ToInt32(retryCounter);
        retryCounterValue++;

        retryCounter = retryCounterValue.ToString();
      }
      HttpContext.Session.SetString("RetryCounter", retryCounter);
      /*if (refresh_token != null)
      {
        HttpContext.Session.SetString("geni_refresh_token", refresh_token);
      }*/

      //string redirectUrl = "/FamilyTree/Analyze/Settings";
      string redirectUrl = "https://improveyourtree.com/FamilyTree/Geni/LoginOk";

      WebAuthentication appAuthentication = new WebAuthentication(_userManager.GetUserId(this.User), _appId.AppId, _appId.AppSecret, FamilyDbContextClass.UpdateGeniAuthentication);

      string redirectTo = appAuthentication.getRequestTokenUrl(code, redirectUrl);

      bool Ok = false;
      int retryCount = 0;
      string returnLine = null;
      do
      {
        try
        {
          clientHandler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
          clientHandler.UseDefaultCredentials = false;
          clientHandler.Credentials = CredentialCache.DefaultCredentials;
          clientHandler.AllowAutoRedirect = true;

          httpClient = new HttpClient(clientHandler);

          //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Uri.EscapeDataString(appAuthentication.GetAccessToken()));
          httpClient.DefaultRequestHeaders.Add("accept-encoding", "gzip,deflate");

          //WebRequest webRequestGetUrl;
          //webRequestGetUrl = WebRequest.Create(redirectTo);

          Task<HttpResponseMessage> responseTask = httpClient.GetAsync(redirectTo);
          HttpResponseMessage response = responseTask.Result;
          ICollection<string> headers = response.Content.Headers.ContentEncoding;
          Stream stream = null;

          if (headers.Contains("gzip"))
          {
            stream = new GZipStream(response.Content.ReadAsStream(), CompressionMode.Decompress);
          }
          else if (headers.Contains("deflate"))
          {
            stream = new DeflateStream(response.Content.ReadAsStream(), CompressionMode.Decompress);
          }
          else
          {
            stream = response.Content.ReadAsStream();
          }
          StreamReader objReader = new StreamReader(stream);

          returnLine = objReader.ReadToEnd();
          objReader.Dispose();
          Ok = true;
        }
        catch (WebException we)
        {
          Message += " Web:" + retryCount.ToString();
          trace.TraceData(TraceEventType.Warning, 0, "Error webException:" + retryCount + " " + we.ToString());
          //HttpContext.Session.SetString("geni_code", "");
          Thread.Sleep(2000);
          //return Redirect("./Login");
        }
        catch (IOException ioe)
        {
          Message += " IO:" + retryCount.ToString();
          trace.TraceData(TraceEventType.Warning, 0, "Error ioException:" + retryCount + " " + ioe.ToString());
          Thread.Sleep(2000);

        }
        catch (Exception e)
        {
          Message += " e:" + retryCount.ToString();
          trace.TraceData(TraceEventType.Warning, 0, "Error unknown Exception:" + retryCount + " " + e.ToString());
          Thread.Sleep(2000);

        }
        retryCount++;
        if (retryCount == 5)
        {
          HttpContext.Session.SetString("geni_code", "");
          return Redirect("./Login");
        }
      } while (!Ok && retryCount < 5);

      HttpAppAuthenticationResponse appAuthenticationResponse = null;

      try
      {
        appAuthenticationResponse = JsonSerializer.Deserialize<HttpAppAuthenticationResponse>(returnLine);

      }
      catch (JsonException e)
      {
        trace.TraceData(TraceEventType.Error, 0, "GeniLoginFailedModel.OnGet() json parse failed " + returnLine.Length + ": "+ returnLine);
        trace.TraceData(TraceEventType.Error, 0, "GeniLoginFailedModel.OnGet() json parse failed " + e.ToString());

      }
      catch (Exception e)
      {
        trace.TraceData(TraceEventType.Error, 0, "GeniLoginFailedModel.OnGet() failed " + returnLine);
        trace.TraceData(TraceEventType.Error, 0, "GeniLoginFailedModel.OnGet() failed " + e.ToString());

      }


      if (appAuthenticationResponse != null)
      {
        trace.TraceData(TraceEventType.Information, 0, "GeniLoginFailedModel.OnGet() get auth " + appAuthenticationResponse.access_token + " refreshToken:" + appAuthenticationResponse.refresh_token + " expiresIn:" + appAuthenticationResponse.expires_in);
        if (!String.IsNullOrEmpty(appAuthenticationResponse.access_token))
        {
          HttpContext.Session.SetString("geni_access_token", appAuthenticationResponse.access_token);
        }
        if (!String.IsNullOrEmpty(appAuthenticationResponse.refresh_token))
        {
          HttpContext.Session.SetString("geni_refresh_token", appAuthenticationResponse.refresh_token);
        }
        HttpContext.Session.SetString("token_expires_in", appAuthenticationResponse.expires_in.ToString());
        HttpContext.Session.SetString("geni_code", "");

        string curUser = _userManager.GetUserId(User);

        FamilyDbContextClass.SaveGeniAuthentication(_context, curUser, appAuthenticationResponse.access_token, appAuthenticationResponse.refresh_token, appAuthenticationResponse.expires_in);
      }


      //ViewData["access_token"] = access_token;
      //ViewData["expires_in"] = expires_in;
      if (redirectTarget == null)
      {
        redirectTarget = "/FamilyTree/Analyze/Settings";
      }

      string AlternativeRedirect = HttpContext.Session.GetString("RedirectAfterGeniLogin");

      if ((AlternativeRedirect != null) && (AlternativeRedirect.Length > 0))
      {
        redirectTarget = AlternativeRedirect;
        trace.TraceData(TraceEventType.Information, 0, "GeniLoginFailedModel.OnGet() redirect to " + redirectTarget);
        HttpContext.Session.SetString("RedirectAfterGeniLogin", "");
      }

      return Redirect(redirectTarget);

    }
    public void OnPost()
    {
      //Message = "Your Login to Geni. post" ;
      trace.TraceData(TraceEventType.Information, 0, "GeniLoginFailedModel.OnPost()");
      //trace.TraceData(TraceEventType.Information, 0, "Geni LoginOk post " + Message);
    }
  }
}
