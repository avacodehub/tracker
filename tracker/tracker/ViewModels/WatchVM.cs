﻿using MvvmHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using tracker.Models;
using tracker.Views;
using Xamarin.Forms;

namespace tracker.ViewModels
{
    public class WatchVM : BaseViewModel
    {
        public WatchVM()
        {
            WatchProjects = new ObservableRangeCollection<Project>();

            var range = new List<Project>(App.DBWatch.GetItems());
            WatchProjects.AddRange(range);

            //fixes indicators when page loads 1st time
            foreach (var p in WatchProjects)
            {
                p.IsBusy = false;
            }

            MessagingCenter.Subscribe<Project>(this, "MsgAddWatchProject", (project) =>
            {
                App.DBWatch.SaveItem(project);
                WatchProjects.Add(project);

                Fetch(project);
            });

            FetchCommand = new Command(Fetch);

            WatchAddCommand = new Command(async () => { 
                await Navigation.PushAsync(new WatchAddPage(new Project())); 
            });

            WatchDeleteCommand = new Command(Delete);
        }

        public INavigation Navigation;
        public ObservableRangeCollection<Project> WatchProjects { get; set; }

        public ICommand FetchCommand { get; }
        public ICommand WatchAddCommand { get; }
        public ICommand WatchDeleteCommand { get; }

        private async void Delete(object parameter)
        {
            bool answer = await Application.Current.MainPage.DisplayAlert("ALERT", "You are going to delete this project \n Continue?", "Yes", "No");
            if (!answer)
            {
                return;
            }
            var project = parameter as Project;
            WatchProjects.Remove(project);
            App.DBWatch.DeleteItem(project.Id);
        }

        private async void Fetch(object parameter)
        {
            var project = parameter as Project;

            if (project.CustomId == null || project.CustomId == "")
            {
                await Application.Current.MainPage.DisplayAlert("Alert", "Custom ID must be at least 1 character long", "OK");
                return;
            }
            project.IsBusy = true;
            var item = await FetchProjectTask(project);
            if (item == null)
            {
                await Application.Current.MainPage.DisplayAlert("Alert", "ID does not exist", "OK");
                project.IsBusy = false;
                return;
            }
            try
            {
                project.Time = TimeSpan.Parse(item["time"]);
            }
            catch (Exception e) {
                await Application.Current.MainPage.DisplayAlert("Alert", "Invalid Time format " + e.Message, "OK");
            }

            App.DBWatch.SaveItem(project);
            project.IsBusy = false;
        }
        public async Task<Dictionary<string, string>> FetchProjectTask(Project project)
        {

            HttpClient client = new HttpClient();
            Uri uri = new Uri(string.Format(App.SERVER_URL, string.Empty) + project.CustomId);

            HttpResponseMessage response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    string content = await response.Content.ReadAsStringAsync();
                    var item = JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
                    return item;
                }
                catch
                {
                    await Application.Current.MainPage.DisplayAlert("Task", "JSON error", "OK");
                    return null;
                }
            }
            else { return null; }

        }
    }
}
