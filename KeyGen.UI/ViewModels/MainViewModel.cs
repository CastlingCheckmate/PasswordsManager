using Microsoft.Win32;
using System;
using System.IO;
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
        private ICommand _clearKeyCommand;
        private ICommand _clearInitializationVectorCommand;
        private byte[] _generatedKey = null;
        private byte[] _generatedInitializationVector = null;
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
            _showOrHideKeyCommand ?? (_showOrHideKeyCommand = new RelayCommand(_ => IsKeyMasked = !IsKeyMasked,
                _ => GeneratedKey != null));

        public ICommand ShowOrHideInitializationVectorCommand =>
            _showOrHideInitializationVectorCommand ?? (_showOrHideInitializationVectorCommand = new RelayCommand(_ => IsInitializationVectorMasked = !IsInitializationVectorMasked,
                _ => GeneratedInitializationVector != null));

        public ICommand SaveKeyCommand =>
            _saveKeyCommand ?? (_saveKeyCommand = new RelayCommand(_ => SaveKey(), _ => GeneratedKey != null));

        public ICommand SaveInitializationVectorCommand =>
            _saveInitializationVectorCommand ?? (_saveInitializationVectorCommand = new RelayCommand(_ => SaveInitializationVector(), _ => GeneratedInitializationVector != null));

        public ICommand ClearKeyCommand =>
            _clearKeyCommand ?? (_clearKeyCommand = new RelayCommand(_ => GeneratedKey = null, _ => GeneratedKey != null));

        public ICommand ClearInitializationVectorCommand =>
            _clearInitializationVectorCommand ?? (_clearInitializationVectorCommand = new RelayCommand(_ => GeneratedInitializationVector = null, _ => GeneratedInitializationVector != null));

        private byte[] GeneratedKey
        {
            get =>
                _generatedKey;

            set
            {
                _generatedKey = value;
                NotifyPropertyChanged(nameof(GeneratedKey), nameof(GeneratedKeyString));
            }
        }

        public string GeneratedKeyString =>
            GeneratedKey == null ? string.Empty : string.Join(Environment.NewLine, (IsKeyMasked ? string.Concat(Enumerable.Repeat(PasswordChar, _generatedKey.Length * 2)) :
                _generatedKey.ToHexadecimalString()).SplitByChunks(ChunkSize));

        private byte[] GeneratedInitializationVector
        {
            get =>
                _generatedInitializationVector;

            set
            {
                _generatedInitializationVector = value;
                NotifyPropertyChanged(nameof(GeneratedInitializationVector), nameof(GeneratedInitializationVectorString));
            }
        }

        public string GeneratedInitializationVectorString =>
            GeneratedInitializationVector == null ? string.Empty : string.Join(Environment.NewLine, (IsInitializationVectorMasked ? string.Concat(Enumerable.Repeat(PasswordChar, _generatedInitializationVector.Length * 2)) :
                _generatedInitializationVector.ToHexadecimalString()).SplitByChunks(ChunkSize));

        public bool IsKeyMasked
        {
            get =>
                _isKeyMasked;

            set
            {
                _isKeyMasked = value;
                NotifyPropertyChanged(nameof(IsKeyMasked), nameof(ShowOrHideKey), nameof(GeneratedKeyString));
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
                NotifyPropertyChanged(nameof(IsInitializationVectorMasked), nameof(ShowOrHideInitializationVector), nameof(GeneratedInitializationVectorString));
            }
        }

        public string ShowOrHideInitializationVector =>
            IsInitializationVectorMasked ? "Show" : "Hide";

        private void GenerateKey()
        {
            var result = new byte[24];
            RandomSource.NextBytes(result);
            IsKeyMasked = true;
            GeneratedKey = result;
        }

        private void GenerateInitializationVector()
        {
            // TODO: add radiogroup to store key/block sizes
            var result = new byte[24];
            RandomSource.NextBytes(result);
            IsInitializationVectorMasked = true;
            GeneratedInitializationVector = result;
        }

        private void SaveKey()
        {
            var saveFileDialog = new SaveFileDialog()
            {
                InitialDirectory = Environment.CurrentDirectory,
                OverwritePrompt = true,
                Title = "Saving key to...",
                AddExtension = true,
                Filter = "Rijndael key file (*.rijnkey)|*.rijnkey",
                CheckFileExists = false
            };
            if (!saveFileDialog.ShowDialog().Value)
            {
                return;
            }
            if (File.Exists(saveFileDialog.FileName))
            {
                File.Delete(saveFileDialog.FileName);
            }
            using (var keyFileStream = new BinaryWriter(new FileStream(saveFileDialog.FileName, FileMode.CreateNew)))
            {
                keyFileStream.Write(GeneratedKey);
            }
        }

        private void SaveInitializationVector()
        {
            var saveFileDialog = new SaveFileDialog()
            {
                InitialDirectory = Environment.CurrentDirectory,
                OverwritePrompt = true,
                Title = "Saving IV to...",
                AddExtension = true,
                Filter = "Rijndael IV file (*.rijniv)|*.rijniv",
                CheckFileExists = false
            };
            if (!saveFileDialog.ShowDialog().Value)
            {
                return;
            }
            if (File.Exists(saveFileDialog.FileName))
            {
                File.Delete(saveFileDialog.FileName);
            }
            using (var IVFileStream = new BinaryWriter(new FileStream(saveFileDialog.FileName, FileMode.CreateNew)))
            {
                IVFileStream.Write(GeneratedInitializationVector);
            }
        }

    }

}