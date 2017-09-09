using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BS.Output.DoneDone
{
  public class OutputAddIn: V3.OutputAddIn<Output>
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

    protected override OutputValueCollection SerializeOutput(Output Output)
    {

      OutputValueCollection outputValues = new OutputValueCollection();

      outputValues.Add(new OutputValue("Name", Output.Name));
      outputValues.Add(new OutputValue("Url", Output.Url));
      outputValues.Add(new OutputValue("UserName", Output.UserName));
      outputValues.Add(new OutputValue("Password",Output.Password, true));
      outputValues.Add(new OutputValue("OpenItemInBrowser", Convert.ToString(Output.OpenItemInBrowser)));
      outputValues.Add(new OutputValue("FileName", Output.FileName));
      outputValues.Add(new OutputValue("FileFormat", Output.FileFormat));
      outputValues.Add(new OutputValue("LastProjectID", Output.LastProjectID.ToString()));
      outputValues.Add(new OutputValue("LastPriorityLevelID", Output.LastPriorityLevelID.ToString()));
      outputValues.Add(new OutputValue("LastFixerID", Output.LastFixerID.ToString()));
      outputValues.Add(new OutputValue("LastTesterID", Output.LastTesterID.ToString()));
      outputValues.Add(new OutputValue("LastIssueID", Output.LastIssueID.ToString()));

      return outputValues;
      
    }

    protected override Output DeserializeOutput(OutputValueCollection OutputValues)
    {

      return new Output(OutputValues["Name", this.Name].Value,
                        OutputValues["Url", ""].Value, 
                        OutputValues["UserName", ""].Value,
                        OutputValues["Password", ""].Value, 
                        OutputValues["FileName", "Screenshot"].Value, 
                        OutputValues["FileFormat", ""].Value,
                        Convert.ToBoolean(OutputValues["OpenItemInBrowser", Convert.ToString(true)].Value),
                        Convert.ToInt32(OutputValues["LastProjectID", "0"].Value),
                        Convert.ToInt32(OutputValues["LastPriorityLevelID", "0"].Value),
                        Convert.ToInt32(OutputValues["LastFixerID", "0"].Value),
                        Convert.ToInt32(OutputValues["LastTesterID", "0"].Value),
                        Convert.ToInt32(OutputValues["LastIssueID", "1"].Value));

    }

    protected override async Task<V3.SendResult> Send(IWin32Window Owner, Output Output, V3.ImageData ImageData)
    {

      try
      {

        string userName = Output.UserName;
        string password = Output.Password;
        bool showLogin = string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password);
        bool rememberCredentials = false;

        string fileName = V3.FileHelper.GetFileName(Output.FileName, Output.FileFormat, ImageData);

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
              return new V3.SendResult(V3.Result.Canceled);
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
              return new V3.SendResult(V3.Result.Failed, projectsResult.FailedMessage);
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
              return new V3.SendResult(V3.Result.Failed, priorityLevelsResult.FailedMessage);
          }

          // Show send window
          Send send = new Send(Output.Url, Output.LastProjectID, Output.LastPriorityLevelID, Output.LastFixerID, Output.LastTesterID, Output.LastIssueID, projectsResult.Projects, priorityLevelsResult.PriorityLevels, userName, password, fileName);

          var sendOwnerHelper = new System.Windows.Interop.WindowInteropHelper(send);
          sendOwnerHelper.Owner = Owner.Handle;

          if (!send.ShowDialog() == true)
          {
            return new V3.SendResult(V3.Result.Canceled);
          }

          string fullFileName = String.Format("{0}.{1}", send.FileName, V3.FileHelper.GetFileExtention(Output.FileFormat));
          string fileMimeType = V3.FileHelper.GetMimeType(Output.FileFormat);
          byte[] fileBytes = V3.FileHelper.GetFileBytes(Output.FileFormat, ImageData);

          int issueID;
          string issueUrl;
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
                return new V3.SendResult(V3.Result.Failed, createIssueResult.FailedMessage);
            }

            issueID = createIssueResult.IssueID;
            issueUrl = createIssueResult.IssueUrl;
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
                return new V3.SendResult(V3.Result.Failed, createIssueCommentResult.FailedMessage);
            }

            issueID = send.IssueID;
            issueUrl = createIssueCommentResult.CommentUrl;
            priorityLevelID = Output.LastPriorityLevelID;
            fixerID = Output.LastFixerID;
            testerID = Output.LastTesterID;

          }


          // Open issue in browser
          if (Output.OpenItemInBrowser)
          {
            V3.WebHelper.OpenUrl(issueUrl);
          }
                             
          return new V3.SendResult(V3.Result.Success,
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
        return new V3.SendResult(V3.Result.Failed, ex.Message);
      }

    }

  }
}
