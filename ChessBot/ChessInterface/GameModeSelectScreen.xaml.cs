using ChessLogic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChessInterface
{
    /// <summary>
    /// Logique d'interaction pour GameModeSelectScreen.xaml
    /// </summary>
    public partial class GameModeSelectScreen : UserControl
    {
        private MainWindow mainWindow;
 

        
        public GameModeSelectScreen(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;

            var humanVsHumanUri = new Uri("pack://application:,,,/Assets/Icons/human-vs-human.png");
            HumanVSHumanImg.Source = new BitmapImage(humanVsHumanUri);

            var humanVsAIUri = new Uri("pack://application:,,,/Assets/Icons/human-vs-ai.png");
            HumanVSAiImg.Source = new BitmapImage(humanVsAIUri);

            var aiVsAIUri = new Uri("pack://application:,,,/Assets/Icons/ai-vs-ai.png");
            AiVSAiImg.Source = new BitmapImage(aiVsAIUri);


        }


        public void HumanVSHumanImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.HumanVSHuman();
        }

        private void AiVSAiImg_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void HumanVSAiImg_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
