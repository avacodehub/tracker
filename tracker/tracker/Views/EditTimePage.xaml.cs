﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tracker.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace tracker.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditTimePage : ContentPage
    {
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }

        public Project LocalProject { get; set; }
        public EditTimePage(Project project)
        {
            InitializeComponent();

            LocalProject = project;
            Hours = (int)project.Time.TotalHours;
            Minutes = project.Time.Minutes;

            BindingContext = this;
        }

        private void btCancelClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }

        private void btnApplyClicked(object sender, EventArgs e)
        {
            errMinutes.IsVisible = false;
            if (Minutes >= 60)
            {
                errMinutes.IsVisible = true;
                return;
            }

            if (Hours > 24)
            {
                Days = Hours / 24;
                Hours %= 24;
            }

            //DAYTIME CORRECTION
            TimeSpan newTime = new TimeSpan(Days, Hours, Minutes, 0);
            TimeSpan gap = newTime - LocalProject.Time;

            if (Math.Abs( gap.TotalHours) > Math.Abs(App.MAX_DAY_DURATION))
            {
                DisplayAlert("Info", "Difference must be less than 12 h", "Close");
                return;
                
            }
            Navigation.PopAsync();
            LocalProject.Time = newTime;
            MessagingCenter.Send<Project>(LocalProject, "EditTimeMessage");
            MessagingCenter.Send<Project, TimeSpan>(LocalProject, "RecalcDays", gap);
            

        }
    }
}