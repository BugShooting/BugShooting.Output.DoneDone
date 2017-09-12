using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BS.Output.DoneDone
{
  partial class Send : Window
  {

    string url;
    string userName;
    string password;

    public Send(string url, int lastProjectID, int lastPriorityLevelID, int lastFixerID, int lastTesterID, int lastIssueID, List<Project> projects, List<PriorityLevel> priorityLevels, string userName, string password, string fileName)
    {
      InitializeComponent();

      this.url = url;
      this.userName = userName;
      this.password = password;

      ProjectComboBox.ItemsSource = projects;
      PriorityLevelComboBox.ItemsSource = priorityLevels;

      Url.Text = url;
      NewIssue.IsChecked = true;
      ProjectComboBox.SelectedValue = lastProjectID;
      PriorityLevelComboBox.SelectedValue = lastPriorityLevelID;

      if (ProjectComboBox.SelectedItem != null)
      {
        FixerComboBox.SelectedValue = lastFixerID;
        TesterComboBox.SelectedValue = lastTesterID;
      }

      IssueIDTextBox.Text = lastIssueID.ToString();
      FileNameTextBox.Text = fileName;

      ProjectComboBox.SelectionChanged += ValidateData;
      PriorityLevelComboBox.SelectionChanged += ValidateData;
      FixerComboBox.SelectionChanged += ValidateData;
      TesterComboBox.SelectionChanged += ValidateData;
      TitleTextBox.TextChanged += ValidateData;
      DescriptionTextBox.TextChanged += ValidateData;
      IssueIDTextBox.TextChanged += ValidateData;
      FileNameTextBox.TextChanged += ValidateData;
      ValidateData(null, null);

    }

    public bool CreateNewIssue
    {
      get { return NewIssue.IsChecked.Value; }
    }
 
    public int ProjectID
    {
      get { return Convert.ToInt32(ProjectComboBox.SelectedValue); }
    }

    public int PriorityLevelID
    {
      get { return Convert.ToInt32(PriorityLevelComboBox.SelectedValue); }
    }

    public int TesterID
    {
      get { return Convert.ToInt32(TesterComboBox.SelectedValue); }
    }

    public int FixerID
    {
      get { return Convert.ToInt32(FixerComboBox.SelectedValue); }
    }

    public string IssueTitle
    {
      get { return TitleTextBox.Text; }
    }

    public string Description
    {
      get { return DescriptionTextBox.Text; }
    }

    public int IssueID
    {
      get { return Convert.ToInt32(IssueIDTextBox.Text); }
    }

    public string Comment
    {
      get { return CommentTextBox.Text; }
    }

    public string FileName
    {
      get { return FileNameTextBox.Text; }
    }
       
    private void NewIssue_CheckedChanged(object sender, EventArgs e)
    {

      if (NewIssue.IsChecked.Value)
      {
        PriorityLevelControls.Visibility = Visibility.Visible;
        FixerControls.Visibility = Visibility.Visible;
        TesterControls.Visibility = Visibility.Visible;
        TitleControls.Visibility = Visibility.Visible;
        DescriptionControls.Visibility = Visibility.Visible;
        IssueIDControls.Visibility = Visibility.Collapsed;
        CommentControls.Visibility = Visibility.Collapsed;

        TitleTextBox.SelectAll();
        TitleTextBox.Focus();
      }
      else
      {
        PriorityLevelControls.Visibility = Visibility.Collapsed;
        FixerControls.Visibility = Visibility.Collapsed;
        TesterControls.Visibility = Visibility.Collapsed;
        TitleControls.Visibility = Visibility.Collapsed;
        DescriptionControls.Visibility = Visibility.Collapsed;
        IssueIDControls.Visibility = Visibility.Visible;
        CommentControls.Visibility = Visibility.Visible;

        IssueIDTextBox.SelectAll();
        IssueIDTextBox.Focus();
      }

      ValidateData(null, null);

    }

    private void IssueID_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
    }
    
    private void ValidateData(object sender, EventArgs e)
    {
      OK.IsEnabled = Validation.IsValid(ProjectComboBox) && 
                     ((CreateNewIssue && Validation.IsValid(TitleTextBox) && Validation.IsValid(DescriptionTextBox) && Validation.IsValid(PriorityLevelComboBox) && Validation.IsValid(FixerComboBox) && Validation.IsValid(TesterComboBox)) ||
                      (!CreateNewIssue && Validation.IsValid(IssueIDTextBox))) &&
                     Validation.IsValid(FileNameTextBox);
    }

    private void OK_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
    }

    private async void ProjectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

      Project project = (Project)ProjectComboBox.SelectedItem;

      GetPeopleInProjectResult peopleInProjectResult = await DoneDoneProxy.GetPeopleInProject(url, userName, password, project.ID);
      
      if (peopleInProjectResult.Status == ResultStatus.Success)
      {
        FixerComboBox.ItemsSource = peopleInProjectResult.Peoples;
        TesterComboBox.ItemsSource = peopleInProjectResult.Peoples;
      }
      else
      {
        FixerComboBox.ItemsSource = null;
        TesterComboBox.ItemsSource = null;
      }

    }
  }

}
