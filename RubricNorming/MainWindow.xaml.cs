﻿using DataAccessFakes;
using DataObjects;
using LogicLayer;
using System;
using System.Windows;

namespace RubricNorming
{

    public partial class MainWindow : Window
    {

        UserManager _userManager = null;
        User _user = null;
        RubricManager _rubricManager = null;


        public MainWindow()
        {
            // uses default user accessor
            _userManager = new UserManager();
            _rubricManager = new RubricManager();

            // uses the fake user accessor
            //_userManager = new UserManager(new UserAccessorFake());
            //_rubricManager = new RubricManager(new RubricAccessorFake());

            InitializeComponent();
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
            datActiveRubrics.Visibility = Visibility.Visible;
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
            datActiveRubrics.Visibility = Visibility.Hidden;

            btnLogin.Content = "Login";
            btnLogin.Focus();

            btnLogin.IsDefault = true;

            //hide all user tabs

            txtUserID.Focus();
        }

        private void frmMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtUserID.Focus();

            // function that toggles visablity on all controls would be nice
            // or like a flag that controls visiablity
            stkRubricControls.Visibility = Visibility.Hidden;

            //hideAllUserTabs();
        }

        private void viewAllActiveRubrics()
        {
            try
            {
                datActiveRubrics.ItemsSource = _rubricManager.RetrieveActiveRubrics();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was a problem retrieving the list of rubrics." + ex.Message);
            }
        }
    }
}
