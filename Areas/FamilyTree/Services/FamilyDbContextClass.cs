using Ekmansoft.FamilyTree.Tools.FamilyTreeSanityCheck;
using Ekmansoft.FamilyTree.WebApp.Controllers;
using Ekmansoft.FamilyTree.WebApp.Data;
//using Newtonsoft.Json;
using Ekmansoft.FamilyTree.WebTools.Data;
using Ekmansoft.FamilyTree.WebTools.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ekmansoft.FamilyTree.WebApp.Services
{

  public class FamilyDbContextClass
  {
    private static readonly TraceSource trace = new TraceSource("FamilyDbContext", SourceLevels.Information);

    public static void StartupCheck(FamilyTreeDbContext _context, WebAppIdentity appId, EmailSendSource sendSource)
    {
      FamilyTreeDbContext context = null;
      trace.TraceData(TraceEventType.Information, 0, "StartupCheck");
      if (_context != null)
      {
        context = _context;
      }
      else
      {
        context = new FamilyTreeDbContext();
        trace.TraceData(TraceEventType.Information, 0, "create context...");
      }
      using (context)
      {
        IList<int> jobs = FamilyDbContextClass.GetUnfinishedJobs(context);
        //int jobsStarted = 0;
        int MaxJobsToStart = FamilyDbContextClass.GetMaxJobs(context);

        //Thread.Sleep(2000);
        foreach (int jobId in jobs)
        {
          Analysis analysis = context.Analyses.Single<Analysis>(a => a.Id == jobId);

          if (analysis != null)
          {
            int jobsStarted = ProgressDbClass.Instance.GetNoOfJobs();

            int progress = ProgressDbClass.Instance.GetProgress(jobId);
            trace.TraceData(TraceEventType.Information, 0, jobsStarted + " jobs running... (max " + MaxJobsToStart + ")");


            if ((analysis.StartCount < 5) && (jobsStarted < MaxJobsToStart) && (progress < 0))
            {
              analysis.StartCount++;
              context.Analyses.Update(analysis);
              context.SaveChanges();
              FileAnalysis jobEngine = new FileAnalysis();

              UserInformation userInfo = FindUserInformation(context, analysis.UserId);
              jobEngine.StartNewJob(userInfo, analysis, appId, sendSource);
              jobsStarted++;
              //Thread.Sleep(20000);
            }
          }
        }
      }
    }

    static public WebAuthentication CreateWebAuthentication(FamilyTreeDbContext _context, string userId, WebAppIdentity appId)
    {
      UserInformation userInfo = FamilyDbContextClass.FindUserInformation(_context, userId);

      WebAuthentication authenticationClass = null;
      authenticationClass = new WebAuthentication(userId, appId.AppId, appId.AppSecret, UpdateGeniAuthentication);
      if (userInfo != null)
      {

        //authenticationClass.geniAuthentication.SetUserId(userId);
        authenticationClass.UpdateAuthenticationData(userInfo.GeniAccessToken, userInfo.GeniRefreshToken, userInfo.GeniExpiresIn, userInfo.GeniAuthenticationTime);
      }
      else
      {
        trace.TraceData(TraceEventType.Warning, 0, "user info not found in db");
      }
      return authenticationClass;
    }

    public static UserInformation FindUserInformation(FamilyTreeDbContext context, string userId)
    {
      UserInformation userInfo = context.UserInformations.Find(userId);

      return userInfo;
    }

    public static int CheckGeniLoginStatus(FamilyTreeDbContext context, string userId)
    {
      UserInformation userInfo = context.UserInformations.Find(userId);

      if (userInfo != null)
      {
        DateTime expiryTime = userInfo.GeniAuthenticationTime.AddSeconds(userInfo.GeniExpiresIn);
        TimeSpan diff = expiryTime - DateTime.Now;

        return Convert.ToInt32(diff.TotalSeconds);
      }

      return 0;
    }

    public static int GetMaxJobs(FamilyTreeDbContext context)
    {
      AppSetting appSettings = null;
      if (context.AppSettings.Any<AppSetting>())
      {
        appSettings = context.AppSettings.First<AppSetting>();
      }
      else
      {
        appSettings = new AppSetting();
        appSettings.MaxSimultaneousJobs = 3;
        context.AppSettings.Add(appSettings);
        context.SaveChanges();
      }
      return appSettings.MaxSimultaneousJobs;
    }

    //public static int GetHttpRequestTypeDb(FamilyTreeDbContext context)
    //{
    //  AppSetting appSettings = null;
    //  if (context.AppSettings.Any<AppSetting>())
    //  {
    //    appSettings = context.AppSettings.First<AppSetting>();
    //  }
    //  else
    //  {
    //    appSettings = new AppSetting();
    //    appSettings.HttpRequestType = 1;
    //    context.AppSettings.Add(appSettings);
    //    context.SaveChanges();
    //  }
    //  return appSettings.HttpRequestType;
    //}

    public static int GetHttpRequestType(FamilyTreeDbContext context)
    {
      return 1;
    }


    private static Issue.IssueType TranslateProblem(SanityCheckLimits.SanityProblemId id)
    {
      switch (id)
      {
        case SanityCheckLimits.SanityProblemId.parentLimitMin_e:
          return Issue.IssueType.ParentLimitMin;
        case SanityCheckLimits.SanityProblemId.motherLimitMax_e:
          return Issue.IssueType.MotherLimitMax;
        case SanityCheckLimits.SanityProblemId.fatherLimitMax_e:
          return Issue.IssueType.FatherLimitMax;
        case SanityCheckLimits.SanityProblemId.eventLimitMin_e:
          return Issue.IssueType.EventLimitMin;
        case SanityCheckLimits.SanityProblemId.eventLimitMax_e:
          return Issue.IssueType.EventLimitMax;
        case SanityCheckLimits.SanityProblemId.noOfChildrenMin_e:
          return Issue.IssueType.NoOfChildrenMin;
        case SanityCheckLimits.SanityProblemId.noOfChildrenMax_e:
          return Issue.IssueType.NoOfChildrenMax;
        case SanityCheckLimits.SanityProblemId.daysBetweenChildren_e:
          return Issue.IssueType.DaysBetweenChildren;
        case SanityCheckLimits.SanityProblemId.twins_e:
          return Issue.IssueType.Twins;
        case SanityCheckLimits.SanityProblemId.inexactBirthDeath_e:
          return Issue.IssueType.InexactBirthDeath;
        case SanityCheckLimits.SanityProblemId.unknownBirth_e:
          return Issue.IssueType.UnknownBirth;
        case SanityCheckLimits.SanityProblemId.unknownDeath_e:
          return Issue.IssueType.UnknownDeath;
        case SanityCheckLimits.SanityProblemId.unknownDeathEmigrated_e:
          return Issue.IssueType.UnknownDeathEmigrated;
        case SanityCheckLimits.SanityProblemId.parentsMissing_e:
          return Issue.IssueType.ParentsMissing;
        case SanityCheckLimits.SanityProblemId.missingMother_e:
          return Issue.IssueType.MissingMother;
        case SanityCheckLimits.SanityProblemId.missingFather_e:
        case SanityCheckLimits.SanityProblemId.missingFatherBastard_e:
          return Issue.IssueType.MissingFather;
        case SanityCheckLimits.SanityProblemId.parentsProblem_e:
          return Issue.IssueType.ParentsProblem;
        case SanityCheckLimits.SanityProblemId.marriageProblem_e:
          return Issue.IssueType.MarriageProblem;
        case SanityCheckLimits.SanityProblemId.missingWeddingDate_e:
          return Issue.IssueType.MissingWeddingDate;
        case SanityCheckLimits.SanityProblemId.missingPartner_e:
          return Issue.IssueType.MissingPartner;
        case SanityCheckLimits.SanityProblemId.missingPartnerMitigated_e:
          return Issue.IssueType.MissingPartnerMitigated;
        case SanityCheckLimits.SanityProblemId.generationlimited_e:
          return Issue.IssueType.GenerationLimited;
        case SanityCheckLimits.SanityProblemId.duplicateCheck_e:
          return Issue.IssueType.DuplicateCheck;
        case SanityCheckLimits.SanityProblemId.unknownSex_e:
          return Issue.IssueType.UnknownSex;
        case SanityCheckLimits.SanityProblemId.shortAddress_e:
          return Issue.IssueType.ShortAddress;
        case SanityCheckLimits.SanityProblemId.unknownGpsPosition_e:
          return Issue.IssueType.UnknownLocation;
        case SanityCheckLimits.SanityProblemId.oldPrivateProfile_e:
          return Issue.IssueType.OldPrivateProfile;
      }
      return Issue.IssueType.ParentLimitMin;
    }

    private static Profile.SexType TranslateSex(string sex)
    {
      switch (sex.ToLower())
      {
        case "woman":
          return Profile.SexType.Female;
        case "man":
          return Profile.SexType.Male;
        case "unknown":
          return Profile.SexType.Unknown;
      }
      return Profile.SexType.Unknown;
    }

    private static bool FindIssue(IEnumerable<Issue> oldIssues, Issue issue, ref int Id)
    {
      if (oldIssues == null)
      {
        return false;
      }
      foreach (Issue i in oldIssues)
      {
        if (i.Type == issue.Type)
        {
          if (i.Description.Equals(issue.Description))
          {
            Id = i.Id;
            return true;
          }
        }
      }
      return false;
    }

    /*private static void AddIssueLink(FamilyTreeDbContext context, int IssueId, Issue.IssueStatus Status, int AnalysisId)
    {
      IssueLink link = new IssueLink();
      link.IssueId = IssueId;
      link.Time = DateTime.Now;
      link.Status = Status;
      link.AnalysisId = AnalysisId;
      context.IssueLinks.Add(link);
    }*/

    static public UserInformation GetGeniAuthentication(FamilyTreeDbContext context, string userId)
    {
      if (context.UserInformations.Any<UserInformation>(u => u.UserId == userId))
      {
        return context.UserInformations.Single<UserInformation>(u => u.UserId == userId);
      }
      return null;
    }


    static public void SaveGeniAuthentication(FamilyTreeDbContext context, string userId, string GeniAccessToken, string GeniRefreshToken, int GeniExpiresIn)
    {
      UserInformation userInfo = null;
      bool create = false;

      userInfo = GetGeniAuthentication(context, userId);

      if (userInfo == null)
      {
        userInfo = new UserInformation();
        create = true;
        userInfo.UserId = userId;
      }

      userInfo.GeniAccessToken = GeniAccessToken;
      userInfo.GeniRefreshToken = GeniRefreshToken;
      userInfo.GeniExpiresIn = GeniExpiresIn;
      userInfo.GeniAuthenticationTime = DateTime.Now;

      SaveGeniAuthentication(context, userInfo, create);
    }

    static public void SaveGeniAuthentication(FamilyTreeDbContext context, UserInformation userInfo, bool create)
    {
      if (create)
      {
        context.UserInformations.Add(userInfo);
      }
      else
      {
        context.UserInformations.Update(userInfo);
        context.Entry(userInfo).State = EntityState.Modified;
      }
      context.SaveChanges();
    }

    static public void UpdateGeniAuthentication(string userId, string GeniAccessToken, string GeniRefreshToken, int GeniExpiresIn)
    {
      using (FamilyTreeDbContext context = new FamilyTreeDbContext())
      {
        SaveGeniAuthentication(context, userId, GeniAccessToken, GeniRefreshToken, GeniExpiresIn);
      }
    }
    static public UserInformation GetGeniAuthentication(string userId)
    {
      using (FamilyTreeDbContext context = new FamilyTreeDbContext())
      {
        return GetGeniAuthentication(context, userId);
      }
    }


    public static IList<int> GetUnfinishedJobs(FamilyTreeDbContext context)
    {
      IList<int> jobList = new List<int>();
      IQueryable<Analysis> jobs = context.Analyses;

      foreach (Analysis job in jobs)
      {
        if (job.EndTime.Year == 1)
        {
          jobList.Add(job.Id);
          trace.TraceData(TraceEventType.Warning, 0, "job hasn't finished:" + job.Id + ", started " + job.StartCount + " times");
        }
      }
      return jobList;
    }

    public static int SaveJobDescription(FamilyTreeDbContext context, AnalysisSettings settings, string userId, string userEmail, string origFilename, string filepath)
    {
      Analysis analysis = new Analysis();

      analysis.StartTime = DateTime.Now;
      analysis.UserId = userId;
      analysis.UserEmail = userEmail;
      analysis.Settings = AnalysisSettings.ToJson(settings);

      analysis.StartPersonName = settings.StartPersonName;
      analysis.StartPersonXref = settings.StartPersonXref;
      analysis.ParentId = 0;
      analysis.Results = "";

      if ((filepath != null) && (filepath.Length > 0))
      {
        analysis.OriginalFilename = origFilename;
        analysis.FileName = filepath;

        FamilyTreeStorage tree = new FamilyTreeStorage();

        tree.Name = analysis.OriginalFilename;
        tree.Filename = analysis.FileName;
        tree.Type = (int)FamilyTreeStorage.TreeType.GedcomFile;
        tree.Url = "";
        context.Add(tree);
        context.SaveChanges();

        analysis.FamilyTreeId = tree.Id;
      }
      else
      {
        analysis.OriginalFilename = "Geni.com";
        analysis.FileName = "Geni.com";
        IQueryable<FamilyTreeStorage> trees = context.FamilyTrees.Where(f => f.Type == (int)FamilyTreeStorage.TreeType.GeniDotCom);

        if ((trees == null) || (trees.Count<FamilyTreeStorage>() == 0))
        {
          FamilyTreeStorage tree = new FamilyTreeStorage();

          tree.Name = analysis.OriginalFilename;
          tree.Filename = analysis.OriginalFilename;
          tree.Type = (int)FamilyTreeStorage.TreeType.GeniDotCom;
          tree.Url = "geni.com";
          context.Add(tree);
          context.SaveChanges();
          analysis.FamilyTreeId = tree.Id;
        }
        else
        {
          foreach (FamilyTreeStorage geniTree in trees)
          {
            analysis.FamilyTreeId = geniTree.Id;
          }
        }
      }
      context.Add(analysis);
      context.SaveChanges();

      return analysis.Id;
    }

    public static void UpdateExportFilename(int _jobId, FamilyTreeDbContext context, string filename)
    {
      if (filename != null)
      {
        Analysis analysis = context.Analyses.Find(_jobId);

        if (analysis != null)
        {
          AnalysisResults results = AnalysisResults.FromJson(analysis.Results);
          if (results != null)
          {
            results.ExportedGedcomName = filename;
            analysis.Results = AnalysisResults.ToJson(results);
            context.Analyses.Update(analysis);
            context.SaveChanges();
          }
          else
          {
            trace.TraceData(TraceEventType.Error, 0, "UpdateExportFilename():Read Analysis id " + _jobId + " but could not decode Results from [" + analysis.Results + "]");
          }
        }
        else
        {
          trace.TraceData(TraceEventType.Error, 0, "UpdateExportFilename():Error finding Analysis id " + _jobId);
        }
      }
    }

    public static void MarkJobFinished(FamilyTreeDbContext context, JobInfo info)
    {
      Analysis analysis = context.Analyses.Find(info.JobId);

      if (analysis != null)
      {
        trace.TraceData(TraceEventType.Information, 0, "Updating Job " + analysis.UserEmail + " " + analysis.OriginalFilename + " " + analysis.FileName + " " + analysis.Id);

        analysis.EndTime = info.EndTime;

        AnalysisResults results = new AnalysisResults();

        results.SearchedProfiles = info.Profiles;
        results.SearchedFamilies = info.Families;
        results.StartTime = info.StartTime;
        results.EndTime = info.EndTime;
        results.JobId = info.JobId;
        results.NoOfProfiles = info.IssueList.Count;

        results.NoOfIssues = 0;
        foreach (AncestorLineInfo ancestor in info.IssueList)
        {
          results.NoOfIssues += ancestor.problemList.Count;

          foreach (SanityProblem inProblem in ancestor.problemList)
          {
            Issue.IssueType type = TranslateProblem(inProblem.id);

            results.IssueCounters.AddIssue(type);
          }
        }

        analysis.Results = AnalysisResults.ToJson(results);
        context.Analyses.Update(analysis);
        context.Entry(analysis).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        context.SaveChanges();
        trace.TraceInformation("results = " + analysis.Results);
      }
    }

    static private string ProfileToString(Profile profile)
    {
      return profile.Name + " (" + profile.Birth + " - " + profile.Death + ") " + profile.TreeId;
    }


    private static Profile FetchProfile(FamilyTreeDbContext context, string ancestorId, int familyTreeId)
    {
      IEnumerator<Profile> profiles = null;
      IList<Profile> profileList = new List<Profile>();

      profiles = context.Profiles.Where<Profile>(p => p.TreeId == ancestorId && p.FamilyTreeId == familyTreeId).GetEnumerator();
      if (profiles.MoveNext())
      {
        Profile profile = null;
        profile = profiles.Current;
        if ((profile.Issues != null) && (profile.Issues.Count > 0))
        {
          //profile.Issues = new List<Issue>();
          trace.TraceData(TraceEventType.Verbose, 0, "  Issues list count > 0 : " + profile.Issues.Count);
        }
        if (profile.Issues == null)
        {
          profile.Issues = new List<Issue>();
        }


        if (profiles.MoveNext())
        {
          trace.TraceData(TraceEventType.Error, 0, "  Error more than one item found" + ancestorId + " " + familyTreeId + " found");
        }
        profileList.Add(profile);
        //return profile;
      }
      profiles.Dispose();

      if (profileList.Count == 1)
      {
        return profileList[0];
      }
      return null;
    }

    public static void DecodeProblemsList(FamilyTreeDbContext context, JobInfo info)
    {
      Analysis analysis = context.Analyses.Find(info.JobId);
      TimeCounterClass timeCounter = new TimeCounterClass(info.IssueList.Count, "Step 1/3");

      try
      {
        context.SaveChanges();
      }
      catch (DbUpdateException ex)
      {
        trace.TraceData(TraceEventType.Error, 0, info.JobId + ": db update failed " + ex.ToString());
      }

      timeCounter.Start(0);

      if (analysis != null)
      {
        if (analysis.IssueLinks == null)
        {
          analysis.IssueLinks = new List<IssueLink>();
        }

        trace.TraceData(TraceEventType.Information, 0, "Trying to update " + analysis.UserEmail + " " + analysis.OriginalFilename + " " + analysis.FileName + " " + analysis.Id);

        AnalysisResults results = null;

        results = AnalysisResults.FromJson(analysis.Results);

        if (results == null)
        {
          results = new AnalysisResults();

          results.SearchedProfiles = info.Profiles;
          results.SearchedFamilies = info.Families;
          results.StartTime = info.StartTime;
          results.EndTime = info.EndTime;
          results.JobId = info.JobId;
        }


        timeCounter.Start(1);

        IList<Profile> newProfiles = new List<Profile>();
        IList<Profile> updatedProfiles = new List<Profile>();

        DatabaseResults dbResults = new DatabaseResults();

        foreach (AncestorLineInfo ancestor in info.IssueList.ToList())
        {
          Profile profile = null;

          timeCounter.Start(2);

          profile = FetchProfile(context, ancestor.rootAncestor, analysis.FamilyTreeId);
          timeCounter.Next();

          if (profile == null)
          {
            timeCounter.Start(4);

            profile = new Profile();

            profile.TreeId = ancestor.rootAncestor;
            profile.Name = ancestor.name;
            profile.Birth = ancestor.birth;
            profile.Death = ancestor.death;
            profile.Sex = TranslateSex(ancestor.sex);
            profile.FamilyTreeId = analysis.FamilyTreeId;
            profile.Issues = new List<Issue>();
            profile.Url = ancestor.url;

            timeCounter.Start(5);
            trace.TraceData(TraceEventType.Information, 0, "  Adding person to db " + ProfileToString(profile));

            newProfiles.Add(profile);

            timeCounter.Start(8);
            dbResults.NoOfProfilesAdded++;
          }
          else
          {
            timeCounter.Start(20);
            bool updatesMade = false;
            string dateStr = ancestor.birth;

            if ((dateStr.Length > 0) && (profile.Birth == null) || (profile.Birth != dateStr))
            {
              profile.Birth = dateStr;
              updatesMade = true;
            }
            dateStr = ancestor.death;

            if ((dateStr.Length > 0) && (profile.Death == null) || (profile.Death != dateStr))
            {
              profile.Death = dateStr;
              updatesMade = true;
            }
            if (profile.Name != ancestor.name)
            {
              trace.TraceData(TraceEventType.Warning, 0, "  person name changed " + ProfileToString(profile) + " " + ancestor.name);
              profile.Name = ancestor.name;
              updatesMade = true;
            }
            if (updatesMade)
            {
              updatedProfiles.Add(profile);

              trace.TraceData(TraceEventType.Information, 0, "  updated existing person " + ProfileToString(profile));
            }

            /* Clear issues, to not add new links to old ones. */
            /*if (profile.Issues != null)
            {
              trace.TraceData(TraceEventType.Information, 0, "  person " + ProfileToString(profile) + " has issues: " + profile.Issues.Count());
              profile.Issues.Clear();
            }
            else
            {
              trace.TraceData(TraceEventType.Verbose, 0, "  person " + ProfileToString(profile) + " has no issues ");
              profile.Issues = new List<Issue>();
            }*/
            timeCounter.Start(21);
          }
          //allDbProfiles.Add(ancestor.rootAncestor, profile);
        }
        timeCounter.Start(7);
        trace.TraceData(TraceEventType.Information, 0, "  Adding list of " + newProfiles.Count + " profiles out of " + info.IssueList.Count);

        if (newProfiles.Count > 0)
        {
          trace.TraceData(TraceEventType.Information, 0, "  Adding db profiles:" + newProfiles.Count);
          context.Profiles.AddRange(newProfiles);
        }
        if (updatedProfiles.Count > 0)
        {
          trace.TraceData(TraceEventType.Information, 0, "  Updating db profiles:" + newProfiles.Count);
          context.Profiles.UpdateRange(updatedProfiles);
        }
        if ((updatedProfiles.Count > 0) || (newProfiles.Count > 0))
        {
          try
          {
            context.SaveChanges();
          }
          catch (DbUpdateException ex)
          {
            trace.TraceData(TraceEventType.Error, 0, info.JobId + ": db update failed " + ex.ToString());
          }
        }

        foreach (Profile profile in newProfiles)
        {
          trace.TraceData(TraceEventType.Information, 0, "  adding profile " + ProfileToString(profile) + " " + profile.Id + " " + profile.TreeId);
        }

        timeCounter.Stop(DateTime.Now);
        timeCounter.Trace();

        trace.TraceData(TraceEventType.Verbose, 0, "  Updating profiles done");

        timeCounter = new TimeCounterClass(info.IssueList.Count, "Step 2/3");

        /* Update Issues belonging to the above Profiles */

        IList<Issue> newIssues = new List<Issue>();

        foreach (AncestorLineInfo ancestor in info.IssueList.ToList())
        {
          Profile profile = null;

          timeCounter.Start(12);

          profile = FetchProfile(context, ancestor.rootAncestor, analysis.FamilyTreeId);

          if (profile == null)
          {
            trace.TraceData(TraceEventType.Information, 0, "    Profile not found " + ancestor.rootAncestor);
            continue;
          }

          timeCounter.Next();

          IList<Issue> oldIssues = context.Issues.Where<Issue>(i => i.ProfileId == profile.Id).OrderBy(i => i.Type).ToList<Issue>();

          timeCounter.Start(21);

          trace.TraceData(TraceEventType.Verbose, 0, "  Adding " + ancestor.problemList.Count + " problems to " + profile.Name);

          IList<Issue> newList = new List<Issue>();
          int issueLink = -1;
          foreach (SanityProblem inProblem in ancestor.problemList.ToList())
          {
            timeCounter.Start(25);
            Issue issue = new Issue();
            issue.Description = inProblem.details;
            if (inProblem.url != null)
            {
              issue.Parameters += ";" + inProblem.url;
            }
            issue.Type = TranslateProblem(inProblem.id);

            timeCounter.Start(26);

            issue.ProfileId = profile.Id;

            timeCounter.Start(27);
            if (!FindIssue(oldIssues, issue, ref issueLink))
            {
              timeCounter.Start(28);
              dbResults.NoOfIssuesAdded++;
              issue.Status = Issue.IssueStatus.Identified;

              newIssues.Add(issue);
              profile.Issues.Add(issue);

              timeCounter.Start(29);
            }
            else
            {
              timeCounter.Start(30);
              dbResults.NoOfIssuesUpdated++;

              timeCounter.Start(31);
            }

            timeCounter.Start(32);
          }
        }
        trace.TraceData(TraceEventType.Information, 0, "    Adding Issues, new: " + newIssues.Count + " old: " + dbResults.NoOfIssuesUpdated);
        if (newIssues.Count > 0)
        {
          trace.TraceData(TraceEventType.Information, 0, "    Add Issues " + newIssues.Count);

          context.Issues.AddRange(newIssues);

          trace.TraceData(TraceEventType.Information, 0, "    Add Issues done ");

          try
          {
            context.SaveChanges();
          }
          catch (DbUpdateException ex)
          {
            trace.TraceData(TraceEventType.Error, 0, info.JobId + ": db update failed " + ex.ToString());
          }
        }

        timeCounter.Stop(DateTime.Now);
        timeCounter.Trace();

        trace.TraceData(TraceEventType.Verbose, 0, "  Adding issues done, add links");

        timeCounter = new TimeCounterClass(info.IssueList.Count, "Step 3/3");

        IList<IssueLink> newLinks = new List<IssueLink>();
        foreach (AncestorLineInfo ancestor in info.IssueList.ToList())
        {
          Profile profile = null;

          profile = FetchProfile(context, ancestor.rootAncestor, analysis.FamilyTreeId);

          if (profile == null)
          {
            trace.TraceData(TraceEventType.Information, 0, "    Profile not found " + ancestor.rootAncestor);
            continue;
          }

          RelationStack relation = ancestor.relationPath;
          string relationString = null;
          string relationDistance = "";
          if (relation != null)
          {
            relationString = relation.ToString(false);
            relationDistance = relation.GetDistance();
          }

          foreach (Issue issue in profile.Issues.ToList())
          {
            timeCounter.Start(25);

            IssueLink link = new IssueLink();

            link.AnalysisId = analysis.Id;
            link.IssueId = issue.Id;
            link.Status = issue.Status;
            link.Time = DateTime.Now;
            link.RelationDistance = relationDistance;
            link.Relation = relationString;

            newLinks.Add(link);

            timeCounter.Start(34);
          }
        }
        trace.TraceData(TraceEventType.Information, 0, "    Adding issuelinks: " + newLinks.Count + " ");
        if (newLinks.Count > 0)
        {
          context.IssueLinks.AddRange(newLinks);
          try
          {
            context.SaveChanges();
          }
          catch (DbUpdateException ex)
          {
            trace.TraceData(TraceEventType.Error, 0, info.JobId + ": db update failed " + ex.ToString());
          }
        }

        timeCounter.Start(27);
        results.DbResults = dbResults;

        analysis.Results = AnalysisResults.ToJson(results);
        trace.TraceInformation("results = " + analysis.Results);
        context.Analyses.Update(analysis);
        context.Entry(analysis).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        timeCounter.Start(40);
        context.SaveChanges();
        timeCounter.Start(41);
      }
      else
      {
        trace.TraceData(TraceEventType.Warning, 0, "couldnt find " + info.JobId);
      }
      timeCounter.Stop(DateTime.Now);
      timeCounter.Trace();
    }
  }

}
