using System;
using System.Diagnostics;
using FamilyTreeLibrary.FamilyTreeStore;
//using FamilyStudioData.FamilyData;
using FamilyTreeWebTools.Data;
using FamilyTreeWebTools.Services;
using FamilyTreeWebApp.Services;
using FamilyTreeTools.CompareResults;
using FamilyTreeTools.FamilyTreeSanityCheck;

namespace FamilyTreeWebApp.Controllers
{
  public class FileAnalysis
  {
    private static readonly TraceSource trace = new TraceSource("FileAnalysis", SourceLevels.Information);
    private IProgressReporterInterface progressReporter;
    private AsyncFamilyTreeWorkerClass analyseTreeWorker;
    private int currentProgress;

    public FileAnalysis()
    {      
    }
    public void LoadCompletedCallback(Boolean result)
    {
      trace.TraceData(TraceEventType.Information, 0, "File loaded result = " + result);
    }

    public void StartNewJob(UserInformation userInfo, Analysis analysis, WebAppIdentity appId, EmailSendSource sendSource)
    {
      trace.TraceData(TraceEventType.Information, 0, "Trying to start job " + analysis.Id);
      if (analysis.StartPersonXref != null)
      {
        progressReporter = new AsyncWorkerProgress(analysis.Id, CompletenessProgress, StopRequestHandler);
        WebAuthentication authenticationClass = null;
        if (analysis.FileName == "Geni.com")
        {
          trace.TraceData(TraceEventType.Information, 0, "Web job");

          if (userInfo != null)
          {
            trace.TraceData(TraceEventType.Information, 0, "Updated geni tokens");
            authenticationClass = new WebAuthentication(userInfo.UserId, appId.AppId, appId.AppSecret, FamilyDbContextClass.UpdateGeniAuthentication);
            //authenticationClass.geniAuthentication.SetUserId(userInfo.UserId);
            authenticationClass.geniAuthentication.UpdateAuthenticationData(userInfo.GeniAccessToken, userInfo.GeniRefreshToken, Convert.ToInt32(userInfo.GeniExpiresIn), userInfo.GeniAuthenticationTime);
          }
        }
        
        FamilyWebTree webTree = new FamilyWebTree(analysis.FileName, authenticationClass);

        AnalysisSettings settings = AnalysisSettings.FromJson(analysis.Settings);

        SanityCheckLimits limits;
        limits = new SanityCheckLimits();
        limits.endYear.value = settings.EndYear;
        limits.generationsBack = settings.GenerationsBack;
        limits.generationsForward = settings.GenerationsForward;

        //FamilyTreeStoreBaseClass familyTree;
        //familyTree = webTree.GetFamilyTree();

        AncestorStatistics stats;
        stats = new AncestorStatistics(webTree.GetFamilyTree(), limits, progressReporter);

        analyseTreeWorker = new AsyncFamilyTreeWorkerClass(progressReporter, analysis, appId, sendSource, stats);
        trace.TraceData(TraceEventType.Information, 0, "Job " + analysis.Id + " started");
      }
    }


    private void CompletenessProgress(int jobId, int progressPercent, string text = null)
    {
      if (progressPercent >= currentProgress + 10)
      {
        trace.TraceData(TraceEventType.Information, 0, " Job " + jobId + " progress " + progressPercent + "%");
        currentProgress = progressPercent;
      }
      ProgressDbClass.Instance.UpdateProgress(jobId, progressPercent);

      if (progressPercent < 0)
      {
        //TODO: How can we extract updated geni tokens from genicodec to database after a long run...?
        trace.TraceData(TraceEventType.Information, 0, " Job " + jobId + " progress " + progressPercent + "% (finished)");
      }
      if (progressPercent >= 100)
      {
        trace.TraceData(TraceEventType.Information, 0, " Job " + jobId + " progress " + progressPercent + "% (finished)");
        if (analyseTreeWorker != null)
        {
          analyseTreeWorker.Dispose();
        }
      }
    }

    private bool StopRequestHandler(int id)
    {
      return ProgressDbClass.Instance.StopRequested(id);
    }
  }
 
}
