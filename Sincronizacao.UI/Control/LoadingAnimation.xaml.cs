using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace LoadingControl.Control
{
    
    public partial class LoadingAnimation : UserControl
    {
        Storyboard LoaderAnimation = null;

        public LoadingAnimation()
        {
            InitializeComponent();
            LoaderAnimation = this.Resources["ProgressAnimation"] as Storyboard;
        }

        public void StartStopLoader(bool operationFlag)
        {
            if (LoaderAnimation != null)
            {
                if (operationFlag)
                    LoaderAnimation.Begin();
                else
                    LoaderAnimation.Stop();
            }
        }

    }
}
