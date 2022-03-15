using Ekmansoft.FamilyTree.Library.FamilyData;
using Ekmansoft.FamilyTree.Library.FamilyTreeStore;
using Ekmansoft.FamilyTree.Tools.FamilyTreeSanityCheck;
using Ekmansoft.FamilyTree.WebApp.Data;
using Ekmansoft.FamilyTree.WebTools.Data;
using Ekmansoft.FamilyTree.WebTools.Email;
using Ekmansoft.FamilyTree.WebTools.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
//using System.Text.Json;
using System.IO;
using System.Text.Json;

namespace Ekmansoft.FamilyTree.WebApp.Services
{
  public class AsyncFamilyTreeWorkerClass : IAsyncWorkerProgressInterface
  {
    private readonly BackgroundWorker backgroundWorker;
    private DateTime startTime;
    private readonly AncestorStatistics stats;
    //private readonly SanityCheckLimits limits;
    private readonly IProgressReporterInterface progressReporter;
    private static readonly TraceSource trace = new TraceSource("AsyncFamilyTreeWorkerClass", SourceLevels.Information);
    private IndividualClass startperson;
    private AnalysisSettings settings;
    private readonly Analysis analysis;
    private string shortTreeInfo;
    //private readonly int _jobId;
    private readonly WebAppIdentity _appId;
    private readonly EmailSendSource _emailSendSource;

    public AsyncFamilyTreeWorkerClass(
      IProgressReporterInterface progress,
      Analysis analysis,
      WebAppIdentity appId,
      EmailSendSource emailSendSource,
      AncestorStatistics _stats)
    {
      progressReporter = progress;
      this.stats = _stats;
      //this.limits = limits;
      //this._jobId = jobId;
      this.analysis = analysis;
      this._appId = appId;
      this._emailSendSource = emailSendSource;
      shortTreeInfo = "";


      backgroundWorker = new BackgroundWorker();

      backgroundWorker.WorkerReportsProgress = true;
      backgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
      backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Completed);
      backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);

      backgroundWorker.RunWorkerAsync();
    }

    public void DoWork(object sender, DoWorkEventArgs eventArgs)
    {
      IFamilyTreeStoreBaseClass familyTree = stats.GetFamilyTree();

      // This method will run on a thread other than the UI thread.
      // Be sure not to manipulate any Windows Forms controls created
      // on the UI thread from this method.
      startTime = DateTime.Now;

      if (familyTree == null)
      {
        trace.TraceData(TraceEventType.Warning, 0, " familytree is null at job " + analysis.Id);
        return;

      }

      /*Stream myFile = File.Create("/tmp/webapp1_job_" + _jobId + ".log");

      TextWriterTraceListener myTextListener = new TextWriterTraceListener(myFile);
      Trace.Listeners.Add(myTextListener);

      // Write output to the file.
      Trace.Write("Test output ");*/

      progressReporter.ReportProgress(0);

      try
      {
        using (FamilyTreeDbContext context = new FamilyTreeDbContext())
        {
          settings = AnalysisSettings.FromJson(analysis.Settings);
          startperson = familyTree.GetIndividual(analysis.StartPersonXref);
          string jsonAnalysisFilename = null;
          string kmlAnalysisFilename = null;
          string gedcomFilename = null;
          string tempFileDirectory = "/tmp/";
          if (!Directory.Exists(tempFileDirectory))
          {
            tempFileDirectory = "";
          }

          if ((settings != null) && (settings.CheckWholeFile))
          {
            List<string> profileXrefs = new List<string>();

            IEnumerator<IndividualClass> profiles = stats.GetFamilyTree().SearchPerson(null);

            while (profiles.MoveNext())
            {
              profileXrefs.Add(profiles.Current.GetXrefName());
            }
            trace.TraceInformation(analysis.Id + " Loaded {0} profiles", profileXrefs.Count);

            stats.CheckProfileList(profileXrefs);
            settings.StartPersonName = "";
            settings.StartPersonXref = "";
            settings.GenerationsBack = -1;
            settings.GenerationsForward = -1;
          }
          else if (startperson != null)
          {
            trace.TraceInformation("Start job " + analysis.Id + " " + 
                                    settings.StartPersonName + " " + 
                                    settings.GenerationsBack + " " + 
                                    settings.GenerationsForward + " " + 
                                    settings.EndYear + " by " + 
                                    analysis.UserEmail);
            stats.AnalyseTree(startperson);
            if (stats.Stopped())
            {
              trace.TraceInformation(analysis.Id + " Job was stopped...");
              Analysis tempAnalysis = context.Analyses.Find(analysis.Id);

              if ((tempAnalysis != null) && (tempAnalysis.StartCount == 1000))
              {
                trace.TraceInformation(analysis.Id + " Job was paused...");
                stats.GetFamilyTree().Dispose();
                return;
              }
            }
          }
          else
          {
            trace.TraceInformation(analysis.Id + " Error no start person in tree analysis...");
            return;
          }
          trace.TraceInformation(stats.GetFamilyTree().GetShortTreeInfo());

          //ICollection<AncestorLineInfo> problemsList = stats.GetAncestorList();


          //AncestorStatistics.AnalysisStatistics work = stats.GetStats();


          //FamilyDbContextClass.JobInfo info = new FamilyDbContextClass.JobInfo();
          //JobInfo info = new JobInfo();

          JobInfo info = stats.GetJobInfo(analysis.Id);

          trace.TraceInformation(analysis.Id + ": work done = " + info.Profiles + " profiles and " + 
            info.Families + " families" + ", found " + info.IssueList.Count + " problem profiles");

          /*info.Profiles = work.people;
          info.Families = work.families;
          info.StartTime = startTime;
          info.EndTime = DateTime.Now;
          info.JobId = _jobId;
          info.IssueList = problemsList;*/

          //FileStream jsonFile = File.Create("/tmp/job_" + _jobId);
          //jsonFile.(JsonConvert.SerializeObject(info));


          FamilyDbContextClass.MarkJobFinished(context, info);

          if (settings.ExportJson)
          {
            jsonAnalysisFilename = "job_" + analysis.Id + "_result.json";
            using (StreamWriter writer = new StreamWriter(tempFileDirectory + FamilyUtility.MakeFilename(jsonAnalysisFilename)))
            {
              writer.Write(JsonSerializer.Serialize(info));

              writer.Close();
            }
            trace.TraceInformation(analysis.Id + ": Intermediate Json file stored:" + jsonAnalysisFilename);
          }

          if (settings.UpdateDatabase)
          {
            FamilyDbContextClass.DecodeProblemsList(context, info);
            trace.TraceInformation(analysis.Id + ": Database updated");
          }

          if (settings.ExportGedcom)
          {
            gedcomFilename = "job_" + analysis.Id + "_export.ged";
            FamilyWebTree.ExportGedcom(stats.GetFamilyTree(), tempFileDirectory + FamilyUtility.MakeFilename(gedcomFilename));

            FamilyDbContextClass.UpdateExportFilename(analysis.Id, context, tempFileDirectory + FamilyUtility.MakeFilename(gedcomFilename));
            trace.TraceInformation(analysis.Id + ": Gedcom exported " + gedcomFilename);
          }

          if (settings.ExportKml)
          {
            kmlAnalysisFilename = "job_" + analysis.Id + "_map.kml";
            string mapFile = MapExportClass.CreateMapFile(stats.GetFamilyTree());

            using (StreamWriter writer = new StreamWriter(tempFileDirectory + FamilyUtility.MakeFilename(kmlAnalysisFilename)))
            {
              writer.Write(mapFile);

              writer.Close();
            }

            trace.TraceInformation(analysis.Id + ": KML exported " + kmlAnalysisFilename);
          }

          if (settings.SendEmail && !string.IsNullOrEmpty(analysis.UserEmail))
          {
            string emailContents = EmailExportClass.ExportHtml(info, settings);
            string analysisType = null;
            List<string> attachmentFilenames = new List<string>();

            if (!settings.CheckWholeFile)
            {
              analysisType = settings.StartPersonName + " " + 
                              settings.GenerationsBack + "/" + 
                              settings.GenerationsForward + "/" + 
                              settings.EndYear;
            }
            else
            {
              analysisType = "whole file";
            }
            if (gedcomFilename != null)
            {
              attachmentFilenames.Add(tempFileDirectory + FamilyUtility.MakeFilename(gedcomFilename));
            }
            if (jsonAnalysisFilename != null)
            {
              attachmentFilenames.Add(tempFileDirectory + FamilyUtility.MakeFilename(jsonAnalysisFilename));
            }
            if (kmlAnalysisFilename != null)
            {
              attachmentFilenames.Add(tempFileDirectory + FamilyUtility.MakeFilename(kmlAnalysisFilename));
            }
            SendMailClass.SendMail(_emailSendSource.Address, 
                                    _emailSendSource.CredentialAddress, 
                                    _emailSendSource.CredentialPassword, 
                                    analysis.UserEmail, 
                                    "Analysis " + analysis.OriginalFilename + " " + analysisType, 
                                    emailContents, 
                                    attachmentFilenames);
          }
          else
          {
            trace.TraceData(TraceEventType.Warning, 0, "no email sent " + analysis.UserEmail);
          }
        }
        if (stats.GetFamilyTree() != null)
        {
          shortTreeInfo = stats.GetFamilyTree().GetShortTreeInfo();
          stats.GetFamilyTree().Dispose();
        }
        else
        {
          trace.TraceData(TraceEventType.Warning, 0, "AnalyseTreeWorker::DoWork() warning tree is null " + analysis.Id + " " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + " to " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }
        trace.TraceInformation("AnalyseTreeWorker::DoWork()" + analysis.Id + " " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + " to " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //progressReporter.Completed();
      }
      catch (Exception ex)
      {
        trace.TraceData(TraceEventType.Error, 0, "db update failed " + ex.ToString());
        //SendMailClass.SendMail("improveyourtree@gmail.com", analysis.UserEmail, "tree analysis: db update failed ", ex.ToString());
        //SendMailClass.SendMail("improveyourtree@gmail.com", "improveyourtree@gmail.com", "tree analysis: db update failed ", ex.ToString());
      }
      shortTreeInfo = stats.GetFamilyTree().GetShortTreeInfo();
      trace.TraceInformation("AnalyseTreeWorker::DoWork()" + analysis.Id + " " + startTime.ToString("yyyy-MM-dd HH:mm:ss") + " to " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
      stats.GetFamilyTree().Dispose();
    }

    public void ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      trace.TraceData(TraceEventType.Information, 0, "ProgressChanged(" + e.ProgressPercentage + ")" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

      if (stats.GetFamilyTree() != null)
      {
        progressReporter.ReportProgress(e.ProgressPercentage, stats.GetFamilyTree().GetShortTreeInfo());
      }
      else
      {
        progressReporter.ReportProgress(e.ProgressPercentage, shortTreeInfo);
      }
    }
    public void Completed(object sender, RunWorkerCompletedEventArgs e)
    {
      trace.TraceData(TraceEventType.Information, 0, "Completed() " + analysis.Id + " (" + startTime.ToString("yyyy-MM-dd HH:mm:ss") + " - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ")");

      stats.Print();
      FamilyTreeDbContext context = new FamilyTreeDbContext();
      FamilyDbContextClass.StartupCheck(context, _appId, _emailSendSource);
      progressReporter.Completed(shortTreeInfo);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (backgroundWorker != null)
        {
          backgroundWorker.DoWork -= new DoWorkEventHandler(DoWork);
          backgroundWorker.RunWorkerCompleted -= new RunWorkerCompletedEventHandler(Completed);
          backgroundWorker.ProgressChanged -= new ProgressChangedEventHandler(ProgressChanged);
          backgroundWorker.Dispose();
        }
      }
    }
  }

}

