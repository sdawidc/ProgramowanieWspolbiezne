
using System;
using System.Collections.ObjectModel;
using Presentation.Model;
using Presentation.ViewModel.MVVMLight;
using ModelIBall = Presentation.Model.IBall;
using System.Windows.Input;

namespace Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
        #region Private Fields
        private IDisposable Observer = null;
        private ModelAbstractApi ModelLayer;
        private bool Disposed = false;

        private string _numberOfBalls;

        #endregion Private Fields

        #region Constructor

        public MainWindowViewModel() : this(null)
    { }

        internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            ModelLayer = modelLayerAPI == null ? ModelAbstractApi.CreateModel() : modelLayerAPI;
            Observer = ModelLayer.Subscribe<ModelIBall>(x => Balls.Add(x));
            NumberOfBalls = "5"; 
        }

        #endregion Constructor

        #region Public Properties

        public string NumberOfBalls
        {
            get => _numberOfBalls;
            set
            {
                if (_numberOfBalls != value)
                {
                    _numberOfBalls = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        private ICommand _startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (_startCommand == null)
                {
                    _startCommand = new RelayCommand(
                      () =>
                      {
                          // Konwersja wprowadzonej wartości na int
                          if (int.TryParse(NumberOfBalls, out int count) && count > 0)
                          {
                              Start(count);
                          }
                        
                      },
                      () => !string.IsNullOrWhiteSpace(NumberOfBalls) 
                    );
                }
                return _startCommand;
            }
        }

        #endregion Public Properties

        #region public API

        public void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));

            Balls.Clear();
            ModelLayer.Start(numberOfBalls);
        }

        #endregion Public API


        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Balls.Clear();
                    Observer.Dispose();
                    ModelLayer.Dispose();
                }
                Disposed = true;
            }
        }

        public void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}