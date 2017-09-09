using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BS.Output.DoneDone
{

  internal class DoneDoneProxy
  {

    const int ConflictRequestRepeatCount = 10;
    const int ConflictRequestRepeatDelay = 1000;

    static internal async Task<GetProjectsResult> GetProjects(string url, string userName, string password)
    {

      int repeatCount = 0;
      
      do
      {

        try
        {

          string requestUrl = GetApiUrl(url, "projects.json");
          string resultData = await GetData(requestUrl, userName, password);
          List<Project> projects = FromJson<List<Project>>(resultData);

          return new GetProjectsResult(ResultStatus.Success, projects, null);

        }
        catch (WebException ex) when (ex.Response is HttpWebResponse)
        {

          HttpWebResponse response = (HttpWebResponse)ex.Response;

          switch (response.StatusCode)
          {
            case HttpStatusCode.Unauthorized:
              return new GetProjectsResult(ResultStatus.LoginFailed, null, null);

            case HttpStatusCode.Conflict:
              
              if (repeatCount < ConflictRequestRepeatCount)
              {
                // Repeat request
                repeatCount += 1;
                Thread.Sleep(ConflictRequestRepeatDelay);
                break;
              }
              else
              {
                return new GetProjectsResult(ResultStatus.Failed, null, response.StatusDescription);
              }
              
            default:
              return new GetProjectsResult(ResultStatus.Failed, null, response.StatusDescription);
          }

        }

      } while (true);

    }

    static internal async Task<GetPriorityLevelsResult> GetPriorityLevels(string url, string userName, string password)
    {

      int intRepeatCount = 0;
      
      do
      {

        try
        {
          string requestUrl = GetApiUrl(url, "priority_levels.json");
          string resultData = await GetData(requestUrl, userName, password);
          List<PriorityLevel> priorityLevels = FromJson<List<PriorityLevel>>(resultData);

          return new GetPriorityLevelsResult(ResultStatus.Success, priorityLevels, null);

        }
        catch (WebException ex) when (ex.Response is HttpWebResponse)
        {

          HttpWebResponse response = (HttpWebResponse)ex.Response;

          switch (response.StatusCode)
          {
            case HttpStatusCode.Unauthorized:
              return new GetPriorityLevelsResult(ResultStatus.LoginFailed, null, null);

            case HttpStatusCode.Conflict:
              
              if (intRepeatCount < ConflictRequestRepeatCount)
              {
                // Repeat request
                intRepeatCount += 1;
                Thread.Sleep(ConflictRequestRepeatDelay);
                break;
              }
              else
              {
                return new GetPriorityLevelsResult(ResultStatus.Failed, null, response.StatusDescription);
              }

            default:
              return new GetPriorityLevelsResult(ResultStatus.Failed, null, response.StatusDescription);
          }

        }

      } while (true);

    }

    static internal async Task<GetPeopleInProjectResult> GetPeopleInProject(string url, string userName, string password, int projectID)
    {

      int repeatCount = 0;
      
      do
      {

        try
        {
          string requestUrl = GetApiUrl(url, String.Format("/projects/{0}/available_for_reassignment.json", projectID));
          string resultData = await GetData(requestUrl, userName, password);
          List<People> peoples = FromJson<List<People>>(resultData);

          return new GetPeopleInProjectResult(ResultStatus.Success, peoples, null);
          
        }
        catch (WebException ex) when (ex.Response is HttpWebResponse)
        {

          HttpWebResponse response = (HttpWebResponse)ex.Response;

          switch (response.StatusCode)
          {
            case HttpStatusCode.Unauthorized:
              return new GetPeopleInProjectResult(ResultStatus.LoginFailed, null, null);

            case HttpStatusCode.Conflict:
              
              if (repeatCount < ConflictRequestRepeatCount)
              {
                // Repeat request
                repeatCount += 1;
                Thread.Sleep(ConflictRequestRepeatDelay);
                break;
              }
              else
              {
                return new GetPeopleInProjectResult(ResultStatus.Failed, null, response.StatusDescription);
              }

            default:
              return new GetPeopleInProjectResult(ResultStatus.Failed, null, response.StatusDescription);
          }

        }

      } while (true);

    }

    static internal async Task<CreateIssueResult> CreateIssue(string url, 
                                                              string userName, 
                                                              string password, 
                                                              int projectID, 
                                                              int priorityLevelID, 
                                                              int fixerID, 
                                                              int testerID, 
                                                              string title,
                                                              string description,
                                                              string fullFileName, 
                                                              string fileMimeType, 
                                                              byte[] fileBytes)
    {

      int intRepeatCount = 0;
      
      do
      {

        try
        {
          SortedList<string, string> parameters = new SortedList<string, string>();
          parameters.Add("title", title);
          parameters.Add("description", description);
          parameters.Add("priority_level_id", priorityLevelID.ToString());
          parameters.Add("fixer_id", fixerID.ToString());
          parameters.Add("tester_id", testerID.ToString());

          string requestUrl = GetApiUrl(url, String.Format("projects/{0}/issues.json", projectID));
          string resultData = await SendFile(requestUrl, userName, password, parameters, fullFileName, fileMimeType, fileBytes);
          CreateIssueData createIssueData = FromJson<CreateIssueData>(resultData);

          return new CreateIssueResult(ResultStatus.Success, createIssueData.IssueURL, createIssueData.IssueID, null);

        }
        catch (WebException ex) when (ex.Response is HttpWebResponse)
        {

          HttpWebResponse response = (HttpWebResponse)ex.Response;

          switch (response.StatusCode)
          {
            case HttpStatusCode.Unauthorized:
              return new CreateIssueResult(ResultStatus.LoginFailed, null, 0, null);

            case HttpStatusCode.Conflict:
              
              if (intRepeatCount < ConflictRequestRepeatCount)
              {
                // Repeat request
                intRepeatCount += 1;
                Thread.Sleep(ConflictRequestRepeatDelay);
                break;
              }
              else
              {
                return new CreateIssueResult(ResultStatus.Failed, null, 0, response.StatusDescription);
              }

            default:
              return new CreateIssueResult(ResultStatus.Failed, null, 0, response.StatusDescription);
          }

        }

      } while (true);

    }

    static internal async Task<CreateIssueCommentResult> CreateIssueComment(string url, 
                                                                            string userName, 
                                                                            string password, 
                                                                            int projectID, 
                                                                            int issueID, 
                                                                            string comment,
                                                                            string fullFileName, 
                                                                            string fileMimeType, 
                                                                            byte[] fileBytes)
    {

      int repeatCount = 0;
      
      do
      {

        try
        {
          SortedList<string, string> parameters = new SortedList<string, string>();
          parameters.Add("comment", comment);

          string requestUrl = GetApiUrl(url, String.Format("projects/{0}/issues/{1}/comments.json", projectID, issueID));
          string resultData = await SendFile(requestUrl, userName, password, parameters, fullFileName, fileMimeType, fileBytes);
          CreateIssueCommentData createIssueCommentData = FromJson<CreateIssueCommentData>(resultData);

          return new CreateIssueCommentResult(ResultStatus.Success, createIssueCommentData.CommentURL, null);
          
        }
        catch (WebException ex) when (ex.Response is HttpWebResponse)
        {

          HttpWebResponse response = (HttpWebResponse)ex.Response;

          switch (response.StatusCode)
          {
            case HttpStatusCode.Unauthorized:
              return new CreateIssueCommentResult(ResultStatus.LoginFailed, null, null);

            case HttpStatusCode.Conflict:
              
              if (repeatCount < ConflictRequestRepeatCount)
              {
                // Repeat request
                repeatCount += 1;
                Thread.Sleep(ConflictRequestRepeatDelay);
                break;
              }
              else
              {
                return new CreateIssueCommentResult(ResultStatus.Failed, null, response.StatusDescription);
              }

            default:
              return new CreateIssueCommentResult(ResultStatus.Failed, null, response.StatusDescription);
          }

        }

      } while (true);

    }
    
    private static async Task<string> GetData(string url, string username, string password)
    {

      WebRequest request = WebRequest.Create(url);
      request.Method = "GET";
      request.ContentType = "application/x-www-form-urlencoded";
      
      // Basic Authorization
      string basicAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)));
      request.Headers.Add("Authorization", String.Format("Basic {0}", basicAuth));

      using (WebResponse response = await request.GetResponseAsync())
      {
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
          return await reader.ReadToEndAsync();
        }

      }

    }

    private static async Task<string> SendFile(string url, string username, string password, SortedList<string, string> parameters, string fullFileName, string fileMimeType, byte[] fileBytes)
    {

      string boundary = String.Format("----------{0}", DateTime.Now.Ticks.ToString("x"));
      
      WebRequest request = WebRequest.Create(url);
      request.Method = "POST";
      request.ContentType = String.Format("multipart/form-data; boundary={0}", boundary);
      
      StringBuilder postData = new StringBuilder();
      foreach (string key in parameters.Keys)
      {
        postData.AppendFormat("--{0}", boundary);
        postData.AppendLine();
        postData.AppendFormat("Content-Disposition: form-data; name=\"{0}\"\r\n", key);
        postData.AppendLine();
        postData.AppendFormat("{0}\r\n", parameters[key]);
      }

      postData.AppendFormat("--{0}", boundary);
      postData.AppendLine();
      postData.AppendFormat("Content-Disposition: form-data; name=\"fileupload\"; filename=\"{0}\"\r\n", fullFileName);
      postData.AppendFormat("Content-Type: {0}\r\n", fileMimeType);
      postData.AppendLine();

      byte[] postBytes = Encoding.UTF8.GetBytes(postData.ToString());
      byte[] boundaryBytes = Encoding.ASCII.GetBytes(String.Format("\r\n--{0}\r\n", boundary));
            
      // Basic Authorization
      string basicAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)));
      request.Headers.Add("Authorization", String.Format("Basic {0}", basicAuth));

      // ------------------------------------------------------------------------------

      request.ContentLength = postBytes.Length + fileBytes.Length + boundaryBytes.Length;

      using (Stream requestStream = await request.GetRequestStreamAsync())
      {
        await requestStream.WriteAsync(postBytes, 0, postBytes.Length);
        await requestStream.WriteAsync(fileBytes, 0, fileBytes.Length);
        await requestStream.WriteAsync(boundaryBytes, 0, boundaryBytes.Length);
        requestStream.Close();
      }

      using (WebResponse response = await request.GetResponseAsync())
      {
        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        {
          return await reader.ReadToEndAsync();
        }
      }
      
    }
    
    private static string GetApiUrl(string url, string method)
    {

      string apiUrl = url;

      if (apiUrl.LastIndexOf("/") != apiUrl.Length - 1)
      {
        apiUrl += "/";
      }

      apiUrl += "issuetracker/api/v2/" + method;

      return apiUrl;

    }

    private static T FromJson<T>(string jsonText)
    {

      DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

      using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(jsonText)))
      {
        return (T)serializer.ReadObject(stream);
      }

    }

  }

  internal enum ResultStatus : int
  {
    Success = 1,
    LoginFailed = 2,
    Failed = 3
  }

  internal class GetProjectsResult
  {

    ResultStatus status;
    List<Project> projects;
    string failedMessage;

    public GetProjectsResult(ResultStatus status,
                             List<Project> projects,
                             string failedMessage)
    {
      this.status = status;
      this.projects = projects;
      this.failedMessage = failedMessage;
    }

    public ResultStatus Status
    {
      get { return status; }
    }

    public List<Project> Projects
    {
      get { return projects; }
    }

    public string FailedMessage
    {
      get { return failedMessage; }
    }


  }

  internal class GetPriorityLevelsResult
  {

    ResultStatus status;
    List<PriorityLevel> priorityLevels;
    string failedMessage;

    public GetPriorityLevelsResult(ResultStatus status,
                                   List<PriorityLevel> priorityLevels,
                                   string failedMessage)
    {
      this.status = status;
      this.priorityLevels = priorityLevels;
      this.failedMessage = failedMessage;
    }

    public ResultStatus Status
    {
      get { return status; }
    }

    public List<PriorityLevel> PriorityLevels
    {
      get { return priorityLevels; }
    }

    public string FailedMessage
    {
      get { return failedMessage; }
    }


  }

  internal class GetPeopleInProjectResult
  {

    ResultStatus status;
    List<People> peoples;
    string failedMessage;

    public GetPeopleInProjectResult(ResultStatus status,
                                    List<People> peoples,
                                    string failedMessage)
    {
      this.status = status;
      this.peoples = peoples;
      this.failedMessage = failedMessage;
    }

    public ResultStatus Status
    {
      get { return status; }
    }

    public List<People> Peoples
    {
      get { return peoples; }
    }

    public string FailedMessage
    {
      get { return failedMessage; }
    }


  }

  internal class CreateIssueResult
  {

    ResultStatus status;
    string issueUrl;
    int issueID;
    string failedMessage;

    public CreateIssueResult(ResultStatus status,
                             string issueUrl,
                             int issueID,
                             string failedMessage)
    {
      this.status = status;
      this.issueUrl = issueUrl;
      this.issueID = issueID;
      this.failedMessage = failedMessage;
    }

    public ResultStatus Status
    {
      get { return status; }
    }

    public string IssueUrl
    {
      get { return issueUrl; }
    }

    public int IssueID
    {
      get { return issueID; }
    }

    public string FailedMessage
    {
      get { return failedMessage; }
    }


  }

  internal class CreateIssueCommentResult
  {

    ResultStatus status;
    string commentUrl;
    string failedMessage;

    public CreateIssueCommentResult(ResultStatus status,
                                    string commentUrl,
                                    string failedMessage)
    {
      this.status = status;
      this.commentUrl = commentUrl;
      this.failedMessage = failedMessage;
    }

    public ResultStatus Status
    {
      get { return status; }
    }

    public string CommentUrl
    {
      get { return commentUrl; }
    }

    public string FailedMessage
    {
      get { return failedMessage; }
    }


  }

  [DataContract()]
  internal class Project
  {

    int id;
    string title;

    [DataMember(Name = "id")]
    public int ID
    {
      get { return id; }
      set { id = value; }
    }

    [DataMember(Name = "title")]
    public string Title
    {
      get { return title; }
      set { title = value; }
    }

  }

  [DataContract()]
  internal class PriorityLevel
  {

    int id;
    string name;

    [DataMember(Name = "id")]
    public int ID
    {
      get { return id; }
      set { id = value; }
    }

    [DataMember(Name = "name")]
    public string Name
    {
      get { return name; }
      set { name = value; }
    }

  }

  [DataContract()]
  internal class People
  {

    int id;
    string name;

    [DataMember(Name = "ID")]
    public int ID
    {
      get { return id; }
      set { id = value; }
    }

    [DataMember(Name = "Value")]
    public string Name
    {
      get { return name; }
      set { name = value; }
    }

  }

  [DataContract()]
  internal class CreateIssueData
  {

    int issueID;
    string issueURL;

    [DataMember(Name = "IssueID")]
    public int IssueID
    {
      get { return issueID; }
      set { issueID = value; }
    }

    [DataMember(Name = "IssueURL")]
    public string IssueURL
    {
      get { return issueURL; }
      set { issueURL = value; }
    }

  }

  [DataContract()]
  internal class CreateIssueCommentData
  {
    
    string commentURL;

    [DataMember(Name = "CommentURL")]
    public string CommentURL
    {
      get { return commentURL; }
      set { commentURL = value; }
    }

  }

}
