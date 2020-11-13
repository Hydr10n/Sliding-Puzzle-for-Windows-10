using System;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace Sliding_Puzzle
{
    class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private TimeSpan time;
        public TimeSpan Time
        {
            get => time;
            set
            {
                time = value;
                OnPropertyChanged(nameof(Time));
            }
        }

        private TimeSpan bestTime;
        public TimeSpan BestTime
        {
            get => bestTime;
            set
            {
                bestTime = value;
                OnPropertyChanged(nameof(BestTime));
            }
        }

        public virtual GameState GameState { get; set; } = GameState.NotStarted;
    }

    sealed class ViewModelEx : ViewModel
    {
        private bool isGamepadActive;
        public bool IsGamepadActive
        {
            get => isGamepadActive;
            set
            {
                isGamepadActive = value;
                OnPropertyChanged(nameof(IsGamepadActive));
            }
        }

        public bool IsVictoryWhenGameOver { get; private set; }
        public double GameStateTextOpacity { get; private set; }
        public Visibility GameStateTextVisibility { get; private set; } = Visibility.Collapsed;
        public override GameState GameState
        {
            get => base.GameState;
            set
            {
                base.GameState = value;
                switch (value)
                {
                    case GameState.Won:
                        IsVictoryWhenGameOver = true;
                        GameStateTextVisibility = Visibility.Visible;
                        GameStateTextOpacity = 0.9;
                        break;
                    default:
                        GameStateTextVisibility = Visibility.Collapsed;
                        GameStateTextOpacity = 0;
                        break;
                }
                OnPropertyChanged(nameof(IsVictoryWhenGameOver));
                OnPropertyChanged(nameof(GameStateTextVisibility));
                OnPropertyChanged(nameof(GameStateTextOpacity));
            }
        }
    }
}