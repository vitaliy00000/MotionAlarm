using MotionAlarm.Models;
using MotionAlarm.PageModels;

namespace MotionAlarm.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}