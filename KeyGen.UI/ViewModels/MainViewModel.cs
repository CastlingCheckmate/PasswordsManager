using System;
using System.Linq;
using System.Windows.Input;

using Extensions.ByteArrayExtensions;
using Extensions.StringExtensions;
using WPF.UI.Commands;
using WPF.UI.ViewModels;

namespace KeyGen.UI.ViewModels
{

    public class MainViewModel : ViewModelBase
    {

        private const int ChunkSize = 16;
        private const char PasswordChar = '*';

        private ICommand _generateKeyCommand;
        private ICommand _generateInitializationVectorCommand;
        private ICommand _showOrHideKeyCommand;
        private ICommand _showOrHideInitializationVectorCommand;
        private ICommand _saveKeyCommand;
        private ICommand _saveInitializationVectorCommand;
        private string _generatedKey = string.Empty;
        private string _generatedInitializationVector = string.Empty;
        private bool _isKeyMasked = true;
        private bool _isInitializationVectorMasked = true;

        private Random RandomSource
        {
            get;

            set;
        } = new Random();

        public ICommand GenerateKeyCommand =>
            _generateKeyCommand ?? (_generateKeyCommand = new RelayCommand(_ => GenerateKey()));

        public ICommand GenerateInitializationVectorCommand =>
            _generateInitializationVectorCommand ?? (_generateInitializationVectorCommand = new RelayCommand(_ => GenerateInitializationVector()));


        public ICommand ShowOrHideKeyCommand =>
            _showOrHideKeyCommand ?? (_showOrHideKeyCommand = new RelayCommand(_ => IsKeyMasked = !IsKeyMasked));

        public ICommand ShowOrHideInitializationVectorCommand =>
            _showOrHideInitializationVectorCommand ?? (_showOrHideInitializationVectorCommand = new RelayCommand(_ => IsInitializationVectorMasked = !IsInitializationVectorMasked));

        public ICommand SaveKeyCommand =>
            _saveKeyCommand ?? (_saveKeyCommand = new RelayCommand(_ => SaveKey(), _ => !string.IsNullOrEmpty(GeneratedKey)));

        public ICommand SaveInitializationVectorCommand =>
            _saveInitializationVectorCommand ?? (_saveInitializationVectorCommand = new RelayCommand(_ => SaveInitializationVector(), _ => !string.IsNullOrEmpty(GeneratedInitializationVector)));

        public string GeneratedKey
        {
            get =>
                string.Join(Environment.NewLine, (IsKeyMasked ? string.Concat(Enumerable.Repeat(PasswordChar, _generatedKey.Length)) : _generatedKey)
                    .SplitByChunks(ChunkSize));

            set
            {
                _generatedKey = value;
                NotifyPropertyChanged(nameof(GeneratedKey));
            }
        }

        public string GeneratedInitializationVector
        {
            get =>
                string.Join(Environment.NewLine, (IsInitializationVectorMasked ? string.Concat(Enumerable.Repeat(PasswordChar, _generatedInitializationVector.Length)) : _generatedInitializationVector)
                    .SplitByChunks(ChunkSize));

            set
            {
                _generatedInitializationVector = value;
                NotifyPropertyChanged(nameof(GeneratedInitializationVector));
            }
        }

        public bool IsKeyMasked
        {
            get =>
                _isKeyMasked;

            set
            {
                _isKeyMasked = value;
                NotifyPropertyChanged(nameof(IsKeyMasked), nameof(ShowOrHideKey), nameof(GeneratedKey));
            }
        }

        public string ShowOrHideKey =>
            IsKeyMasked ? "Show" : "Hide";

        public bool IsInitializationVectorMasked
        {
            get =>
                _isInitializationVectorMasked;

            set
            {
                _isInitializationVectorMasked = value;
                NotifyPropertyChanged(nameof(IsInitializationVectorMasked), nameof(ShowOrHideInitializationVector), nameof(GeneratedInitializationVector));
            }
        }

        public string ShowOrHideInitializationVector =>
            IsInitializationVectorMasked ? "Show" : "Hide";

        private void GenerateKey()
        {
            var result = new byte[24];
            RandomSource.NextBytes(result);
            IsKeyMasked = true;
            GeneratedKey = result.ToHexadecimalString();
        }

        private void GenerateInitializationVector()
        {
            // TODO: add radiogroup to store key/block sizes
            var result = new byte[24];
            RandomSource.NextBytes(result);
            IsInitializationVectorMasked = true;
            GeneratedInitializationVector = result.ToHexadecimalString();
        }

        private void SaveKey()
        {
            
        }

        private void SaveInitializationVector()
        {

        }

    }

}