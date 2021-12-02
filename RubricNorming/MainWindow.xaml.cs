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
using System.ComponentModel;

namespace RubricNorming
{
    public partial class MainWindow : Window
    {

        UserManager _userManager = null;
        User _user = null;
        FacetManager _facetManager = null;
        CriteriaManager _criteriaManager = null;
        RubricVMManager _rubricVMManager = null;
        ScoreTypeManager _scoreTypeManager = null;
        FacetTypeManager _facetTypeManager = null;

        CreateOrUpdateMode _createOrUpdateMode;

        //try to get rid of this guy
        IRubricManager<Rubric> _rubricManager = null;

        string _executionChoice = "";

        List<ScoreType> _scoreTypes = new List<ScoreType>();
        List<FacetType> _facetTypes = new List<FacetType>();

        Rubric _rubric = null;
        RubricVM _rubricVM = null;

        public MainWindow()
        {
            //// uses default accessors
            _userManager = new UserManager();
            _rubricManager = new RubricManager();
            _facetManager = new FacetManager();
            _criteriaManager = new CriteriaManager();
            _rubricVMManager = new RubricVMManager();
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
                if (i == _user.Roles.Count - 2)
                {
                    rolesList += " and";
                }
                else if (i < _user.Roles.Count - 2)
                {
                    rolesList += ",";
                }
            }
            //MessageBox.Show("Welcome back, " + _user.GivenName +
            //    "\n\n" + "Your roles are:" + rolesList);


            staMessage.Content = _user.UserID + " logged in as" + rolesList + " on " + DateTime.Now.ToShortTimeString();

            txtUserID.Text = "";
            pwdPassword.Password = "";
            txtUserID.Visibility = Visibility.Hidden;
            pwdPassword.Visibility = Visibility.Hidden;
            lblUserID.Visibility = Visibility.Hidden;
            lblPassword.Visibility = Visibility.Hidden;

            btnLogin.Content = "Log Out";
            btnLogin.IsDefault = false;

            prepareUIForRoles();

        }

        private void updateUIforLogOut()
        {
            _user = null;

            staMessage.Content = "Welcome. Please log in to continue.";

            txtUserID.Visibility = Visibility.Visible;
            pwdPassword.Visibility = Visibility.Visible;
            lblUserID.Visibility = Visibility.Visible;
            lblPassword.Visibility = Visibility.Visible;

            hideAllControls();

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
                _scoreTypes = _scoreTypeManager.RetrieveScoreTypes();
                cmbBoxScoreTypes.ItemsSource = _scoreTypes.Select(st => st.ScoreTypeID);
                cmbBoxScoreTypes.SelectedItem = _scoreTypes.ElementAt(0).ScoreTypeID;
                txtBlockScoreTypeDescription.Text = _scoreTypes.ElementAt(0).Description;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem retrieving score types.\n" + ex.Message);
            }

            try
            {
                _facetTypes = _facetTypeManager.RetrieveFacetTypes();
                cmbBoxFacetType.ItemsSource = _facetTypes.Select(ft => ft.FacetTypeID);
                cmbBoxFacetType.SelectedItem = _facetTypes.ElementAt(0).FacetTypeID;
                txtBlockFacetTypeDescription.Text = _facetTypes.ElementAt(0).Description;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem retrieving facet types.\n" + ex.Message);
            }

            hideAllControls();


            // for testing purposes only

            //datViewList.Visibility = Visibility.Visible;
            //viewAllActiveRubrics();

        }

        private void hideAllControls()
        {
            stkRubricControls.Visibility = Visibility.Hidden;
            stkCreateRubric.Visibility = Visibility.Hidden;

            datViewList.Visibility = Visibility.Hidden;
            tabsetCreateControls.Visibility = Visibility.Collapsed;

            detailActionArea.Visibility = Visibility.Hidden;

            mnuView.Visibility = Visibility.Collapsed;
            mnuEdit.Visibility = Visibility.Collapsed;
            mnuCreate.Visibility = Visibility.Collapsed;

            tabsetCreateControls.Visibility = Visibility.Collapsed;

            btnSave.Visibility = Visibility.Hidden;
            btnCancel.Visibility = Visibility.Hidden;
        }

        private void prepareUIForRoles()
        {
            // ('Creator', 'Can create new rubrics and add examples')
            //,('Admin', 'Manages users, rubrics, tests, examples')
            //,('Assessor', 'Can view rubrics')
            //,('Norming Trainee', 'Can train and take tests for rubrics')

            foreach (var role in _user.Roles)
            {
                switch (role)
                {
                    case "Admin":
                        creatorUI();
                        break;
                    case "Creator":
                        creatorUI();
                        break;
                    case "Assessor":
                        viewerUI();
                        break;
                    default:
                        break;
                }
            }
        }

        private void creatorUI()
        {
            stkRubricControls.Visibility = Visibility.Visible;
            stkCreateRubric.Visibility = Visibility.Visible;

            datViewList.Visibility = Visibility.Visible;            

            mnuView.Visibility = Visibility.Visible;
            mnuEdit.Visibility = Visibility.Visible;
            mnuCreate.Visibility = Visibility.Visible;

            detailActionArea.Visibility = Visibility.Visible;

            viewAllActiveRubrics();
            
        }

        private void viewerUI()
        {
            stkRubricControls.Visibility = Visibility.Visible;

            detailActionArea.Visibility = Visibility.Visible;
            datViewList.Visibility = Visibility.Visible;

            mnuView.Visibility = Visibility.Visible;

            viewAllActiveRubrics();
        }

        private void viewAllActiveRubrics()
        {
            tabsetCreateControls.Visibility = Visibility.Hidden;

            List<Rubric> rubricList = null;
            try
            {
                rubricList = _rubricManager.RetrieveActiveRubrics();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem retrieving the list of rubrics." + ex.Message);
            }

            lblActionAreaTitle.Content = "All Rubrics";
            lblActionAreaTitle.Visibility = Visibility.Visible;

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
                _rubricVM = _rubricVMManager.RetrieveRubricByRubricID(_rubric.RubricID);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Problem retrieving the single rubric." + ex.Message);
            }
            rubricVMDetailView();

            _createOrUpdateMode = CreateOrUpdateMode.Edit;

        }

        private void rubricVMDetailView()
        {

            datViewList.Visibility = Visibility.Hidden;
            toggleListAndDetails();

            lblActionAreaTitle.Content = _rubricVM.Name;

            this.DataContext = _rubricVM;
            icFacetCriteria.ItemsSource = _rubricVM.FacetCriteria;
            icScores.ItemsSource = _rubricVM.RubricScoreColumn();

        }

        private void toggleListAndDetails()
        {
            if (datViewList.Visibility == Visibility.Visible)
            {
                btnSave.Visibility = Visibility.Hidden;
                btnCancel.Visibility = Visibility.Hidden;
                //lblActionAreaTitle.Visibility = Visibility.Visible;
                icFacetCriteria.Visibility = Visibility.Hidden;
                icScores.Visibility = Visibility.Hidden;
            }
            else
            {
                btnSave.Visibility = Visibility.Visible;
                btnCancel.Visibility = Visibility.Visible;
                //lblActionAreaTitle.Visibility = Visibility.Visible;
                icFacetCriteria.Visibility = Visibility.Visible;
                icScores.Visibility = Visibility.Visible;
            }
        }

        private void mnuConfirmUpdatesToRubric_Click(object sender, RoutedEventArgs e)
        {
            UpdateRubric();
        }

        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnCreateRubric_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _rubricManager.CreateRubric(txtBoxTitle.Text, txtBoxDescription.Text, cmbBoxScoreTypes.SelectedItem.ToString(), _user.UserID);

                _rubric = _rubricManager.RetrieveRubricByNameDescriptionScoreTypeIDRubricCreator(txtBoxTitle.Text, txtBoxDescription.Text, cmbBoxScoreTypes.SelectedItem.ToString(), _user.UserID);

                _rubricVM = _rubricVMManager.RetrieveRubricByNameDescriptionScoreTypeIDRubricCreator(txtBoxTitle.Text, txtBoxDescription.Text, cmbBoxScoreTypes.SelectedItem.ToString(), _user.UserID);

                rubricVMDetailView();
                tabFacets.IsEnabled = true;

                tabFacets.Focus();

            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem creating the rubric.\n" + ex.Message, "Problem Creating Rubric");
            }

            sldCriteriaBottomRange.Value = 1;
            sldCriteriaTopRange.Value = 4;
            _createOrUpdateMode = CreateOrUpdateMode.Create;
        }



        private void btnFacetAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _facetManager.CreateFacet(_rubricVM.RubricID, txtBoxFacetTitle.Text, txtBoxFacetDescription.Text, cmbBoxFacetType.SelectedItem.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem creating this facet.\n" + ex.Message, "Problem Creating Rubric");
            }

            try
            {
                _rubricVM = _rubricVMManager.RetrieveRubricByNameDescriptionScoreTypeIDRubricCreator(txtBoxTitle.Text, txtBoxDescription.Text, cmbBoxScoreTypes.SelectedItem.ToString(), _user.UserID);
            }
            catch (Exception ex)
            {

                MessageBox.Show("There was a problem creating the rubric.\n" + ex.Message, "Problem Creating Rubric");
            }

            rubricVMDetailView();
        }

        private void sldCriteriaBottomRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sldCriteriaBottomRange == null || sldCriteriaTopRange == null)
            {
                // do nothing, stops problems on load, saves results if changed during course of program
            }
            else if (sldCriteriaBottomRange.Value >= sldCriteriaTopRange.Value && sldCriteriaTopRange.Value <= sldCriteriaTopRange.Maximum)
            {
                sldCriteriaTopRange.Value++;
            }
        }

        private void sldCriteriaTopRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (sldCriteriaBottomRange == null || sldCriteriaTopRange == null)
            {

                // do nothing, stops problems on load, saves results if changed during course of program
            }
            else if (sldCriteriaTopRange.Value <= sldCriteriaBottomRange.Value && sldCriteriaBottomRange.Value >= sldCriteriaBottomRange.Minimum)
            {
                sldCriteriaBottomRange.Value--;
            }
        }

        //private void btnCreateFacetNext_Click(object sender, RoutedEventArgs e)
        //{
        //    tabCriteria.Focus();
        //}

        private void mnuCancelUpdatesToRubric_Click(object sender, RoutedEventArgs e)
        {
            cancelUpdatesToRubric();
        }

        private void cancelUpdatesToRubric()
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to undo all the edits? All changes will not be changed and the original rubric will be loaded.", "Cancel Edits", MessageBoxButton.OKCancel);

            switch (messageBoxResult)
            {
                case MessageBoxResult.None:
                    break;
                case MessageBoxResult.OK:
                    try
                    {
                        RubricVM oldRubricVM = _rubricVMManager.RetrieveRubricByRubricID(_rubric.RubricID);
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

        private void UpdateRubric()
        {
            RubricVM oldRubricVM = _rubricVMManager.RetrieveRubricByRubricID(_rubric.RubricID);

            // not great, but removes focus if the event is trigger while the cursor is in the box
            pwdPassword.Focus();

            //RubricVM oldRubricVM = createRubricVM(_rubric);            

            if (isValidFacetCriteriaDictionary())
            {
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

        private bool isValidFacetCriteriaDictionary()
        {
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
                        MessageBox.Show("The criteria content of:\n" + criteria.Content + "\nis too long. Please shorten.", "Criteria Content Too Long");
                        isValid = false;
                        break;
                    }
                }
            }



            return isValid;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            switch (_createOrUpdateMode)
            {
                case CreateOrUpdateMode.Create:
                    addFacetCriteriaDictionaryToDB();
                    break;
                case CreateOrUpdateMode.Edit:
                    UpdateRubric();
                    break;
                default:
                    break;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            switch (_createOrUpdateMode)
            {
                case CreateOrUpdateMode.Create:

                    break;
                case CreateOrUpdateMode.Edit:
                    cancelUpdatesToRubric();
                    break;
                default:
                    break;
            }
        }

        private void addFacetCriteriaDictionaryToDB()
        {
            bool isAdded = false;
            pwdPassword.Focus();

            if (isValidFacetCriteriaDictionary())
            {
                try
                {
                    isAdded = _criteriaManager.CreateCriteriaFromFacetCriteriaDictionary(_rubricVM.FacetCriteria);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problem saving facets and criteria.\n" + ex.Message);
                }
            }

            if (isAdded)
            {
                MessageBox.Show("Successfully saved the rubric.");
                
            }

            icScores.Visibility = Visibility.Visible;
            icFacetCriteria.Visibility = Visibility.Visible;
        }

        private void btnScoreRangeAdd_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                _rubricVM = _rubricVMManager.RetrieveRubricByNameDescriptionScoreTypeIDRubricCreator(txtBoxTitle.Text, txtBoxDescription.Text, cmbBoxScoreTypes.SelectedItem.ToString(), _user.UserID);
                
            }
            catch (Exception ex)
            {

                MessageBox.Show("There was a problem creating the rubric.\n" + ex.Message, "Problem Creating Rubric");
            }

            foreach (var entry in _rubricVM.FacetCriteria)
            {
                List<Criteria> criteriaList = _rubricVMManager.CreateBlankCriteriaForCreateRubricVM(entry.Key.RubricID, entry.Key.FacetID, sldCriteriaBottomRange.Value, sldCriteriaTopRange.Value);
                entry.Value.Clear();
                entry.Value.AddRange(criteriaList);
            }

            rubricVMDetailView();
        }

        private void cmbBoxScoreTypes_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            string selectedItem = comboBox.Text;

            foreach (ScoreType scoreType in _scoreTypes)
            {
                if (scoreType.ScoreTypeID == selectedItem)
                {
                    txtBlockScoreTypeDescription.Text = scoreType.Description;
                    break;
                }
            }
        }

        private void cmbBoxFacetType_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;

            string selectedItem = comboBox.Text;

            foreach (FacetType facet in _facetTypes)
            {
                if (facet.FacetTypeID == selectedItem)
                {
                    txtBlockFacetTypeDescription.Text = facet.Description;
                    break;
                }
            }
        }

        private void mnuCreateNewRubric_Click(object sender, RoutedEventArgs e)
        {
            tabsetCreateControls.Visibility = Visibility.Visible;
            tabFacets.IsEnabled = false;
            tabCreate.Focus();
            
            lblActionAreaTitle.Visibility = Visibility.Hidden;
            datViewList.Visibility = Visibility.Hidden;
            toggleListAndDetails();

            icScores.Visibility = Visibility.Hidden;
            icFacetCriteria.Visibility = Visibility.Hidden;

            txtBoxTitle.Focus();
        }
    }

    enum CreateOrUpdateMode
    {
        Create,
        Edit
    }
}
