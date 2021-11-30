﻿using DataAccessFakes;
using DataObjects;
using LogicLayer;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace RubricNorming
{
    public partial class MainWindow : Window
    {

        UserManager _userManager = null;
        User _user = null;
        FacetManager _facetManager = null;
        CriteriaManager _criteriaManager = null;
        IRubricManager<RubricVM> _rubricManagerVM = null;
        ScoreTypeManager _scoreTypeManager = null;
        FacetTypeManager _facetTypeManager = null;



        //try to get rid of this guy
        IRubricManager<Rubric> _rubricManager = null;

        string _executionChoice = "";

        Rubric _rubric = null;
        RubricVM _rubricVM = null;

        public MainWindow()
        {
            //// uses default accessors
            _userManager = new UserManager();
            _rubricManager = new RubricManager();
            _facetManager = new FacetManager();
            _criteriaManager = new CriteriaManager();
            _rubricManagerVM = new RubricVMManager();
            _scoreTypeManager = new ScoreTypeManager();
            _facetTypeManager = new FacetTypeManager();


            // uses the fake accessors
            //UserAccessorFake userAccessorFake = new UserAccessorFake();
            //_rubricManager = new RubricManager(new RubricAccessorFake(), userAccessorFake);
            //_userManager = new UserManager(userAccessorFake);
            //_facetManager = new FacetManager(new FacetAccessorFake());
            //_criteriaManager = new CriteriaManager(new CriteriaAccessorFake());
            //_rubricManagerVM = new RubricVMManager(_rubricManager, _userManager, _facetManager, _criteriaManager);
            //_scoreTypeManager = new ScoreTypeManager(new ScoreTypeFake());
            //_facetTypeManager = new FacetTypeManager(new FacetTypeFakes());


            InitializeComponent();

            RoutedCommand closeWindow = new RoutedCommand();
            closeWindow.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(closeWindow, mnuExit_Click));
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var userID = this.txtUserID.Text;
            var pwd = this.pwdPassword.Password;


            if (btnLogin.Content.ToString() == "Login")
            {

                try
                {
                    _user = _userManager.LoginUser(userID, pwd);


                    string instructions = "On first login, all new users must choose a password to continue.";

                    if (_user != null && pwd == "newuser")
                    {
                        var upDateWindow = new UpdatePasswordWindow(_userManager, _user, instructions, true);

                        bool? result = upDateWindow.ShowDialog();
                        if (result == true)
                        {
                            updateUIforUser();
                            string rolesList = "";
                            foreach (var role in _user.Roles)
                            {
                                rolesList += " " + role;
                            }
                            MessageBox.Show("Welcome back, " + _user.GivenName +
                                "\n\n" + "Your roles are:" + rolesList);
                        }
                        else
                        {
                            _user = null;
                            updateUIforLogOut();
                            MessageBox.Show("You did not update your password. You will be logged out.");
                        }
                    }
                    else if (_user != null)
                    {
                        updateUIforUser();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + "\n\n" +
                        ex.InnerException.Message,
                        "Alert!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    pwdPassword.Password = "";
                    txtUserID.Select(0, Int32.MaxValue);
                    txtUserID.Focus();
                }
            }
            else
            {
                updateUIforLogOut();
            }            
        }

        private void updateUIforUser()
        {
            string rolesList = "";
            for (int i = 0; i < _user.Roles.Count; i++)
            {
                rolesList += " " + _user.Roles[i];
                if (i == _user.Roles.Count -2)
                {
                    rolesList += " and";
                }
                else if (i< _user.Roles.Count-2)
                {
                    rolesList += ",";
                }
            }
            MessageBox.Show("Welcome back, " + _user.GivenName +
                "\n\n" + "Your roles are:" + rolesList);

            lblLogin.Content = "";
            staMessage.Content = _user.UserID + " logged in as" + rolesList + " on " + DateTime.Now.ToShortTimeString();

            txtUserID.Text = "";
            pwdPassword.Password = "";
            txtUserID.Visibility = Visibility.Hidden;
            pwdPassword.Visibility = Visibility.Hidden;
            lblUserID.Visibility = Visibility.Hidden;
            lblPassword.Visibility = Visibility.Hidden;

            stkRubricControls.Visibility = Visibility.Visible;
            datViewList.Visibility = Visibility.Visible;
            mnuView.Visibility = Visibility.Visible;
            viewAllActiveRubrics();

            btnLogin.Content = "Log Out";
            btnLogin.IsDefault = false;

            //showTabsForUser();

        }

        private void updateUIforLogOut()
        {
            _user = null;

            staMessage.Content = "Welcome. Please log in to continue.";

            txtUserID.Visibility = Visibility.Visible;
            pwdPassword.Visibility = Visibility.Visible;
            lblUserID.Visibility = Visibility.Visible;
            lblPassword.Visibility = Visibility.Visible;

            stkRubricControls.Visibility = Visibility.Hidden;
            datViewList.Visibility = Visibility.Hidden;
            mnuView.Visibility = Visibility.Hidden;


            btnLogin.Content = "Login";
            btnLogin.Focus();

            btnLogin.IsDefault = true;

            //hide all user tabs

            txtUserID.Focus();
        }

        //private void preparegrdCreateControlsForRetrieveFacetsByRubricID()
        //{
        //    txtblkDockPanelTitle.Text = "Retrieve Facet by Rubric ID";
        //    lblInput1.Content = "Rubric ID:";
        //    _executionChoice = "FacetsByRubricID";

        //    grdCreateControls.Visibility = Visibility.Visible;
        //    txtBoxInput1.Focus();
        //}

        //private void preparegrdCreateControlsForRetrieveCriteriaByRubicID()
        //{
        //    txtblkDockPanelTitle.Text = "Retrieve Criteria by Rubric ID";
        //    lblInput1.Content = "Rubric ID:";
        //    _executionChoice = "CriteriaByRubricID";

        //    grdCreateControls.Visibility = Visibility.Visible;
        //    txtBoxInput1.Focus();
        //}

        private void frmMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtUserID.Focus();

            try
            {                
                cmbBoxScoreTypes.ItemsSource = _scoreTypeManager.RetrieveScoreTypes().Select(st => st.ScoreTypeID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem retrieving score types.\n" + ex.Message);
            }

            try
            {
                cmbBoxFacetType.ItemsSource = _facetTypeManager.RetrieveFacetTypes().Select(ft => ft.FacetTypeID);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem retrieving facet types.\n" + ex.Message);
            }


            stkRubricControls.Visibility = Visibility.Hidden;
            datViewList.Visibility = Visibility.Hidden;
            //grdCreateControls.Visibility = Visibility.Hidden;

            // for testing purposes, comment out
            //mnuView.Visibility = Visibility.Hidden;

            datViewList.Visibility = Visibility.Visible;
            viewAllActiveRubrics();
            //hideAllUserTabs();
        }

        private void viewAllActiveRubrics()
        {
            List<Rubric> rubricList = null;
            try
            {
                rubricList = _rubricManager.RetrieveActiveRubrics();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem retrieving the list of rubrics." + ex.Message);
            }

            datViewList.ItemsSource = rubricList;

            datViewList.Visibility = Visibility.Visible;
            toggleListAndDetails();
         
        }


        private void viewAllActivateFacets()
        {
            List<Facet> facets = null;

            try
            {
                facets = _facetManager.RetrieveActiveFacets();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem retrieving the list of facets." + ex.Message);
            }

            var facetListSorted = facets.Select(f => new { f.FacetID, f.Description, f.DateCreated, f.DateUpdated, f.FacetType });            
            
            datViewList.ItemsSource = facetListSorted;
            datViewList.Visibility = Visibility.Visible;
        }

        private void btnConfirmForm_Click(object sender, RoutedEventArgs e)
        {
            switch (_executionChoice)
            {
                case "FacetsByRubricID":

                    List<Facet> facetList = null;
                    try
                    {
                        //facetList = _facetManager.RetrieveFacetsByRubricID(Int32.Parse(txtBoxInput1.Text));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There was a problem retrieving the list of facets." + ex.Message);
                    }

                    var facetListSorted = facetList.Select(f => new { f.FacetID, f.Description, f.FacetType });

                    datViewList.ItemsSource = facetListSorted;
                    datViewList.Visibility = Visibility.Visible;

                    break;
                case "CriteriaByRubricID":

                    List<Criteria> criteriaList = null;
                    try
                    {
                        //criteriaList = _criteriaManager.RetrieveCriteriasForRubricByRubricID(Int32.Parse(txtBoxInput1.Text));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There was a problem retrieving the list of criteria." + ex.Message);
                    }

                    var criteriaListSorted = criteriaList.Select(c => new { c.FacetID, c.CriteriaID, c.Content, c.Score });


                    datViewList.ItemsSource = criteriaListSorted;
                    datViewList.Visibility = Visibility.Visible;

                    break;
                default:
                    break;
            }           

        }

        private void btnRetrieveActiveRubrics_Click(object sender, RoutedEventArgs e)
        {
            viewAllActiveRubrics();
        }

        private void mnuViewAllRubrics_Click(object sender, RoutedEventArgs e)
        {
            viewAllActiveRubrics();
        }

        private void mnuViewAllFacets_Click(object sender, RoutedEventArgs e)
        {
            viewAllActivateFacets();
        }
        //private void mnuRetrieveFacetsByRubricID_Click(object sender, RoutedEventArgs e)
        //{
        //    preparegrdCreateControlsForRetrieveFacetsByRubricID();
        //}

        //private void mnuRetrieveCriteriaByRubricID_Click(object sender, RoutedEventArgs e)
        //{
        //    //preparegrdCreateControlsForRetrieveCriteriaByRubicID();
        //}

        private void datViewList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _rubric = (Rubric)datViewList.SelectedItem;

            try
            {
                // errors with fakes here
                _rubricVM = _rubricManagerVM.RetrieveRubricByRubricID(_rubric.RubricID);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem retrieving the single rubric." + ex.Message);
            }
            rubricVMDetailView();

        }

        private void rubricVMDetailView()
        {            

            datViewList.Visibility = Visibility.Hidden;
            toggleListAndDetails();

            this.DataContext = _rubricVM;
            icFacetCriteria.ItemsSource = _rubricVM.FacetCriteria;
            icScores.ItemsSource = _rubricVM.RubricScoreColumn();

        }

        private void toggleListAndDetails()
        {
            if (datViewList.Visibility == Visibility.Visible)
            {
                btnConfirmEdits.Visibility = Visibility.Hidden;
                btnCancelEdits.Visibility = Visibility.Hidden;
                lblDetailRubricTitle.Visibility = Visibility.Hidden;
                icFacetCriteria.Visibility = Visibility.Hidden;
                icScores.Visibility = Visibility.Hidden;
            }
            else
            {
                btnConfirmEdits.Visibility = Visibility.Visible;
                btnCancelEdits.Visibility = Visibility.Visible;
                lblDetailRubricTitle.Visibility = Visibility.Visible;
                icFacetCriteria.Visibility = Visibility.Visible;
                icScores.Visibility = Visibility.Visible;
            }
        }

        private void mnuConfirmUpdatesToRubric_Click(object sender, RoutedEventArgs e)
        {

            RubricVM oldRubricVM = _rubricManagerVM.RetrieveRubricByRubricID(_rubric.RubricID);

            // not great, but removes focus if the event is trigger while the cursor is in the box
            pwdPassword.Focus();

            //RubricVM oldRubricVM = createRubricVM(_rubric);

            bool isValid = true;

            foreach (var entry in _rubricVM.FacetCriteria)
            {
                foreach (Criteria criteria in entry.Value)
                {
                    if (!criteria.CriteriaID.IsValidLength(50))
                    {
                        MessageBox.Show("The criteria name of:\n" + criteria.CriteriaID + "\nis too long. Please shorten.", "Criteria Name Too Long");
                        isValid = false;
                        break;
                    }
                    if (!criteria.Content.IsValidLength(255))
                    {
                        MessageBox.Show("The criteria content of:\n" +criteria.Content + "\nis too long. Please shorten.", "Criteria Content Too Long");
                        isValid = false;
                        break;
                    }                    
                }               
            }

            if (isValid)
            {
                //Dictionary<Facet, List<Criteria>> 
                string resultMessage = "";

                try
                {
                    bool result = _criteriaManager.UpdateCriteriaByCriteriaFacetDictionary(oldRubricVM.FacetCriteria, _rubricVM.FacetCriteria);

                    if (result)
                    {
                        resultMessage = "Successfully updated rubric.";
                    }
                    else
                    {
                        resultMessage = "Did not update the rubric.";
                    }
                }
                catch (Exception ex)
                {
                    resultMessage = "There was a problem updating:\n " + ex.Message;
                }
                MessageBox.Show(resultMessage);
            }
           
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCancelEdits_Click(object sender, RoutedEventArgs e)
        {


            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to undo all the edits? All changes will not be changed and the original rubric will be loaded.","Cancel Edits", MessageBoxButton.OKCancel);

            switch (messageBoxResult)
            {
                case MessageBoxResult.None:
                    break;
                case MessageBoxResult.OK:
                    try
                    {
                        RubricVM oldRubricVM = _rubricManagerVM.RetrieveRubricByRubricID(_rubric.RubricID);
                        _rubricVM = oldRubricVM;

                        this.DataContext = _rubricVM;
                        icFacetCriteria.ItemsSource = _rubricVM.FacetCriteria;
                        icScores.ItemsSource = _rubricVM.RubricScoreColumn();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Problem retrieving old rubric" + ex.Message);
                    }
                    break;
                case MessageBoxResult.Cancel:
                    break;                    
                default:
                    break;
            }
        }

        private void btnCreateRubric_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _rubricManager.CreateRubric(txtBoxTitle.Text, txtBoxDescription.Text, cmbBoxScoreTypes.SelectedItem.ToString(), _user.UserID);
                //_rubric = _rubricManager.RetrieveRubricByNameDescriptionScoreTypeIDRubricCreator(txtBoxTitle.Text, txtBoxDescription.Text, cmbBoxScoreTypes.SelectedItem.ToString(), _user.UserID);

                _rubricVM = _rubricManagerVM.RetrieveRubricByNameDescriptionScoreTypeIDRubricCreator(txtBoxTitle.Text, txtBoxDescription.Text, cmbBoxScoreTypes.SelectedItem.ToString(), _user.UserID);

                rubricVMDetailView();

                tabFacets.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem creating the rubric.\n" + ex.Message, "Problem Creating Rubric");
            }
        }

        private void btnCreateRubricBack_Click(object sender, RoutedEventArgs e)
        {
            tabTemplate.Focus();
        }

        private void btnFacetAdd_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                _facetManager.CreateFacet(_rubricVM.RubricID, txtBoxFacetTitle.Text, txtBoxFacetDescription.Text, cmbBoxFacetType.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem creating this facet.\n" + ex.Message);
            }

            try
            {
                _rubricVM = _rubricManagerVM.RetrieveRubricByNameDescriptionScoreTypeIDRubricCreator(txtBoxTitle.Text, txtBoxDescription.Text, cmbBoxScoreTypes.SelectedItem.ToString(), _user.UserID);
            }
            catch (Exception ex)
            {

                MessageBox.Show("There was a problem creating the rubric.\n" + ex.Message, "Problem Creating Rubric");
            }

            rubricVMDetailView();


        }




        // Build rubric the hard way
        //var rubric = (Rubric)datViewList.SelectedItem;

        //datViewList.Visibility = Visibility.Collapsed;


        //    _rubricVM = createRubricVM(rubric);

        //    this.DataContext = _rubricVM;


        //    icFacetCriteria.ItemsSource = _rubricVM.Facets;






        //int rowCount = 0;

        //for (int i = 0; i < _rubricVM.FacetCriteria.Count; i++)
        //{
        //    ColumnDefinition column = new ColumnDefinition();
        //    grdActionArea.ColumnDefinitions.Add(column);

        //    if (_rubricVM.FacetCriteria.ElementAt(i).Value.Count > rowCount)
        //    {
        //        rowCount = _rubricVM.FacetCriteria.ElementAt(i).Value.Count;
        //    }
        //}

        //foreach (var entry in _rubricVM.FacetCriteria)
        //{
        //    ColumnDefinition column = new ColumnDefinition();
        //    grdActionArea.ColumnDefinitions.Add(column);

        //    if (entry.Value.Count > rowCount)
        //    {
        //        rowCount = entry.Value.Count;
        //    }
        //}

        //// add one extra row for the header

        //for (int i = 0; i < rowCount + 1; i++)

        //for (int i = 0; i < rowCount +1; i++)

        //{
        //    RowDefinition row = new RowDefinition();
        //    grdActionArea.RowDefinitions.Add(row);
        //}


        //for (int i = 0; i < _rubricVM.FacetCriteria.Count; i++)
        //{
        //    Label facetHeader = new Label();
        //    facetHeader.Content = _rubricVM.FacetCriteria.ElementAt(i).Key.FacetID;

        //    // add more formating for header   
        //    facetHeader.FontWeight = FontWeights.Bold;
        //    facetHeader.FontSize = facetHeader.FontSize * HEADING_ONE_MULTIPLIER;
        //    facetHeader.HorizontalAlignment = HorizontalAlignment.Center;
        //    facetHeader.VerticalAlignment = VerticalAlignment.Center;


        //    //Grid.SetColumn(facetHeader, i + 1);
        //    //Grid.SetRow(facetHeader, 0);
        //    //grdActionArea.Children.Add(facetHeader);

        //    facetHeader.SetValue(Grid.ColumnProperty, i + 1);
        //    facetHeader.SetValue(Grid.RowProperty, 0);
        //    grdActionArea.Children.Add(facetHeader);

        //    foreach (Criteria criteria in _rubricVM.FacetCriteria.ElementAt(i).Value)
        //    {

        //        //TextBox textBox = new TextBox();
        //        //textBox.Text = criteria.Content;
        //        //Grid.SetColumn(textBox, i + 1);
        //        //Grid.SetRow(textBox, rubricVM.FacetCriteria.ElementAt(i).Value.IndexOf(criteria) + 1);
        //        //grdActionArea.Children.Add(textBox);

        //        RichTextBox criteriaTxtBox = new RichTextBox();
        //        FlowDocument document = new FlowDocument();
        //        Paragraph paragraph = new Paragraph();

        //        paragraph.Inlines.Add(criteria.Content);
        //        document.Blocks.Add(paragraph);
        //        criteriaTxtBox.Document.Blocks.Add(paragraph);

        //        //Grid.SetColumn(criteriaTxtBox, i + 1);
        //        //Grid.SetRow(criteriaTxtBox, rubricVM.FacetCriteria.ElementAt(i).Value.IndexOf(criteria) + 1);

        //        criteriaTxtBox.SetValue(Grid.ColumnProperty, i + 1);
        //        criteriaTxtBox.SetValue(Grid.RowProperty, _rubricVM.FacetCriteria.ElementAt(i).Value.IndexOf(criteria) + 1);

        //        grdActionArea.Children.Add(criteriaTxtBox);
        //    }
        //}

        //        grdActionArea.Children.Add(criteriaTxtBox);                    
        //    }
        //}

    }
}
