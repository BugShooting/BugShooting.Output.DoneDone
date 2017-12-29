using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BS.Plugin.V3.Output;
using BS.Plugin.V3.Common;
using BS.Plugin.V3.Utilities;

namespace BugShooting.Output.DoneDone
{
  public class OutputPlugin: OutputPlugin<Output>
  {

    protected override string Name
    {
      get { return "DoneDone"; }
    }

    protected override Image Image64
    {
      get  { return Properties.Resources.logo_64; }
    }

    protected override Image Image16
    {
      get { return Properties.Resources.logo_16 ; }
    }

    protected override bool Editable
    {
      get { return true; }
    }

    protected override string Description
    {
      get { return "Attach screenshots to DoneDone issues."; }
    }
    
    protected override Output CreateOutput(IWin32Window Owner)
    {
      
      Output output = new Output(Name, 
                                 String.Empty, 
                                 String.Empty, 
                                 String.Empty, 
                                 "Screenshot",
                                 String.Empty, 
                                 true,
                                 0,
                                 0,
                                 0,
                                 0,
                                 1);

      return EditOutput(Owner, output);

    }

    protected override Output EditOutput(IWin32Window Owner, Output Output)
    {

      Edit edit = new Edit(Output);

      var ownerHelper = new System.Windows.Interop.WindowInteropHelper(edit);
      ownerHelper.Owner = Owner.Handle;
      
      if (edit.ShowDialog() == true) {

        return new Output(edit.OutputName,
                          edit.Url,
                          edit.UserName,
                          edit.Password,
                          edit.FileName,
                          edit.FileFormat,
                          edit.OpenItemInBrowser,
                          Output.LastProjectID,
                          Output.LastPriorityLevelID,
                          Output.LastFixerID,
                          Output.LastTesterID,
                          Output.LastIssueID);
      }
      else
      {
        return null; 
      }

    }

    protected override OutputValues SerializeOutput(Output Output)
    {

      OutputValues outputValues = new OutputValues();

      outputValues.Add("Name", Output.Name);
      outputValues.Add("Url", Output.Url);
      outputValues.Add("UserName", Output.UserName);
      outputValues.Add("Password",Output.Password, true);
      outputValues.Add("OpenItemInBrowser", Convert.ToString(Output.OpenItemInBrowser));
      outputValues.Add("FileName", Output.FileName);
      outputValues.Add("FileFormat", Output.FileFormat);
      outputValues.Add("LastProjectID", Output.LastProjectID.ToString());
      outputValues.Add("LastPriorityLevelID", Output.LastPriorityLevelID.ToString());
      outputValues.Add("LastFixerID", Output.LastFixerID.ToString());
      outputValues.Add("LastTesterID", Output.LastTesterID.ToString());
      outputValues.Add("LastIssueID", Output.LastIssueID.ToString());

      return outputValues;
      
    }

    protected override Output DeserializeOutput(OutputValues OutputValues)
    {

      return new Output(OutputValues["Name", this.Name],
                        OutputValues["Url", ""], 
                        OutputValues["UserName", ""],
                        OutputValues["Password", ""], 
                        OutputValues["FileName", "Screenshot"], 
                        OutputValues["FileFormat", ""],
                        Convert.ToBoolean(OutputValues["OpenItemInBrowser", Convert.ToString(true)]),
                        Convert.ToInt32(OutputValues["LastProjectID", "0"]),
                        Convert.ToInt32(OutputValues["LastPriorityLevelID", "0"]),
                        Convert.ToInt32(OutputValues["LastFixerID", "0"]),
                        Convert.ToInt32(OutputValues["LastTesterID", "0"]),
                        Convert.ToInt32(OutputValues["LastIssueID", "1"]));

    }

    protected override async Task<SendResult> Send(IWin32Window Owner, Output Output, ImageData ImageData)
    {

      try
      {

        string userName = Output.UserName;
        string password = Output.Password;
        bool showLogin = string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password);
        bool rememberCredentials = false;

        string fileName = AttributeHelper.ReplaceAttributes(Output.FileName, ImageData);

        while (true)
        {

          if (showLogin)
          {

            // Show credentials window
            Credentials credentials = new Credentials(Output.Url, userName, password, rememberCredentials);

            var credentialsOwnerHelper = new System.Windows.Interop.WindowInteropHelper(credentials);
            credentialsOwnerHelper.Owner = Owner.Handle;

            if (credentials.ShowDialog() != true)
            {
              return new SendResult(Result.Canceled);
            }

            userName = credentials.UserName;
            password = credentials.Password;
            rememberCredentials = credentials.Remember;

          }

          GetProjectsResult projectsResult = await DoneDoneProxy.GetProjects(Output.Url, userName, password);
          switch (projectsResult.Status)
          {
            case ResultStatus.Success:
              break;
            case ResultStatus.LoginFailed:
              showLogin = true;
              continue;
            case ResultStatus.Failed:
              return new SendResult(Result.Failed, projectsResult.FailedMessage);
          }

          GetPriorityLevelsResult priorityLevelsResult = await DoneDoneProxy.GetPriorityLevels(Output.Url, userName, password);
          switch (priorityLevelsResult.Status)
          {
            case ResultStatus.Success:
              break;
            case ResultStatus.LoginFailed:
              showLogin = true;
              continue;
            case ResultStatus.Failed:
              return new SendResult(Result.Failed, priorityLevelsResult.FailedMessage);
          }

          // Show send window
          Send send = new Send(Output.Url, Output.LastProjectID, Output.LastPriorityLevelID, Output.LastFixerID, Output.LastTesterID, Output.LastIssueID, projectsResult.Projects, priorityLevelsResult.PriorityLevels, userName, password, fileName);

          var sendOwnerHelper = new System.Windows.Interop.WindowInteropHelper(send);
          sendOwnerHelper.Owner = Owner.Handle;

          if (!send.ShowDialog() == true)
          {
            return new SendResult(Result.Canceled);
          }

          string fullFileName = String.Format("{0}.{1}", send.FileName, FileHelper.GetFileExtention(Output.FileFormat));
          string fileMimeType = FileHelper.GetMimeType(Output.FileFormat);
          byte[] fileBytes = FileHelper.GetFileBytes(Output.FileFormat, ImageData);

          int issueID;
          int priorityLevelID;
          int fixerID;
          int testerID;
          
          if (send.CreateNewIssue)
          {

            // Create issue
            CreateIssueResult createIssueResult = await DoneDoneProxy.CreateIssue(Output.Url, userName, password, send.ProjectID, send.PriorityLevelID, send.FixerID, send.TesterID, send.IssueTitle, send.Description, fullFileName, fileMimeType, fileBytes);
            switch (createIssueResult.Status)
            {
              case ResultStatus.Success:
                break;
              case ResultStatus.LoginFailed:
                showLogin = true;
                continue;
              case ResultStatus.Failed:
                return new SendResult(Result.Failed, createIssueResult.FailedMessage);
            }

            issueID = createIssueResult.IssueID;
            priorityLevelID = send.PriorityLevelID;
            fixerID = send.FixerID;
            testerID = send.TesterID;

          }
          else
          {
           
            // Add attachment to issue
            CreateIssueCommentResult createIssueCommentResult = await DoneDoneProxy.CreateIssueComment(Output.Url, userName, password, send.ProjectID, send.IssueID, send.Comment, fullFileName, fileMimeType, fileBytes);
            switch (createIssueCommentResult.Status)
            {
              case ResultStatus.Success:
                break;
              case ResultStatus.LoginFailed:
                showLogin = true;
                continue;
              case ResultStatus.Failed:
                return new SendResult(Result.Failed, createIssueCommentResult.FailedMessage);
            }

            issueID = send.IssueID;
            priorityLevelID = Output.LastPriorityLevelID;
            fixerID = Output.LastFixerID;
            testerID = Output.LastTesterID;

          }


          // Open issue in browser
          if (Output.OpenItemInBrowser)
          {
            WebHelper.OpenUrl(String.Format("{0}/issuetracker/projects/{1}/issues/{2}",Output.Url, send.ProjectID, issueID));
          }
                             
          return new SendResult(Result.Success,
                                new Output(Output.Name,
                                            Output.Url,
                                            (rememberCredentials) ? userName : Output.UserName,
                                            (rememberCredentials) ? password : Output.Password,
                                            Output.FileName,
                                            Output.FileFormat,
                                            Output.OpenItemInBrowser,
                                            send.ProjectID,
                                            priorityLevelID,
                                            fixerID,
                                            testerID,
                                            issueID));
          
        }

      }
      catch (Exception ex)
      {
        return new SendResult(Result.Failed, ex.Message);
      }

    }

  }
}
